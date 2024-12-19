﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Razor;

/// <summary>
/// 调试页面
/// </summary>
public partial class DriverDebugPage
{
    private List<PluginOutput> PluginOutputs = new();
    private List<TreeViewItem<PluginOutput>> PluginTreeViewItems = new();

    [Inject]
    private IPluginService PluginService { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        var pluginOutputs = PluginService.GetList().Adapt<List<PluginOutput>>();

        foreach (var pluginOutput in pluginOutputs.ToList())
        {
            try
            {
                var driver = GlobalData.CollectDeviceHostedService.GetDebugUI(pluginOutput.FullName);
                if (driver == null)
                {
                    pluginOutputs.Remove(pluginOutput);
                }
            }
            catch
            {
                pluginOutputs.Remove(pluginOutput);
            }
        }

        PluginOutputs = pluginOutputs;

        var pluginGroups = PluginOutputs.GroupBy(a => a.FileName).Select(a =>
          {
              return new PluginOutput()
              {
                  Name = a.Key,
                  Children = a.ToList()
              };
          }
          ).ToList();
        PluginTreeViewItems = pluginGroups.BuildTreeItemList();
        base.OnParametersSet();
    }

    /// <inheritdoc/>
    private async Task<RenderFragment?> GetDebugUIAsync(PluginOutput plugin)
    {
        try
        {
            var pluginName = plugin.FullName;
            if (!pluginName.IsNullOrWhiteSpace())
            {
                var driver = GlobalData.CollectDeviceHostedService.GetDebugUI(pluginName);
                if (driver == null)
                {
                    await ToastService.Warning(null, Localizer["PluginUINotNull"]);
                    return null;
                }
                var debugComponent = new ThingsGatewayDynamicComponent(driver);
                var debugRender = debugComponent.Render();
                return debugRender;
            }
            return null;
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, ex.Message);
            return null;
        }
    }
    [Inject]
    [NotNull]
    private WinBoxService? WinBoxService { get; set; }

    private async Task NewPluginWinboxRender(ContextMenuItem item, object value)
    {
        var pluginOutput = (PluginOutput)value;
        if (pluginOutput.Children.Count == 0)
        {
            var debugRender = await GetDebugUIAsync(pluginOutput);
            if (debugRender != null)
            {
                var option = new WinBoxOption()
                {
                    Title = pluginOutput.Name,
                    ContentTemplate = debugRender,
                    Width = "1200px",
                    Height = "900px",
                    Top = "100px",
                    Left = "220px",
                    Background = "var(--bb-primary-color)",
                };
                await WinBoxService.Show(option);
            }

        }
    }

    private async Task NewPluginRender(ContextMenuItem item, object value)
    {
        var pluginOutput = (PluginOutput)value;
        if (pluginOutput.Children.Count == 0)
        {
            var debugRender = await GetDebugUIAsync(pluginOutput);
            if (debugRender != null)
            {
                tab.AddTab(new Dictionary<string, object?>
                {
                    [nameof(TabItem.Text)] = pluginOutput.Name,
                    [nameof(TabItem.IsActive)] = true,
                    [nameof(TabItem.ChildContent)] = debugRender
                });
            }
        }

    }
}
