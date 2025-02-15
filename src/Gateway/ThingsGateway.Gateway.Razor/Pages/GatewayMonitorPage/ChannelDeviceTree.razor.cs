//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using SqlSugar;

using ThingsGateway.Admin.Razor;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Razor;

public partial class ChannelDeviceTree : IDisposable
{
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

    [Parameter]
    public EventCallback<ShowTypeEnum?> ShowTypeChanged { get; set; }
    [Parameter]
    public ShowTypeEnum? ShowType { get; set; }

    private async Task OnShowTypeChanged(ShowTypeEnum? showType)
    {
        ShowType = showType;
        if (showType != null && Module != null)
            await Module!.InvokeVoidAsync("saveShowType", ShowType);
        if (ShowTypeChanged.HasDelegate)
            await ShowTypeChanged.InvokeAsync(showType);
    }
    protected override async Task InvokeInitAsync()
    {
        await base.InvokeInitAsync();
        var showType = await Module!.InvokeAsync<ShowTypeEnum>("getShowType");
        await OnShowTypeChanged(showType);
    }

    [Parameter]
    public bool AutoRestartThread { get; set; }


    [Inject]
    private MaskService MaskService { get; set; }
    private static string GetClass(ChannelDeviceTreeItem item)
    {
        if (item.TryGetChannelRuntime(out var channelRuntime))
        {
            return channelRuntime.DeviceThreadManage != null ? "enable--text" : "disabled--text";

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
    WinBoxService WinBoxService { get; set; }



    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    #region 通道

    async Task EditChannel(ContextMenuItem item, object value, ItemChangedType itemChangedType)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
        };
        PluginTypeEnum? pluginTypeEnum = null;
        Channel oneModel = null;
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            oneModel = channelRuntime.Adapt<Channel>();
            if (itemChangedType == ItemChangedType.Add)
            {
                oneModel.Id = 0;
                oneModel.Name = $"{oneModel.Name}-Copy";
            }
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            oneModel = new();
            oneModel.PluginName = pluginName;
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            oneModel = new();
            pluginTypeEnum = pluginType;
        }
        else
        {
            return;
        }

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(() =>GlobalData.ChannelRuntimeService.SaveChannelAsync(oneModel,itemChangedType));
            }},
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),false },
            {nameof(ChannelEditComponent.PluginType),  pluginTypeEnum },
        });

        await DialogService.Show(op);

    }

    async Task BatchEditChannel(ContextMenuItem item, object value)
    {

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
            ShowFooter = false,
            ShowCloseButton = false,
        };

        Channel oldModel = null;
        Channel oneModel = null;
        IEnumerable<Channel>? changedModels = null;
        IEnumerable<Channel>? models = null;

        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            await EditChannel(item, value, ItemChangedType.Update);
            return;
        }
        //批量编辑只有分类和插件名称节点
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            models = data.Where(a => a.Value.PluginName == pluginName).Select(a => a.Value);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.Adapt<Channel>();

        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            models = data.Where(a => a.Value.PluginType == pluginType).Select(a => a.Value);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.Adapt<Channel>();


        }
        else
        {
            return;
        }


        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                  await InvokeAsync(async ()=>
            {

                  await MaskService.Show(new MaskOption()
                {
                    ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                });
                });
                await Task.Run(() => GlobalData.ChannelRuntimeService.BatchEditAsync(changedModels, oldModel, oneModel));
                       await InvokeAsync(async ()=>
            {

                await MaskService.Close();
             StateHasChanged();
                });
            }},
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);

    }

    async Task DeleteCurrentChannel(ContextMenuItem item, object value)
    {
        IEnumerable<ChannelRuntime> modelIds = null;
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            modelIds = new List<ChannelRuntime> { channelRuntime };
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            modelIds = data.Where(a => a.Value.PluginName == pluginName).Select(a => a.Value);

        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            modelIds = data.Where(a => a.Value.PluginType == pluginType).Select(a => a.Value);

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
                    var data = modelIds.Select(a => a.Name).ToJsonNetString();
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

                await InvokeAsync(async () =>
                {
                    await MaskService.Show(new MaskOption()
                    {
                        ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                    });
                });
                await Task.Run(() => GlobalData.ChannelRuntimeService.DeleteChannelAsync(modelIds.Select(a => a.Id)));
                await InvokeAsync(async () =>
                {
                    await MaskService.Close();
                    StateHasChanged();
                });
            }

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }
    async Task DeleteAllChannel(ContextMenuItem item, object value)
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

                await InvokeAsync(async () =>
                {
                    await MaskService.Show(new MaskOption()
                    {
                        ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                    });
                });
                var key = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
                await Task.Run(() => GlobalData.ChannelRuntimeService.DeleteChannelAsync(key.Select(a => a.Key)));
                await InvokeAsync(async () =>
                {
                    await MaskService.Close();
                    StateHasChanged();
                });
            }

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }


    async Task ExportCurrentChannel(ContextMenuItem item, object value)
    {
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            await GatewayExportService.OnChannelExport(new() { QueryPageOptions = new(), DeviceId = channelRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            await GatewayExportService.OnChannelExport(new() { QueryPageOptions = new(), PluginName = pluginName });
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            await GatewayExportService.OnChannelExport(new ExportFilter() { QueryPageOptions = new(), PluginType = pluginType });
        }
        else
        {
            return;
        }

        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }
    async Task ExportAllChannel(ContextMenuItem item, object value)
    {
        await GatewayExportService.OnChannelExport(new() { QueryPageOptions = new() });

        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }


    async Task ImportChannel(ContextMenuItem item, object value)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = item.Text,
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
            await InvokeAsync(async () =>
            {
                await MaskService.Show(new MaskOption()
                {
                    ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                });
            });
            await Task.Run(() => GlobalData.ChannelRuntimeService.ImportChannelAsync(value));
            await InvokeAsync(async () =>
            {
                await MaskService.Close();
                StateHasChanged();
            });

        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        await DialogService.Show(op);

        //await InvokeAsync(table.QueryAsync);
    }


    #endregion

    #region 设备

    async Task EditDevice(ContextMenuItem item, object value, ItemChangedType itemChangedType)
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
        Device oneModel = null;
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            oneModel = deviceRuntime.Adapt<Device>();
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
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
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
            models = data.Select(a => a.Value).Where(a => a.ChannelId == channelRuntime.Id);
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.Adapt<Device>();
        }
        //批量编辑只有分类和插件名称节点
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            models = data.Select(a => a.Value).Where(a => a.PluginName == pluginName); ;
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.Adapt<Device>();

        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集

            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            models = data.Select(a => a.Value).Where(a => a.PluginType == pluginType); ;
            oldModel = models.FirstOrDefault();
            changedModels = models;
            oneModel = oldModel.Adapt<Device>();


        }
        else
        {
            return;
        }


        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
            {
                    await InvokeAsync(async () =>
            {
                await MaskService.Show(new MaskOption()
                {
                    ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                });
                });
                await Task.Run(() =>GlobalData.DeviceRuntimeService.BatchEditAsync(changedModels,oldModel,oneModel,AutoRestartThread));
                         await InvokeAsync(async () =>
            {
                await MaskService.Close();
                await OnClickSearch(SearchText);
                });
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);

    }

    async Task DeleteCurrentDevice(ContextMenuItem item, object value)
    {
        IEnumerable<DeviceRuntime> modelIds = null;
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            modelIds = new List<DeviceRuntime> { deviceRuntime };
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Select(a => a.Value).Where(a => a.ChannelId == channelRuntime.Id);
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Select(a => a.Value).Where(a => a.PluginName == pluginName);

        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集
            var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            modelIds = data.Select(a => a.Value).Where(a => a.PluginType == pluginType);
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
                    var data = modelIds.Select(a => a.Name).ToJsonNetString();
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

                await InvokeAsync(async () =>
                {
                    await MaskService.Show(new MaskOption()
                    {
                        ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                    });
                });
                await Task.Run(() => GlobalData.DeviceRuntimeService.DeleteDeviceAsync(modelIds.Select(a => a.Id), AutoRestartThread));
                await InvokeAsync(async () =>
                {
                    await MaskService.Close();
                    await OnClickSearch(SearchText);
                });
            }

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }

    async Task DeleteAllDevice(ContextMenuItem item, object value)
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

                await InvokeAsync(async () =>
                {
                    await MaskService.Show(new MaskOption()
                    {
                        ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                    });
                });
                var data = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);

                await Task.Run(() => GlobalData.DeviceRuntimeService.DeleteDeviceAsync(data.Select(a => a.Key), AutoRestartThread));
                await InvokeAsync(async () =>
                {
                    await MaskService.Close();
                    await OnClickSearch(SearchText);
                });
            }

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }

    async Task ExportCurrentDevice(ContextMenuItem item, object value)
    {
        if (value is not ChannelDeviceTreeItem channelDeviceTreeItem) return;

        if (channelDeviceTreeItem.TryGetDeviceRuntime(out var deviceRuntime))
        {
            await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), DeviceId = deviceRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetChannelRuntime(out var channelRuntime))
        {
            await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), ChannelId = channelRuntime.Id });
        }
        else if (channelDeviceTreeItem.TryGetPluginName(out var pluginName))
        {
            //插件名称
            await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), PluginName = pluginName });
        }
        else if (channelDeviceTreeItem.TryGetPluginType(out var pluginType))
        {
            //采集
            await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new(), PluginType = pluginType });
        }
        else
        {
            return;
        }

        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }
    async Task ExportAllDevice(ContextMenuItem item, object value)
    {
        await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() });

        // 返回 true 时自动弹出提示框
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
            OnCloseAsync = async () =>
            {
                await InvokeAsync(StateHasChanged);
            },
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => GlobalData.DeviceRuntimeService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (async value =>
        {
            await InvokeAsync(async () =>
            {

                await MaskService.Show(new MaskOption()
                {
                    ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">loading ....</span>"))
                });
            });

            await Task.Run(() => GlobalData.DeviceRuntimeService.ImportDeviceAsync(value, AutoRestartThread));
            await InvokeAsync(async () =>
            {

                await MaskService.Close();
                await OnClickSearch(SearchText);
            });

        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
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
    private ChannelDeviceTreeItem UnknownItem = new() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginType, PluginType = null };

    private TreeViewItem<ChannelDeviceTreeItem> BusinessTreeViewItem;
    protected override async Task OnInitializedAsync()
    {
        BusinessTreeViewItem = new TreeViewItem<ChannelDeviceTreeItem>(UnknownItem)
        {
            Text = GatewayLocalizer["Unknown"],
            IsActive = Value == UnknownItem,
            IsExpand = true,
        };
        ZItem = new List<TreeViewItem<ChannelDeviceTreeItem>>() {new TreeViewItem<ChannelDeviceTreeItem>(CollectItem)
        {
            Text = GatewayLocalizer["Collect"],
            IsActive = Value == CollectItem,
            IsExpand = true,
        },
        new TreeViewItem<ChannelDeviceTreeItem>(BusinessItem)
        {
            Text = GatewayLocalizer["Business"],
            IsActive = Value == BusinessItem,
            IsExpand = true,
        }};

        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);

        ZItem[0].Items = ResourceUtil.BuildTreeItemList(channels.Where(a => a.Value.IsCollect == true).Select(a => a.Value), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        ZItem[1].Items = ResourceUtil.BuildTreeItemList(channels.Where(a => a.Value.IsCollect == false).Select(a => a.Value), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        var item2 = ResourceUtil.BuildTreeItemList(channels.Where(a => a.Value.IsCollect == null).Select(a => a.Value), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
        if (item2.Count > 0)
        {
            BusinessTreeViewItem.Items = item2;
            if (ZItem.Count >= 2)
            {

            }
            else
            {
                ZItem.Add(BusinessTreeViewItem);
            }

        }
        else
        {
            if (ZItem.Count >= 2)
            {
                ZItem.Remove(BusinessTreeViewItem);
            }
            else
            {
            }
        }

        Items = ZItem;
        context = ExecutionContext.Capture();
        ChannelRuntimeDispatchService.Subscribe(Refresh);
        DeviceRuntimeDispatchService.Subscribe(Refresh);
        await base.OnInitializedAsync();
    }

    private ExecutionContext? context;

    private WaitLock WaitLock = new();

    protected override void OnInitialized()
    {
        _ = Task.Run(async () =>
        {
            while (!Disposed)
            {
                try
                {
                    await Notify();
                }
                catch
                {

                }
                finally
                {
                    await Task.Delay(5000);
                }
            }
        });
        base.OnInitialized();
    }

    private async Task Notify()
    {
        if (WaitLock.Waited) return;
        try
        {
            await WaitLock.WaitAsync();
            await Task.Delay(500);
            var current = ExecutionContext.Capture();
            try
            {
                ExecutionContext.Restore(context);
                await InvokeAsync(async () =>
                {
                    await OnClickSearch(SearchText);
                    StateHasChanged();
                });
            }
            finally
            {
                ExecutionContext.Restore(current);
            }
        }
        finally
        {
            WaitLock.Release();
        }
    }
    private async Task Refresh(DispatchEntry<DeviceRuntime> entry)
    {
        await Notify();
    }
    private async Task Refresh(DispatchEntry<ChannelRuntime> entry)
    {
        await Notify();
    }
    [Inject]
    private IDispatchService<DeviceRuntime> DeviceRuntimeDispatchService { get; set; }
    [Inject]
    private IDispatchService<ChannelRuntime> ChannelRuntimeDispatchService { get; set; }
    private string SearchText;

    private async Task<List<TreeViewItem<ChannelDeviceTreeItem>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;

        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        if (searchText.IsNullOrWhiteSpace())
        {
            var items = channels.Select(a => a.Value).WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));

            ZItem[0].Items = ResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == true), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[0].Items);
            ZItem[1].Items = ResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == false), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[1].Items);

            var item2 = ResourceUtil.BuildTreeItemList(items.Where(a => a.IsCollect == null), new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
            if (item2.Count > 0)
            {
                BusinessTreeViewItem.Items = item2;
                if (ZItem.Count >= 2)
                {

                }
                else
                {
                    ZItem.Add(BusinessTreeViewItem);
                }

            }
            else
            {
                if (ZItem.Count >= 2)
                {
                    ZItem.Remove(BusinessTreeViewItem);
                }
                else
                {
                }
            }
            Items = ZItem;
            return Items;
        }
        else
        {
            var items = channels.Select(a => a.Value).WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));
            var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
            var deviceItems = devices.Select(a => a.Value).WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));

            Dictionary<ChannelRuntime, List<DeviceRuntime>> collectChannelDevices = new();
            Dictionary<ChannelRuntime, List<DeviceRuntime>> businessChannelDevices = new();
            Dictionary<ChannelRuntime, List<DeviceRuntime>> otherChannelDevices = new();

            foreach (var item in items)
            {
                if (item.PluginType == PluginTypeEnum.Collect)
                    collectChannelDevices.Add(item, new());
                else if (item.PluginType == PluginTypeEnum.Collect)
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
            ZItem[1].Items = businessChannelDevices.BuildTreeItemList(new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem, items: ZItem[1].Items);
            var item2 = otherChannelDevices.BuildTreeItemList(new List<ChannelDeviceTreeItem> { Value }, RenderTreeItem);
            if (item2.Count > 0)
            {
                BusinessTreeViewItem.Items = item2;
                if (ZItem.Count >= 2)
                {

                }
                else
                {
                    ZItem.Add(BusinessTreeViewItem);
                }

            }
            else
            {
                if (ZItem.Count >= 2)
                {
                    ZItem.Remove(BusinessTreeViewItem);
                }
                else
                {
                }
            }

            Items = ZItem;
            return Items;
        }

    }

    private static bool ModelEqualityComparer(ChannelDeviceTreeItem x, ChannelDeviceTreeItem y) => x.Equals(y);
    private bool Disposed;
    public void Dispose()
    {
        Disposed = true;
        context?.Dispose();
        ChannelRuntimeDispatchService.UnSubscribe(Refresh);
        DeviceRuntimeDispatchService.UnSubscribe(Refresh);
        GC.SuppressFinalize(this);
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


}
