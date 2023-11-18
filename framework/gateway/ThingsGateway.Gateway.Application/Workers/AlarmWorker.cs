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

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Gateway.Core.Extensions;

namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 变量报警事件委托
/// </summary>
public delegate void VariableAlarmEventHandler(AlarmVariable alarmVariable);
/// <summary>
/// 设备采集报警后台服务
/// </summary>
public class AlarmWorker : BackgroundService
{
    protected IServiceScope _serviceScope;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger _logger;
    private ConcurrentQueue<DeviceVariableRunTime> _deviceVariables = new();
    private GlobalDeviceData _globalDeviceData;
    /// <inheritdoc cref="AlarmWorker"/>
    public AlarmWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("实时报警服务");
        _appLifetime = appLifetime;
    }
    /// <summary>
    /// 报警变化事件
    /// </summary>
    public event VariableAlarmEventHandler OnAlarmChanged;

    /// <summary>
    /// 实时报警列表
    /// </summary>
    public ConcurrentList<DeviceVariableRunTime> RealAlarmDeviceVariables { get; } = new();

    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; private set; } = new OperResult("初始化");


    #region 核心实现

    /// <summary>
    /// 重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();

    private Task RealAlarmTask;

    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    private ConcurrentList<CancellationTokenSource> StoppingTokens = new();

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
                item.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange += DeviceVariableChange; });
            }
            StoppingTokens.Add(new());
            await InitAsync();
            if (RealAlarmTask.Status == TaskStatus.Created)
                RealAlarmTask.Start();
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

            foreach (var device in _globalDeviceData.CollectDevices)
            {
                device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange -= DeviceVariableChange; });
            }

            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.Cancel();
                cancellationToken.SafeDispose();
            }
            StoppingTokens.Clear();
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

            RealAlarmDeviceVariables.Clear();
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

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    private static AlarmEnum GetBoolAlarmCode(DeviceVariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag?.Value == null)
        {
            return AlarmEnum.None;
        }
        if (tag.BoolCloseAlarmEnable && !tag.Value.ToString().ToBool(true))
        {
            limit = false.ToString();
            expressions = tag.BoolCloseRestrainExpressions;
            text = tag.BoolCloseAlarmText;
            return AlarmEnum.Close;
        }
        if (tag.BoolOpenAlarmEnable && tag.Value.ToString().ToBool(false))
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
        if (tag?.Value == null)
        {
            return AlarmEnum.None;
        }
        if (tag.HHAlarmEnable && tag.Value.ToString().ToDecimal() > tag.HHAlarmCode.ToString().ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString();
            expressions = tag.HHRestrainExpressions;
            text = tag.HHAlarmText;
            return AlarmEnum.HH;
        }

        if (tag.HAlarmEnable && tag.Value.ToString().ToDecimal() > tag.HAlarmCode.ToString().ToDecimal())
        {
            limit = tag.HAlarmCode.ToString();
            expressions = tag.HRestrainExpressions;
            text = tag.HAlarmText;
            return AlarmEnum.H;
        }

        if (tag.LAlarmEnable && tag.Value.ToString().ToDecimal() < tag.LAlarmCode.ToString().ToDecimal())
        {
            limit = tag.LAlarmCode.ToString();
            expressions = tag.LRestrainExpressions;
            text = tag.LAlarmText;
            return AlarmEnum.L;
        }
        if (tag.LLAlarmEnable && tag.Value.ToString().ToDecimal() < tag.LLAlarmCode.ToString().ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString();
            expressions = tag.LLRestrainExpressions;
            text = tag.LLAlarmText;
            return AlarmEnum.LL;
        }
        return AlarmEnum.None;
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
            item.AlarmTime = DateTimeExtensions.CurrentDateTime;
            item.EventTime = DateTimeExtensions.CurrentDateTime;
        }
        else if (eventEnum == EventEnum.Finish)
        {
            var oldAlarm = RealAlarmDeviceVariables.FirstOrDefault(it => it.Id == item.Id);
            item.AlarmTypeEnum = oldAlarm.AlarmTypeEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = oldAlarm.AlarmLimit;
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTimeExtensions.CurrentDateTime;
        }
        else if (eventEnum == EventEnum.Check)
        {
            item.AlarmTypeEnum = alarmEnum;
            item.EventTypeEnum = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTimeExtensions.CurrentDateTime;
        }

        OnAlarmChanged?.Invoke(item.Adapt<AlarmVariable>());

        if (eventEnum == EventEnum.Alarm)
        {
            lock (RealAlarmDeviceVariables)
            {
                RealAlarmDeviceVariables.RemoveWhere(it => it.Id == item.Id);
                RealAlarmDeviceVariables.Add(item);
            }
        }
        else
        {
            RealAlarmDeviceVariables.RemoveWhere(it => it.Id == item.Id);
        }
    }


    private void DeviceVariableChange(DeviceVariableRunTime variable)
    {
        //这里不能序列化变量，报警服务需改变同一个变量指向的属性
        _deviceVariables.Enqueue(variable);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync()
    {
        var stoppingToken = StoppingTokens.Last().Token;
        RealAlarmTask = await Task.Factory.StartNew(async () =>
        {
            _logger?.LogInformation($"实时报警线程开始");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, stoppingToken);
                    var list = _deviceVariables.ToListWithDequeue();
                    foreach (var item in list)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;
                        if (!item.AlarmEnable) continue;
                        if (!item.IsOnline) continue;
                        AlarmAnalysis(item);
                    }
                    if (stoppingToken.IsCancellationRequested)
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

    }
    #endregion



    #region worker服务
    private EasyLock _easyLock = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("报警服务启动");
        await _easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
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
        await _easyLock?.WaitAsync();

        _globalDeviceData = _serviceScope.ServiceProvider.GetService<GlobalDeviceData>();

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


}


