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

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Admin.Application;
using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

using UAParser;

using Yitter.IdGenerator;

namespace ThingsGateway.Application;

/// <summary>
/// 设备采集报警后台服务
/// </summary>
public class AlarmWorker : BackgroundService
{
    private readonly GlobalDeviceData _globalDeviceData;
    private readonly ILogger<AlarmWorker> _logger;
    /// <inheritdoc cref="AlarmWorker"/>
    public AlarmWorker(ILogger<AlarmWorker> logger)
    {
        _logger = logger;
        _globalDeviceData = ServiceHelper.Services.GetService<GlobalDeviceData>();
    }
    /// <summary>
    /// 报警变化事件
    /// </summary>
    public event VariableChangeEventHandler OnAlarmChanged;
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
    private ConcurrentQueue<HistoryAlarm> HisAlarmDeviceVariables { get; set; } = new();
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<SqlSugarClient>> GetAlarmDbAsync()
    {
        var ConfigService = ServiceHelper.Services.GetService<IConfigService>();
        var alarmEnable = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConfigConst.Config_Alarm_Enable))?.ConfigValue?.ToBoolean();
        var alarmDbType = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConfigConst.Config_Alarm_DbType))?.ConfigValue;
        var alarmConnstr = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_AlarmConfig_Base, ThingsGatewayConfigConst.Config_Alarm_ConnStr))?.ConfigValue;

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

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    private static AlarmEnum GetBoolAlarmCode(DeviceVariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag.BoolCloseAlarmEnable && tag.Value.ToBoolean() == false)
        {
            limit = false.ToString();
            expressions = tag.BoolCloseRestrainExpressions;
            text = tag.BoolCloseAlarmText;
            return AlarmEnum.Close;
        }
        if (tag.BoolOpenAlarmEnable && tag.Value.ToBoolean() == true)
        {
            limit = true.ToString();
            expressions = tag.BoolOpenRestrainExpressions;
            text = tag.BoolOpenAlarmText;
            return AlarmEnum.Open;
        }
        return AlarmEnum.None;
    }

    /// <summary>
    /// 获取value报警类型
    /// </summary>
    private static AlarmEnum GetDecimalAlarmDegree(DeviceVariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;

        if (tag.HHAlarmEnable && tag.Value.ToDecimal() > tag.HHAlarmCode.ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString();
            expressions = tag.HHRestrainExpressions;
            text = tag.HHAlarmText;
            return AlarmEnum.HH;
        }

        if (tag.HAlarmEnable && tag.Value.ToDecimal() > tag.HAlarmCode.ToDecimal())
        {
            limit = tag.HAlarmCode.ToString();
            expressions = tag.HRestrainExpressions;
            text = tag.HAlarmText;
            return AlarmEnum.H;
        }

        if (tag.LAlarmEnable && tag.Value.ToDecimal() < tag.LAlarmCode.ToDecimal())
        {
            limit = tag.LAlarmCode.ToString();
            expressions = tag.LRestrainExpressions;
            text = tag.LAlarmText;
            return AlarmEnum.L;
        }
        if (tag.LLAlarmEnable && tag.Value.ToDecimal() < tag.LLAlarmCode.ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString();
            expressions = tag.LLRestrainExpressions;
            text = tag.LLAlarmText;
            return AlarmEnum.LL;
        }
        return AlarmEnum.None;
    }
    #region worker服务
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken token)
    {
        _logger?.LogInformation("报警服务启动");
        await base.StartAsync(token);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken token)
    {
        _logger?.LogInformation("报警服务停止");
        return base.StopAsync(token);
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
    /// <summary>
    /// 全部重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();

    private Task HisAlarmTask;
    private bool IsExited;
    private Task RealAlarmTask;
    private CacheDb CacheDb { get; set; }
    /// <summary>
    /// 重启
    /// </summary>
    /// <returns></returns>
    public async Task RestartAsync()
    {
        await StopAsync();
        await StartAsync();
    }

    internal async Task StartAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            foreach (var item in _globalDeviceData.CollectDevices)
            {
                item.DeviceStatusChange += DeviceStatusChange;
                item.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange += DeviceVariableChange; });
            }
            StoppingTokens.Add(new());
            await InitAsync();
            if (RealAlarmTask.Status == TaskStatus.Created)
                RealAlarmTask.Start();
            if (HisAlarmTask.Status == TaskStatus.Created)
                HisAlarmTask.Start();
            IsExited = false;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启错误");
        }
        finally
        {
            restartLock.Release();
        }
    }
    internal async Task StopAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            IsExited = true;

            foreach (var device in _globalDeviceData.CollectDevices)
            {
                device.DeviceStatusChange -= DeviceStatusChange;
                device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange -= DeviceVariableChange; });
            }

            foreach (var token in StoppingTokens)
            {
                token.Cancel();
            }
            if (RealAlarmTask != null)
            {
                try
                {
                    _logger?.LogInformation($"实时报警线程停止中");
                    await RealAlarmTask.WaitAsync(TimeSpan.FromSeconds(10));
                    _logger?.LogInformation($"实时报警线程已停止");
                }
                catch (ObjectDisposedException)
                {

                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning($"实时报警线程停止超时，已强制取消");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "等待实时报警线程停止错误");
                }

                RealAlarmTask?.SafeDispose();
            }


            if (HisAlarmTask != null)
            {
                try
                {
                    _logger?.LogInformation($"历史报警线程停止中");
                    await HisAlarmTask.WaitAsync(TimeSpan.FromSeconds(10));
                    _logger?.LogInformation($"历史报警线程已停止");
                }
                catch (ObjectDisposedException)
                {

                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning($"历史报警线程停止超时，已强制取消");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "等待历史报警线程停止错误");
                }
            }

            HisAlarmTask?.SafeDispose();
            foreach (var token in StoppingTokens)
            {
                token.SafeDispose();
            }
            StoppingTokens.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启错误");
        }
        finally
        {
            restartLock.Release();
        }
    }

    private void AlarmAnalysis(DeviceVariableRunTime item)
    {
        string limit;
        string ex;
        string text;
        AlarmEnum alarmEnum;
        if (item.DataTypeEnum.GetSystemType() == typeof(bool))
        {
            alarmEnum = GetBoolAlarmCode(item, out limit, out ex, out text);
        }
        else
        {
            alarmEnum = GetDecimalAlarmDegree(item, out limit, out ex, out text);
        }
        if (alarmEnum == AlarmEnum.None)
        {
            //需恢复报警，如果存在的话
            AlarmChange(item, null, text, EventEnum.Finish, alarmEnum);
        }
        else
        {
            //需更新报警，不管是否存在
            if (!string.IsNullOrEmpty(ex))
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
            item.AlarmTime = SysDateTimeExtensions.CurrentDateTime;
            item.EventTime = SysDateTimeExtensions.CurrentDateTime;
        }
        else if (eventEnum == EventEnum.Finish)
        {
            var oldAlarm = RealAlarmDeviceVariables.FirstOrDefault(it => it.Id == item.Id);
            item.AlarmTypeEnum = oldAlarm.AlarmTypeEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = oldAlarm.AlarmLimit;
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = SysDateTimeExtensions.CurrentDateTime;
        }
        else if (eventEnum == EventEnum.Check)
        {
            item.AlarmTypeEnum = alarmEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = SysDateTimeExtensions.CurrentDateTime;
        }

        OnAlarmChanged?.Invoke(item.Adapt<DeviceVariableRunTime>());
        if (!IsExited)
        {
            HisAlarmDeviceVariables.Enqueue(item.Adapt<HistoryAlarm>());
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


    private void DeviceStatusChange(CollectDeviceRunTime device)
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
    private async Task InitAsync()
    {
        CacheDb = new("HistoryAlarmCache");
        CancellationTokenSource stoppingToken = StoppingTokens.Last();
        RealAlarmTask = await Task.Factory.StartNew(async () =>
        {
            _logger?.LogInformation($"实时报警线程开始");
            while (!stoppingToken.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, stoppingToken.Token);
                    var list = DeviceVariables.ToListWithDequeue();
                    foreach (var item in list)
                    {
                        if (stoppingToken.Token.IsCancellationRequested)
                            break;
                        if (!item.AlarmEnable) continue;
                        AlarmAnalysis(item);
                    }
                    if (stoppingToken.Token.IsCancellationRequested)
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

        }
 , TaskCreationOptions.LongRunning);

        HisAlarmTask = await Task.Factory.StartNew(async () =>
        {
            _logger?.LogInformation($"历史报警线程开始");
            await Task.Yield();//返回线程控制，不再阻塞
            try
            {
                await Task.Delay(500, stoppingToken.Token);

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
                        await sqlSugarClient.Queryable<HistoryAlarm>().FirstAsync(stoppingToken.Token);
                        isSuccess = true;
                        StatuString = OperResult.CreateSuccessResult();
                    }
                    catch (Exception)
                    {
                        if (stoppingToken.Token.IsCancellationRequested)
                        {
                            IsExited = true;
                            return;
                        }
                        try
                        {
                            _logger.LogWarning("连接历史报警表失败，尝试初始化表");
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

                    while (!stoppingToken.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(500, stoppingToken.Token);

                            if (stoppingToken.Token.IsCancellationRequested)
                                break;

                            //缓存值
                            var cacheData = await CacheDb.GetCacheData();
                            if (cacheData.Count > 0)
                            {
                                var data = cacheData.SelectMany(a => a.CacheStr.FromJson<List<HistoryAlarm>>()).ToList();
                                try
                                {
                                    var count = await sqlSugarClient.Insertable(data).ExecuteCommandAsync(stoppingToken.Token);
                                    await CacheDb.DeleteCacheData(cacheData.Select(a => a.Id).ToArray());
                                }
                                catch (Exception ex)
                                {
                                    if (isSuccess)
                                        _logger.LogWarning(ex, "写入历史报警失败");
                                }
                            }
                            if (stoppingToken.Token.IsCancellationRequested)
                                break;


                            var list = HisAlarmDeviceVariables.ToListWithDequeue();
                            if (list.Count != 0)
                            {
                                ////Sql保存
                                list.ForEach(it =>
                                {
                                    it.Id = YitIdHelper.NextId();
                                });
                                //插入
                                try
                                {
                                    await sqlSugarClient.Insertable(list).ExecuteCommandAsync(stoppingToken.Token);
                                    isSuccess = true;
                                }
                                catch (Exception ex)
                                {
                                    if (isSuccess)
                                        _logger.LogWarning(ex, "写入历史报警失败");

                                    var cacheDatas = list.ChunkTrivialBetter(500);
                                    foreach (var a in cacheDatas)
                                    {
                                        await CacheDb.AddCacheData("", a.ToJson(), 50000);
                                    }
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
        }
 , TaskCreationOptions.LongRunning);
    }
    #endregion
}


