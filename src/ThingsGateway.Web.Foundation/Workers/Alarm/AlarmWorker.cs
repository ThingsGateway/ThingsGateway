#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Enumerator;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备采集报警后台服务
/// </summary>
public class AlarmWorker : BackgroundService
{
    private static IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlarmWorker> _logger;
    private GlobalDeviceData _globalDeviceData;
    /// <inheritdoc cref="AlarmWorker"/>
    public AlarmWorker(ILogger<AlarmWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _globalDeviceData = scopeFactory.CreateScope().ServiceProvider.GetService<GlobalDeviceData>();
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
    public ConcurrentList<DeviceVariableRunTime> RealAlarmDeviceVariables { get; set; } = new();
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");
    private ConcurrentQueue<DeviceVariableRunTime> DeviceVariables { get; set; } = new();
    private ConcurrentQueue<DeviceVariableRunTime> HisAlarmDeviceVariables { get; set; } = new();
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<SqlSugarClient>> GetAlarmDbAsync()
    {
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
            catch (ObjectDisposedException)
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
    private CacheDb CacheDb { get; set; }

    /// <summary>
    /// 重启服务
    /// </summary>
    public void Restart()
    {
        Stop(_globalDeviceData.CollectDevices);
        Start();
    }

    internal void Start()
    {
        foreach (var item in _globalDeviceData.CollectDevices)
        {
            DeviceChange(item);
        }
        StoppingTokens.Add(new());
        Init();
        RealAlarmTask.Start();
        HisAlarmTask.Start();

    }

    internal void Stop(IEnumerable<CollectDeviceRunTime> devices)
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
            bool? realTaskResult = false;
            try
            {
                realTaskResult = realAlarmResult?.Wait(10000);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "等待线程停止错误");
            }
            if (realTaskResult == true)
            {
                _logger?.LogInformation($"实时报警线程已停止");
            }
            else
            {
                _logger?.LogWarning($"实时报警线程停止超时，已强制取消");
            }
        }
        RealAlarmTask?.SafeDispose();

        _logger?.LogInformation($"历史报警线程停止中");
        var hisAlarmResult = HisAlarmTask?.GetAwaiter().GetResult();
        bool? hisTaskResult = false;
        try
        {
            hisTaskResult = hisAlarmResult?.Wait(10000);
        }
        catch (ObjectDisposedException)
        {

        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "等待线程停止错误");
        }
        if (hisTaskResult == true)
        {
            _logger?.LogInformation($"历史报警线程已停止");
        }
        else
        {
            _logger?.LogWarning($"历史报警线程停止超时，已强制取消");
        }
        HisAlarmTask?.SafeDispose();
        StoppingToken?.SafeDispose();
        StoppingTokens.Remove(StoppingToken);

    }

    private void AlarmAnalysis(DeviceVariableRunTime item)
    {
        string limit = string.Empty;
        string ex = string.Empty;
        string text = string.Empty;

        AlarmEnum alarmEnum = AlarmEnum.None;

        if (item.DataTypeEnum.GetSystemType() == typeof(bool))
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
    private bool IsExited;

    private void AlarmChange(DeviceVariableRunTime item, object limit, string text, EventEnum eventEnum, AlarmEnum alarmEnum)
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

        OnAlarmChanged?.Invoke(item.Adapt<DeviceVariableRunTime>());
        if (!IsExited)
        {
            HisAlarmDeviceVariables.Enqueue(item);
        }

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

    private void DeviceVariableChange(DeviceVariableRunTime variable)
    {
        //这里不能序列化变量，报警服务需改变同一个变量指向的属性
        DeviceVariables.Enqueue(variable);

    }
    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        CacheDb = new("HistoryAlarmCache");
        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        RealAlarmTask = new Task<Task>(async () =>
        {
            await Task.Yield();//
            _logger?.LogInformation($"实时报警线程开始");
            while (!StoppingToken.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, StoppingToken.Token);
                    var list = DeviceVariables.ToListWithDequeue();
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
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"实时报警循环异常");
                }
            }

        }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);

        HisAlarmTask = new Task<Task>(async () =>
        {
            await Task.Yield();//
            _logger?.LogInformation($"历史报警线程开始");
            IsExited = false;
            try
            {
                await Task.Delay(500, StoppingToken.Token);

                var result = await GetAlarmDbAsync();
                if (!result.IsSuccess)
                {
                    _logger?.LogWarning($"历史报警线程即将退出：" + result.Message);
                    StatuString = new OperResult($"已退出：{result.Message}");
                    IsExited = true;
                    return;
                }
                else
                {
                    var sqlSugarClient = result.Content;
                    bool isSuccess = true;
                    /***创建/更新单个表***/
                    try
                    {
                        await sqlSugarClient.Queryable<HistoryAlarm>().FirstAsync(StoppingToken.Token);
                        isSuccess = true;
                        StatuString = OperResult.CreateSuccessResult();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            sqlSugarClient.CodeFirst.InitTables(typeof(HistoryAlarm));
                            isSuccess = true;
                            StatuString = OperResult.CreateSuccessResult();
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            StatuString = new OperResult(ex);
                            _logger.LogWarning(ex, "连接历史报警数据库失败");
                        }
                    }

                    while (!StoppingToken.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(500, StoppingToken.Token);

                            if (StoppingToken.Token.IsCancellationRequested)
                                break;

                            //缓存值
                            var cacheData = await CacheDb.GetCacheData();
                            var data = cacheData.SelectMany(a => a.CacheStr.FromJson<List<HistoryAlarm>>()).ToList();
                            try
                            {
                                var count = await sqlSugarClient.Insertable<HistoryAlarm>(data).ExecuteCommandAsync(StoppingToken.Token);
                                await CacheDb.DeleteCacheData(cacheData.Select(a => a.Id).ToArray());
                            }
                            catch (Exception ex)
                            {
                                if (isSuccess)
                                    _logger.LogWarning(ex, "写入历史报警失败");
                            }

                            if (StoppingToken.Token.IsCancellationRequested)
                                break;


                            var list = HisAlarmDeviceVariables.ToListWithDequeue();
                            if (list.Count != 0)
                            {
                                var hisalarm = list.Adapt<List<HistoryAlarm>>();
                                ////Sql保存
                                hisalarm.ForEach(it =>
                                {
                                    it.Id = YitIdHelper.NextId();
                                });
                                //插入
                                try
                                {
                                    await sqlSugarClient.Insertable(hisalarm).ExecuteCommandAsync(StoppingToken.Token);
                                    isSuccess = true;
                                }
                                catch (Exception ex)
                                {
                                    if (isSuccess)
                                        _logger.LogWarning(ex, "写入历史报警失败");

                                    var cacheDatas = hisalarm.ChunkTrivialBetter(500);
                                    await cacheDatas.ForeachAsync(async a =>
                                    {
                                        await CacheDb.AddCacheData("", a.ToJson(), 50000);
                                    });
                                }

                            }


                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        catch (Exception ex)
                        {
                            if (isSuccess)
                                _logger?.LogWarning($"历史报警循环异常:" + ex.Message);
                            StatuString = new OperResult(ex);
                            isSuccess = false;
                        }
                    }
                    IsExited = true;

                }
            }
            catch (TaskCanceledException)
            {
                IsExited = true;

            }
            catch (ObjectDisposedException)
            {
                IsExited = true;
            }
            catch (Exception ex)
            {
                IsExited = true;
                _logger?.LogError($"历史报警异常:" + ex.Message);
            }
        }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
    }
    #endregion
}


