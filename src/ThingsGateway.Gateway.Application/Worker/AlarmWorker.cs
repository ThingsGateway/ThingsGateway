﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.Logging.Extensions;

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

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
    private ConcurrentQueue<VariableRunTime> _deviceVariables = new();
    private GlobalData GlobalData;

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
    public ConcurrentList<VariableRunTime> RealAlarmVariables { get; } = new();

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
            foreach (var item in GlobalData.CollectDevices)
            {
                item.VariableRunTimes?.ForEach(v => { v.VariableCollectChange += DeviceVariableChange; });
            }
            StoppingTokens.Add(new());
            await InitAsync(StoppingTokens.Last().Token);
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
            await restartLock.WaitAsync();

            foreach (var device in GlobalData.CollectDevices)
            {
                device.VariableRunTimes?.ForEach(v => { v.VariableCollectChange -= DeviceVariableChange; });
            }

            StoppingTokens.ParallelForEach(cancellationToken =>
         {
             _ = Task.Run(() =>
             {
                 try
                 {
                     if (!cancellationToken.IsCancellationRequested)
                     {
                         cancellationToken?.Cancel();
                     }
                 }
                 catch
                 {
                 }
             });
         });

            await Task.Delay(100);

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
                RealAlarmTask = null;
            }
            StoppingTokens.ParallelForEach(cancellationToken =>
            {
                try
                {
                    cancellationToken?.SafeDispose();
                }
                catch
                {
                }
            });
            StoppingTokens.Clear();
            RealAlarmVariables.Clear();
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
    private static AlarmTypeEnum? GetBoolAlarmCode(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag?.Value == null)
        {
            return null;
        }
        if (tag.BoolCloseAlarmEnable && !tag.Value.ToString().ToBool(true))
        {
            limit = false.ToString();
            expressions = tag.BoolCloseRestrainExpressions;
            text = tag.BoolCloseAlarmText;
            return AlarmTypeEnum.Close;
        }
        if (tag.BoolOpenAlarmEnable && tag.Value.ToString().ToBool(false))
        {
            limit = true.ToString();
            expressions = tag.BoolOpenRestrainExpressions;
            text = tag.BoolOpenAlarmText;
            return AlarmTypeEnum.Open;
        }
        return null;
    }

    /// <summary>
    /// 获取value报警类型
    /// </summary>
    private static AlarmTypeEnum? GetDecimalAlarmDegree(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag?.Value == null)
        {
            return null;
        }
        if (tag.HHAlarmEnable && tag.Value.ToString().ToDecimal() > tag.HHAlarmCode.ToString().ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString();
            expressions = tag.HHRestrainExpressions;
            text = tag.HHAlarmText;
            return AlarmTypeEnum.HH;
        }

        if (tag.HAlarmEnable && tag.Value.ToString().ToDecimal() > tag.HAlarmCode.ToString().ToDecimal())
        {
            limit = tag.HAlarmCode.ToString();
            expressions = tag.HRestrainExpressions;
            text = tag.HAlarmText;
            return AlarmTypeEnum.H;
        }

        if (tag.LAlarmEnable && tag.Value.ToString().ToDecimal() < tag.LAlarmCode.ToString().ToDecimal())
        {
            limit = tag.LAlarmCode.ToString();
            expressions = tag.LRestrainExpressions;
            text = tag.LAlarmText;
            return AlarmTypeEnum.L;
        }
        if (tag.LLAlarmEnable && tag.Value.ToString().ToDecimal() < tag.LLAlarmCode.ToString().ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString();
            expressions = tag.LLRestrainExpressions;
            text = tag.LLAlarmText;
            return AlarmTypeEnum.LL;
        }
        return null;
    }

    /// <summary>
    /// 获取自定义报警类型
    /// </summary>
    private static AlarmTypeEnum? GetCustomAlarmDegree(VariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag?.Value == null)
        {
            return null;
        }
        if (tag.CustomAlarmEnable)
        {
            var result = tag.CustomAlarmCode.GetExpressionsResult(tag.Value);
            if (result is bool boolResult)
            {
                if (boolResult)
                {
                    limit = tag.CustomAlarmCode;
                    expressions = tag.CustomAlarmText;
                    text = tag.CustomAlarmText;
                    return AlarmTypeEnum.Custom;
                }
            }
        }

        return null;
    }

    private void AlarmAnalysis(VariableRunTime item)
    {
        string limit;
        string ex;
        string text;
        AlarmTypeEnum? alarmEnum;
        if (item.DataType.GetSystemType() == typeof(bool))
        {
            alarmEnum = GetBoolAlarmCode(item, out limit, out ex, out text);
        }
        else
        {
            alarmEnum = GetDecimalAlarmDegree(item, out limit, out ex, out text);
        }
        //自定义报警
        if (alarmEnum == null)
        {
            alarmEnum = GetCustomAlarmDegree(item, out limit, out ex, out text);
        }
        if (alarmEnum == null)
        {
            //需恢复报警，如果存在的话
            AlarmChange(item, null, text, EventTypeEnum.Finish, alarmEnum);
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
                        AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum);
                    }
                }
            }
            else
            {
                AlarmChange(item, limit, text, EventTypeEnum.Alarm, alarmEnum);
            }
        }
    }

    private void AlarmChange(VariableRunTime item, object limit, string text, EventTypeEnum eventEnum, AlarmTypeEnum? alarmEnum)
    {
        if (eventEnum == EventTypeEnum.Finish)
        {
            //实时报警没有找到的话直接返回
            if (!RealAlarmVariables.Any(it => it.Id == item.Id))
            {
                return;
            }
        }
        else if (eventEnum == EventTypeEnum.Alarm)
        {
            var variable = RealAlarmVariables.FirstOrDefault(it => it.Id == item.Id);
            if (variable != null)
            {
                if (item.AlarmType == alarmEnum)
                    return;
            }
        }

        if (eventEnum == EventTypeEnum.Alarm)
        {
            item.AlarmType = alarmEnum;
            item.EventType = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.AlarmTime = DateTimeUtil.Now;
            item.EventTime = DateTimeUtil.Now;
        }
        else if (eventEnum == EventTypeEnum.Finish)
        {
            var oldAlarm = RealAlarmVariables.FirstOrDefault(it => it.Id == item.Id);
            item.AlarmType = oldAlarm.AlarmType;
            item.EventType = eventEnum;
            item.AlarmLimit = oldAlarm.AlarmLimit;
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTimeUtil.Now;
        }
        else if (eventEnum == EventTypeEnum.Check)
        {
            item.AlarmType = alarmEnum;
            item.EventType = eventEnum;
            item.AlarmLimit = limit.ToString();
            item.AlarmCode = item.Value.ToString();
            item.AlarmText = text;
            item.EventTime = DateTimeUtil.Now;
        }

        OnAlarmChanged?.Invoke(item.Adapt<AlarmVariable>());

        if (eventEnum == EventTypeEnum.Alarm)
        {
            lock (RealAlarmVariables)
            {
                RealAlarmVariables.RemoveWhere(it => it.Id == item.Id);
                RealAlarmVariables.Add(item);
            }
        }
        else
        {
            RealAlarmVariables.RemoveWhere(it => it.Id == item.Id);
        }
    }

    private void DeviceVariableChange(VariableRunTime variable)
    {
        //这里不能序列化变量，报警服务需改变同一个变量指向的属性
        _deviceVariables.Enqueue(variable);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync(CancellationToken cancellation)
    {
        RealAlarmTask = await Task.Factory.StartNew(async (a) =>
        {
            var stoppingToken = (CancellationToken)a!;
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
 , cancellation, TaskCreationOptions.LongRunning);
    }

    #endregion 核心实现

    #region worker服务

    private EasyLock _easyLock = new(false);

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _easyLock?.WaitAsync();

        GlobalData = _serviceScope.ServiceProvider.GetService<GlobalData>();
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

    #endregion worker服务
}