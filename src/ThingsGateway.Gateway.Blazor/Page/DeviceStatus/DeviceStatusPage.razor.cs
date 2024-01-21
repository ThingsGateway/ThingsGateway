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

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class DeviceStatusPage
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        base.Dispose(disposing);
    }

    [Inject]
    private IPluginService PluginService { get; set; }

    public List<PluginOutput> PluginOutputs { get; set; } = new();

    public List<Device> Devices { get; set; } = new();

    public List<Channel> Channels { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        Devices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).ToList();
        Channels = (_serviceScope.ServiceProvider.GetService<IChannelService>().GetCacheList());
        //获取插件信息
        PluginOutputs = PluginService.GetList();
        return base.OnParametersSetAsync();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        CollectDeviceWorker = WorkerUtil.GetWoker<CollectDeviceWorker>();
        BusinessDeviceWorker = WorkerUtil.GetWoker<BusinessDeviceWorker>();
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }
}