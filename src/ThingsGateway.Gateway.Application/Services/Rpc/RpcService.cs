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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量写入/执行变量方法
/// </summary>
public class RpcService : IRpcService
{
    /// <summary>
    /// 写入变量说明
    /// </summary>
    public const string WriteVariable = "写入变量";

    private readonly IServiceScope _serviceScope;
    private readonly CollectDeviceWorker CollectDeviceWorker;
    private readonly GlobalData GlobalData;
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<RpcLog> _logQueues = new();
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="RpcService"/>
    public RpcService(
    IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory,
        IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = loggerFactory.CreateLogger("RPC服务");
        GlobalData = _serviceScope.ServiceProvider.GetService<GlobalData>();
        CollectDeviceWorker = WorkerUtil.GetWoker<CollectDeviceWorker>();
        _appLifetime = appLifetime;
        Task.Factory.StartNew(RpcLogInsertAsync);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, OperResult>> InvokeDeviceMethodAsync(string sourceDes, Dictionary<string, string> items, CancellationToken cancellationToken = default)
    {
        Dictionary<CollectBase, Dictionary<VariableRunTime, JToken>> WriteVariables = new();
        Dictionary<CollectBase, Dictionary<VariableRunTime, string>> WriteMethods = new();
        ConcurrentDictionary<string, OperResult> results = new();

        //检查
        foreach (var item in items)
        {
            var tag = GlobalData.AllVariables.FirstOrDefault(it => it.Name == item.Key);
            if (tag == null)
            {
                results.TryAdd(item.Key, new OperResult("不存在变量:" + item.Key));
                continue;
            }
            if (tag.ProtectType == ProtectTypeEnum.ReadOnly)
            {
                results.TryAdd(item.Key, new OperResult("只读变量:" + item.Key));
                continue;
            }
            if (!tag.RpcWriteEnable)
            {
                results.TryAdd(item.Key, new OperResult("不允许远程写入:" + item.Key));
                continue;
            }

            var dev = (CollectBase)CollectDeviceWorker.DriverBases.FirstOrDefault(it => it.DeviceId == tag.DeviceId);
            if (dev == null)
            {
                results.TryAdd(item.Key, new OperResult("系统错误，不存在对应采集设备，请稍候重试"));
                continue;
            }
            if (dev.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine)
            {
                //results.TryAdd(item.Key, new OperResult("设备已离线"));
                //continue;
                //取消条件，离西安状态也尝试写入
            }
            if (dev.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
            {
                results.TryAdd(item.Key, new OperResult("设备已暂停"));
                continue;
            }

            if (!results.ContainsKey(item.Key))
            {
                //添加到字典

                if (string.IsNullOrEmpty(tag.OtherMethod))
                {
                    //写入变量值
                    JToken tagValue = JTokenUtil.GetJTokenFromString(item.Value);
                    if (WriteVariables.ContainsKey(dev))
                    {
                        WriteVariables[dev].Add(tag, tagValue);
                    }
                    else
                    {
                        WriteVariables.Add(dev, new());
                        WriteVariables[dev].Add(tag, tagValue);
                    }
                }
                else
                {
                    //执行方法
                    if (WriteMethods.ContainsKey(dev))
                    {
                        WriteMethods[dev].Add(tag, item.Value);
                    }
                    else
                    {
                        WriteMethods.Add(dev, new());
                        WriteMethods[dev].Add(tag, item.Value);
                    }
                }
            }
        }

        //写入变量
        await WriteVariables.ParallelForEachAsync(async (item, cancellationToken) =>
         {
             try
             {
                 var result = await item.Key.InVokeWriteAsync(item.Value, cancellationToken);

                 #region 写入日志

                 foreach (var resultItem in result)
                 {
                     string operObj;
                     string parJson;
                     if (resultItem.Key.IsNullOrEmpty())
                     {
                         operObj = items.Select(x => x.Key).ToJsonString();
                         parJson = items.Select(x => x.Value).ToJsonString();
                     }
                     else
                     {
                         operObj = resultItem.Key;
                         parJson = items[resultItem.Key];
                     }
                     _logQueues.Enqueue(
           new RpcLog()
           {
               LogTime = DateTimeUtil.Now,
               OperateMessage = resultItem.Value.IsSuccess ? null : resultItem.Value.ToString(),
               IsSuccess = resultItem.Value.IsSuccess,
               OperateMethod = WriteVariable,
               OperateObject = operObj,
               OperateSource = sourceDes,
               ParamJson = parJson,
               ResultJson = null
           }
           );
                     //if (!resultItem.Value.IsSuccess)
                     //{
                     //    _logger.LogWarning($"写入变量[{resultItem.Key}]失败：{resultItem.Value}");
                     //}

                     //不返回详细错误
                     if (!resultItem.Value.IsSuccess)
                         resultItem.Value.Exception = null;
                 }

                 #endregion 写入日志

                 results.AddRange(result);
             }
             catch (Exception ex)
             {
                 _logger.LogWarning($"写入变量异常：{ex}");

                 results.AddRange(item.Value.Select((KeyValuePair<VariableRunTime, JToken> a) =>
                 {
                     return new KeyValuePair<string, OperResult>(a.Key.Name, new OperResult($"意外错误：{ex.Message}"));
                 }));
             }
         }, Environment.ProcessorCount / 2, cancellationToken);

        //执行方法
        await WriteMethods.ParallelForEachAsync(async (item, cancellationToken) =>
        {
            await item.Value.ParallelForEachAsync(async (writeMethod, cancellationToken) =>
            //foreach (var writeMethod in item.Value)
            {
                //执行变量附带的方法
                var method = item.Key.CurrentDevice.VariableMethods.FirstOrDefault(it => it.Variable == writeMethod.Key);
                OperResult<JToken> result;
                try
                {
                    result = await item.Key.InvokeMethodAsync(method, writeMethod.Value, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    result = new(ex);
                }

                #region 写入日志

                _logQueues.Enqueue(
    new RpcLog()
    {
        LogTime = DateTimeUtil.Now,
        OperateMessage = result.IsSuccess ? null : result.ToString(),
        IsSuccess = result.IsSuccess,
        OperateMethod = writeMethod.Key.OtherMethod,
        OperateObject = writeMethod.Key.Name,
        OperateSource = sourceDes,
        ParamJson = writeMethod.Value?.ToString(),
        ResultJson = result.Content?.ToString()
    }
    );

                //不返回详细错误
                result.Exception = null;
                results.TryAdd(writeMethod.Key.Name, result);

                #endregion 写入日志

                //if (!result.IsSuccess)
                //{
                //    _logger.LogWarning($"执行变量[{writeMethod.Key.Name}]方法[{writeMethod.Key.OtherMethod}]失败：{result}");
                //}
            }, item.Key.DriverPropertys.ConcurrentCount, cancellationToken);
        }, Environment.ProcessorCount / 2, cancellationToken);

        return new(results);
    }

    private async Task RpcLogInsertAsync()
    {
        var db = DbContext.Db.CopyNew();
        while (!(_appLifetime.ApplicationStopping.IsCancellationRequested || _appLifetime.ApplicationStopped.IsCancellationRequested))
        {
            try
            {
                var data = _logQueues.ToListWithDequeue();
                await db.InsertableWithAttr(data).ExecuteCommandAsync(_appLifetime.ApplicationStopping);//入库
            }
            catch
            {
            }
            finally
            {
                await Task.Delay(3000);
            }
        }
    }
}