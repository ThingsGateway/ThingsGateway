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

using Mapster;

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Blazor;

public abstract partial class DevicePage
{
    private List<PluginOutput> _pluginOutputs;
    private List<Device> _devices = new();
    private List<Channel> _channels = new();

    [Inject]
    private IPluginService PluginService { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _devices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).Where(a => a.PluginType == PluginType).ToList();
        _channels = (_serviceScope.ServiceProvider.GetService<IChannelService>().GetCacheList());
        //获取插件信息
        _pluginOutputs = PluginService.GetList(PluginType);

        _queryDevices = _devices.OrderBy(a => a.Name.Length).Take(20).ToList();
        _queryChannels = _channels.OrderBy(a => a.Name.Length).Take(20).ToList();
        _queryPluginOutputs = _pluginOutputs.SelectMany(a => a.Children).OrderBy(a => a.Name.Length).Take(20).ToList();

        await base.OnParametersSetAsync();
    }

    private async Task DriverValueChangedAsync(DeviceAddInput context, string pluginName)
    {
        if (pluginName.IsNullOrEmpty()) return;

        if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0 || context.PluginName != pluginName)
        {
            try
            {
                var currentDependencyProperty = GetDriverProperties(pluginName, context.Id);
                context.DevicePropertys = currentDependencyProperty;
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex);
            }
        }
        context.PluginName = pluginName;
    }

    private List<DependencyProperty> GetDriverProperties(string pluginName, long devId)
    {
        return WorkerUtil.GetWoker<CollectDeviceWorker>().GetDevicePropertys(pluginName, devId);
    }

    private async Task CopyClickAsync(IEnumerable<Device> devices)
    {
        if (!devices.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }
        var input = await PopupService.PromptAsync(AppService.I18n.T("复制设备"), AppService.I18n.T("输入复制数量")
            , a => int.TryParse(a, out var result1) ? devices.Count() * result1 > 100000 ? "不支持大批量" : true : "填入数字");
        if (int.TryParse(input, out var result))
        {
            await _serviceScope.ServiceProvider.GetService<IDeviceService>().CopyAsync(devices, result);
            await _datatable.QueryClickAsync();
            await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task RefreshClickAsync(Device? device = null)
    {
        if (device == null)
        {
            _devices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).Where(a => a.PluginType == PluginType).ToList();
            foreach (var dev in _devices)
            {
                var currentDependencyProperty = GetDriverProperties(dev.PluginName, dev.Id);
                dev.DevicePropertys = currentDependencyProperty;
            }
            await _serviceScope.ServiceProvider.GetService<IDeviceService>().EditAsync(_devices);
            await PopupService.EnqueueSnackbarAsync("刷新成功", AlertTypes.Success);
        }
        else
        {
            if (!string.IsNullOrEmpty(device.PluginName))
            {
                var currentDependencyProperty = GetDriverProperties(device.PluginName, device.Id);
                device.DevicePropertys = currentDependencyProperty;
                await PopupService.EnqueueSnackbarAsync("插件属性已更新", AlertTypes.Success);
            }
        }
    }

    private async Task ImportClickAsync()
    {
        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => _serviceScope.ServiceProvider.GetService<IDeviceService>().PreviewAsync(a));
        var import = EventCallback.Factory.Create<Dictionary<string, ImportPreviewOutputBase>>(this, value => _serviceScope.ServiceProvider.GetService<IDeviceService>().ImportAsync(value));
        var data = (bool?)await PopupService.OpenAsync(typeof(ImportExcel), new Dictionary<string, object?>()
        {
            {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        if (data == true)
        {
            await _datatable.QueryClickAsync();
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        _search.PluginType = PluginType;
        var query = _search?.Adapt<DeviceInput>();
        query.All = isAll;
        await AppService.DownFileAsync("gatewayExport/device", DateTime.Now.ToFileDateTimeFormat(), query);
    }
}