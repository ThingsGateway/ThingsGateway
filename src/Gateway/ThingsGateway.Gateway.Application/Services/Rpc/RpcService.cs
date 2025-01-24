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
using Microsoft.Extensions.Localization;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Extension;
using ThingsGateway.Extension.Generic;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量写入/执行变量方法
/// </summary>
internal sealed class RpcService : IRpcService
{
    private readonly ConcurrentQueue<RpcLog> _logQueues = new();
    private readonly RpcLogOptions? _rpcLogOptions;
    /// <inheritdoc cref="RpcService"/>
    public RpcService(IStringLocalizer<RpcService> localizer)
    {
        Localizer = localizer;
        Task.Factory.StartNew(async () => await RpcLogInsertAsync().ConfigureAwait(false), TaskCreationOptions.LongRunning);
        _rpcLogOptions = App.GetOptions<RpcLogOptions>();

    }

    private IStringLocalizer Localizer { get; }

    /// <inheritdoc />
    public async Task<Dictionary<string, OperResult>> InvokeDeviceMethodAsync(string sourceDes, Dictionary<string, string> items, CancellationToken cancellationToken = default)
    {
        // 初始化用于存储将要写入的变量和方法的字典
        Dictionary<CollectBase, Dictionary<VariableRuntime, JToken>> writeVariables = new();
        Dictionary<CollectBase, Dictionary<VariableRuntime, JToken>> writeMethods = new();
        // 用于存储结果的并发字典
        ConcurrentDictionary<string, OperResult> results = new();
        var dict = GlobalData.Variables;

        // 对每个要操作的变量进行检查和处理
        foreach (var item in items)
        {
            // 查找变量是否存在
            if (!dict.TryGetValue(item.Key, out var tag))
            {
                // 如果变量不存在，则添加错误信息到结果中并继续下一个变量的处理
                results.TryAdd(item.Key, new OperResult(Localizer["VariableNotNull", item.Key]));
                continue;
            }

            // 检查变量的保护类型和远程写入权限
            if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
            {
                results.TryAdd(item.Key, new OperResult(Localizer["VariableReadOnly", item.Key]));
                continue;
            }
            if (!tag.RpcWriteEnable)
            {
                results.TryAdd(item.Key, new OperResult(Localizer["VariableWriteDisable", item.Key]));
                continue;
            }

            // 查找变量对应的设备
            var collect = tag.DeviceRuntime.Driver as CollectBase;
            if (collect == null)
            {
                // 如果设备不存在，则添加错误信息到结果中并继续下一个变量的处理
                results.TryAdd(item.Key, new OperResult(Localizer["DriverNotNull"]));
                continue;
            }
            // 检查设备状态，如果设备处于暂停状态，则添加相应的错误信息到结果中并继续下一个变量的处理
            if (collect.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
            {
                results.TryAdd(item.Key, new OperResult(Localizer["DevicePause", collect.CurrentDevice.Name]));
                continue;
            }

            JToken tagValue = JTokenUtil.GetJTokenFromString(item.Value);
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

        // 使用并行方式写入变量
        await writeVariables.ParallelForEachAsync(async (item, cancellationToken) =>
        {
            try
            {
                // 调用设备的写入方法
                var result = await item.Key.InVokeWriteAsync(item.Value, cancellationToken).ConfigureAwait(false);

                // 写入日志
                foreach (var resultItem in result)
                {
                    var empty = string.IsNullOrEmpty(resultItem.Key);
                    string operObj = empty ? items.Select(x => x.Key).ToJsonNetString() : resultItem.Key;

                    string parJson = empty ? items.Select(x => x.Value).ToJsonNetString() : items[resultItem.Key];

                    if (_rpcLogOptions.SuccessLog)
                        _logQueues.Enqueue(
                            new RpcLog()
                            {
                                LogTime = DateTime.Now,
                                OperateMessage = resultItem.Value.IsSuccess ? null : resultItem.Value.ToString(),
                                IsSuccess = resultItem.Value.IsSuccess,
                                OperateMethod = Localizer["WriteVariable"],
                                OperateObject = operObj,
                                OperateSource = sourceDes,
                                ParamJson = parJson,
                                ResultJson = null
                            }
                        );

                    // 不返回详细错误
                    if (!resultItem.Value.IsSuccess)
                    {
                        OperResult result1 = resultItem.Value;
                        result1.Exception = null;
                        result[resultItem.Key] = result1;
                    }
                }

                // 将结果添加到结果字典中
                results.AddRange(result);
            }
            catch (Exception ex)
            {
                // 将异常信息添加到结果字典中
                results.AddRange(item.Value.Select((KeyValuePair<VariableRuntime, JToken> a) =>
                {
                    return new KeyValuePair<string, OperResult>(a.Key.Name, new OperResult(ex));
                }));
            }
        }, Environment.ProcessorCount, cancellationToken).ConfigureAwait(false);

        // 使用并行方式执行方法
        await writeMethods.ParallelForEachAsync(async (item, cancellationToken) =>
        {
            try
            {
                // 调用设备的写入方法
                var result = await item.Key.InvokeMethodAsync(item.Value, cancellationToken).ConfigureAwait(false);

                Dictionary<string, string> operateMethods = item.Value.Select(a => a.Key).ToDictionary(a => a.Name, a => a.OtherMethod!);

                // 写入日志
                foreach (var resultItem in result)
                {
                    // 写入日志
                    if (_rpcLogOptions.SuccessLog)
                        _logQueues.Enqueue(
                            new RpcLog()
                            {
                                LogTime = DateTime.Now,
                                OperateMessage = resultItem.Value.IsSuccess ? null : resultItem.Value.ToString(),
                                IsSuccess = resultItem.Value.IsSuccess,
                                OperateMethod = operateMethods[resultItem.Key],
                                OperateObject = resultItem.Key,
                                OperateSource = sourceDes,
                                ParamJson = items[resultItem.Key]?.ToString(),
                                ResultJson = resultItem.Value.Content?.ToString()
                            }
                        );

                    // 不返回详细错误
                    if (!resultItem.Value.IsSuccess)
                    {
                        OperResult<object> result1 = resultItem.Value;
                        result1.Exception = null;
                        result[resultItem.Key] = result1;
                    }
                }
            }
            catch (Exception ex)
            {
                // 将异常信息添加到结果字典中
                results.AddRange(item.Value.Select((KeyValuePair<VariableRuntime, JToken> a) =>
                {
                    return new KeyValuePair<string, OperResult>(a.Key.Name, new OperResult(ex));
                }));
            }
        }, Environment.ProcessorCount, cancellationToken).ConfigureAwait(false);

        // 返回结果字典
        return new(results);
    }

    /// <summary>
    /// 异步执行RPC日志插入操作的方法。
    /// </summary>
    private async Task RpcLogInsertAsync()
    {
        var db = DbContext.Db.GetConnectionScopeWithAttr<RpcLog>().CopyNew(); // 创建一个新的数据库上下文实例
        var appLifetime = App.RootServices!.GetService<IHostApplicationLifetime>()!;
        while (!appLifetime.ApplicationStopping.IsCancellationRequested)
        {
            try
            {
                var data = _logQueues.ToListWithDequeue(); // 从日志队列中获取数据
                if (data.Count > 0)
                {
                    // 将数据插入到数据库中
                    await db.InsertableWithAttr(data).ExecuteCommandAsync(appLifetime.ApplicationStopping).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                NewLife.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(3000).ConfigureAwait(false); // 在finally块中等待一段时间后继续下一次循环
            }
        }
    }
}
