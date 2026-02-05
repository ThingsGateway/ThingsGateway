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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Debug;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.LinqExtension;
using ThingsGateway.Foundation.Common.StringExtension;
using ThingsGateway.SqlOrm;

using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace ThingsGateway.Management.Razor;

public partial class ManagementTree : IDisposable
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


    private static string GetClass(ManagementTreeItem item)
    {
        if (!item.Enable) return "disabled--text";



        var data = GetValue(item);

        if (data is IOnlineClient onlineClient)
        {
            if (onlineClient.Online)
            {
                return "green--text";
            }
        }

        if (item.IsServer && GetTask(item)?.Started == true)
        {
            return "green--text";
        }
        return "red--text";
    }

    [Inject]
    DialogService DialogService { get; set; }


    [Inject]
    [NotNull]
    private IManagementExportService? ManagementExportService { get; set; }


    Task EditManagementConfig(ContextMenuItem item, object value, ItemChangedType itemChangedType)
    {
        return EditManagementConfig(item.Text, value as ManagementTreeItem, itemChangedType);
    }
    async Task EditManagementConfig(string text, ManagementTreeItem managementTreeItem, ItemChangedType itemChangedType)
    {
        var oneModel = ManagementConfigHelpers.GetManagementConfigModel(itemChangedType, managementTreeItem);
        var op = new EditDialogOption<ManagementConfig>()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = text,
            ShowFooter = false,
            ShowCloseButton = false,
            Model = oneModel,
            ItemChangedType = itemChangedType,
            OnSaveAsync = async () =>
            {
                return await Task.Run(() => ManagementGlobalData.ManagementConfigService.SaveAsync(oneModel, itemChangedType));
            },
            OnEditAsync = async (a) =>
            {
                return await Task.Run(() => ManagementGlobalData.ManagementConfigService.SaveAsync(oneModel, itemChangedType));
            }
        };


        await DialogService.ShowEditDialog(op);
    }



    Task ExcelManagementConfig(ContextMenuItem item, object value)
    {
        return ExcelManagementConfig(item.Text);
    }
    async Task ExcelManagementConfig(string text)
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

        var models = await ManagementGlobalData.GetCurrentUserManagementConfigs().ConfigureAwait(false);
        var uSheetDatas = ManagementConfigServiceHelpers.ExportManagementConfig(models);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                Spinner.SetRun(true);

                await Task.Run(async ()=>
                {
              var importData=await  ManagementConfigServiceHelpers.ImportAsync(data);
                await    ManagementGlobalData.ManagementConfigService.ImportManagementConfigAsync(importData);
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

    Task DeleteManagementConfig(ContextMenuItem item, object value)
    {
        return DeleteManagementConfig(value as ManagementTreeItem);
    }
    async Task DeleteManagementConfig(ManagementTreeItem managementTreeItem)
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
     await DeleteCurrentManagementConfig(managementTreeItem);
 }));
                 builder.AddComponentParameter(5, nameof(Button.Text), Localizer["DeleteCurrentManagementConfig"].Value);

                 builder.CloseComponent();

                 builder.OpenComponent<Button>(12);
                 builder.AddAttribute(13, "class", "ms-3");
                 builder.AddComponentParameter(14, nameof(Button.Icon), "fa-solid fa-xmark");
                 builder.AddComponentParameter(15, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, async e =>
                 {
                     await op.CloseDialogAsync();
                     await DeleteAllManagementConfig();
                 }));
                 builder.AddComponentParameter(16, nameof(Button.Text), Localizer["DeleteAllManagementConfig"].Value);

                 builder.CloseComponent();

                 builder.CloseElement();
             });
        }

        await DialogService.Show(op);
    }
    async Task DeleteCurrentManagementConfig(ManagementTreeItem managementTreeItem)
    {
        IEnumerable<ManagementConfig> modelIds = Enumerable.Empty<ManagementConfig>();

        if (ManagementGlobalData.ReadOnlyManagementConfigs.TryGetValue(managementTreeItem.Name, out var managementConfig))
        {
            modelIds = new List<ManagementConfig> { managementConfig };
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

                await Task.Run(() => ManagementGlobalData.ManagementConfigService.DeleteAsync(modelIds.Select(a => a.Id)));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }
    async Task DeleteAllManagementConfig()
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
                    __builder.AddAttribute(5, "title", Localizer["AllManagementConfig"]);
                    __builder.AddContent(6, Localizer["AllManagementConfig"]);
                    __builder.CloseElement();
                    __builder.CloseElement();
                }
            };
            var ret = await SwalService.ShowModal(op);
            if (ret)
            {
                Spinner.SetRun(true);

                var key = await ManagementGlobalData.GetCurrentUserManagementConfigs().ConfigureAwait(false);
                await Task.Run(() => ManagementGlobalData.ManagementConfigService.DeleteAsync(key.Select(a => a.Id)));
                //await Notify();
                await InvokeAsync(() => Spinner.SetRun(false));
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }

    Task ExportManagementConfig(ContextMenuItem item, object value)
    {
        return ExportAllManagementConfig();
    }

    async Task ExportAllManagementConfig()
    {
        bool ret;
        ret = await ManagementExportService.OnManagementConfigExport(new() { QueryPageOptions = new() });

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    Task ImportManagementConfig(ContextMenuItem item, object value)
    {
        return ImportManagementConfig(item.Text);
    }
    async Task ImportManagementConfig(string text)
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
        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> import = (a =>
        {

            return ManagementGlobalData.ManagementConfigService.ImportManagementConfigAsync(a);

        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
        });

        await DialogService.Show(op);

        //await InvokeAsync(table.QueryAsync);
    }


    [Inject]
    SwalService SwalService { get; set; }
    [Inject]
    ToastService ToastService { get; set; }

    [Parameter]
    [NotNull]
    public IDmtpActorObject DmtpActorObject { get; set; }
    public ManagementTreeItem Item { get; set; }

    [Parameter]
    public Func<IDmtpActorObject, TouchSocket.Core.ILog, Task> DmtpActorObjectChanged { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Management.Razor.ManagementTree> Localizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports> AdminLocalizer { get; set; }

    private async Task OnTreeItemClick(TreeViewItem<ManagementTreeItem> item)
    {

        Item = item.Value;
        DmtpActorObject = GetValue(item.Value);
        if (DmtpActorObjectChanged != null)
        {
            await DmtpActorObjectChanged.Invoke(DmtpActorObject, GetTask(item.Value)?.TextLogger);
        }
    }

    private List<TreeViewItem<ManagementTreeItem>> TreeItems = new();

    private TreeViewItem<ManagementTreeItem> TreeItem;

    private Task Refresh(DispatchEntry<ManagementConfig> entry)
    {
        scheduler.Trigger();
        return Task.CompletedTask;
    }

    [Inject]
    private IDispatchService<ManagementConfig> ManagementConfigDispatchService { get; set; }

    SmartTriggerScheduler? scheduler;

    private ManagementTreeItem RootItem = new ManagementTreeItem()
    {
        Name = "Management"
    };
    protected override async Task OnInitializedAsync()
    {
        context = ExecutionContext.Capture();

        TreeItem = new(RootItem)
        {
            Text = Localizer["Management"],
            IsActive = false,
            IsExpand = true,
        };
        ManagementConfigDispatchService.Subscribe(Refresh);
        TreeItems.Add(TreeItem);

        var channels = await ManagementGlobalData.GetCurrentUserManagementConfigs().ConfigureAwait(false);


        TreeItem.Items = GetItems(null, channels);

        scheduler = new SmartTriggerScheduler(Notify, TimeSpan.FromMilliseconds(3000));

        _ = Task.Run(async () =>
        {
            while (!Disposed)
            {
                try
                {

                    var data = TreeItem.Items.Where(a => a.Value.Enable && a.Value.IsServer);
                    foreach (var item in data)
                    {
                        var task = GetTask(item.Value);
                        var hashSet = item.Items.Select(a => a.Value.Name).ToHashSet();
                        var keys = task.GetKeys().Select(a => (a.Id, a.GetIPPort())).ToHashSet();
                        foreach (var key in keys)
                        {
                            if (!hashSet.Contains(key.Id))
                            {
                                //var dmtpActorObject = task.GetClient(key);
                                ManagementTreeItem managementTreeItem = new();
                                managementTreeItem.Name = key.Id;
                                managementTreeItem.Enable = true;
                                managementTreeItem.IsServer = false;
                                managementTreeItem.ParentName = item.Value.Name;
                                managementTreeItem.Uri = key.Item2;

                                item.Items.Add(new TreeViewItem<ManagementTreeItem>(managementTreeItem)
                                {
                                    Text = key.Id,
                                    Parent = null,
                                    IsExpand = false,
                                    IsActive = true,
                                    Template = RenderTreeItem,
                                });
                            }
                        }
                        var idKeys = keys.Select(a => a.Id).ToHashSet();
                        foreach (var child in item.Items.ToList())
                        {
                            if (!idKeys.Contains(child.Value.Name))
                            {
                                item.Items.Remove(child);
                            }
                        }
                    }

                    await InvokeAsync(StateHasChanged);
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
        _ = Task.Run(async () =>
        {
            while (!Disposed)
            {
                try
                {

                    var data = GetValue(Item);
                    if (data != DmtpActorObject || data?.DmtpActor == null || (data is IOnlineClient onlineClient && onlineClient.Online))
                    {
                        DmtpActorObject = data;
                        if (DmtpActorObjectChanged != null)
                        {
                            await DmtpActorObjectChanged.Invoke(DmtpActorObject, GetTask(Item)?.TextLogger);
                        }
                    }

                }
                catch
                {
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        });
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

            DmtpActorObject = GetValue(Item);
            if (DmtpActorObjectChanged != null)
            {
                await DmtpActorObjectChanged.Invoke(DmtpActorObject, GetTask(Item)?.TextLogger);
            }
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            ExecutionContext.Restore(current);
        }

    }




    private static ManagementTask GetTask(ManagementTreeItem managementTreeItem)
    {
        if (managementTreeItem == null) return null;
        if (managementTreeItem.ParentName.IsNullOrWhiteSpace() && (!managementTreeItem.Name.IsNullOrWhiteSpace()) && ManagementGlobalData.ReadOnlyManagementConfigs.TryGetValue(managementTreeItem.Name, out var managementConfig))
        {
            return managementConfig.ManagementTask;
        }
        else if (managementTreeItem.ParentName != null && ManagementGlobalData.ReadOnlyManagementConfigs.TryGetValue(managementTreeItem.ParentName, out var managementConfig1))
        {
            return managementConfig1.ManagementTask;
        }

        return null;
    }
    private static IDmtpActorObject GetValue(ManagementTreeItem managementTreeItem)
    {
        if (managementTreeItem == null) return null;
        if (managementTreeItem.ParentName.IsNullOrWhiteSpace() && managementTreeItem.Name != null && ManagementGlobalData.ReadOnlyManagementConfigs.TryGetValue(managementTreeItem.Name, out var managementConfig) && managementConfig.IsServer == false)
        {
            return managementConfig.ManagementTask.GetClient();
        }
        else if (managementTreeItem.ParentName != null && ManagementGlobalData.ReadOnlyManagementConfigs.TryGetValue(managementTreeItem.ParentName, out var managementConfig1))
        {
            return managementConfig1.ManagementTask.GetClient(managementTreeItem.Name);
        }

        return null;
    }
    [Inject]
    DrawerService DrawerService { get; set; }

    async Task ShowLog(ManagementTreeItem item)
    {
        var managementTask = GetTask(item);
        if (managementTask?.TextLogger == null) return;
        await DrawerService.Show(new DrawerOption()
        {
            Placement = Placement.Right,
            ChildContent = new BootstrapDynamicComponent(typeof(LocalLogConsole), new Dictionary<string, object?>
            {
                [nameof(LocalLogConsole.HeightString)] = "calc(100% - 50px)",
                [nameof(LocalLogConsole.LogPath)] = managementTask.TextLogger.LogPath,
                [nameof(LocalLogConsole.LogLevel)] = managementTask.TextLogger.LogLevel,
                [nameof(LocalLogConsole.LogLevelChanged)] = EventCallback.Factory.Create<TouchSocket.Core.LogLevel>(this, v => managementTask.TextLogger.LogLevel = v!)

            }).Render(),
            AllowResize = true,
            IsBackdrop = true,
            Width = "80%"
        });
    }
    async Task StartAsync(ManagementTreeItem item)
    {
        if (item == null) return;
        var managementTask = GetTask(item);
        if (managementTask != null)
        {
            await managementTask.StartAsync(CancellationToken.None);
            DmtpActorObject = GetValue(Item);
            if (DmtpActorObjectChanged != null)
            {
                await DmtpActorObjectChanged.Invoke(DmtpActorObject, GetTask(Item)?.TextLogger);
            }
        }
    }
    async Task StopAsync(ManagementTreeItem item)
    {
        if (item == null) return;
        var managementTask = GetTask(item);
        if (managementTask != null)
        {
            await managementTask.StopAsync(CancellationToken.None);
            DmtpActorObject = GetValue(Item);
            if (DmtpActorObjectChanged != null)
            {
                await DmtpActorObjectChanged.Invoke(DmtpActorObject, GetTask(Item)?.TextLogger);
            }
        }
    }

    private string SearchText;

    private async Task<List<TreeViewItem<ManagementTreeItem>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;

        var channels = await ManagementGlobalData.GetCurrentUserManagementConfigs().ConfigureAwait(false);
        if (searchText.IsNullOrWhiteSpace())
        {
            var items = channels.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));


            TreeItem.Items = GetItems(searchText, items);
            return TreeItems;
        }
        else
        {
            var items = channels.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText));
            var devices = await ManagementGlobalData.GetCurrentUserManagementConfigs().ConfigureAwait(false);
            GetItems(searchText, devices);

            return TreeItems;
        }
    }

    private List<TreeViewItem<ManagementTreeItem>> GetItems(string searchText, IEnumerable<ManagementConfig> devices)
    {
        return devices.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText)).Select(a =>
        {
            ManagementTreeItem managementTreeItem = new();
            managementTreeItem.Name = a.Name;
            managementTreeItem.Uri = a.ServerUri;
            managementTreeItem.Enable = a.Enable;
            managementTreeItem.IsServer = a.IsServer;

            return new TreeViewItem<ManagementTreeItem>(managementTreeItem)
            {
                Text = a.Name,
                Parent = null,
                IsExpand = true,
                IsActive = true,
                Template = RenderTreeItem,
            };
        }).ToList();
    }

    private static bool ModelEqualityComparer(ManagementTreeItem x, ManagementTreeItem y)
    {
        return x.Equals(y);
    }

    private bool Disposed;
    public void Dispose()
    {
        context?.Dispose();
        Disposed = true;
        ManagementConfigDispatchService.UnSubscribe(Refresh);

    }

    ManagementTreeItem? SelectModel = default;

    Task OnBeforeShowCallback(object? item)
    {
        if (item is ManagementTreeItem managementTreeItem)
        {
            SelectModel = managementTreeItem;
        }
        else
        {
            SelectModel = null;
        }
        return Task.CompletedTask;
    }


}
