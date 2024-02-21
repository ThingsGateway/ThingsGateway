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

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class RealAlarmPage
{
    private List<Device> _devices = new();

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _devices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).Where(a => a.PluginType == PluginTypeEnum.Collect).ToList();
        _queryDevices = _devices.OrderBy(a => a.Name.Length).Take(20).ToList();
        await base.OnParametersSetAsync();
    }
}