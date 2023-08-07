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

using Furion.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;

namespace ThingsGateway.Application;
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
        _globalDeviceData = ServiceHelper.Services.GetService<GlobalDeviceData>();
        _collectDeviceHostService = ServiceHelper.GetBackgroundService<CollectDeviceWorker>();
        Task.Factory.StartNew(RpcLogInsertAsync, TaskCreationOptions.LongRunning);
    }
    /// <summary>
    /// 反向RPC入口方法
    /// </summary>
    /// <param name="sourceDes">触发该方法的源说明</param>
    /// <param name="item">指定键为变量名称，值为附带方法参数或写入值</param>
    /// <param name="isBlazor">如果是true，不检查<see cref="MemoryVariable.RpcWriteEnable"/>字段</param>
    /// <param name="token"><see cref="CancellationToken"/> 取消源</param>
    /// <returns></returns>
    public async Task<OperResult> InvokeDeviceMethodAsync(string sourceDes, KeyValuePair<string, string> item, bool isBlazor = false, CancellationToken token = default)
    {
        //避免并发过高，这里延时10ms
        await Task.Delay(10, token);
        OperResult data = new();
        var tag = _globalDeviceData.AllVariables.FirstOrDefault(it => it.Name == item.Key);
        if (tag == null) return new OperResult("不存在变量:" + item.Key);
        if (tag.ProtectTypeEnum == ProtectTypeEnum.ReadOnly) return new OperResult("只读变量");
        if (!tag.RpcWriteEnable && !isBlazor) return new OperResult("不允许远程写入");

        if (tag.IsMemoryVariable == true)
        {
            return tag.SetValue(item.Value);
        }
        var dev = _collectDeviceHostService.CollectDeviceCores.FirstOrDefault(it => it.Device.Id == tag.DeviceId);
        if (dev == null) return new OperResult("系统错误，不存在对应采集设备，请稍候重试");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.OffLine) return new OperResult("设备已离线");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.Pause) return new OperResult("设备已暂停");
        if (string.IsNullOrEmpty(tag.OtherMethod))
        {
            //写入变量
            JToken tagValue;
            try
            {
                tagValue = JToken.Parse(item.Value);
            }
            catch (Exception)
            {
                tagValue = JToken.Parse("\"" + item.Value + "\"");
            }

            data = await dev.InVokeWriteAsync(tag, tagValue, token);
            _logQueues.Enqueue(
                new RpcLog()
                {
                    LogTime = SysDateTimeExtensions.CurrentDateTime,
                    OperateMessage = data.Exception,
                    IsSuccess = data.IsSuccess,
                    OperateMethod = WriteVariable,
                    OperateObject = tag.Name,
                    OperateSource = sourceDes,
                    ParamJson = item.Value,
                    ResultJson = data.Message
                }
                );
            if (!data.IsSuccess)
            {
                _logger.LogWarning($"写入变量[{tag.Name}]失败：{data.Message}");
            }
        }
        else
        {
            //执行变量附带的方法
            var method = dev.DeviceVariableMethodSources.FirstOrDefault(it => it.DeviceVariable == tag);
            try
            {
                data = await dev.InvokeMethodAsync(method, false, item.Value, token);
            }
            catch (Exception ex)
            {
                data = new OperResult<string>(ex);
            }
            _logQueues.Enqueue(
new RpcLog()
{
    LogTime = SysDateTimeExtensions.CurrentDateTime,
    OperateMessage = data.Exception,
    IsSuccess = data.IsSuccess,
    OperateMethod = tag.OtherMethod,
    OperateObject = tag.Name,
    OperateSource = sourceDes,
    ParamJson = item.Value?.ToString(),
    ResultJson = data.Message
}
);
            if (!data.IsSuccess)
            {
                _logger.LogWarning($"执行变量[{tag.Name}]方法[{tag.OtherMethod}]失败：{data.Message}");
            }
        }
        return data;
    }

    private async Task RpcLogInsertAsync()
    {
        var db = DbContext.Db.CopyNew();
        while (true)
        {
            try
            {
                var data = _logQueues.ToListWithDequeue();
                db.InsertableWithAttr(data).ExecuteCommand();//入库
            }
            catch
            {

            }

            await Task.Delay(3000);
        }
    }
}
