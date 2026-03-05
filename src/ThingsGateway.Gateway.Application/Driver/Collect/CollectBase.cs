//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;

using ThingsGateway.Common.Extension;
using ThingsGateway.Foundation.Common.Json.Extension;


using ThingsGateway.Gateway.Application.Extensions;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// </summary>
public abstract partial class CollectBase : DriverBase, IRpcDriver
{
    /// <summary>
    /// 插件配置项
    /// </summary>
    public abstract CollectPropertyBase CollectProperties { get; }

    public sealed override object DriverProperties => CollectProperties;

    public virtual string GetAddressDescription()
    {
        return string.Empty;
    }


    /// <summary>
    /// 特殊方法
    /// </summary>
    public List<DriverMethodInfo>? DriverMethodInfos { get; private set; }
    protected virtual bool IsRuntimeSourceValid(VariableRuntime a)
    {
        //筛选特殊变量地址
        //1、DeviceStatus
        return !a.RegisterAddress.Equals(nameof(DeviceRuntime.DeviceStatus), StringComparison.OrdinalIgnoreCase) &&
        !a.RegisterAddress.Equals("Script", StringComparison.OrdinalIgnoreCase) &&
        !a.RegisterAddress.Equals("ScriptRead", StringComparison.OrdinalIgnoreCase)
        ;
    }
    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        LogMessage?.LogInformation("Refresh variable");
        var currentDevice = CurrentDevice;
        IdVariableRuntimes = currentDevice.VariableRuntimes.Where(a => a.Value.Enable).ToFrozenDictionary(a => a.Value.Id, a => a.Value);

        if (!this.CurrentDevice.IsMemory)
        {
            //预热脚本，加速编译
            IdVariableRuntimes.Where(a => !string.IsNullOrWhiteSpace(a.Value.ReadExpressions))
             .Select(b => b.Value.ReadExpressions).Distinct().ToArray().ParallelForEach(script =>
             {
                 try
                 {
                     _ = ExpressionEvaluatorExtension.GetOrAddScript(script);
                 }
                 catch
                 {
                 }
             });
        }
        // 连读打包
        // 从收集的变量运行时信息中筛选需要读取的变量
        var tags = IdVariableRuntimes.Select(a => a.Value)
            .Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly
            && string.IsNullOrEmpty(it.OtherMethod)
            && !string.IsNullOrEmpty(it.RegisterAddress));


        var now = DateTime.Now;
#pragma warning disable CA1851
        try
        {

            currentDevice.VariableScriptReads = tags.Where(a => !IsRuntimeSourceValid(a)).Select(a =>
            {
                var data = new VariableScriptRead();
                data.VariableRuntime = a;
                data.IntervalTime = a.IntervalTime ?? currentDevice.IntervalTime;
                return data;
            }).ToList();
        }
        catch (Exception ex)
        {
            // 如果出现异常，记录日志并初始化 VariableSourceReads 属性为新实例
            currentDevice.VariableScriptReads = new();
            LogMessage?.LogWarning(ex, string.Format(AppResource.VariablePackError, ex.Message));
            tags.Where(a => !IsRuntimeSourceValid(a)).ForEach(a => a.SetValue(null, now, isOnline: false));
        }
        var variableReads = tags.Where(IsRuntimeSourceValid).ToList();
        try
        {
            // 将打包后的结果存储在当前设备的 VariableSourceReads 属性中
            currentDevice.VariableSourceReads = await ProtectedLoadSourceReadAsync(variableReads).ConfigureAwait(false);
#pragma warning restore CA1851
        }
        catch (Exception ex)
        {
            currentDevice.VariableSourceReads = new();
            LogMessage?.LogWarning(ex, string.Format(AppResource.VariablePackError, ex.Message));
            variableReads.ForEach(a => a.SetValue(null, now, isOnline: false));
        }
        try
        {
            // 初始化动态方法
            var variablesMethod = IdVariableRuntimes.Select(a => a.Value).Where(it => !string.IsNullOrEmpty(it.OtherMethod));

            // 处理可读的动态方法
            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly);
                List<VariableMethod> variablesMethodResult = GetMethod(tag);
                currentDevice.ReadVariableMethods = variablesMethodResult;
            }

            // 处理可写的动态方法
            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.ReadOnly);
                currentDevice.MethodVariableCount = tag.Count();
            }
        }
        catch (Exception ex)
        {
            // 如果出现异常，记录日志并初始化 ReadVariableMethods 和 VariableMethods 属性为新实例
            currentDevice.ReadVariableMethods ??= new();
            LogMessage?.LogWarning(ex, string.Format(AppResource.GetMethodError, ex.Message));
        }

        RefreshVariableTasks(cancellationToken);

        // 根据标签获取方法信息的局部函数
        List<VariableMethod> GetMethod(IEnumerable<VariableRuntime> tag)
        {
            var variablesMethodResult = new List<VariableMethod>();
            foreach (var item in tag)
            {
                // 根据标签查找对应的方法信息
                var method = DriverMethodInfos.FirstOrDefault(it => it.Name == item.OtherMethod);
                if (method != null)
                {
                    // 构建 VariableMethod 对象
                    var methodResult = new VariableMethod(new TouchSocket.Core.Method(method.MethodInfo), item, string.IsNullOrWhiteSpace(item.IntervalTime) ? item.DeviceRuntime.IntervalTime : item.IntervalTime);
                    variablesMethodResult.Add(methodResult);
                }
                else
                {
                    // 如果找不到对应方法，抛出异常
                    throw new(string.Format(AppResource.MethodNotNull, item.Name, item.OtherMethod));
                }
            }
            return variablesMethodResult;
        }
    }
    private volatile bool _addVariableTasks;
    protected void RefreshVariableTasks(CancellationToken cancellationToken)
    {
        if (_addVariableTasks)
        {
            if (VariableTasks != null)
            {
                foreach (var item in VariableTasks)
                {
                    item.Stop();
                    TaskSchedulerLoop?.Remove(item);
                }

                VariableTasks = AddVariableTask(cancellationToken);

                foreach (var item in VariableTasks)
                {
                    TaskSchedulerLoop?.Add(item);
                    item.Start();
                }
            }
        }
    }

    internal override void ProtectedInitDevice(DeviceRuntime device)
    {
        // 调用基类的初始化方法
        base.ProtectedInitDevice(device);

        // 从插件服务中获取当前设备关联的驱动方法信息列表
        DriverMethodInfos = GlobalData.PluginService.GetDriverMethodInfos(device.PluginName, this);

        ReadWriteLock = new(CollectProperties.DutyCycle, CollectProperties.WritePriority);
    }


    protected virtual bool VariableSourceReadsEnable => true;

    protected List<IScheduledTask> VariableTasks = new List<IScheduledTask>();
    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        var tasks = new List<IScheduledTask>();

        LogMessage?.LogInformation("Get collect tasks");

        var setDeviceStatusTask = new ScheduledSyncTask(10000, SetDeviceStatus, null, LogMessage, cancellationToken);
        tasks.Add(setDeviceStatusTask);

        var testOnline = new ScheduledAsyncTask(30000, TestOnline, null, LogMessage, cancellationToken);
        tasks.Add(testOnline);

        VariableTasks = AddVariableTask(cancellationToken);
        _addVariableTasks = true;
        tasks.AddRange(VariableTasks);
        return tasks;
    }

    protected List<IScheduledTask> AddVariableTask(CancellationToken cancellationToken)
    {
        List<IScheduledTask> variableTasks = new();
        if (VariableSourceReadsEnable)
        {
            for (int i = 0; i < CurrentDevice.VariableSourceReads.Count; i++)
            {
                var variableSourceRead = CurrentDevice.VariableSourceReads[i];

                var executeTask = ScheduledTaskHelper.GetTask(variableSourceRead.IntervalTime, ReadVariableSource, variableSourceRead, LogMessage, cancellationToken);
                variableTasks.Add(executeTask);
            }
        }

        for (int i = 0; i < CurrentDevice.ReadVariableMethods.Count; i++)
        {
            var variableMethod = CurrentDevice.ReadVariableMethods[i];

            var executeTask = ScheduledTaskHelper.GetTask(variableMethod.IntervalTime, ReadVariableMed, variableMethod, LogMessage, cancellationToken);
            variableTasks.Add(executeTask);
        }

        for (int i = 0; i < CurrentDevice.VariableScriptReads.Count; i++)
        {
            var variableScriptRead = CurrentDevice.VariableScriptReads[i];

            var executeTask = ScheduledTaskHelper.GetTask(variableScriptRead.IntervalTime, ScriptVariableRun, variableScriptRead, LogMessage, cancellationToken);
            variableTasks.Add(executeTask);
        }

        return variableTasks;
    }

    protected virtual void SetDeviceStatus(object? state, CancellationToken cancellationToken)
    {
        CurrentDevice.SetDeviceStatus(TimerX.Now);
        if (IsConnected())
        {
            if (CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine)
            {
                if (IdVariableRuntimes.Any(a => a.Value.IsOnline))
                    CurrentDevice.SetDeviceStatus(TimerX.Now, false);
            }
            else
            {
                if (IdVariableRuntimes.All(a => !a.Value.IsOnline))
                    CurrentDevice.SetDeviceStatus(TimerX.Now, true);
            }
        }
        else if (IsStarted)
        {
            CurrentDevice.SetDeviceStatus(TimerX.Now, true);
        }
    }


    #region 执行方法
    ValueTask ReadVariableMed(object? state, CancellationToken cancellationToken)
    {
        if (state is not VariableMethod readVariableMethods)
            return ValueTask.CompletedTask;
        if (Pause)
            return ValueTask.CompletedTask;
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.CompletedTask;
        return ReadVariableMed(this, readVariableMethods, cancellationToken);

        static async PooledValueTask ReadVariableMed(CollectBase @this, VariableMethod readVariableMethods, CancellationToken cancellationToken)
        {

            var readErrorCount = 0;

            //if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
            //    LogMessage?.LogTrace(string.Format("{0} - Executing method [{1}]", DeviceName, readVariableMethods.MethodInfo.Name));
            var readResult = await @this.InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken).ConfigureAwait(false);

            // 方法调用失败时重试一定次数
            while (!readResult.IsSuccess && readErrorCount < @this.CollectProperties.RetryCount)
            {
                if (@this.Pause)
                    return;
                if (cancellationToken.IsCancellationRequested)
                    return;

                readErrorCount++;
                if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                    @this.LogMessage?.LogTrace(string.Format("{0} - Execute method [{1}] - failed - {2}", @this.DeviceName, readVariableMethods.MethodInfo.Name, readResult.ErrorMessage));

                //if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                //    LogMessage?.LogTrace(string.Format("{0} - Executing method [{1}]", DeviceName, readVariableMethods.MethodInfo.Name));
                readResult = await @this.InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (readResult.IsSuccess)
            {
                // 方法调用成功时记录日志并增加成功计数器
                if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                    @this.LogMessage?.LogTrace(string.Format("{0} - Execute method [{1}] - Succeeded {2}", @this.DeviceName, readVariableMethods.MethodInfo.Name, readResult.Content?.ToSystemTextJsonString()));
                @this.CurrentDevice.PerSecondCounter.Increment();
                @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null);
            }
            else
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // 方法调用失败时记录日志并增加失败计数器，更新错误信息
                if (readVariableMethods.LastErrorMessage != readResult.ErrorMessage)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        @this.LogMessage?.LogWarning(readResult.Exception, string.Format(AppResource.MethodFail, @this.DeviceName, readVariableMethods.MethodInfo.Name, readResult.ErrorMessage));
                }
                else
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                            @this.LogMessage?.LogTrace(string.Format("{0} - Execute method [{1}] - failed - {2}", @this.DeviceName, readVariableMethods.MethodInfo.Name, readResult.ErrorMessage));
                    }
                }

                readVariableMethods.LastErrorMessage = readResult.ErrorMessage;
                @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, readResult.ErrorMessage);
            }

            return;
        }
    }

    #endregion
    private readonly LinkedCancellationTokenSourceCache _linkedCtsCache = new();

    #region 执行默认读取
    ValueTask ReadVariableSource(object? state, CancellationToken cancellationToken)
    {
        return ReadVariableSource(this, state, cancellationToken);
        static async PooledValueTask ReadVariableSource(CollectBase @this, object? state, CancellationToken cancellationToken)
        {
            if (state is not VariableSourceRead variableSourceRead) return;

            if (@this.Pause) return;
            if (cancellationToken.IsCancellationRequested) return;
            CancellationToken readToken = default;
            var readerLockTask = @this.ReadWriteLock.ReaderLockAsync(cancellationToken);
            if (!readerLockTask.IsCompletedSuccessfully)
            {
                readToken = await readerLockTask.ConfigureAwait(false);
            }
            else
            {
                readToken = readerLockTask.Result;
            }

            if (readToken.IsCancellationRequested)
            {
                await @this.ReadVariableSource(state, cancellationToken).ConfigureAwait(false);
                return;
            }

            var allTokenSource = @this._linkedCtsCache.GetLinkedTokenSource(cancellationToken, readToken);
            var allToken = allTokenSource.Token;

            //if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
            //    LogMessage?.LogTrace(string.Format("{0} - Collecting [{1} - {2}]", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length));

            OperResult<ReadOnlyMemory<byte>> readResult = default;
            var readTask = @this.ReadSourceAsync(variableSourceRead, allToken);
            if (!readTask.IsCompletedSuccessfully)
            {
                readResult = await readTask.ConfigureAwait(false);
            }
            else
            {
                readResult = readTask.Result;
            }

            var readErrorCount = 0;

            // 读取失败时重试一定次数
            while (!readResult.IsSuccess && readErrorCount < @this.CollectProperties.RetryCount)
            {
                if (@this.Pause)
                    return;
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (readToken.IsCancellationRequested)
                {
                    await @this.ReadVariableSource(state, cancellationToken).ConfigureAwait(false);
                    return;
                }

                readErrorCount++;
                if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                    @this.LogMessage?.LogTrace(string.Format("{0} - Failed to collect data [{1} - {2}] - {3}", @this.DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult.ErrorMessage));

                //if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                //    LogMessage?.LogTrace(string.Format("{0} - Collecting [{1} - {2}]", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length));
                var readTask1 = @this.ReadSourceAsync(variableSourceRead, allToken);
                if (!readTask1.IsCompletedSuccessfully)
                {
                    readResult = await readTask1.ConfigureAwait(false);
                }
                else
                {
                    readResult = readTask1.Result;
                }

            }

            if (readResult.IsSuccess)
            {
                // 读取成功时记录日志并增加成功计数器
                if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                    @this.LogMessage?.LogTrace(string.Format("{0} - Collected [{1} - {2}] data successfully {3}", @this.DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult.Content.Span.ToHexString(' ')));
                @this.CurrentDevice.PerSecondCounter.Increment();
                @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null);
            }
            else
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (readToken.IsCancellationRequested)
                {
                    await @this.ReadVariableSource(state, cancellationToken).ConfigureAwait(false);
                    return;
                }

                // 读取失败时记录日志并增加失败计数器，更新错误信息并清除变量状态
                if (variableSourceRead.LastErrorMessage != readResult.ErrorMessage)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        @this.LogMessage?.LogWarning(readResult.Exception, string.Format(AppResource.CollectFail, @this.DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult.ErrorMessage));
                }
                else
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                            @this.LogMessage?.LogTrace(string.Format("{0} - Failed to collect data [{1} - {2}] - {3}", @this.DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult.ErrorMessage));
                    }
                }

                variableSourceRead.LastErrorMessage = readResult.ErrorMessage;
                @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, readResult.ErrorMessage);
                var time = DateTime.Now;
                foreach (var item in variableSourceRead.Variables)
                {
                    item.SetValue(null, time, isOnline: false);
                }
            }
        }
    }

    #endregion


    protected virtual ValueTask TestOnline(object? state, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    protected void ScriptVariableRun(object? state, CancellationToken cancellationToken)
    {
        DateTime dateTime = TimerX.Now;
        if (state is not VariableScriptRead variableScriptRead) return;
        //特殊地址变量

        if (cancellationToken.IsCancellationRequested)
            return;
        {
            var variableRuntime = variableScriptRead.VariableRuntime;
            if (variableRuntime.RegisterAddress.Equals(nameof(DeviceRuntime.DeviceStatus), StringComparison.OrdinalIgnoreCase))
            {
                variableRuntime.SetValue(variableRuntime.DeviceRuntime.DeviceStatus, dateTime);
            }
            else if (variableRuntime.RegisterAddress.Equals("ScriptRead", StringComparison.OrdinalIgnoreCase))
            {
                variableRuntime.SetValue(variableRuntime.Value, dateTime);
            }
        }
    }


    /// <summary>
    /// 连读打包，返回实际通讯包信息<see cref="VariableSourceRead"/>
    /// <br></br>每个驱动打包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    protected abstract Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables);

    protected AsyncReadWriteLock ReadWriteLock;

    /// <summary>
    /// 采集驱动读取，读取成功后直接赋值变量
    /// </summary>
    protected virtual ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(new NotImplementedException()));
    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果
    /// </summary>
    /// <returns></returns>
    protected virtual ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    protected Task Check(Dictionary<VariableRuntime, JsonNode> writeInfoLists, NonBlockingDictionary<string, OperResult> operResults, CancellationToken cancellationToken)
    {
        return Check(this, writeInfoLists, operResults, cancellationToken);

        static async PooledTask Check(CollectBase @this, Dictionary<VariableRuntime, JsonNode> writeInfoLists, NonBlockingDictionary<string, OperResult> operResults, CancellationToken cancellationToken)
        {
            if (@this.VariableSourceReadsEnable)
            {
                // 如果成功，每个变量都读取一次最新值，再次比较写入值
                var successfulWriteNames = operResults.Where(a => a.Value.IsSuccess).Select(a => a.Key).ToHashSet();

                var groups = writeInfoLists.Select(a => a.Key).Where(a => a.RpcWriteCheck && a.ProtectType != ProtectTypeEnum.WriteOnly && successfulWriteNames.Contains(a.Name) && a.VariableSource != null).GroupBy(a => a.VariableSource as VariableSourceRead).Where(a => a.Key != null).ToArray();

                await groups.ParallelForEachAsync(async (varRead, token) =>
                {
                    var result = await @this.ReadSourceAsync(varRead.Key, token).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        foreach (var item in varRead)
                        {
                            var cValue = writeInfoLists[item].Deserialize(item.RawValue?.GetType());
                            if (!item.RawValue.Equals(cValue))
                            {
                                // 如果写入值与读取值不同，则更新操作结果为失败
                                if (cValue is IComparable)
                                {
                                    operResults[item.Name] = new OperResult($"The write value is inconsistent with the read value,  Write value: {writeInfoLists[item].Deserialize(item.Value?.GetType())}, read value: {item.Value}");
                                }
                                else
                                {
                                    if (cValue != null)
                                    {
                                        if (item.RawValue.ToSystemTextJsonString(false) != cValue.ToSystemTextJsonString(false))
                                            operResults[item.Name] = new OperResult($"The write value is inconsistent with the read value,  Write value: {writeInfoLists[item].Deserialize(item.Value?.GetType())}, read value: {item.Value}");
                                    }
                                    else
                                        operResults[item.Name] = new OperResult($"The write value is inconsistent with the read value,  Write value: {writeInfoLists[item].Deserialize(item.Value?.GetType())}, read value: {item.Value}");
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in varRead)
                        {
                            // 如果写入值与读取值不同，则更新操作结果为失败
                            operResults[item.Name] = new OperResult($"Reading and rechecking resulted in an error: {result.ErrorMessage}", result.Exception);
                        }
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    #region 写入方法

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public ValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeMethodAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        return InvokeMethodAsync(this, writeInfoLists, cancellationToken);

        static async PooledValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeMethodAsync(CollectBase @this, Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
        {
            // 初始化结果字典
            Dictionary<string, OperResult<object>> results = new Dictionary<string, OperResult<object>>();

            // 遍历写入信息列表
            foreach (var (deviceVariable, jToken) in writeInfoLists)
            {
                // 检查是否有写入表达式
                if (!string.IsNullOrEmpty(deviceVariable.WriteExpressions))
                {
                    // 提取原始数据
                    object rawdata = jToken.GetObjectFromJsonNode();
                    try
                    {
                        // 根据写入表达式转换数据
                        object data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata, @this.LogMessage);
                        // 将转换后的数据重新赋值给写入信息列表
                        writeInfoLists[deviceVariable] = JsonUtil.GetJsonNodeFromObj(data);
                    }
                    catch (Exception ex)
                    {
                        // 如果转换失败，则记录错误信息
                        results.Add(deviceVariable.Name, new OperResult<object>(string.Format(AppResource.WriteExpressionsError, deviceVariable.Name, deviceVariable.WriteExpressions, ex.Message), ex));
                    }
                }
            }

            NonBlockingDictionary<string, OperResult<object>> operResults = new();


            using var writeLock = await @this.ReadWriteLock.WriterLockAsync().ConfigureAwait(false);
            var list = writeInfoLists
            .Where(a => !results.Any(b => b.Key == a.Key.Name))
            .ToArray();
            // 使用并发方式遍历写入信息列表，并进行异步写入操作
            await list.ParallelForEachAsync(async (writeInfo, cancellationToken) =>
            {
                try
                {
                    // 调用协议的写入方法，将写入信息中的数据写入到对应的寄存器地址，并获取操作结果
                    var result = await @this.InvokeMethodAsync(writeInfo.Key.VariableMethod, writeInfo.Value?.ToString(), false, cancellationToken).ConfigureAwait(false);

                    // 将操作结果添加到结果字典中，使用变量名称作为键
                    operResults.TryAdd(writeInfo.Key.Name, result);
                }
                catch (Exception ex)
                {
                    operResults.TryAdd(writeInfo.Key.Name, new(ex));
                }
            }, @this.CollectProperties.MaxConcurrentCount, cancellationToken).ConfigureAwait(false);

            // 将转换失败的变量和写入成功的变量的操作结果合并到结果字典中
            return new Dictionary<string, IDictionary<string, OperResult<object>>>()
        {
            {
             @this.DeviceName ,
             results.Concat(operResults).ToDictionary()
            }
        };
        }
    }

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public ValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InVokeWriteAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        return InVokeWriteAsync(this, writeInfoLists, cancellationToken);

        static async PooledValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InVokeWriteAsync(CollectBase @this, Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
        {
            // 初始化结果字典
            Dictionary<string, OperResult> results = new Dictionary<string, OperResult>();

            // 遍历写入信息列表
            foreach (var (deviceVariable, jToken) in writeInfoLists)
            {
                // 检查是否有写入表达式
                if (!string.IsNullOrEmpty(deviceVariable.WriteExpressions))
                {
                    // 提取原始数据
                    object rawdata = jToken.GetObjectFromJsonNode();
                    try
                    {
                        // 根据写入表达式转换数据
                        object data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata, @this.LogMessage);
                        // 将转换后的数据重新赋值给写入信息列表
                        writeInfoLists[deviceVariable] = JsonUtil.GetJsonNodeFromObj(data);
                    }
                    catch (Exception ex)
                    {
                        // 如果转换失败，则记录错误信息
                        results.Add(deviceVariable.Name, new OperResult(string.Format(AppResource.WriteExpressionsError, deviceVariable.Name, deviceVariable.WriteExpressions, ex.Message), ex));
                    }
                }
            }

            var writePList = writeInfoLists.Where(a => !@this.CurrentDevice.VariableScriptReads.Select(a => a.VariableRuntime).Any(b => a.Key.Name == b.Name));
            var writeSList = writeInfoLists.Where(a => @this.CurrentDevice.VariableScriptReads.Select(a => a.VariableRuntime).Any(b => a.Key.Name == b.Name));

            var now = DateTime.Now;
            foreach (var item in writeSList)
            {
                results.TryAdd(item.Key.Name, item.Key.SetValue(item.Value, now));
            }

            // 过滤掉转换失败的变量，只保留写入成功的变量进行写入操作
            var results1 = await @this.WriteValuesAsync(writePList
                .Where(a => !results.Any(b => b.Key == a.Key.Name))
                .ToDictionary(item => item.Key, item => item.Value),
                cancellationToken).ConfigureAwait(false);

            // 将转换失败的变量和写入成功的变量的操作结果合并到结果字典中

            return new Dictionary<string, IDictionary<string, OperResult<object>>>()
        {
            {
               @this. DeviceName ,
                results.Concat(results1).ToDictionary(a=>a.Key,a=>new OperResult<object>(a.Value))
            }
        };
        }
    }

    /// <summary>
    /// 异步调用方法
    /// </summary>
    /// <param name="variableMethod">要调用的方法</param>
    /// <param name="value">传递给方法的参数值（可选）</param>
    /// <param name="isRead">指示是否为读取操作</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>操作结果，包含执行方法的结果</returns>
    protected virtual ValueTask<OperResult<object>> InvokeMethodAsync(VariableMethod variableMethod, string? value = null, bool isRead = true, CancellationToken cancellationToken = default)
    {
        return InvokeMethodAsync(this, variableMethod, value, isRead, cancellationToken);

        static async PooledValueTask<OperResult<object>> InvokeMethodAsync(CollectBase @this, VariableMethod variableMethod, string? value, bool isRead, CancellationToken cancellationToken)
        {
            try
            {
                // 初始化操作结果
                OperResult<object> result = new OperResult<object>();

                // 获取要执行的方法
                var method = variableMethod.MethodInfo;

                // 如果方法未找到，则返回错误结果
                if (method == null)
                {
                    result.OperCode = 999;
                    result.ErrorMessage = string.Format(AppResource.MethodNotNull, variableMethod.Variable.Name, variableMethod.Variable.OtherMethod);
                    return result;
                }
                else
                {
                    // 调用方法并获取结果
                    var data = await variableMethod.InvokeMethodAsync(@this, value, cancellationToken).ConfigureAwait(false);

                    result = data.GetOperResult();

                    // 如果方法有返回值，并且是读取操作
                    if (method.HasReturn && isRead)
                    {
                        var time = DateTime.Now;
                        if (result.IsSuccess == true)
                        {
                            // 将结果序列化并设置到变量中
                            var variableResult = variableMethod.Variable.SetValue(result.Content, time);
                            if (!variableResult.IsSuccess)
                                variableMethod.LastErrorMessage = result.ErrorMessage;
                        }
                        else
                        {
                            // 如果读取操作失败，则将变量标记为离线
                            var variableResult = variableMethod.Variable.SetValue(null, time, isOnline: false);
                            if (!variableResult.IsSuccess)
                                variableMethod.LastErrorMessage = result.ErrorMessage;
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并返回错误结果
                return new OperResult<object>(ex);
            }
        }
    }


    #endregion 写入方法

    protected override async Task DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing).ConfigureAwait(false);
        _linkedCtsCache?.TryDispose();
        if (ReadWriteLock != null)
            ReadWriteLock.Dispose();
    }

}
