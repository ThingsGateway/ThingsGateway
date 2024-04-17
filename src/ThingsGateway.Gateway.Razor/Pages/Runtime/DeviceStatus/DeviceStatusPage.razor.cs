
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceStatusPage
{
    private IEnumerable<DriverBase>? CollectBases;
    private IEnumerable<DriverBase>? BusinessBases;
    private IEnumerable<SelectedItem>? Channels;
    private IEnumerable<SelectedItem>? CollectDevices;
    private IEnumerable<SelectedItem>? BusinessDevices;
    private IEnumerable<SelectedItem>? Plugins;

    [Inject]
    private IChannelService ChannelService { get; set; }

    [Inject]
    private IDeviceService DeviceService { get; set; }

    [Inject]
    private IPluginService PluginService { get; set; }

    protected override Task OnParametersSetAsync()
    {
        Channels = ChannelService.GetAll().BuildChannelSelectList().Concat(new List<SelectedItem>() { new SelectedItem("0", "null") });
        CollectDevices = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Collect).BuildDeviceSelectList().Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "null") });
        BusinessDevices = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Business).BuildDeviceSelectList().Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "null") });
        //获取插件信息
        Plugins = PluginService.GetList().BuildPluginSelectList().Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "null") });
        CollectBases = HostedServiceUtil.CollectDeviceHostedService?.DriverBases.Select(a => (CollectBase)a)!;
        BusinessBases = HostedServiceUtil.BusinessDeviceHostedService?.DriverBases.Select(a => (BusinessBase)a)!;
        return base.OnParametersSetAsync();
    }

    private void CollectDeviceQuery()
    {
        CollectBases = HostedServiceUtil.CollectDeviceHostedService?.DriverBases.Select(a => (CollectBase)a)!;
    }

    private void BusinessDeviceQuery()
    {
        BusinessBases = HostedServiceUtil.BusinessDeviceHostedService?.DriverBases.Select(a => (BusinessBase)a)!;
    }
}