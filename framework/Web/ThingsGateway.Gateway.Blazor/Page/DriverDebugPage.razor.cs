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

using BlazorComponent;

using Furion;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Gateway.Blazor;
/// <summary>
/// 调试页面
/// </summary>
public partial class DriverDebugPage
{
    private BootstrapDynamicComponent _importComponent;
    private object _importRef;
    private RenderFragment _importRender;
    string _searchName;
    List<DriverPluginCategory> DriverPlugins;
    bool IsShowTreeView = true;
    PluginDebugUIInput SearchModel { get; set; } = new();

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        var driverPlugins = App.GetService<DriverPluginService>().GetDriverPluginChildrenList();

        foreach (var deviceGroup in driverPlugins.ToArray())
        {
            foreach (var item in (deviceGroup.Children ?? new()).ToArray())
            {
                var driver = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>().GetDebugUI(item.Id);
                if (driver == null)
                {
                    deviceGroup.Children.Remove(item);
                }
            }
            if (deviceGroup.Children.Count == 0)
            {
                driverPlugins.Remove(deviceGroup);
            }
        }

        DriverPlugins = driverPlugins;
        base.OnParametersSet();
    }

    /// <inheritdoc/>
    async Task ImportVaiableAsync(long driverId)
    {
        var driver = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>().GetDebugUI(driverId);
        if (driver == null)
        {
            await PopupService.EnqueueSnackbarAsync("插件未实现调试页面", AlertTypes.Warning);
            return;
        }

        _importComponent = new BootstrapDynamicComponent(driver);
        _importRender = _importComponent.Render(a => _importRef = a);
    }

    class PluginDebugUIInput
    {
        public long PluginId { get; set; }
        public string PluginName { get; set; }
    }
}