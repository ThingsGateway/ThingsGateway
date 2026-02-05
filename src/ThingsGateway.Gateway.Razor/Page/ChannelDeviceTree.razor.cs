//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.LinqExtension;
using ThingsGateway.Foundation.Common.StringExtension;

namespace ThingsGateway.Gateway.Razor;

public partial class ChannelDeviceTree
{
    SpinnerComponent Spinner;
    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    public string RouteName => NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole(RouteName, operate);
    }

#if !Management
    private async Task ShowChannelRuntimeTable(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            var ChannelRuntimes = Enumerable.Repeat(channelRuntime, 1);
            await ShowChannelTable(ChannelRuntimes);

        }
        else if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {

            var ChannelRuntimes = Enumerable.Repeat(deviceRuntime.ChannelRuntime, 1);
            await ShowChannelTable(ChannelRuntimes);

        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {

            var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            var ChannelRuntimes = channels.Where(a => a.PluginName == pluginName);
            await ShowChannelTable(ChannelRuntimes);

        }
        else
        {
            var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);

            if (channelDeviceTreeItem.TryGetPluginType(out var pluginTypeEnum))
            {
                if (pluginTypeEnum != null)
                {
                    var ChannelRuntimes = channels.Where(a => a.PluginType == pluginTypeEnum);
                    await ShowChannelTable(ChannelRuntimes);

                }
                else
                {
                    var ChannelRuntimes = channels;
                    await ShowChannelTable(ChannelRuntimes);
                }
            }
        }

    }


    private async Task ShowDeviceRuntimeTable(ChannelDeviceTreeItem channelDeviceTreeItem)
    {


        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {

            var DeviceRuntimes = channelRuntime.ReadDeviceRuntimes.Select(a => a.Value);

            await ShowDeviceTable(DeviceRuntimes);

        }
        else if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {

            var DeviceRuntimes = Enumerable.Repeat(deviceRuntime, 1);
            await ShowDeviceTable(DeviceRuntimes);


        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);

            var DeviceRuntimes = devices.Where(a => a.PluginName == pluginName);
            await ShowDeviceTable(DeviceRuntimes);

        }
        else
        {

            if (channelDeviceTreeItem.TryGetPluginType(out var pluginTypeEnum))
            {
                if (pluginTypeEnum != null)
                {
                    var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);

                    var DeviceRuntimes = devices.Where(a => a.PluginType == pluginTypeEnum);
                    await ShowDeviceTable(DeviceRuntimes);

                }
                else
                {
                    var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
                    var DeviceRuntimes = devices;
                    await ShowDeviceTable(DeviceRuntimes);
                }
            }
        }


    }

    private async Task ShowLogInfo(ChannelDeviceTreeItem channelDeviceTreeItem)
    {

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            await ShowLogInfo(channelRuntime);
        }
        else if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {

            await ShowLogInfo(deviceRuntime);


        }


    }
    private async Task ShowLogInfo(ChannelRuntime channel)
    {

        var renderFragment = BootstrapDynamicComponent.CreateComponent(typeof(ChannelRuntimeInfo), new Dictionary<string, object?>()
        {
            {nameof(ChannelRuntimeInfo.ChannelRuntime),channel},
        }).Render();
        if (renderFragment != null)
        {
            var option = new WinBoxOption()
            {
                Title = Localizer["LogInfo"],
                ContentTemplate = renderFragment,
                Max = false,
                Width = "60%",
                Height = "80%",
                Top = "0%",
                Left = "10%",
                Background = "var(--bb-primary-color)",
                Overflow = true
            };
            await WinBoxService.Show(option);
        }



    }
    private async Task ShowLogInfo(DeviceRuntime device)
    {
        var renderFragment = BootstrapDynamicComponent.CreateComponent(typeof(DeviceRuntimeInfo), new Dictionary<string, object?>()
        {
            {nameof(DeviceRuntimeInfo.DeviceRuntime),device},
        }).Render();
        if (renderFragment != null)
        {
            var option = new WinBoxOption()
            {
                Title = Localizer["LogInfo"],
                ContentTemplate = renderFragment,
                Max = false,
                Width = "60%",
                Height = "80%",
                Top = "0%",
                Left = "10%",
                Background = "var(--bb-primary-color)",
                Overflow = true
            };
            await WinBoxService.Show(option);
        }


    }


    private async Task ShowChannelTable(IEnumerable<ChannelRuntime> ChannelRuntimes)
    {
        var renderFragment = BootstrapDynamicComponent.CreateComponent(typeof(ChannelTable), new Dictionary<string, object?>()
        {
            {nameof(ChannelTable.SelectModel),SelectModel},
            {nameof(ChannelTable.Items),ChannelRuntimes},
            {nameof(ChannelTable.AutoRestartThread),AutoRestartThread},
        }).Render();
        if (renderFragment != null)
        {
            var option = new WinBoxOption()
            {
                Title = Localizer["ChannelTable"],
                ContentTemplate = renderFragment,
                Max = false,
                Width = "60%",
                Height = "60%",
                Top = "0%",
                Left = "10%",
                Background = "var(--bb-primary-color)",
                Overflow = true
            };
            await WinBoxService.Show(option);
        }
    }
    private async Task ShowDeviceTable(IEnumerable<DeviceRuntime> DeviceRuntimes)
    {
        var renderFragment = BootstrapDynamicComponent.CreateComponent(typeof(DeviceTable), new Dictionary<string, object?>()
        {
            {nameof(DeviceTable.SelectModel),SelectModel},
            {nameof(DeviceTable.Items),DeviceRuntimes},
            {nameof(DeviceTable.AutoRestartThread),AutoRestartThread},
        }).Render();
        if (renderFragment != null)
        {
            var option = new WinBoxOption()
            {
                Title = Localizer["DeviceTable"],
                ContentTemplate = renderFragment,
                Max = false,
                Width = "60%",
                Height = "60%",
                Top = "0%",
                Left = "10%",
                Background = "var(--bb-primary-color)",
                Overflow = true
            };
            await WinBoxService.Show(option);
        }
    }

    [Inject]
    WinBoxService WinBoxService { get; set; }
#endif

    //[Parameter]
    //public EventCallback<ShowTypeEnum?> ShowTypeChanged { get; set; }
    //[Parameter]
    //public ShowTypeEnum? ShowType { get; set; }
    //[Inject]
    //IJSRuntime JSRuntime { get; set; }
    //private async Task OnShowTypeChanged(ShowTypeEnum? showType)
    //{
    //    ShowType = showType;
    //    if (showType != null)
    //        await JSRuntime.SetLocalStorage("showType", ShowType);
    //    if (ShowTypeChanged.HasDelegate)
    //        await ShowTypeChanged.InvokeAsync(showType);
    //}

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {
    //        var showType = await JSRuntime!.GetLocalStorage<ShowTypeEnum>("showType");
    //        await OnShowTypeChanged(showType);
    //        StateHasChanged();
    //    }
    //    await base.OnAfterRenderAsync(firstRender);
    //}


    [Parameter]
    public bool AutoRestartThread { get; set; }

    private static string GetClass(ChannelDeviceTreeItem item)
    {
        if (item.TryGetChannelRuntime(out var channelRuntime))
        {
            return channelRuntime.DeviceThreadManage != null ? " enable--text" : " disabled--text ";
        }
        else if (item.TryGetDeviceRuntime(out var deviceRuntime))
        {
            if (deviceRuntime.Driver?.DeviceThreadManage != null)
            {
                if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OnLine)
                {
                    return "green--text";
                }
                else
                {
                    return "red--text";
                }
            }
            else
            {
                return "disabled--text";
            }
        }
        return "enable--text";
    }

    private static string GetClass(ChannelDeviceTreeItemStruct item)
    {
        if (item.TryGetChannelRuntime(out var channelRuntime))
        {
            return channelRuntime.DeviceThreadManage != null ? " enable--text" : " disabled--text ";
        }
        else if (item.TryGetDeviceRuntime(out var deviceRuntime))
        {
            if (deviceRuntime.Driver?.DeviceThreadManage != null)
            {
                if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OnLine)
                {
                    return "green--text";
                }
                else
                {
                    return "red--text";
                }
            }
            else
            {
                return "disabled--text";
            }
        }
        return "enable--text";
    }

    [Inject]
    DialogService DialogService { get; set; }



    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }
    #region 通道

    Task EditChannel(ContextMenuItem item, object value, ItemChangedType itemChangedType)
    {
        return EditChannel(item.Text, value as ChannelDeviceTreeItem, itemChangedType);
    }
    async Task EditChannel(string text, ChannelDeviceTreeItem channelDeviceTreeItem, ItemChangedType itemChangedType)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        PluginTypeEnum? pluginTypeEnum = ChannelDeviceHelpers.GetPluginType(channelDeviceTreeItem);
        var oneModel = ChannelDeviceHelpers.GetChannelModel(itemChangedType, channelDeviceTreeItem);

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(() =>GlobalData.ChannelRuntimeService.SaveChannelAsync(oneModel,itemChangedType,AutoRestartThread));
               ////await Notify();
            }},
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),false },
            {nameof(ChannelEditComponent.PluginType),  pluginTypeEnum },
        });

        await DialogService.Show(op);
    }

    Task CopyChannel(ContextMenuItem item, object value)
    {
        return CopyChannel(item.Text, value as ChannelDeviceTreeItem);
    }

    async Task CopyChannel(string text, ChannelDeviceTreeItem channelDeviceTreeItem)
    {

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {

        }
        else
        {
            return;
        }

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelCopyComponent.OnSave), async (int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber) =>
            {
                await Task.Run(() =>ChannelPageService.CopyChannelAsync(CopyCount,CopyChannelNamePrefix,CopyChannelNameSuffixNumber,CopyDeviceNamePrefix,CopyDeviceNameSuffixNumber,channelRuntime.Id,AutoRestartThread));
            }},
        });

        await DialogService.Show(op);
    }
    [Inject]
#if Management
    ThingsGateway.Management.Application.IChannelPageService ChannelPageService { get; set; }
#else
    ThingsGateway.Gateway.Application.IChannelPageService ChannelPageService { get; set; }
#endif
    Task BatchEditChannel(ContextMenuItem item, object value)
    {
        return BatchEditChannel(item.Text, value as ChannelDeviceTreeItem);
    }

    async Task BatchEditChannel(string text, ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        Channel oldModel = null;
        Channel oneModel = null;
        IEnumerable<Channel>? changedModels = null;
        IEnumerable<Channel>? models = null;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            await EditChannel(text, channelDeviceTreeItem, ItemChangedType.Update);
            return;
        }
        //批量编辑只有分类和插件名称节点
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            models = data.Where(a => a.PluginName == pluginName);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.AdaptChannel();
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            models = data.Where(a => a.PluginType == pluginType);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.AdaptChannel();
        }
        else
        {
            return;
        }

        changedModels = changedModels.AdaptListChannel();
        oldModel = oldModel.AdaptChannel();
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                Spinner.SetRun(true);
                await Task.Run(() => GlobalData.ChannelRuntimeService.BatchEditChannelAsync(changedModels.ToList(), oldModel, oneModel,AutoRestartThread));

              //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            } },
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);
    }

    Task ExcelChannel(ContextMenuItem item, object value)
    {
        return ExcelChannel(item.Text);
    }
    async Task ExcelChannel(string text)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        var models = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        var uSheetDatas = ChannelServiceHelpers.ExportChannel(models);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                Spinner.SetRun(true);

                await Task.Run(async ()=>
                {
              var importData=await  ChannelServiceHelpers.ImportAsync(data);
                await    GlobalData.ChannelRuntimeService.ImportChannelAsync(importData,AutoRestartThread);
                })
                    ;
    }
finally
                {
               //await Notify();
            await InvokeAsync( ()=> Spinner.SetRun(false));
                }
            }},
            {nameof(USheet.Model),uSheetDatas },
        });

        await DialogService.Show(op);
    }

    Task DeleteChannel(ContextMenuItem item, object value)
    {
        return DeleteChannel(value as ChannelDeviceTreeItem);
    }
    async Task DeleteChannel(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        var op = new DialogOption();
        {
            op.Size = Size.ExtraSmall;
            op.ShowCloseButton = false;
            op.ShowMaximizeButton = false;
            op.ShowSaveButton = false;
            op.Title = string.Empty;
            op.BodyTemplate = new RenderFragment(builder =>
             {
                 builder.OpenElement(0, "div");
                 builder.AddAttribute(1, "class", "swal2-actions");
                 builder.OpenComponent<Button>(2);
                 builder.AddComponentParameter(3, nameof(Button.Icon), "fa-solid fa-xmark");
                 builder.AddComponentParameter(4, nameof(Button.OnClick),
 EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
 {
     await op.CloseDialogAsync();
     await DeleteCurrentChannel(channelDeviceTreeItem);
 }));
                 builder.AddComponentParameter(5, nameof(Button.Text), GatewayLocalizer["DeleteCurrentChannel"].Value);

                 builder.CloseComponent();

                 builder.OpenComponent<Button>(12);
                 builder.AddAttribute(13, "class", "ms-3");
                 builder.AddComponentParameter(14, nameof(Button.Icon), "fa-solid fa-xmark");
                 builder.AddComponentParameter(15, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                 {
                     await op.CloseDialogAsync();
                     await DeleteAllChannel();
                 }));
                 builder.AddComponentParameter(16, nameof(Button.Text), GatewayLocalizer["DeleteAllChannel"].Value);

                 builder.CloseComponent();

                 builder.CloseElement();
             });
        }

        await DialogService.Show(op);
    }
    async Task DeleteCurrentChannel(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        IEnumerable<ChannelRuntime> modelIds = null;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            modelIds = new List<ChannelRuntime> { channelRuntime };
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            modelIds = data.Where(a => a.PluginName == pluginName);
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            modelIds = data.Where(a => a.PluginType == pluginType);
        }
        else
        {
            return;
        }

        try
        {
            var op = new SwalOption()
            {
                Title = GatewayLocalizer["DeleteConfirmTitle"],
                BodyTemplate = (__builder) =>
                {
                    var data = modelIds.Select(a => a.Name).ToSystemTextJsonString();
                    __builder.OpenElement(0, "div");
                    __builder.AddAttribute(1, "class", "w-100 ");
                    __builder.OpenElement(2, "span");
                    __builder.AddAttribute(3, "class", "text-truncate px-2");
                    __builder.AddAttribute(4, "style", "display: flow;");
                    __builder.AddAttribute(5, "title", data);
                    __builder.AddContent(6, data);
                    __builder.CloseElement();
                    __builder.CloseElement();
                }
            };
            var ret = await SwalService.ShowModal(op);
            if (ret)
            {
                Spinner.SetRun(true);

                await Task.Run(() => GlobalData.ChannelRuntimeService.DeleteChannelAsync(modelIds.Select(a => a.Id).ToList(), AutoRestartThread));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }
    async Task DeleteAllChannel()
    {
        try
        {
            var op = new SwalOption()
            {
                Title = GatewayLocalizer["DeleteConfirmTitle"],
                BodyTemplate = (__builder) =>
                {
                    __builder.OpenElement(0, "div");
                    __builder.AddAttribute(1, "class", "w-100 ");
                    __builder.OpenElement(2, "span");
                    __builder.AddAttribute(3, "class", "text-truncate px-2");
                    __builder.AddAttribute(4, "style", "display: flow;");
                    __builder.AddAttribute(5, "title", GatewayLocalizer["AllChannel"]);
                    __builder.AddContent(6, GatewayLocalizer["AllChannel"]);
                    __builder.CloseElement();
                    __builder.CloseElement();
                }
            };
            var ret = await SwalService.ShowModal(op);
            if (ret)
            {
                Spinner.SetRun(true);

                var key = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
                await Task.Run(() => GlobalData.ChannelRuntimeService.DeleteChannelAsync(key.Select(a => a.Id).ToList(), AutoRestartThread));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }

    Task ExportChannel(ContextMenuItem item, object value)
    {
        return ExportChannel(value as ChannelDeviceTreeItem);
    }
    async Task ExportChannel(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        var op = new DialogOption();
        {
            op.Size = Size.ExtraSmall;
            op.ShowCloseButton = false;
            op.ShowMaximizeButton = false;
            op.ShowSaveButton = false;
            op.Title = string.Empty;
            op.BodyTemplate = new RenderFragment(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "swal2-actions");
                builder.OpenComponent<Button>(2);
                builder.AddComponentParameter(3, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(4, nameof(Button.OnClick),
EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
{
    await op.CloseDialogAsync();
    await ExportCurrentChannel(channelDeviceTreeItem);
}));
                builder.AddComponentParameter(5, nameof(Button.Text), GatewayLocalizer["ExportCurrentChannel"].Value);

                builder.CloseComponent();

                builder.OpenComponent<Button>(12);
                builder.AddAttribute(13, "class", "ms-3");
                builder.AddComponentParameter(14, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(15, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                {
                    await op.CloseDialogAsync();
                    await ExportAllChannel();
                }));
                builder.AddComponentParameter(16, nameof(Button.Text), GatewayLocalizer["ExportAllChannel"].Value);

                builder.CloseComponent();

                builder.CloseElement();
            });
        }

        await DialogService.Show(op);
    }
    async Task ExportCurrentChannel(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        bool ret;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new(), DeviceId = channelRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new(), PluginName = pluginName });
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new(), PluginType = pluginType });
        }
        else
        {
            return;
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }
    async Task ExportAllChannel()
    {
        bool ret;
        ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() });

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    Task ImportChannel(ContextMenuItem item, object value)
    {
        return ImportChannel(item.Text);
    }
    async Task ImportChannel(string text)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () =>
            {
                await InvokeAsync(StateHasChanged);
                //await InvokeAsync(table.QueryAsync);
            },
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => GlobalData.ChannelRuntimeService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (async value =>
        {
            await InvokeAsync(() => Spinner.SetRun(true));
            await Task.Run(() => GlobalData.ChannelRuntimeService.ImportChannelAsync(value, AutoRestartThread));
            //await Notify();
            await InvokeAsync(() => Spinner.SetRun(false));
        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcelConfirm>(new Dictionary<string, object?>
        {
             {nameof(ImportExcelConfirm.Import),import },
            {nameof(ImportExcelConfirm.Preview),preview },
        });
        await DialogService.Show(op);

        //await InvokeAsync(table.QueryAsync);
    }

    #endregion

    #region 设备

    async Task CopyDevice(ContextMenuItem item, object value)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
        };
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (!channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            return;
        }

        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceCopyComponent.OnSave), async (int CopyCount,  string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber) =>
            {
                await Task.Run(() =>GlobalData.DeviceRuntimeService.CopyDeviceAsync(CopyCount,CopyDeviceNamePrefix,CopyDeviceNameSuffixNumber,deviceRuntime.Id,AutoRestartThread));
               //await Notify();

            }},
        });

        await DialogService.Show(op);
    }

    Task EditDevice(ContextMenuItem item, object value, ItemChangedType itemChangedType)
    {
        return EditDevice(item.Text, value as ChannelDeviceTreeItem, itemChangedType);
    }
    async Task EditDevice(string text, ChannelDeviceTreeItem channelDeviceTreeItem, ItemChangedType itemChangedType)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
        };
        Device oneModel = null;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            oneModel = deviceRuntime.AdaptDevice();
            if (itemChangedType == ItemChangedType.Add)
            {
                oneModel.Id = 0;
                oneModel.Name = $"{oneModel.Name}-Copy";
            }
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            oneModel = new();
            oneModel.ChannelId = channelRuntime.Id;
        }
        else
        {
            return;
        }

        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
             {
                 await Task.Run(() =>GlobalData.DeviceRuntimeService.SaveDeviceAsync(oneModel,itemChangedType, AutoRestartThread));
               //await Notify();
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.AutoRestartThread),AutoRestartThread },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),false },
        });

        await DialogService.Show(op);
    }

    async Task BatchEditDevice(ContextMenuItem item, object value)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        Device oldModel = null;
        Device oneModel = null;
        IEnumerable<Device>? changedModels = null;
        IEnumerable<Device>? models = null;

        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            await EditDevice(item, value, ItemChangedType.Update);
            return;
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            models = data.Where(a => a.ChannelId == channelRuntime.Id);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.AdaptDevice();
        }
        //批量编辑只有分类和插件名称节点
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            models = data.Where(a => a.PluginName == pluginName);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.AdaptDevice();
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            models = data.Where(a => a.PluginType == pluginType);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.AdaptDevice();
        }
        else
        {
            return;
        }

        changedModels = changedModels.AdaptListDevice();
        oldModel = oldModel.AdaptDevice();
        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
            {
                    await InvokeAsync( () => Spinner.SetRun(true));
                await Task.Run(() =>GlobalData.DeviceRuntimeService.BatchEditDeviceAsync(changedModels.ToList(),oldModel,oneModel,AutoRestartThread));
                //await Notify();
                         await InvokeAsync(() => Spinner.SetRun(false));
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.AutoRestartThread),AutoRestartThread },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);
    }

    async Task ExcelDevice(ContextMenuItem item, object value)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        var models = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        var uSheetDatas = DeviceServiceHelpers.ExportDevice(models);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                Spinner.SetRun(true);

                await Task.Run(async ()=>
                {
              var importData=await  DeviceServiceHelpers.ImportAsync(data);
                await    GlobalData.DeviceRuntimeService.ImportDeviceAsync(importData,AutoRestartThread);
                })
                    ;
    }
finally
                {
                //await Notify();
                                 await InvokeAsync( ()=> Spinner.SetRun(false));
                }
            }},
            {nameof(USheet.Model),uSheetDatas },
        });

        await DialogService.Show(op);
    }

    Task DeleteDevice(ContextMenuItem item, object value)
    {
        return DeleteDevice(value as ChannelDeviceTreeItem);
    }

    async Task DeleteDevice(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        var op = new DialogOption();
        {
            op.Size = Size.ExtraSmall;
            op.ShowCloseButton = false;
            op.ShowMaximizeButton = false;
            op.ShowSaveButton = false;
            op.Title = string.Empty;
            op.BodyTemplate = new RenderFragment(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "swal2-actions");
                builder.OpenComponent<Button>(2);
                builder.AddComponentParameter(3, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(4, nameof(Button.OnClick),
EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
{
    await op.CloseDialogAsync();
    await DeleteCurrentDevice(channelDeviceTreeItem);
}));
                builder.AddComponentParameter(5, nameof(Button.Text), GatewayLocalizer["DeleteCurrentDevice"].Value);

                builder.CloseComponent();

                builder.OpenComponent<Button>(12);
                builder.AddAttribute(13, "class", "ms-3");
                builder.AddComponentParameter(14, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(15, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                {
                    await op.CloseDialogAsync();
                    await DeleteAllDevice();
                }));
                builder.AddComponentParameter(16, nameof(Button.Text), GatewayLocalizer["DeleteAllDevice"].Value);

                builder.CloseComponent();

                builder.CloseElement();
            });
        }

        await DialogService.Show(op);
    }

    async Task DeleteCurrentDevice(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        IEnumerable<DeviceRuntime> modelIds = null;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            modelIds = new List<DeviceRuntime> { deviceRuntime };
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Where(a => a.ChannelId == channelRuntime.Id);
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Where(a => a.PluginName == pluginName);
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Where(a => a.PluginType == pluginType);
        }
        else
        {
            return;
        }

        try
        {
            var op = new SwalOption()
            {
                Title = GatewayLocalizer["DeleteConfirmTitle"],
                BodyTemplate = (__builder) =>
                {
                    var data = modelIds.Select(a => a.Name).ToSystemTextJsonString();
                    __builder.OpenElement(0, "div");
                    __builder.AddAttribute(1, "class", "w-100 ");
                    __builder.OpenElement(2, "span");
                    __builder.AddAttribute(3, "class", "text-truncate px-2");
                    __builder.AddAttribute(4, "style", "display: flow;");
                    __builder.AddAttribute(5, "title", data);
                    __builder.AddContent(6, data);
                    __builder.CloseElement();
                    __builder.CloseElement();
                }
            };
            var ret = await SwalService.ShowModal(op);
            if (ret)
            {
                Spinner.SetRun(true);

                await Task.Run(() => GlobalData.DeviceRuntimeService.DeleteDeviceAsync(modelIds.Select(a => a.Id).ToList(), AutoRestartThread));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }

    async Task DeleteAllDevice()
    {
        try
        {
            var op = new SwalOption()
            {
                Title = GatewayLocalizer["DeleteConfirmTitle"],
                BodyTemplate = (__builder) =>
                {
                    __builder.OpenElement(0, "div");
                    __builder.AddAttribute(1, "class", "w-100 ");
                    __builder.OpenElement(2, "span");
                    __builder.AddAttribute(3, "class", "text-truncate px-2");
                    __builder.AddAttribute(4, "style", "display: flow;");
                    __builder.AddAttribute(5, "title", GatewayLocalizer["AllDevice"]);
                    __builder.AddContent(6, GatewayLocalizer["AllDevice"]);
                    __builder.CloseElement();
                    __builder.CloseElement();
                }
            };
            var ret = await SwalService.ShowModal(op);
            if (ret)
            {
                Spinner.SetRun(true);

                var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);

                await Task.Run(() => GlobalData.DeviceRuntimeService.DeleteDeviceAsync(data.Select(a => a.Id).ToList(), AutoRestartThread));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }

    async Task ExportDevice(ContextMenuItem item, object value)
    {
        var op = new DialogOption();
        {
            op.Size = Size.ExtraSmall;
            op.ShowCloseButton = false;
            op.ShowMaximizeButton = false;
            op.ShowSaveButton = false;
            op.Title = string.Empty;
            op.BodyTemplate = new RenderFragment(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "swal2-actions");
                builder.OpenComponent<Button>(2);
                builder.AddComponentParameter(3, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(4, nameof(Button.OnClick),
EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
{
    await op.CloseDialogAsync();
    await ExportCurrentDevice(item, value);
}));
                builder.AddComponentParameter(5, nameof(Button.Text), GatewayLocalizer["ExportCurrentDevice"].Value);

                builder.CloseComponent();

                builder.OpenComponent<Button>(12);
                builder.AddAttribute(13, "class", "ms-3");
                builder.AddComponentParameter(14, nameof(Button.Icon), "fa-solid fa-xmark");
                builder.AddComponentParameter(15, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                {
                    await op.CloseDialogAsync();
                    await ExportAllDevice(item, value);
                }));
                builder.AddComponentParameter(16, nameof(Button.Text), GatewayLocalizer["ExportAllDevice"].Value);

                builder.CloseComponent();

                builder.CloseElement();
            });
        }

        await DialogService.Show(op);
    }

    async Task ExportCurrentDevice(ContextMenuItem item, object value)
    {
        bool ret;
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), DeviceId = deviceRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), ChannelId = channelRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), PluginName = pluginName });
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), PluginType = pluginType });
        }
        else
        {
            return;
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }
    async Task ExportAllDevice(ContextMenuItem item, object value)
    {
        bool ret;
        ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() });

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    async Task ImportDevice(ContextMenuItem item, object value)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () => await InvokeAsync(StateHasChanged),
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => GlobalData.DeviceRuntimeService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (async value =>
        {
            await InvokeAsync(() => Spinner.SetRun(true));

            await Task.Run(() => GlobalData.DeviceRuntimeService.ImportDeviceAsync(value, AutoRestartThread));
            //await Notify();
            await InvokeAsync(() => Spinner.SetRun(false));
        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcelConfirm>(new Dictionary<string, object?>
        {
             {nameof(ImportExcelConfirm.Import),import },
            {nameof(ImportExcelConfirm.Preview),preview },
        });
        await DialogService.Show(op);

        //await InvokeAsync(table.QueryAsync);
    }

    #endregion

    [Inject]
    SwalService SwalService { get; set; }
    [Inject]
    ToastService ToastService { get; set; }

    [Parameter]
    [NotNull]
    public ChannelDeviceTreeItem Value { get; set; }

    [Parameter]
    public Func<ChannelDeviceTreeItem, Task> ChannelDeviceChanged { get; set; }

    [NotNull]
    private List<TreeViewItem<ChannelDeviceTreeItem>> Items { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Gateway.Razor.ChannelDeviceTree> Localizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports> AdminLocalizer { get; set; }

    private async Task OnTreeItemClick(TreeViewItem<ChannelDeviceTreeItem> item)
    {
        if (Value != item.Value)
        {
            Value = item.Value;
            if (ChannelDeviceChanged != null)
            {
                await ChannelDeviceChanged.Invoke(item.Value);
            }
        }
        else
        {
            Value = item.Value;
        }
    }

    private List<TreeViewItem<ChannelDeviceTreeItem>> ZItem;

    private ChannelDeviceTreeItem CollectItem = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = PluginTypeEnum.Collect };
    private ChannelDeviceTreeItem BusinessItem = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = PluginTypeEnum.Business };
    private ChannelDeviceTreeItem MemoryItem = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Device, DeviceRuntimeId = MemoryConst.MemoryDeviceId, Id = MemoryConst.MemoryDeviceId };
    private ChannelDeviceTreeItem UnknownItem = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = null };

    private TreeViewItem<ChannelDeviceTreeItem> UnknownTreeViewItem;

    SmartTriggerScheduler? scheduler;


    protected override async Task OnInitializedAsync()
    {
        UnknownTreeViewItem = new TreeViewItem<ChannelDeviceTreeItem>(UnknownItem)
        {
            Text = GatewayLocalizer["Unknown"],
            IsActive = false,
            IsExpand = true,
        };
        ZItem = new List<TreeViewItem<ChannelDeviceTreeItem>>() {new TreeViewItem<ChannelDeviceTreeItem>(CollectItem)
        {
            Text = GatewayLocalizer["Collect"],
            IsActive = false,
            IsExpand = true,
        },
        new TreeViewItem<ChannelDeviceTreeItem>(MemoryItem)
        {
            Text = GatewayLocalizer["Memory"],
            IsActive = false,
            IsExpand = true,
        },
        new TreeViewItem<ChannelDeviceTreeItem>(BusinessItem)
        {
            Text = GatewayLocalizer["Business"],
            IsActive = false,
            IsExpand = true,
        }};

        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);

        ZItem[0].Items = GatewayResourceUtil.BuildTreeItemList(channels.Where(a => a.IsCollect == true), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        ZItem[2].Items = GatewayResourceUtil.BuildTreeItemList(channels.Where(a => a.IsCollect == false), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        var item2 = GatewayResourceUtil.BuildTreeItemList(channels.Where(a => a.IsCollect == null), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        if (item2.Count > 0)
        {
            UnknownTreeViewItem.Items = item2;
            if (ZItem.Count >= 3)
            {
            }
            else
            {
                ZItem.Add(UnknownTreeViewItem);
            }
        }
        else
        {
            if (ZItem.Count >= 3)
            {
                ZItem.Remove(UnknownTreeViewItem);
            }
        }

        Items = ZItem.AdaptListTreeViewItemChannelDeviceTreeItem();
        ChannelRuntimeDispatchService.Subscribe(Refresh);

        context = ExecutionContext.Capture();
        scheduler = new SmartTriggerScheduler(Notify, TimeSpan.FromMilliseconds(3000));

        await base.OnInitializedAsync();
    }

    private ExecutionContext? context;


    private async Task Notify(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (Disposed) return;
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);

            await OnClickSearch(SearchText);

            Value = GetValue(Value);
            if (ChannelDeviceChanged != null)
            {
                await ChannelDeviceChanged.Invoke(Value);
            }
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            ExecutionContext.Restore(current);
        }
    }

    private static ChannelDeviceTreeItem GetValue(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        switch (channelDeviceTreeItem.ChannelDevicePluginType)
        {
            case ChannelDevicePluginTypeEnum.PluginType:
            case ChannelDevicePluginTypeEnum.PluginName:
            default:
                return channelDeviceTreeItem;
            case ChannelDevicePluginTypeEnum.Channel:
                return new ChannelDeviceTreeItem()
                {
                    ChannelRuntimeId = channelDeviceTreeItem.ChannelRuntimeId,
                    ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Channel
                };

            case ChannelDevicePluginTypeEnum.Device:
                return new ChannelDeviceTreeItem()
                {
                    DeviceRuntimeId = channelDeviceTreeItem.DeviceRuntimeId,
                    ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Device
                };
        }
    }

    private Task Refresh(DispatchEntry<ChannelRuntime> entry)
    {
        scheduler.Trigger();
        return Task.CompletedTask;
    }

    [Inject]
    private IDispatchService<ChannelRuntime> ChannelRuntimeDispatchService { get; set; }

    private string SearchText;

    private async Task<List<TreeViewItem<ChannelDeviceTreeItem>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;

        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        if (searchText.IsNullOrWhiteSpace())
        {
            var items = channels.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));

            ZItem[0].Items = GatewayResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == true), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[0].Items);
            ZItem[2].Items = GatewayResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == false), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[2].Items);

            var item2 = GatewayResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == null), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
            if (item2.Count > 0)
            {
                UnknownTreeViewItem.Items = item2;
                if (ZItem.Count >= 3)
                {
                }
                else
                {
                    ZItem.Add(UnknownTreeViewItem);
                }
            }
            else
            {
                if (ZItem.Count >= 3)
                {
                    ZItem.Remove(UnknownTreeViewItem);
                }
            }
            Items = ZItem.AdaptListTreeViewItemChannelDeviceTreeItem();
            return Items;
        }
        else
        {
            var items = channels.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));
            var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            var deviceItems = devices.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText)).ToArray();

            Dictionary<ChannelRuntime, List<DeviceRuntime>> collectChannelDevices = new();
            Dictionary<ChannelRuntime, List<DeviceRuntime>> businessChannelDevices = new();
            Dictionary<ChannelRuntime, List<DeviceRuntime>> otherChannelDevices = new();

            foreach (var item in items)
            {
                if (item.PluginType == PluginTypeEnum.Collect)
                    collectChannelDevices.Add(item, new());
                else if (item.PluginType == PluginTypeEnum.Business)
                    businessChannelDevices.Add(item, new());
                else
                    otherChannelDevices.Add(item, new());
            }
            foreach (var item in deviceItems.Where(a => a.IsCollect == true))
            {
                if (collectChannelDevices.TryGetValue(item.ChannelRuntime, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    collectChannelDevices[item.ChannelRuntime] = new List<DeviceRuntime> { item };
                }
            }
            foreach (var item in deviceItems.Where(a => a.IsCollect == false))
            {
                if (businessChannelDevices.TryGetValue(item.ChannelRuntime, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    businessChannelDevices[item.ChannelRuntime] = new List<DeviceRuntime> { item };
                }
            }
            foreach (var item in deviceItems.Where(a => a.IsCollect == null))
            {
                if (otherChannelDevices.TryGetValue(item.ChannelRuntime, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    otherChannelDevices[item.ChannelRuntime] = new List<DeviceRuntime> { item };
                }
            }

            ZItem[0].Items = collectChannelDevices.BuildTreeItemList(new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[0].Items);
            ZItem[2].Items = businessChannelDevices.BuildTreeItemList(new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[2].Items);
            var item2 = otherChannelDevices.BuildTreeItemList(new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
            if (item2.Count > 0)
            {
                UnknownTreeViewItem.Items = item2;
                if (ZItem.Count >= 3)
                {
                }
                else
                {
                    ZItem.Add(UnknownTreeViewItem);
                }
            }
            else
            {
                if (ZItem.Count >= 3)
                {
                    ZItem.Remove(UnknownTreeViewItem);
                }
            }

            Items = ZItem.AdaptListTreeViewItemChannelDeviceTreeItem();
            return Items;
        }
    }

    private static bool ModelEqualityComparer(ChannelDeviceTreeItem x, ChannelDeviceTreeItem y)
    {
        return x.Equals(y);
    }

    private bool Disposed;

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        context?.Dispose();
        Disposed = true;
        ChannelRuntimeDispatchService?.UnSubscribe(Refresh);

        if (Module != null)
            await Module.InvokeVoidAsync("dispose", Id);

        await base.DisposeAsync(disposing);
    }

    ChannelDeviceTreeItem? SelectModel = default;

    Task OnBeforeShowCallback(object? item)
    {
        if (item is ChannelDeviceTreeItem channelDeviceTreeItem)
        {
            SelectModel = channelDeviceTreeItem;
        }
        else
        {
            SelectModel = null;
        }
        return Task.CompletedTask;
    }

    #region js

    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Id, Interop, new { Method = nameof(TriggerStateChanged) });



    [JSInvokable]
    public List<string> TriggerStateChanged(List<string> jsstring)
    {
        List<string> ret = new(jsstring.Count);
        foreach (var str in jsstring)
        {
            var item = ChannelDeviceTreeItemStruct.FromJSString(str);
            ret.Add(GetClass(item));
        }
        return ret;
    }

    private static string GetId(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        return ChannelDeviceTreeItem.ToJSString(channelDeviceTreeItem);
    }

    #endregion
}
