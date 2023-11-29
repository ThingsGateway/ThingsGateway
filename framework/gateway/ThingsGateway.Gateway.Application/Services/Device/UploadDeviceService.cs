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

using Mapster;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Gateway.Application;

[Injection(Proxy = typeof(OperDispatchProxy))]
public class UploadDeviceService : DeviceService<Device>, ITransient, IUploadDeviceService
{
    protected override string DeviceSheetName => ExportHelpers.UploadDeviceSheetName;
    public UploadDeviceService(IServiceScopeFactory serviceScopeFactory, IFileService fileService) : base(serviceScopeFactory, fileService)
    {
    }
    /// <inheritdoc/>
    public IEnumerable<DeviceRunTime> GetDeviceRuntime(long devId = 0)
    {
        if (devId == 0)
        {
            var devices = GetCacheList(false).Where(a => a.Enable).ToList();
            var runtime = devices.Adapt<List<DeviceRunTime>>().ToDictionary(a => a.Id);
            return runtime.Values.ToList();
        }
        else
        {
            var device = GetCacheList(false).Where(a => a.Enable).ToList().FirstOrDefault(it => it.Id == devId);
            var runtime = device.Adapt<CollectDeviceRunTime>();
            return new List<DeviceRunTime>() { runtime };
        }
    }
    /// <summary>
    /// 编辑多个设备，不检测名称重复
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [OperDesc("编辑上传设备", IsRecordPar = false)]
    public async Task EditsAsync(List<Device> input)
    {
        if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
            RemoveCache();
    }
}