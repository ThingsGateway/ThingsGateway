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

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 变量写入值服务
/// </summary>
public class RpcSingletonService : ISingleton
{
    private readonly ILogger<RpcSingletonService> _logger;
    private CollectDeviceWorker _collectDeviceHostService;

    /// <summary>
    /// 全局设备信息
    /// </summary>
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private ConcurrentQueue<RpcLog> _logQueues = new();
    private IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="RpcSingletonService"/>
    public RpcSingletonService(ILogger<RpcSingletonService> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var serviceScope = scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();
        Task.Factory.StartNew(RpcLogInsertAsync);
    }
    /// <summary>
    /// 反向RPC入口方法
    /// </summary>
    /// <exception cref="Exception"></exception>
    public async Task<OperResult> InvokeDeviceMethodAsync(string sourceName, NameValue item, CancellationToken cancellationToken, bool isBlazorWeb = false)
    {
        OperResult data = new();
        var tag = _globalCollectDeviceData.CollectVariables.FirstOrDefault(it => it.Name == item.Name);
        if (tag == null) return new OperResult("不存在变量:" + item.Name);
        if (tag.ProtectTypeEnum == ProtectTypeEnum.ReadOnly) return new OperResult("只读变量");
        var dev = _collectDeviceHostService.CollectDeviceCores.FirstOrDefault(it => it.Device.Id == tag.DeviceId);
        if (dev == null) return new OperResult("系统错误，不存在对应采集设备，请稍候重试");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.OffLine) return new OperResult("设备已离线");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.Pause) return new OperResult("设备已暂停");
        if (!tag.RpcWriteEnable && !isBlazorWeb) return new OperResult("不允许远程写入");
        if (tag.OtherMethod.IsNullOrEmpty())
        {
            data = (await dev.InVokeWriteAsync(tag, item.Value, cancellationToken)).Copy();

            _logQueues.Enqueue(
                new RpcLog()
                {
                    LogTime = DateTime.UtcNow,
                    OperateMessage = data.Exception,
                    IsSuccess = data.IsSuccess,
                    OperateMethod = "写入变量",
                    OperateObject = tag.Name,
                    OperateSource = sourceName,
                    ParamJson = item.Value?.ToString(),
                    ResultJson = data.Message
                }
                );
        }
        else
        {
            var med = dev.Methods.FirstOrDefault(it => it.Name == tag.OtherMethod);
            var coreMed = new Method(med);
            try
            {
                data = await dev.InvokeMedAsync(coreMed, item.Value);
            }
            catch (Exception ex)
            {
                data = new OperResult<string>(ex);
            }
            _logQueues.Enqueue(
new RpcLog()
{
    LogTime = DateTime.UtcNow,
    OperateMessage = data.Exception,
    IsSuccess = data.IsSuccess,
    OperateMethod = tag.OtherMethod,
    OperateObject = tag.Name,
    OperateSource = sourceName,
    ParamJson = item.Value?.ToString(),
    ResultJson = data.Message
}
);

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
