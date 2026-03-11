// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using ThingsGateway.Foundation.Common.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量写入/执行变量方法
/// </summary>
internal sealed class RpcService : IRpcService
{
    private readonly ConcurrentQueue<RpcLog> _logQueues = new();
    private readonly RpcLogOptions? _rpcLogOptions;
    private ISqlClient _db = DbContext.GetDB<RpcLog>(); // 创建一个新的数据库上下文实例

    private IStringLocalizer Localizer { get; }

    /// <inheritdoc cref="RpcService"/>
    public RpcService(IStringLocalizer<RpcService> localizer)
    {
        Localizer = localizer;
        Task.Factory.StartNew(RpcLogInsertAsync, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        _rpcLogOptions = App.GetOptions<RpcLogOptions>();
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeDeviceMethodAsync(
        string sourceDes,
        Dictionary<string, Dictionary<string, string>> deviceDatas,
        CancellationToken cancellationToken = default)
    {
        // 初始化用于存储将要写入的变量和方法的字典
        Dictionary<IRpcDriver, Dictionary<VariableRuntime, JsonNode>> writeVariables = new();
        Dictionary<IRpcDriver, Dictionary<VariableRuntime, JsonNode>> writeMethods = new();
        Dictionary<VariableRuntime, string> memoryVariables = new();

        // 用于存储结果的并发字典
        NonBlockingDictionary<string, IDictionary<string, OperResult<object>>> results = new();
        deviceDatas.ForEach(a => results.TryAdd(a.Key, new Dictionary<string, OperResult<object>>()));

        // 对每个要操作的变量进行检查和处理（内存变量）
        foreach (var item in deviceDatas.Where(a => a.Key.IsNullOrEmpty() || a.Key.Equals(MemoryConst.MemoryName, StringComparison.OrdinalIgnoreCase)).SelectMany(a => a.Value))
        {
            // 查找变量是否存在
            if (!(GlobalData.TryGetVariable(item.Key, out var tag) &&
                  tag is IMemoryVariableRpc memoryVariableRuntime))
            {
                // 如果变量不存在，则添加错误信息到结果中并继续下一个变量的处理
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableNotNull", item.Key]));
                continue;
            }

            // 检查变量的保护类型和远程写入权限
            if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
            {
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableReadOnly", item.Key]));
                continue;
            }

            if (!tag.RpcWriteEnable)
            {
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableWriteDisable", item.Key]));
                continue;
            }

            try
            {
                var start = DateTime.Now;

                var variableResult = memoryVariableRuntime.MemoryVariableRpc(JsonUtil.GetJsonNodeFromString(item.Value), cancellationToken);
                var end = DateTime.Now;

                string operObj = tag.Name;
                string parJson = deviceDatas[MemoryConst.MemoryName][tag.Name];

                if (!variableResult.IsSuccess || _rpcLogOptions.SuccessLog)
                {
                    _logQueues.Enqueue(
                        new RpcLog()
                        {
                            LogTime = start,
                            ExecutionTime = (int)(end - start).TotalMilliseconds,
                            OperateMessage = variableResult.IsSuccess ? null : variableResult.ToString(),
                            IsSuccess = variableResult.IsSuccess,
                            OperateMethod = AppResource.WriteVariable,
                            OperateDevice = string.Empty,
                            OperateObject = operObj,
                            OperateSource = sourceDes,
                            ParamJson = parJson,
                            ResultJson = null
                        });
                }

                // 不返回详细错误
                if (!variableResult.IsSuccess)
                {
                    var result1 = variableResult;
                    result1.Exception = null;
                    variableResult = result1;
                }

                results[MemoryConst.MemoryName].Add(tag.Name, variableResult);
            }
            catch (Exception ex)
            {
                // 将异常信息添加到结果字典中
                results[MemoryConst.MemoryName].Add(tag.Name, new OperResult<object>(ex));
            }
        }

        var deviceDict = GlobalData.Devices;

        // 对每个要操作的变量进行检查和处理（设备变量）
        foreach (var deviceData in deviceDatas.Where(a => (!a.Key.IsNullOrEmpty() && !a.Key.Equals(MemoryConst.MemoryName, StringComparison.OrdinalIgnoreCase))))
        {
            // 查找设备是否存在
            if (!deviceDict.TryGetValue(deviceData.Key, out var device))
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DeviceNotNull", deviceData.Key])));
                continue;
            }

            // 查找变量对应的设备
            var collect = device.Driver as IRpcDriver ?? device.RpcDriver;
            if (collect == null)
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DriverNotNull", deviceData.Key])));
                continue;
            }

            // 检查设备状态
            if (device.DeviceStatus == DeviceStatusEnum.Pause)
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DevicePause", deviceData.Key])));
                continue;
            }

            foreach (var item in deviceData.Value)
            {
                // 查找变量是否存在
                if (!device.VariableRuntimes.TryGetValue(item.Key, out var tag))
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableNotNull", item.Key]));
                    continue;
                }

                // 检查变量的保护类型和远程写入权限
                if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableReadOnly", item.Key]));
                    continue;
                }

                if (!tag.RpcWriteEnable)
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableWriteDisable", item.Key]));
                    continue;
                }

                JsonNode tagValue = JsonUtil.GetJsonNodeFromString(item.Value);
                bool isOtherMethodEmpty = string.IsNullOrEmpty(tag.OtherMethod);
                var collection = isOtherMethodEmpty ? writeVariables : writeMethods;

                if (collection.TryGetValue(collect, out var value))
                {
                    value.Add(tag, tagValue);
                }
                else
                {
                    collection.Add(collect, new());
                    collection[collect].Add(tag, tagValue);
                }
            }
        }

        // 使用并行方式写入变量
        var writeVariableArrays = writeVariables;
        var writeVariableArraysTasks = writeVariableArrays.Select(async driverData =>
    {
        try
        {
            var start = DateTime.Now;
            var result = await driverData.Key.InVokeWriteAsync(driverData.Value, cancellationToken).ConfigureAwait(false);
            var end = DateTime.Now;

            foreach (var resultItem in result)
            {
                foreach (var variableResult in resultItem.Value)
                {
                    string operObj = variableResult.Key;
                    string parJson = deviceDatas[resultItem.Key][variableResult.Key];

                    if (!variableResult.Value.IsSuccess || _rpcLogOptions.SuccessLog)
                    {
                        _logQueues.Enqueue(
                            new RpcLog()
                            {
                                LogTime = start,
                                ExecutionTime = (int)(end - start).TotalMilliseconds,
                                OperateMessage = variableResult.Value.IsSuccess ? null : variableResult.Value.ToString(),
                                IsSuccess = variableResult.Value.IsSuccess,
                                OperateMethod = AppResource.WriteVariable,
                                OperateDevice = resultItem.Key,
                                OperateObject = operObj,
                                OperateSource = sourceDes,
                                ParamJson = parJson,
                                ResultJson = null
                            });
                    }

                    if (!variableResult.Value.IsSuccess)
                    {
                        var result1 = variableResult.Value;
                        result1.Exception = null;
                        resultItem.Value[variableResult.Key] = result1;
                    }
                }

                results[resultItem.Key].AddRange(resultItem.Value);
            }
        }
        catch (Exception ex)
        {
            foreach (var item in driverData.Value)
            {
                results[item.Key.DeviceName].Add(item.Key.Name, new OperResult<object>(ex));
            }
        }
    }).ToArray();
        await Task.WhenAll(writeVariableArraysTasks).ConfigureAwait(false);

        // 使用并行方式执行方法
        var writeMethodArrays = writeMethods;
        var writeMethodArraysTasks = writeMethodArrays.Select(async driverData =>
        {
            try
            {
                var start = DateTime.Now;
                var result = await driverData.Key.InvokeMethodAsync(driverData.Value, cancellationToken).ConfigureAwait(false);

                Dictionary<string, string> operateMethods = driverData.Value
                    .Select(a => a.Key)
                    .ToDictionary(a => a.Name, a => a.OtherMethod!);

                var end = DateTime.Now;

                foreach (var resultItem in result)
                {
                    foreach (var variableResult in resultItem.Value)
                    {
                        string operObj = variableResult.Key;
                        string parJson = deviceDatas[resultItem.Key][variableResult.Key];

                        if (!variableResult.Value.IsSuccess || _rpcLogOptions.SuccessLog)
                        {
                            _logQueues.Enqueue(
                                new RpcLog()
                                {
                                    LogTime = start,
                                    ExecutionTime = (int)(end - start).TotalMilliseconds,
                                    OperateMessage = variableResult.Value.IsSuccess ? null : variableResult.Value.ToString(),
                                    IsSuccess = variableResult.Value.IsSuccess,
                                    OperateMethod = operateMethods[variableResult.Key],
                                    OperateDevice = resultItem.Key,
                                    OperateObject = operObj,
                                    OperateSource = sourceDes,
                                    ParamJson = parJson?.ToString(),
                                    ResultJson = variableResult.Value is IOperResult<object> operResult
                                        ? operResult.Content?.ToSystemTextJsonString()
                                        : string.Empty
                                });
                        }

                        if (!variableResult.Value.IsSuccess)
                        {
                            var result1 = variableResult.Value;
                            result1.Exception = null;
                            resultItem.Value[variableResult.Key] = result1;
                        }
                    }

                    results[resultItem.Key].AddRange(resultItem.Value.ToDictionary(a => a.Key, a => a.Value));
                }
            }
            catch (Exception ex)
            {
                foreach (var item in driverData.Value)
                {
                    results[item.Key.DeviceName].Add(item.Key.Name, new OperResult<object>(ex));
                }
            }
        }).ToArray();

        await Task.WhenAll(writeMethodArraysTasks).ConfigureAwait(false);

        // 返回结果字典
        return results;
    }


    /// <inheritdoc />
    public async Task<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeDeviceMethodAsync(
        string sourceDes,
        Dictionary<string, Dictionary<string, JsonNode>> deviceDatas,
        CancellationToken cancellationToken = default)
    {
        // 初始化用于存储将要写入的变量和方法的字典
        Dictionary<IRpcDriver, Dictionary<VariableRuntime, JsonNode>> writeVariables = new();
        Dictionary<IRpcDriver, Dictionary<VariableRuntime, JsonNode>> writeMethods = new();
        Dictionary<VariableRuntime, string> memoryVariables = new();

        // 用于存储结果的并发字典
        NonBlockingDictionary<string, IDictionary<string, OperResult<object>>> results = new();
        deviceDatas.ForEach(a => results.TryAdd(a.Key, new Dictionary<string, OperResult<object>>()));

        // 对每个要操作的变量进行检查和处理（内存变量）
        foreach (var item in deviceDatas.Where(a => a.Key.IsNullOrEmpty() || a.Key.Equals(MemoryConst.MemoryName, StringComparison.OrdinalIgnoreCase)).SelectMany(a => a.Value))
        {
            // 查找变量是否存在
            if (!(GlobalData.TryGetVariable(item.Key, out var tag) &&
                  tag is IMemoryVariableRpc memoryVariableRuntime))
            {
                // 如果变量不存在，则添加错误信息到结果中并继续下一个变量的处理
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableNotNull", item.Key]));
                continue;
            }

            // 检查变量的保护类型和远程写入权限
            if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
            {
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableReadOnly", item.Key]));
                continue;
            }

            if (!tag.RpcWriteEnable)
            {
                results[MemoryConst.MemoryName].TryAdd(item.Key, new OperResult<object>(Localizer["VariableWriteDisable", item.Key]));
                continue;
            }

            try
            {
                var start = DateTime.Now;

                var variableResult = memoryVariableRuntime.MemoryVariableRpc(item.Value, cancellationToken);
                var end = DateTime.Now;

                string operObj = tag.Name;
                string parJson = deviceDatas[MemoryConst.MemoryName][tag.Name].ToJsonString(SystemTextJsonExtension.SystemTextJsonService.IndentedOptions);

                if (!variableResult.IsSuccess || _rpcLogOptions.SuccessLog)
                {
                    _logQueues.Enqueue(
                        new RpcLog()
                        {
                            LogTime = start,
                            ExecutionTime = (int)(end - start).TotalMilliseconds,
                            OperateMessage = variableResult.IsSuccess ? null : variableResult.ToString(),
                            IsSuccess = variableResult.IsSuccess,
                            OperateMethod = AppResource.WriteVariable,
                            OperateDevice = string.Empty,
                            OperateObject = operObj,
                            OperateSource = sourceDes,
                            ParamJson = parJson,
                            ResultJson = null
                        });
                }

                // 不返回详细错误
                if (!variableResult.IsSuccess)
                {
                    var result1 = variableResult;
                    result1.Exception = null;
                    variableResult = result1;
                }

                results[MemoryConst.MemoryName].Add(tag.Name, variableResult);
            }
            catch (Exception ex)
            {
                // 将异常信息添加到结果字典中
                results[MemoryConst.MemoryName].Add(tag.Name, new OperResult<object>(ex));
            }
        }

        var deviceDict = GlobalData.Devices;

        // 对每个要操作的变量进行检查和处理（设备变量）
        foreach (var deviceData in deviceDatas.Where(a => (!a.Key.IsNullOrEmpty() && !a.Key.Equals(MemoryConst.MemoryName, StringComparison.OrdinalIgnoreCase))))
        {
            // 查找设备是否存在
            if (!deviceDict.TryGetValue(deviceData.Key, out var device))
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DeviceNotNull", deviceData.Key])));
                continue;
            }

            // 查找变量对应的设备
            var collect = device.Driver as IRpcDriver ?? device.RpcDriver;
            if (collect == null)
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DriverNotNull", deviceData.Key])));
                continue;
            }

            // 检查设备状态
            if (device.DeviceStatus == DeviceStatusEnum.Pause)
            {
                deviceData.Value.ForEach(a =>
                    results[deviceData.Key].TryAdd(a.Key, new OperResult<object>(Localizer["DevicePause", deviceData.Key])));
                continue;
            }

            foreach (var item in deviceData.Value)
            {
                // 查找变量是否存在
                if (!device.VariableRuntimes.TryGetValue(item.Key, out var tag))
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableNotNull", item.Key]));
                    continue;
                }

                // 检查变量的保护类型和远程写入权限
                if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableReadOnly", item.Key]));
                    continue;
                }

                if (!tag.RpcWriteEnable)
                {
                    results[deviceData.Key].TryAdd(item.Key, new OperResult<object>(Localizer["VariableWriteDisable", item.Key]));
                    continue;
                }

                JsonNode tagValue = item.Value;
                bool isOtherMethodEmpty = string.IsNullOrEmpty(tag.OtherMethod);
                var collection = isOtherMethodEmpty ? writeVariables : writeMethods;

                if (collection.TryGetValue(collect, out var value))
                {
                    value.Add(tag, tagValue);
                }
                else
                {
                    collection.Add(collect, new());
                    collection[collect].Add(tag, tagValue);
                }
            }
        }

        // 使用并行方式写入变量
        var writeVariableArrays = writeVariables;
        var writeVariableArraysTasks = writeVariableArrays.Select(async driverData =>
        {
            try
            {
                var start = DateTime.Now;
                var result = await driverData.Key.InVokeWriteAsync(driverData.Value, cancellationToken).ConfigureAwait(false);
                var end = DateTime.Now;

                foreach (var resultItem in result)
                {
                    foreach (var variableResult in resultItem.Value)
                    {
                        string operObj = variableResult.Key;
                        string parJson = deviceDatas[resultItem.Key][variableResult.Key].ToJsonString(SystemTextJsonExtension.SystemTextJsonService.IndentedOptions);

                        if (!variableResult.Value.IsSuccess || _rpcLogOptions.SuccessLog)
                        {
                            _logQueues.Enqueue(
                                new RpcLog()
                                {
                                    LogTime = start,
                                    ExecutionTime = (int)(end - start).TotalMilliseconds,
                                    OperateMessage = variableResult.Value.IsSuccess ? null : variableResult.Value.ToString(),
                                    IsSuccess = variableResult.Value.IsSuccess,
                                    OperateMethod = AppResource.WriteVariable,
                                    OperateDevice = resultItem.Key,
                                    OperateObject = operObj,
                                    OperateSource = sourceDes,
                                    ParamJson = parJson,
                                    ResultJson = null
                                });
                        }

                        if (!variableResult.Value.IsSuccess)
                        {
                            var result1 = variableResult.Value;
                            result1.Exception = null;
                            resultItem.Value[variableResult.Key] = result1;
                        }
                    }

                    results[resultItem.Key].AddRange(resultItem.Value);
                }
            }
            catch (Exception ex)
            {
                foreach (var item in driverData.Value)
                {
                    results[item.Key.DeviceName].Add(item.Key.Name, new OperResult<object>(ex));
                }
            }
        }).ToArray();

        await Task.WhenAll(writeVariableArraysTasks).ConfigureAwait(false);

        // 使用并行方式执行方法
        var writeMethodArrays = writeMethods;
        var writeMethodArraysTasks = writeMethodArrays.Select(async driverData =>
        {
            try
            {
                var start = DateTime.Now;
                var result = await driverData.Key.InvokeMethodAsync(driverData.Value, cancellationToken).ConfigureAwait(false);

                Dictionary<string, string> operateMethods = driverData.Value
                    .Select(a => a.Key)
                    .ToDictionary(a => a.Name, a => a.OtherMethod!);

                var end = DateTime.Now;

                foreach (var resultItem in result)
                {
                    foreach (var variableResult in resultItem.Value)
                    {
                        string operObj = variableResult.Key;
                        string parJson = deviceDatas[resultItem.Key][variableResult.Key].ToJsonString(SystemTextJsonExtension.SystemTextJsonService.IndentedOptions);

                        if (!variableResult.Value.IsSuccess || _rpcLogOptions.SuccessLog)
                        {
                            _logQueues.Enqueue(
                                new RpcLog()
                                {
                                    LogTime = start,
                                    ExecutionTime = (int)(end - start).TotalMilliseconds,
                                    OperateMessage = variableResult.Value.IsSuccess ? null : variableResult.Value.ToString(),
                                    IsSuccess = variableResult.Value.IsSuccess,
                                    OperateMethod = operateMethods[variableResult.Key],
                                    OperateDevice = resultItem.Key,
                                    OperateObject = operObj,
                                    OperateSource = sourceDes,
                                    ParamJson = parJson?.ToString(),
                                    ResultJson = variableResult.Value is IOperResult<object> operResult
                                        ? operResult.Content?.ToSystemTextJsonString()
                                        : string.Empty
                                });
                        }

                        if (!variableResult.Value.IsSuccess)
                        {
                            var result1 = variableResult.Value;
                            result1.Exception = null;
                            resultItem.Value[variableResult.Key] = result1;
                        }
                    }

                    results[resultItem.Key].AddRange(resultItem.Value.ToDictionary(a => a.Key, a => a.Value));
                }
            }
            catch (Exception ex)
            {
                foreach (var item in driverData.Value)
                {
                    results[item.Key.DeviceName].Add(item.Key.Name, new OperResult<object>(ex));
                }
            }
        }).ToArray();

        await Task.WhenAll(writeMethodArraysTasks).ConfigureAwait(false);

        // 返回结果字典
        return results;
    }

    /// <summary>
    /// 异步执行RPC日志插入操作的方法。
    /// </summary>
    private async Task RpcLogInsertAsync()
    {
        var appLifetime = App.RootServices!.GetService<IHostApplicationLifetime>()!;
        while (!appLifetime.ApplicationStopping.IsCancellationRequested)
        {
            try
            {
                var data = _logQueues.ToListWithDequeue();
                if (data.Count > 0)
                {
                    await _db.Insertable(data)
                        .ExecuteCommandAsync(appLifetime.ApplicationStopping)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Foundation.Common.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(3000).ConfigureAwait(false);
            }
        }
    }
}
