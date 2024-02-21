//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class DeviceStatus
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        base.Dispose(disposing);
    }

    [Parameter, EditorRequired]
    public DeviceWorker DeviceWorker { get; set; }

    [Parameter, EditorRequired]
    public EventCallback DeviceQuery { get; set; }

    [Parameter, EditorRequired]
    public IEnumerable<DriverBase> DriverBases { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    [Parameter, EditorRequired]
    public List<PluginOutput> PluginOutputs { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Device> Devices { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Channel> Channels { get; set; } = new();

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _queryDevices = Devices.OrderBy(a => a.Name.Length).Take(20).ToList();
        _queryChannels = Channels.OrderBy(a => a.Name.Length).Take(20).ToList();
        _queryPluginOutputs = PluginOutputs.SelectMany(a => a.Children).OrderBy(a => a.Name.Length).Take(20).ToList();

        await base.OnParametersSetAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }
}