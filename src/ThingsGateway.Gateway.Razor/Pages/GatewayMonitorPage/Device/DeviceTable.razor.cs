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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Foundation.Common.LinqExtension;
using ThingsGateway.Foundation.Common.Log;
using ThingsGateway.Foundation.Common.StringExtension;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceTable : IDisposable
{

#if !Management
    [Parameter]
    public ChannelDeviceTreeItem SelectModel { get; set; }
    [Parameter]
    public IEnumerable<DeviceRuntime>? Items { get; set; } = Enumerable.Empty<DeviceRuntime>();

    private IEnumerable<DeviceRuntime>? _previousItemsRef;
    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_previousItemsRef, Items))
        {
            _previousItemsRef = Items;
            Refresh();
        }
        base.OnParametersSet();
    }

#else

    private async Task DrawerServiceShowDeviceRuntimeInfo(DeviceRuntime deviceRuntime) => await DrawerService.Show(new DrawerOption()
    {
        Class = "h-100",
        Width = "80%",
        Placement = Placement.Right,
        ChildContent = BootstrapDynamicComponent.CreateComponent<DeviceRuntimeInfo>(new Dictionary<string, object?>
        {
             {nameof(DeviceRuntimeInfo.DeviceRuntime), deviceRuntime }
        }).Render(),
        ShowBackdrop = true,
        AllowResize = true,
        IsBackdrop = true
    });
#endif

    [Inject]
    DrawerService DrawerService { get; set; }
#if !Management

    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<DeviceRuntime> tableEditDialogOption)
    {
        tableEditDialogOption.Model = tableEditDialogOption.Model.AdaptDeviceRuntime();
    }
#else
    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<DeviceRuntime> tableEditDialogOption)
    {
    }
#endif
    public bool Disposed { get; set; }


    public void Dispose()
    {
        context?.Dispose();
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        context = ExecutionContext.Capture();
        scheduler = new SmartTriggerScheduler(Notify, TimeSpan.FromMilliseconds(1000));
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private ExecutionContext? context;

    private SmartTriggerScheduler scheduler;

    private void Refresh()
    {
        scheduler.Trigger();
    }
    private async Task Notify(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (Disposed) return;
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);
            if (table != null)
                await InvokeAsync(table.QueryAsync);
        }
        finally
        {
            ExecutionContext.Restore(current);
        }

    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                var current = ExecutionContext.Capture();
                try
                {
                    ExecutionContext.Restore(context);
#if Management
                Refresh();
#else
                    await InvokeAsync(StateHasChanged);
#endif
                }
                finally
                {
                    ExecutionContext.Restore(current);
                }

            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(5000);
            }
        }
    }

    #region 查询

    private QueryPageOptions _option = new();
    private Task<QueryData<DeviceRuntime>> OnQueryAsync(QueryPageOptions options)
    {
#if Management
        var data = DevicePageService.OnDeviceQueryAsync(options);

        _option = options;
        return data;
#else

        var data = Items
                .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
                .GetQueryData(options);
        _option = options;
        return Task.FromResult(data);
#endif
    }

    #endregion 查询

    #region 编辑

    [Inject]
    IDevicePageService DevicePageService { get; set; }

    #region 修改
    private async Task Copy(IEnumerable<DeviceRuntime> devices)
    {
        var deviceRuntime = devices.FirstOrDefault();
        if (deviceRuntime == null)
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["Copy"],
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceCopyComponent.OnSave), async (int CopyCount,  string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber) =>
            {
                await Task.Run(() =>DevicePageService.CopyDeviceAsync(CopyCount,CopyDeviceNamePrefix,CopyDeviceNameSuffixNumber,deviceRuntime.Id,AutoRestartThread));
               //await Notify();

            }},
        });

        await DialogService.Show(op);

    }

    private async Task BatchEdit(IEnumerable<Device> changedModels)
    {
        var datas = changedModels.ToList();
        var oldModel = datas.FirstOrDefault();//默认值显示第一个
        if (oldModel == null)
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        datas = datas.AdaptListDevice();
        oldModel = oldModel.AdaptDevice();
        var oneModel = oldModel.AdaptDevice();//默认值显示第一个

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(() => DevicePageService.BatchEditDeviceAsync(datas, oldModel, oneModel,AutoRestartThread));

                   await InvokeAsync(table.QueryAsync);
            } },
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);

    }

    private async Task<bool> Delete(IEnumerable<Device> devices)
    {
        try
        {
            return await Task.Run(async () => await DevicePageService.DeleteDeviceAsync(devices.Select(a => a.Id).ToList(), AutoRestartThread));
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    private async Task<bool> Save(Device device, ItemChangedType itemChangedType)
    {
        try
        {
            device.DevicePropertys = PluginServiceUtil.SetDict(device.ModelValueValidateForm.Value);
            device = device.AdaptDevice();
            return await Task.Run(() => DevicePageService.SaveDeviceAsync(device, itemChangedType, AutoRestartThread));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

    private Task<DeviceRuntime> OnAdd()
    {
#if !Management
        return Task.FromResult(ChannelDeviceHelpers.GetDeviceModel(ItemChangedType.Add, SelectModel).AdaptDeviceRuntime());
#else
        return Task.FromResult(new DeviceRuntime());
#endif
    }

    #region 导出


    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<DeviceRuntime> tableExportContext, bool all = false)
    {
        bool ret;
        if (all)
        {
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });
        }
        else
        {
#if !Management
            switch (SelectModel.ChannelDevicePluginType)
            {

                case ChannelDevicePluginTypeEnum.PluginName:
                    ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, PluginName = SelectModel.PluginName });
                    break;
                case ChannelDevicePluginTypeEnum.Channel:
                    ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, ChannelId = SelectModel.ChannelRuntimeId });
                    break;
                case ChannelDevicePluginTypeEnum.Device:
                    ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, DeviceId = SelectModel.DeviceRuntimeId, PluginType = SelectModel.TryGetDeviceRuntime(out var deviceRuntime) ? deviceRuntime?.PluginType : null });
                    break;
                default:
                    ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });

                    break;
            }
#else
            ret = await GatewayExportService.OnDeviceExport(new() { QueryPageOptions = _option });
#endif
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    async Task ExcelDeviceAsync(ITableExportContext<DeviceRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = GatewayLocalizer["ExcelDevice"],
            ShowFooter = false,
            ShowCloseButton = false,
        };

        var option = _option;
        option.IsPage = false;

#if !Management
        var models = Items
                .WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText)).GetData(option, out var total).Cast<Device>().ToList();

#else

        var models = await DevicePageService.GetDeviceListAsync(option, 2000);

#endif


        if (models.Count > 2000)
        {
            await ToastService.Warning("online Excel max data count 2000");
            return;
        }
        var uSheetDatas = await DevicePageService.ExportDeviceAsync(models);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                await Task.Run(async ()=>
                {
                    await    DevicePageService.ImportDeviceUSheetDatasAsync(data,AutoRestartThread);
                })
                    ;
    }
finally
                {
                                 await InvokeAsync( async ()=>
            {
                    await table.QueryAsync();
             StateHasChanged();
                });
                }
            }},
            {nameof(USheet.Model),uSheetDatas },
        });

        await DialogService.Show(op);
    }

    private async Task ExcelImportAsync(ITableExportContext<DeviceRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = GatewayLocalizer["ImportDevice"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () => await InvokeAsync(table.QueryAsync),
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> import = (a =>
        {

            return DevicePageService.ImportDeviceAsync(a, AutoRestartThread);

        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
        });

        await DialogService.Show(op);
    }

    #endregion 导出

    #region 清空

    private async Task ClearAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
#if !Management
                await DevicePageService.DeleteDeviceAsync(Items.Select(a => a.Id).ToList(), AutoRestartThread);
#else
                await DevicePageService.ClearDeviceAsync(AutoRestartThread);
#endif

                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await table.QueryAsync();
                });
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }
    #endregion

    [Parameter]
    public bool AutoRestartThread { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Gateway.Razor._Imports>? GatewayLocalizer { get; set; }
    #endregion

}
