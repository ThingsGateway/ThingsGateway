﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 调试页面
/// </summary>
public partial class DriverDebugPage
{
    private BootstrapDynamicComponent _debugComponent;
    private RenderFragment _debugRender;
    private List<PluginOutput> _driverPlugins;
    private object _importRef;
    private bool _isShowTreeView = true;
    private string _searchName;

    [Inject]
    private IPluginService PluginService { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        var driverPlugins = PluginService.GetList();

        foreach (var deviceGroup in driverPlugins.ToArray())
        {
            foreach (var item in (deviceGroup.Children ?? new()).ToArray())
            {
                try
                {
                    var driver = WorkerUtil.GetWoker<CollectDeviceWorker>().GetDebugUI(item.FullName);
                    if (driver == null)
                    {
                        deviceGroup.Children.Remove(item);
                    }
                }
                catch
                {
                    deviceGroup.Children.Remove(item);
                }
            }
            if (deviceGroup.Children.Count == 0)
            {
                driverPlugins.Remove(deviceGroup);
            }
        }

        _driverPlugins = driverPlugins;
        base.OnParametersSet();
    }

    /// <inheritdoc/>
    private async Task GetDebugUIAsync(string pluginName)
    {
        try
        {
            if (!pluginName.IsNullOrEmpty())
            {
                var driver = WorkerUtil.GetWoker<CollectDeviceWorker>().GetDebugUI(pluginName);
                if (driver == null)
                {
                    await PopupService.EnqueueSnackbarAsync("插件未实现调试页面", AlertTypes.Warning);
                    return;
                }
                _debugComponent = new BootstrapDynamicComponent(driver);
                _debugRender = _debugComponent.Render(a => _importRef = a);
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex);
        }
    }
}