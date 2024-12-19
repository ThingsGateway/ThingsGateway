//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceStatusPage : IDisposable
{
    private IEnumerable<DriverBase>? BusinessBases;
    private IEnumerable<SelectedItem>? BusinessDevices;
    private IEnumerable<SelectedItem>? Channels;
    private IEnumerable<DriverBase>? CollectBases;
    private IEnumerable<SelectedItem>? CollectDevices;
    private IEnumerable<SelectedItem>? Plugins;

    [Inject]
    private IChannelService ChannelService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<DeviceRunTime>? DeviceRunTimeDispatchService { get; set; }

    [Inject]
    private IDeviceService DeviceService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<PluginOutput>? PluginDispatchService { get; set; }

    [Inject]
    private IPluginService PluginService { get; set; }

    public void Dispose()
    {
        DeviceRunTimeDispatchService.UnSubscribe(Notify);
        PluginDispatchService.UnSubscribe(Notify);
    }
    private ExecutionContext? context;

    [Inject]
    private ISysUserService SysUserService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        DataScope = await SysUserService.GetCurrentUserDataScopeAsync();
        context = ExecutionContext.Capture();
        DeviceRunTimeDispatchService.Subscribe(Notify);
        PluginDispatchService.Subscribe(Notify);
        await base.OnInitializedAsync();
    }

    private List<long>? DataScope;
    protected override async Task OnParametersSetAsync()
    {
        Channels = new List<SelectedItem>() { new SelectedItem("0", "All") }.Concat((await ChannelService.GetAllByOrgAsync()).BuildChannelSelectList());
        CollectDevices = new List<SelectedItem>() { new SelectedItem(string.Empty, "All") }.Concat((await DeviceService.GetAllByOrgAsync()).Where(a => a.PluginType == PluginTypeEnum.Collect).BuildDeviceSelectList());
        BusinessDevices = new List<SelectedItem>() { new SelectedItem(string.Empty, "All") }.Concat((await DeviceService.GetAllByOrgAsync()).Where(a => a.PluginType == PluginTypeEnum.Business).BuildDeviceSelectList());
        //获取插件信息
        Plugins = new List<SelectedItem>() { new SelectedItem(string.Empty, "All") }.Concat(PluginService.GetList().BuildPluginSelectList());
        CollectBases = GlobalData.CollectDeviceHostedService?.DriverBases
             .WhereIF(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.CurrentDevice.CreateOrgId))//在指定机构列表查询
         .WhereIF(DataScope?.Count == 0, u => u.CurrentDevice.CreateUserId == UserManager.UserId)
            .Select(a => (CollectBase)a)!;
        BusinessBases = GlobalData.BusinessDeviceHostedService?.DriverBases
                     .WhereIF(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.CurrentDevice.CreateOrgId))//在指定机构列表查询
         .WhereIF(DataScope?.Count == 0, u => u.CurrentDevice.CreateUserId == UserManager.UserId)
         .Select(a => (BusinessBase)a)!;
        await base.OnParametersSetAsync();
    }

    private void BusinessDeviceQuery()
    {
        BusinessBases = GlobalData.BusinessDeviceHostedService?.DriverBases
            .WhereIF(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.CurrentDevice.CreateOrgId))//在指定机构列表查询
         .WhereIF(DataScope?.Count == 0, u => u.CurrentDevice.CreateUserId == UserManager.UserId).Select(a => (BusinessBase)a)!;
    }

    private void CollectDeviceQuery()
    {
        CollectBases = GlobalData.CollectDeviceHostedService?.DriverBases
            .WhereIF(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.CurrentDevice.CreateOrgId))//在指定机构列表查询
         .WhereIF(DataScope?.Count == 0, u => u.CurrentDevice.CreateUserId == UserManager.UserId).Select(a => (CollectBase)a)!;
    }
    private async Task Notify()
    {
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);
            await OnParametersSetAsync();
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            ExecutionContext.Restore(current);
        }
    }

    private async Task Notify(DispatchEntry<PluginOutput> entry)
    {
        await Notify();
    }

    private async Task Notify(DispatchEntry<DeviceRunTime> entry)
    {
        await Notify();
    }
}
