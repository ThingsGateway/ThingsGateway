using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Linq;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 驱动插件服务
/// </summary>
public class RpcCore : ISingleton
{
    private readonly ILogger<RpcCore> _logger;
    /// <summary>
    /// 全局设备信息
    /// </summary>
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private CollectDeviceHostService _collectDeviceHostService;

    private IServiceScopeFactory _scopeFactory;
    public RpcCore(ILogger<RpcCore> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        using var serviceScope = scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _collectDeviceHostService = serviceScope.ServiceProvider.GetBackgroundService<CollectDeviceHostService>();
        Task.Factory.StartNew(RpcLogInsert);
    }
    private ConcurrentQueue<RpcLog> _logQueues = new();

    private async Task RpcLogInsert()
    {
        var db = DbContext.Db.CopyNew();
        while (true)
        {
            var data = _logQueues.ToListWithDequeue();
            db.InsertableWithAttr(data).ExecuteCommand();//入库
            await Task.Delay(3000);
        }
    }
    /// <summary>
    /// 反向RPC入口方法
    /// </summary>
    /// <param name="MethodBase"></param>
    /// <param name="par"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<OperResult> InvokeDeviceMethod(string sourceName, NameVaue item, bool isBlazorWeb = false)
    {
        OperResult data = new();
        var tag = _globalCollectDeviceData.CollectVariables.FirstOrDefault(it => it.Name == item.Name);
        if (tag == null) throw new Exception("不存在变量:" + item.Name);
        if (tag.ProtectTypeEnum == ProtectTypeEnum.ReadOnly) throw new Exception("只读变量");
        var dev = _collectDeviceHostService.CollectDeviceCores.FirstOrDefault(it => it.Device.Id == tag.DeviceId);
        if (dev == null) throw new Exception("系统错误，不存在对应采集设备，请稍候重试");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.OffLine) throw new Exception("设备已离线");
        if (dev.Device.DeviceStatus == DeviceStatusEnum.Pause) throw new Exception("设备已暂停");
        if (tag.RpcWriteEnable && !isBlazorWeb) throw new Exception("不允许远程写入");
        if (tag.OtherMethod == null)
        {
            data = (await dev.InVokeWriteAsync(tag, item.Value)).Copy();

            _logQueues.Enqueue(
                new RpcLog()
                {
                    LogTime = DateTime.Now,
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
                data = await dev.InvokeMed(coreMed, item.Value);
            }
            catch (Exception ex)
            {
                data = new OperResult<string>(ex);
            }
            _logQueues.Enqueue(
new RpcLog()
{
    LogTime = DateTime.Now,
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
}
