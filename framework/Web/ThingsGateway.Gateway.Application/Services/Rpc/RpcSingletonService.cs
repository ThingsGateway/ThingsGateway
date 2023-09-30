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

using Furion;
using Furion.DependencyInjection;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;

namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 变量写入/执行变量附带方法，单例服务
/// </summary>
public class RpcSingletonService : ISingleton
{
    /// <summary>
    /// 写入变量说明
    /// </summary>
    public const string WriteVariable = "写入变量";
    private readonly ILogger<RpcSingletonService> _logger;
    private readonly CollectDeviceWorker _collectDeviceHostService;
    private readonly GlobalDeviceData _globalDeviceData;
    private readonly ConcurrentQueue<RpcLog> _logQueues = new();
    /// <inheritdoc cref="RpcSingletonService"/>
    public RpcSingletonService(ILogger<RpcSingletonService> logger)
    {
        _logger = logger;
        _globalDeviceData = App.GetService<GlobalDeviceData>();
        _collectDeviceHostService = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>();
        Task.Factory.StartNew(RpcLogInsertAsync, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 反向RPC入口方法
    /// </summary>
    /// <param name="sourceDes">触发该方法的源说明</param>
    /// <param name="items">指定键为变量名称，值为附带方法参数或写入值</param>
    /// <param name="isBlazor">如果是true，不检查<see cref="MemoryVariable.RpcWriteEnable"/>字段</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> 取消源</param>
    /// <returns></returns>
    public async Task<Dictionary<string, OperResult>> InvokeDeviceMethodAsync(string sourceDes, Dictionary<string, string> items, bool isBlazor = false, CancellationToken cancellationToken = default)
    {
        Dictionary<CollectDeviceCore, Dictionary<DeviceVariableRunTime, JToken>> WriteVariables = new();
        Dictionary<CollectDeviceCore, Dictionary<DeviceVariableRunTime, string>> WriteMethods = new();
        Dictionary<string, OperResult> results = new();
        foreach (var item in items)
        {
            var tag = _globalDeviceData.AllVariables.FirstOrDefault(it => it.Name == item.Key);
            if (tag == null)
            {
                results.Add(item.Key, new OperResult("不存在变量:" + item.Key));
                continue;
            }
            if (tag.ProtectTypeEnum == ProtectTypeEnum.ReadOnly)
            {
                results.Add(item.Key, new OperResult("只读变量:" + item.Key));
                continue;
            }
            if (!tag.RpcWriteEnable && !isBlazor)
            {
                results.Add(item.Key, new OperResult("不允许远程写入:" + item.Key));
                continue;
            }

            if (tag.IsMemoryVariable == true)
            {
                results.Add(item.Key, tag.SetValue(item.Value));
                continue;
            }

            var dev = _collectDeviceHostService.CollectDeviceCores.FirstOrDefault(it => it.Device.Id == tag.DeviceId);
            if (dev == null)
            {
                results.Add(item.Key, new OperResult("系统错误，不存在对应采集设备，请稍候重试"));
                continue;

            }
            if (dev.Device.DeviceStatus == DeviceStatusEnum.OffLine)
            {
                results.Add(item.Key, new OperResult("设备已离线"));
                continue;
            }
            if (dev.Device.DeviceStatus == DeviceStatusEnum.Pause)
            {
                results.Add(item.Key, new OperResult("设备已暂停"));
                continue;
            }

            if (!results.ContainsKey(item.Key))
            {
                if (string.IsNullOrEmpty(tag.OtherMethod))
                {
                    JToken tagValue = JTokenUtil.GetJTokenFromObj(item.Value);
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
                    if (WriteMethods.ContainsKey(dev))
                    {
                        WriteMethods[dev].Add(tag, item.Value);
                    }
                    else
                    {
                        WriteVariables.Add(dev, new());
                        WriteVariables[dev].Add(tag, item.Value);
                    }
                }
            }
        }
        await WriteVariables.ParallelForEachAsync(async (item, cancellationToken) =>
         {
             try
             {
                 var result = await item.Key.InVokeWriteAsync(item.Value, cancellationToken);
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
               LogTime = DateTimeExtensions.CurrentDateTime,
               OperateMessage = resultItem.Value.IsSuccess ? resultItem.Value.Message : resultItem.Value.ExceptionString ?? resultItem.Value.Message,
               IsSuccess = resultItem.Value.IsSuccess,
               OperateMethod = WriteVariable,
               OperateObject = operObj,
               OperateSource = sourceDes,
               ParamJson = parJson,
               ResultJson = resultItem.Value.Message
           }
           );
                     if (!resultItem.Value.IsSuccess)
                     {
                         _logger.LogWarning($"写入变量[{resultItem.Key}]失败：{resultItem.Value.Message}");
                     }
                 }

                 results.AddRange(result);
             }
             catch (Exception ex)
             {
                 _logger.LogWarning($"写入变量异常：{ex}");

                 results.AddRange(item.Value.Select((KeyValuePair<DeviceVariableRunTime, JToken> a) =>
                 {
                     return new KeyValuePair<string, OperResult>(a.Key.Name, new OperResult($"捕捉错误：{ex.Message}"));
                 }));

             }

         }, 10, cancellationToken);

        await WriteMethods.ParallelForEachAsync(async (item, cancellationToken) =>
        {
            foreach (var writeMethod in item.Value)
            {
                //执行变量附带的方法
                var method = item.Key.DeviceVariableMethodSources.FirstOrDefault(it => it.DeviceVariable == writeMethod.Key);
                OperResult<string> result;
                try
                {
                    result = await item.Key.InvokeMethodAsync(method, false, writeMethod.Value, cancellationToken);
                    results.Add(writeMethod.Key.Name, result);
                }
                catch (Exception ex)
                {
                    result = new OperResult<string>(ex);
                    results.Add(writeMethod.Key.Name, result);
                }
                _logQueues.Enqueue(
    new RpcLog()
    {
        LogTime = DateTimeExtensions.CurrentDateTime,
        OperateMessage = result.IsSuccess ? result.Message : result.ExceptionString ?? result.Message,
        IsSuccess = result.IsSuccess,
        OperateMethod = writeMethod.Key.OtherMethod,
        OperateObject = writeMethod.Key.Name,
        OperateSource = sourceDes,
        ParamJson = writeMethod.Value?.ToString(),
        ResultJson = result.Message
    }
    );
                if (!result.IsSuccess)
                {
                    _logger.LogWarning($"执行变量[{writeMethod.Key.Name}]方法[{writeMethod.Key.OtherMethod}]失败：{result.Message}");
                }


            }

        }, 10, cancellationToken);

        return results;
    }

    private async Task RpcLogInsertAsync()
    {
        var db = DbContext.Db.CopyNew();
        while (true)
        {
            try
            {
                var data = _logQueues.ToListWithDequeue();
                await db.InsertableWithAttr(data).ExecuteCommandAsync();//入库
            }
            catch
            {

            }

            await Task.Delay(3000);
        }
    }
}
