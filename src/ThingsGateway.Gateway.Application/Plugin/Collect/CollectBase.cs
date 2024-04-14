
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Mapster;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using NewLife.Threading;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.Gateway.Application.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// </summary>
public abstract class CollectBase : DriverBase
{
    public new CollectDeviceRunTime CurrentDevice => (CollectDeviceRunTime)base.CurrentDevice;

    /// <summary>
    /// 特殊方法
    /// </summary>
    public List<DriverMethodInfo>? DeviceMethods { get; private set; }

    public override object DriverProperties => CollectProperties;

    /// <summary>
    /// 插件配置项
    /// </summary>
    public abstract CollectPropertyBase CollectProperties { get; }

    private IStringLocalizer Localizer { get; set; }

    internal override void Init(DeviceRunTime device)
    {
        Localizer = App.CreateLocalizerByType(typeof(CollectBase))!;
        // 调用基类的初始化方法
        base.Init(device);

        // 从插件服务中获取当前设备关联的驱动方法信息列表，并转换为列表形式
        var data = PluginService.GetDriverMethodInfos(device.PluginName, this);

        // 将获取到的驱动方法信息列表赋值给 DeviceMethods
        DeviceMethods = data;

        // 使用全局锁确保多线程安全地更新全局数据
        //lock (GlobalData.CollectDevices)
        {
            // 从全局设备字典中移除具有相同 Id 的设备
            GlobalData.CollectDevices.RemoveWhere(it => it.Value.Id == device.Id);

            // 尝试向全局设备字典中添加当前设备，使用设备名称作为键
            GlobalData.CollectDevices.TryAdd(CurrentDevice.Name, CurrentDevice);

            // 从全局变量字典中移除与当前设备关联的变量
            GlobalData.Variables.RemoveWhere(it => it.Value.DeviceId == device.Id);

            // 遍历当前设备的变量运行时集合，将其中的变量添加到全局变量字典中
            foreach (var item in CurrentDevice.VariableRunTimes)
            {
                GlobalData.Variables.TryAdd(item.Key, item.Value);
            }
        }
    }

    /// <summary>
    /// 采集驱动读取，读取成功后直接赋值变量，失败不做处理，注意非通用设备需重写
    /// </summary>
    protected virtual async Task<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        // 如果是单线程模式，并且有其他线程正在等待写入锁
        if (IsSingleThread && WriteLock.IsWaitting)
        {
            // 等待写入锁释放
            await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            // 立即释放写入锁，允许其他线程继续执行写入操作
            WriteLock.Release();
        }
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException();
        // 从协议读取数据
        OperResult<byte[]> read = await Protocol.ReadAsync(variableSourceRead.RegisterAddress, variableSourceRead.Length, cancellationToken);

        // 增加变量源的读取次数
        Interlocked.Increment(ref variableSourceRead.ReadCount);

        // 如果读取成功且有有效内容，则解析结构化内容
        if (read?.IsSuccess == true)
        {
            variableSourceRead.VariableRunTimes.PraseStructContent(Protocol, read.Content);
        }

        // 返回读取结果
        return read;
    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果，注意非通用设备需重写
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            // 如果是单线程模式，则等待写入锁
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);

            // 检查协议是否为空，如果为空则抛出异常
            if (Protocol == null)
                throw new NotSupportedException();

            // 创建用于存储操作结果的并发字典
            ConcurrentDictionary<string, OperResult> operResults = new();

            // 使用并发方式遍历写入信息列表，并进行异步写入操作
            await writeInfoLists.ParallelForEachAsync(async (writeInfo, cancellationToken) =>
            {
                // 调用协议的写入方法，将写入信息中的数据写入到对应的寄存器地址，并获取操作结果
                var result = await Protocol.WriteAsync(writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, cancellationToken);

                // 将操作结果添加到结果字典中，使用变量名称作为键
                operResults.TryAdd(writeInfo.Key.Name, result);
            }, CollectProperties.ConcurrentCount, cancellationToken);

            // 返回包含操作结果的字典
            return new Dictionary<string, OperResult>(operResults);
        }
        finally
        {
            // 如果是单线程模式，则释放写入锁
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    /// <summary>
    /// 注意非通用设备需重写
    /// </summary>
    /// <returns></returns>
    protected virtual string GetAddressDescription()
    {
        return Protocol?.GetAddressDescription();
    }

    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    public override void LoadSourceRead(IEnumerable<VariableRunTime> collectVariableRunTimes)
    {
        var currentDevice = CurrentDevice;
        try
        {
            // 连读打包
            // 从收集的变量运行时信息中筛选需要读取的变量
            var tags = collectVariableRunTimes
                .Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly
                && string.IsNullOrEmpty(it.OtherMethod)
                && !string.IsNullOrEmpty(it.RegisterAddress)).ToList();
            // 将打包后的结果存储在当前设备的 VariableSourceReads 属性中
            currentDevice.VariableSourceReads = this.ProtectedLoadSourceRead(tags);
        }
        catch (Exception ex)
        {
            // 如果出现异常，记录日志并初始化 VariableSourceReads 属性为新实例
            currentDevice.VariableSourceReads = new();
            LogMessage.LogWarning(ex, Localizer["VariablePackError", ex.Message]);
        }
        try
        {
            // 初始化动态方法
            var variablesMethod = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));

            // 处理可读的动态方法
            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly);
                List<VariableMethod> variablesMethodResult = GetMethod(tag);
                currentDevice.ReadVariableMethods = variablesMethodResult;
            }

            // 处理可写的动态方法
            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.ReadOnly);
                List<VariableMethod> variablesMethodResult = GetMethod(tag);
                currentDevice.VariableMethods = variablesMethodResult;
            }
        }
        catch (Exception ex)
        {
            // 如果出现异常，记录日志并初始化 ReadVariableMethods 和 VariableMethods 属性为新实例
            currentDevice.ReadVariableMethods ??= new();
            currentDevice.VariableMethods ??= new();
            LogMessage.LogWarning(ex, Localizer["GetMethodError", ex.Message]);
        }

        // 根据标签获取方法信息的局部函数
        List<VariableMethod> GetMethod(IEnumerable<VariableRunTime> tag)
        {
            var variablesMethodResult = new List<VariableMethod>();
            foreach (var item in tag)
            {
                // 根据标签查找对应的方法信息
                var method = DeviceMethods.FirstOrDefault(it => it.Name == item.OtherMethod);
                if (method != null)
                {
                    // 构建 VariableMethod 对象
                    var methodResult = new VariableMethod(new Method(method.MethodInfo), item, item.IntervalTime ?? item.CollectDeviceRunTime.IntervalTime);
                    variablesMethodResult.Add(methodResult);
                }
                else
                {
                    // 如果找不到对应方法，抛出异常
                    throw new(Localizer["MethodNotNull", item.Name, item.OtherMethod]);
                }
            }
            return variablesMethodResult;
        }
    }

    private class ReadResultCount
    {
        // 初始化成功和失败的计数器
        public int deviceMethodsVariableSuccessNum = 0;

        public int deviceMethodsVariableFailedNum = 0;
        public int deviceSourceVariableSuccessNum = 0;
        public int deviceSourceVariableFailedNum = 0;
    }

    /// <summary>
    /// 执行读取等方法，如果插件不支持读取，而是自更新值的话，需重写此方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        ReadResultCount readResultCount = new();
        if (cancellationToken.IsCancellationRequested)
            return;
        if (CollectProperties.ConcurrentCount > 1 && !IsSingleThread)
        {
            // 并行处理每个变量读取
            await CurrentDevice.VariableSourceReads.ParallelForEachAsync(async (variableSourceRead, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                await ReadVariableSource(readResultCount, variableSourceRead, cancellationToken, false);
            }
            , CollectProperties.ConcurrentCount, cancellationToken);
        }
        else
        {
            foreach (var variableSourceRead in CurrentDevice.VariableSourceReads)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                await ReadVariableSource(readResultCount, variableSourceRead, cancellationToken);
            }
        }

        if (CollectProperties.ConcurrentCount > 1 && !IsSingleThread)
        {
            // 并行处理每个方法调用
            await CurrentDevice.ReadVariableMethods.ParallelForEachAsync(async (readVariableMethods, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                await ReadVariableMed(readResultCount, readVariableMethods, cancellationToken, false);
            }
        , CollectProperties.ConcurrentCount, cancellationToken);
        }
        else
        {
            foreach (var readVariableMethods in CurrentDevice.ReadVariableMethods)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                await ReadVariableMed(readResultCount, readVariableMethods, cancellationToken);
            }
        }
        // 如果所有方法和变量读取都成功，则清零错误计数器
        if (readResultCount.deviceMethodsVariableFailedNum == 0 && readResultCount.deviceSourceVariableFailedNum == 0 && (readResultCount.deviceMethodsVariableSuccessNum != 0 || readResultCount.deviceSourceVariableSuccessNum != 0))
        {
            //只有成功读取一次，失败次数都会清零
            CurrentDevice.SetDeviceStatus(TimerX.Now, 0);
        }

        async Task ReadVariableMed(ReadResultCount readResultCount, VariableMethod readVariableMethods, CancellationToken cancellationToken, bool delay = true)
        {
            if (KeepRun != true)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;

            // 如果请求更新时间已到，则执行方法调用
            if (readVariableMethods.CheckIfRequestAndUpdateTime(DateTime.Now))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var readErrorCount = 0;
                var readResult = await InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken);

                // 方法调用失败时重试一定次数
                while (readResult != null && !readResult.IsSuccess && readErrorCount < CollectProperties.RetryCount)
                {
                    if (KeepRun != true)
                        return;
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    readErrorCount++;
                    if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage?.Trace(string.Format("{0} - Execute method[{1}] - Failed {2}", DeviceName, readVariableMethods.MethodInfo.Name, readResult?.ErrorMessage));

                    readResult = await InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken);
                }

                if (readResult != null && readResult.IsSuccess)
                {
                    // 方法调用成功时记录日志并增加成功计数器
                    if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage?.Trace(string.Format("{0} - Execute method[{1}] - Succeeded {2}", DeviceName, readVariableMethods.MethodInfo.Name, readResult?.Content?.ToSystemTextJsonString()));
                    readResultCount.deviceMethodsVariableSuccessNum++;
                }
                else
                {
                    if (readResult != null)
                    {
                        // 方法调用失败时记录日志并增加失败计数器，更新错误信息
                        if (readVariableMethods.LastErrorMessage != readResult?.ErrorMessage)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                                LogMessage?.LogWarning(readResult.Exception, Localizer["MethodFail", DeviceName, readVariableMethods.MethodInfo.Name, readResult?.ErrorMessage]);
                        }
                        else
                        {
                            if (!cancellationToken.IsCancellationRequested)
                                if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                                    LogMessage?.Trace(string.Format("{0} - Execute method[{1}] - Failed {2}", DeviceName, readVariableMethods.MethodInfo.Name, readResult?.ErrorMessage));
                        }

                        readResultCount.deviceMethodsVariableFailedNum++;
                        readVariableMethods.LastErrorMessage = readResult?.ErrorMessage;
                        CurrentDevice.SetDeviceStatus(DateTime.Now, CurrentDevice.ErrorCount + 1, readResult?.ToString());
                    }
                }
                if (delay)
                    await Task.Delay(ChannelThread.CycleInterval, cancellationToken);
            }
        }

        async Task ReadVariableSource(ReadResultCount readResultCount, VariableSourceRead? variableSourceRead, CancellationToken cancellationToken, bool delay = true)
        {
            if (KeepRun != true)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;

            // 如果请求更新时间已到，则执行变量读取
            if (variableSourceRead.CheckIfRequestAndUpdateTime(DateTime.Now))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var readErrorCount = 0;
                var readResult = await ReadSourceAsync(variableSourceRead, cancellationToken);

                // 读取失败时重试一定次数
                while (readResult != null && !readResult.IsSuccess && readErrorCount < CollectProperties.RetryCount)
                {
                    if (KeepRun != true)
                        return;
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    readErrorCount++;
                    if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage?.Trace(string.Format("{0} - Collection[{1} - {2}] data failed {3}", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult?.ErrorMessage));
                    readResult = await ReadSourceAsync(variableSourceRead, cancellationToken);
                }

                if (readResult != null && readResult.IsSuccess)
                {
                    // 读取成功时记录日志并增加成功计数器
                    if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage?.Trace(string.Format("{0} - Collection[{1} - {2}] data succeeded {3}", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult?.Content?.ToHexString(' ')));
                    readResultCount.deviceSourceVariableSuccessNum++;
                }
                else
                {
                    if (readResult != null)
                    {
                        // 读取失败时记录日志并增加失败计数器，更新错误信息并清除变量状态
                        if (variableSourceRead.LastErrorMessage != readResult?.ErrorMessage)
                        {
                            if (!cancellationToken.IsCancellationRequested)
                                LogMessage?.LogWarning(readResult.Exception, Localizer["CollectFail", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult?.ErrorMessage]);
                        }
                        else
                        {
                            if (!cancellationToken.IsCancellationRequested)
                                if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                                    LogMessage?.Trace(string.Format("{0} - Collection[{1} - {2}] data failed {3}", DeviceName, variableSourceRead?.RegisterAddress, variableSourceRead?.Length, readResult?.ErrorMessage));
                        }

                        readResultCount.deviceSourceVariableFailedNum++;
                        variableSourceRead.LastErrorMessage = readResult?.ErrorMessage;
                        CurrentDevice.SetDeviceStatus(DateTime.Now, CurrentDevice.ErrorCount + 1, readResult?.ErrorMessage);
                        variableSourceRead.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    }
                }
                if (delay)
                    await Task.Delay(ChannelThread.CycleInterval, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 连读打包，返回实际通讯包信息<see cref="VariableSourceRead"/>
    /// <br></br>每个驱动打包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    protected abstract List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables);

    #region 写入方法

    /// <summary>
    /// 异步调用方法
    /// </summary>
    /// <param name="variableMethod">要调用的方法</param>
    /// <param name="value">传递给方法的参数值（可选）</param>
    /// <param name="isRead">指示是否为读取操作</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>操作结果，包含执行方法的结果</returns>
    internal async Task<OperResult<object>> InvokeMethodAsync(VariableMethod variableMethod, string? value = null, bool isRead = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // 如果配置为单线程模式，则获取写入锁定
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);

            // 初始化操作结果
            OperResult<object> result = new OperResult<object>();

            // 获取要执行的方法
            var method = variableMethod.MethodInfo;

            // 如果方法未找到，则返回错误结果
            if (method == null)
            {
                result.OperCode = 999;
                result.ErrorMessage = Localizer["MethodNotNull", variableMethod.Variable.Name, variableMethod.Variable.OtherMethod];
                return result;
            }
            else
            {
                // 调用方法并获取结果
                var data = await variableMethod.InvokeMethodAsync(this, value, cancellationToken);
                var result1 = data?.Adapt<OperResult<object>>();

                // 将结果转换为 JToken 格式，并将操作结果设置为成功
                if (result1 != null)
                {
                    result = result1;
                }

                // 如果方法有返回值，并且是读取操作
                if (method.HasReturn && isRead)
                {
                    if (result?.IsSuccess == true)
                    {
                        // 将结果序列化并设置到变量中
                        var variableResult = variableMethod.Variable.SetValue(result.Content);
                        if (!variableResult.IsSuccess)
                            variableMethod.LastErrorMessage = result.ErrorMessage;
                    }
                    else
                    {
                        // 如果读取操作失败，则将变量标记为离线
                        var variableResult = variableMethod.Variable.SetValue(null, isOnline: false);
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
            return new(ex);
        }
        finally
        {
            // 如果配置为单线程模式，则释放写入锁定
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    internal async Task<Dictionary<string, OperResult>> InVokeWriteAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
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
                object rawdata = jToken is JValue jValue ? jValue.Value : jToken is JArray jArray ? jArray : jToken.ToString();
                try
                {
                    // 根据写入表达式转换数据
                    object data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata);
                    // 将转换后的数据重新赋值给写入信息列表
                    writeInfoLists[deviceVariable] = JToken.FromObject(data);
                }
                catch (Exception ex)
                {
                    // 如果转换失败，则记录错误信息
                    results.Add(deviceVariable.Name, new OperResult(Localizer["WriteExpressionsError", deviceVariable.Name, deviceVariable.WriteExpressions, ex.Message], ex));
                }
            }
        }

        // 过滤掉转换失败的变量，只保留写入成功的变量进行写入操作
        var results1 = await WriteValuesAsync(writeInfoLists
            .Where(a => !results.Any(b => b.Key == a.Key.Name))
            .ToDictionary(item => item.Key, item => item.Value),
            cancellationToken);

        // 将转换失败的变量和写入成功的变量的操作结果合并到结果字典中
        return results.Concat(results1).ToDictionary(a => a.Key, a => a.Value);
    }

    #endregion 写入方法
}