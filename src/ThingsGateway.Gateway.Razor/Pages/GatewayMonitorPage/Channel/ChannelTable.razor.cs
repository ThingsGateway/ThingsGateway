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

public partial class ChannelTable : IDisposable
{

#if !Management
    [Parameter]
    public ChannelDeviceTreeItem SelectModel { get; set; }

    [Parameter]
    public IEnumerable<ChannelRuntime>? Items { get; set; } = Enumerable.Empty<ChannelRuntime>();

    private IEnumerable<ChannelRuntime>? _previousItemsRef;
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

    private async Task DrawerServiceShowChannelRuntimeInfo(ChannelRuntime channelRuntime) => await DrawerService.Show(new DrawerOption()
    {
        Class = "h-100",
        Width = "80%",
        Placement = Placement.Right,
        ChildContent = BootstrapDynamicComponent.CreateComponent<ChannelRuntimeInfo>(new Dictionary<string, object?>
        {
             {nameof(ChannelRuntimeInfo.ChannelRuntime), channelRuntime }
        }).Render(),
        ShowBackdrop = true,
        AllowResize = true,
        IsBackdrop = true
    });
#endif

    [Inject]
    DrawerService DrawerService { get; set; }

#if !Management
    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<ChannelRuntime> tableEditDialogOption)
    {
        tableEditDialogOption.Model = tableEditDialogOption.Model.AdaptChannelRuntime();
    }
#else
    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<ChannelRuntime> tableEditDialogOption)
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
    private Task<QueryData<ChannelRuntime>> OnQueryAsync(QueryPageOptions options)
    {
#if Management
        var data = ChannelPageService.OnChannelQueryAsync(options);

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


    [Inject]
#if Management
    ThingsGateway.Management.Application.IChannelPageService ChannelPageService { get; set; }
#else
    ThingsGateway.Gateway.Application.IChannelPageService ChannelPageService { get; set; }
#endif

    #region 修改
    private async Task Copy(IEnumerable<ChannelRuntime> channels)
    {
        var channelRuntime = channels.FirstOrDefault();
        if (channelRuntime == null)
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

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelCopyComponent.OnSave), async (int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber) =>
            {
                await Task.Run(() =>ChannelPageService.CopyChannelAsync(CopyCount,CopyChannelNamePrefix,CopyChannelNameSuffixNumber,CopyDeviceNamePrefix,CopyDeviceNameSuffixNumber,channelRuntime.Id,AutoRestartThread));
                    await InvokeAsync(table.QueryAsync);
            }},
        });

        await DialogService.Show(op);

    }

    private async Task BatchEdit(IEnumerable<Channel> changedModels)
    {
        var datas = changedModels.ToList();
        var oldModel = datas.FirstOrDefault();//默认值显示第一个
        if (oldModel == null)
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        datas = datas.AdaptListChannel();
        oldModel = oldModel.AdaptChannel();
        var oneModel = oldModel.AdaptChannel();//默认值显示第一个

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(() => ChannelPageService.BatchEditChannelAsync(datas, oldModel, oneModel,AutoRestartThread));

                   await InvokeAsync(table.QueryAsync);
            } },
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),true },
        });

        await DialogService.Show(op);

    }

    private async Task<bool> Delete(IEnumerable<Channel> channels)
    {
        try
        {
            return await Task.Run(async () => await ChannelPageService.DeleteChannelAsync(channels.Select(a => a.Id).ToList(), AutoRestartThread));
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    private async Task<bool> Save(Channel channel, ItemChangedType itemChangedType)
    {
        try
        {
            channel = channel.AdaptChannel();
            return await Task.Run(() => ChannelPageService.SaveChannelAsync(channel, itemChangedType, AutoRestartThread));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

    private Task<ChannelRuntime> OnAdd()
    {
#if !Management
        return Task.FromResult(ChannelDeviceHelpers.GetChannelModel(ItemChangedType.Add, SelectModel).AdaptChannelRuntime());
#else
        return Task.FromResult(new ChannelRuntime());
#endif
    }

    #region 导出

    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }
    private async Task ExcelExportAsync(ITableExportContext<ChannelRuntime> tableExportContext, bool all = false)
    {
        bool ret;
        if (all)
        {
            ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });
        }
        else
        {
#if !Management
            switch (SelectModel.ChannelDevicePluginType)
            {

                case ChannelDevicePluginTypeEnum.PluginName:
                    ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, PluginName = SelectModel.PluginName });
                    break;
                case ChannelDevicePluginTypeEnum.Channel:
                    ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, ChannelId = SelectModel.ChannelRuntimeId });
                    break;
                case ChannelDevicePluginTypeEnum.Device:
                    ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, DeviceId = SelectModel.DeviceRuntimeId, PluginType = SelectModel.TryGetDeviceRuntime(out var deviceRuntime) ? deviceRuntime?.PluginType : null });
                    break;
                default:
                    ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = new() });

                    break;
            }
#else
            ret = await GatewayExportService.OnChannelExport(new GatewayExportFilter() { QueryPageOptions = _option });
#endif
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    async Task ExcelChannelAsync(ITableExportContext<ChannelRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = GatewayLocalizer["ExcelChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
        };

        var option = _option;
        option.IsPage = false;
#if !Management
        var models = Items
                .WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText)).GetData(option, out var total).Cast<Channel>().ToList();

#else

        var models = await ChannelPageService.GetChannelListAsync(option, 2000);

#endif
        if (models.Count > 2000)
        {
            await ToastService.Warning("online Excel max data count 2000");
            return;
        }
        var uSheetDatas = await ChannelPageService.ExportChannelAsync(models);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                await Task.Run(async ()=>
                {
              var importData=await  ChannelPageService.ImportChannelUSheetDatasAsync(data,AutoRestartThread);

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

    private async Task ExcelImportAsync(ITableExportContext<ChannelRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = GatewayLocalizer["ImportChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () => await InvokeAsync(table.QueryAsync),
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> import = (a =>
{

    return ChannelPageService.ImportChannelAsync(a, AutoRestartThread);

});
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
        });

        //Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => GlobalData.ChannelRuntimeService.PreviewAsync(a));
        //Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (value => GlobalData.ChannelRuntimeService.ImportChannelAsync(value, AutoRestartThread));
        //op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        //{
        //     {nameof(ImportExcel.Import),import },
        //    {nameof(ImportExcel.Preview),preview },
        //});
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
                await ChannelPageService.DeleteChannelAsync(Items.Select(a => a.Id).ToList(), AutoRestartThread);
#else
                await ChannelPageService.ClearChannelAsync(AutoRestartThread);
#endif
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await InvokeAsync(table.QueryAsync);
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

}
