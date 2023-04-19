using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备采集报警后台服务
/// </summary>
public class AlarmWorker : BackgroundService
{
    private static IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlarmWorker> _logger;
    private GlobalCollectDeviceData _globalCollectDeviceData;
    /// <inheritdoc cref="AlarmWorker"/>
    public AlarmWorker(ILogger<AlarmWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _globalCollectDeviceData = scopeFactory.CreateScope().ServiceProvider.GetService<GlobalCollectDeviceData>();
    }
    /// <summary>
    /// 报警变化事件
    /// </summary>
    public event VariableCahngeEventHandler OnAlarmChanged;
    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event DelegateOnDeviceChanged OnDeviceStatusChanged;
    /// <summary>
    /// 实时报警列表
    /// </summary>
    public ConcurrentList<CollectVariableRunTime> RealAlarmDeviceVariables { get; set; } = new();
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");
    private ConcurrentQueue<CollectVariableRunTime> CollectDeviceVariables { get; set; } = new();
    private ConcurrentQueue<CollectVariableRunTime> HisAlarmDeviceVariables { get; set; } = new();
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<SqlSugarClient>> GetAlarmDbAsync()
    {
        await Task.CompletedTask;
        using var serviceScope = _scopeFactory.CreateScope();
        var ConfigService = serviceScope.ServiceProvider.GetService<IConfigService>();
        var alarmEnable = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConst.Config_Alarm_Enable))?.ConfigValue?.ToBoolean();
        var alarmDbType = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConst.Config_Alarm_DbType))?.ConfigValue;
        var alarmConnstr = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConst.Config_Alarm_ConnStr))?.ConfigValue;

        if (!(alarmEnable == true))
        {
            return new OperResult<SqlSugarClient>("历史报警已配置为Disable");
        }

        var configureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 修改列可空-1、带?问号 2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                    column.IsNullable = true;
            },
        };

        DbType type = DbType.SqlServer;
        if (!string.IsNullOrEmpty(alarmDbType))
        {
            if (Enum.TryParse<DbType>(alarmDbType, ignoreCase: true, out var result))
            {
                type = result;
            }
            else
            {
                return new OperResult<SqlSugarClient>("数据库类型转换失败");
            }
        }

        var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = alarmConnstr,//连接字符串
            DbType = type,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
            MoreSettings = new ConnMoreSettings
            {
                SqlServerCodeFirstNvarchar = true,//设置默认nvarchar
                TableEnumIsString = true,

            },

        });

        return OperResult.CreateSuccessResult(sqlSugarClient);
    }

    #region worker服务
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("报警服务启动");
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("报警服务停止");
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
        }
    }


    #endregion

    #region 核心实现
    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    private Task<Task> HisAlarmTask;
    private Task<Task> RealAlarmTask;
    /// <summary>
    /// 重启服务
    /// </summary>
    public void Restart()
    {
        Stop(_globalCollectDeviceData.CollectDevices);
        Start();
    }

    internal void Start()
    {
        foreach (var item in _globalCollectDeviceData.CollectDevices)
        {
            DeviceChange(item);
        }
        StoppingTokens.Add(new());
        Init();
        RealAlarmTask.Start();
        HisAlarmTask.Start();

    }

    internal void Stop(IEnumerable<CollectDeviceRunTime> devices = null)
    {
        foreach (var device in devices)
        {
            device.DeviceStatusCahnge -= DeviceStatusCahnge;
            device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange -= DeviceVariableChange; });
        }

        CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
        StoppingToken?.Cancel();

        _logger?.LogInformation($"实时报警线程停止中");
        var realAlarmResult = RealAlarmTask?.Result;
        if (realAlarmResult?.Status != TaskStatus.Canceled)
        {
            if (realAlarmResult?.Wait(5000) == true)
            {
                _logger?.LogInformation($"实时报警线程已停止");
            }
            else
            {
                _logger?.LogInformation($"实时报警线程停止超时，已强制取消");
            }
        }
        RealAlarmTask?.Dispose();


        _logger?.LogInformation($"历史报警线程停止中");
        var hisAlarmResult = HisAlarmTask?.Result;
        if (hisAlarmResult?.Status != TaskStatus.Canceled)
        {
            if (hisAlarmResult?.Wait(5000) == true)
            {
                _logger?.LogInformation($"历史报警线程已停止");
            }
            else
            {
                _logger?.LogInformation($"历史报警线程停止超时，已强制取消");
            }
        }
        HisAlarmTask?.Dispose();
        StoppingTokens.Remove(StoppingToken);

    }

    private void AlarmAnalysis(CollectVariableRunTime item)
    {
        string limit = string.Empty;
        string ex = string.Empty;
        string text = string.Empty;

        AlarmEnum alarmEnum = AlarmEnum.None;

        if (item.DataTypeEnum.GetNetType() == typeof(bool))
        {

            alarmEnum = AlarmHostServiceHelpers.GetBoolAlarmCode(item, out limit, out ex, out text);
        }
        else
        {
            alarmEnum = AlarmHostServiceHelpers.GetDecimalAlarmDegree(item, out limit, out ex, out text);
        }
        if (alarmEnum == AlarmEnum.None)
        {
            //需恢复报警，如果存在的话
            AlarmChange(item, null, text, EventEnum.Finish, alarmEnum);
        }
        else
        {
            //需更新报警，不管是否存在
            if (!ex.IsNullOrEmpty())
            {
                var data = ex.GetExpressionsResult(item.Value);
                if (data is bool result)
                {
                    if (result)
                    {
                        AlarmChange(item, limit, text, EventEnum.Alarm, alarmEnum);
                    }
                }
            }
            else
            {
                AlarmChange(item, limit, text, EventEnum.Alarm, alarmEnum);
            }

        }
    }

    private void AlarmChange(CollectVariableRunTime item, object limit, string text, EventEnum eventEnum, AlarmEnum alarmEnum)
    {
        if (eventEnum == EventEnum.Finish)
        {
            //实时报警没有找到的话直接返回
            if (!RealAlarmDeviceVariables.Any(it => it.Id == item.Id))
            {
                return;
            }
        }
        else if (eventEnum == EventEnum.Alarm)
        {
            var variable = RealAlarmDeviceVariables.FirstOrDefault(it => it.Id == item.Id);
            if (variable != null)
            {
                if (item.AlarmTypeEnum == alarmEnum)
                    return;
            }
        }

        if (eventEnum == EventEnum.Alarm)
        {
            item.AlarmTypeEnum = alarmEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.AlarmTime = DateTime.UtcNow;
            item.EventTime = DateTime.UtcNow;
        }
        else if (eventEnum == EventEnum.Finish)
        {
            var oldAlarm = RealAlarmDeviceVariables.FirstOrDefault(it => it.Id == item.Id);
            item.AlarmTypeEnum = oldAlarm.AlarmTypeEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = oldAlarm.AlarmLimit;
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTime.UtcNow;
        }
        else if (eventEnum == EventEnum.Check)
        {
            item.AlarmTypeEnum = alarmEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTime.UtcNow;
        }

        OnAlarmChanged?.Invoke(item.Adapt<CollectVariableRunTime>());

        HisAlarmDeviceVariables.Enqueue(item);

        if (eventEnum == EventEnum.Alarm)
        {
            RealAlarmDeviceVariables.RemoveWhere(it => it.Id == item.Id);
            RealAlarmDeviceVariables.Add(item);
        }
        else
        {
            RealAlarmDeviceVariables.RemoveWhere(it => it.Id == item.Id);
        }
    }

    private void DeviceChange(CollectDeviceRunTime device)
    {
        device.DeviceStatusCahnge += DeviceStatusCahnge;
        device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange += DeviceVariableChange; });
    }

    private void DeviceStatusCahnge(CollectDeviceRunTime device)
    {
        OnDeviceStatusChanged?.Invoke(device.Adapt<CollectDeviceRunTime>());
    }

    private void DeviceVariableChange(CollectVariableRunTime variable)
    {
        //这里不能序列化变量，报警服务需改变同一个变量指向的属性
        CollectDeviceVariables.Enqueue(variable);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        RealAlarmTask = new Task<Task>(() =>
        {
            CancellationTokenSource StoppingToken = StoppingTokens.Last();
            return Task.Factory.StartNew(async (a) =>
            {
                _logger?.LogInformation($"实时报警线程开始");
                while (!StoppingToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(500, StoppingToken.Token);
                        var list = CollectDeviceVariables.ToListWithDequeue();
                        foreach (var item in list)
                        {
                            if (StoppingToken.Token.IsCancellationRequested)
                                break;
                            if (!item.AlarmEnable) continue;
                            AlarmAnalysis(item);
                        }
                        if (StoppingToken.Token.IsCancellationRequested)
                            break;
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, $"实时报警循环异常");
                    }
                }
            }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
        }
         );

        HisAlarmTask = new Task<Task>(() =>
        {
            CancellationTokenSource StoppingToken = StoppingTokens.Last();
            return Task.Factory.StartNew(async (a) =>
            {
                _logger?.LogInformation($"历史报警线程开始");

                try
                {
                    await Task.Delay(500, StoppingToken.Token);

                    var result = await GetAlarmDbAsync();
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning($"历史报警线程即将退出：" + result.Message);
                        StatuString = new OperResult($"已退出：{result.Message}");
                        return;
                    }
                    else
                    {
                        var sqlSugarClient = result.Content;
                        bool LastIsSuccess = true;
                        /***创建/更新单个表***/
                        try
                        {
                            await sqlSugarClient.Queryable<AlarmHis>().FirstAsync();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                sqlSugarClient.CodeFirst.InitTables(typeof(AlarmHis));
                            }
                            catch (Exception)
                            {
                            }
                        }

                        while (!StoppingToken.Token.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(600, StoppingToken.Token);

                                try
                                {
                                    await sqlSugarClient.Queryable<AlarmHis>().FirstAsync();
                                }
                                catch (Exception)
                                {
                                    sqlSugarClient.CodeFirst.InitTables(typeof(AlarmHis));
                                    throw new("数据库测试连接失败");
                                }
                                LastIsSuccess = true;
                                StatuString = OperResult.CreateSuccessResult();
                                var list = HisAlarmDeviceVariables.ToListWithDequeue();
                                if (list.Count == 0) continue;
                                if (!sqlSugarClient.Ado.IsValidConnection()) throw new("数据库测试连接失败");
                                ////Sql保存
                                var hisalarm = list.Adapt<List<AlarmHis>>();
                                hisalarm.ForEach(it =>
                                {
                                    it.Id = YitIdHelper.NextId();
                                }
                                    );
                                //插入
                                await sqlSugarClient.Insertable(hisalarm).ExecuteCommandAsync();

                                if (StoppingToken.Token.IsCancellationRequested)
                                    break;
                            }
                            catch (TaskCanceledException)
                            {

                            }
                            catch (Exception ex)
                            {
                                if (LastIsSuccess)
                                    _logger?.LogWarning($"历史报警循环异常:" + ex.Message);
                                StatuString = new OperResult($"异常：请查看后台日志");
                                LastIsSuccess = false;
                            }
                        }

                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    _logger?.LogError($"历史报警异常:" + ex.Message);
                }
            }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
        }
 );
    }
    #endregion
}


