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
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.LinqExtension;
using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableRuntimeInfo
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
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Razor._Imports>? RazorLocalizer { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Inject]
    [NotNull]
    public DialogService? DialogService { get; set; }

    [NotNull]
    public IStringLocalizer? Localizer { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<OperDescAttribute>? OperDescLocalizer { get; set; }

    [Inject]
    [NotNull]
    public ToastService? ToastService { get; set; }

    #region row


    /// <summary>
    /// 获得指定列头固定列样式
    /// </summary>
    protected string? GetFixedCellStyleString(ITableColumn col, TableRowContext<VariableRuntime> row, int margin = 0)
    {
        string? ret = null;
        if (col.Fixed)
        {
            ret = IsTail(col, row) ? GetRightStyle(col, row, margin) : GetLeftStyle(col, row);
        }
        return ret;
    }

    private string? GetLeftStyle(ITableColumn col, TableRowContext<VariableRuntime> row)
    {
        var columns = row.Columns.ToList();
        var defaultWidth = 200;
        var width = 0;
        var start = 0;
        var index = columns.IndexOf(col);
        //if (GetFixedDetailRowHeaderColumn)
        //{
        //    width += DetailColumnWidth;
        //}
        //if (GetFixedMultipleSelectColumn)
        //{
        //    width += MultiColumnWidth;
        //}
        if (GetFixedLineNoColumn)
        {
            width += LineNoColumnWidth;
        }
        while (index > start)
        {
            var column = columns[start++];
            width += column.Width ?? defaultWidth;
        }
        return $"left: {width}px;";
    }
    private bool GetFixedLineNoColumn = false;

    private string? GetRightStyle(ITableColumn col, TableRowContext<VariableRuntime> row, int margin)
    {
        var columns = row.Columns.ToList();
        var defaultWidth = 200;
        var width = 0;
        var index = columns.IndexOf(col);

        // after
        while (index + 1 < columns.Count)
        {
            var column = columns[index++];
            width += column.Width ?? defaultWidth;
        }
        //if (ShowExtendButtons && FixedExtendButtonsColumn)
        {
            width += ExtendButtonColumnWidth;
        }

        // 如果是固定表头时增加滚动条位置
        if (IsFixedHeader && (index + 1) == columns.Count)
        {
            width += margin;
        }
        return $"right: {width}px;";
    }
    private bool IsFixedHeader = true;

    public int LineNoColumnWidth { get; set; } = 60;
    public int ExtendButtonColumnWidth { get; set; } = 220;


    private bool IsTail(ITableColumn col, TableRowContext<VariableRuntime> row)
    {
        var middle = Math.Floor(row.Columns.Count() * 1.0 / 2);
        var index = Columns.IndexOf(col);
        return middle < index;
    }

    /// <summary>
    /// 获得 Cell 文字样式
    /// </summary>
    protected string? GetCellClassString(ITableColumn col, string data, bool hasChildren, bool inCell)
    {
        bool trigger = false;
        return CssBuilder.Default("table-cell")
.AddClass(col.GetAlign().ToDescriptionString(), col.Align == Alignment.Center || col.Align == Alignment.Right)
.AddClass("green--text", data == "Online")
.AddClass("red--text", data == "Offline")
.AddClass("is-wrap", col.GetTextWrap())
.AddClass("is-ellips", col.GetTextEllipsis())
.AddClass("is-tips", col.GetShowTips())
.AddClass("is-resizable", AllowResizing)
.AddClass("is-tree", IsTree && hasChildren)
.AddClass("is-incell", inCell)
.AddClass("is-dbcell", trigger)
.AddClass(col.CssClass)
.Build();

    }

    private bool AllowResizing = true;
    private bool IsTree = false;

    /// <summary>
    /// 获得指定列头固定列样式
    /// </summary>
    protected string? GetFixedCellClassString(ITableColumn col, TableRowContext<VariableRuntime> row)
    {
        return CssBuilder.Default()
      .AddClass("fixed", col.Fixed)
      .AddClass("fixed-right", col.Fixed && IsTail(col, row))
      .AddClass("fr", IsLastColumn(col, row))
      .AddClass("fl", IsFirstColumn(col, row))
      .Build();

    }

    public List<ITableColumn> Columns => table?.Columns;

    private bool IsLastColumn(ITableColumn col, TableRowContext<VariableRuntime> row)
    {
        var ret = false;
        if (col.Fixed && !IsTail(col, row))
        {
            var index = Columns.IndexOf(col) + 1;
            ret = index < Columns.Count && Columns[index].Fixed == false;
        }
        return ret;

    }
    private bool IsFirstColumn(ITableColumn col, TableRowContext<VariableRuntime> row)
    {

        var ret = false;
        if (col.Fixed && IsTail(col, row))
        {
            // 查找前一列是否固定
            var index = Columns.IndexOf(col) - 1;
            if (index > 0)
            {
                ret = !Columns[index].Fixed;
            }
        }
        return ret;
    }

    #endregion

    #region js

    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Id, Interop, new { Method = nameof(TriggerStateChanged) });

    private Task OnColumnVisibleChanged(string name, bool visible)
    {
        _cachedFields = table.GetVisibleColumns.ToArray();
        return Task.CompletedTask;
    }
    private Task OnColumnCreating(List<ITableColumn> columns)
    {
        foreach (var column in columns)
        {
            column.OnCellRender = a =>
            {
                a.Class = $"{column.GetFieldName()}";
            };
        }
        return Task.CompletedTask;
    }

    private ITableColumn[] _cachedFields = Array.Empty<ITableColumn>();

    [JSInvokable]
    public List<List<string>> TriggerStateChanged()
    {
        try
        {
            if (table == null) return null;
            List<List<string>> ret = new();
            if (_cachedFields.Length == 0) _cachedFields = table.GetVisibleColumns.ToArray();
            foreach (var row in table.Rows)
            {
                var list = new List<string>(_cachedFields.Length);
                foreach (var col in _cachedFields)
                {
                    var fieldName = col.GetFieldName();
                    list.Add(VariableModelUtils.GetValue(row, fieldName));
                }
                ret.Add(list);
            }

            return ret;

        }
        catch (Exception)
        {
            return new();
        }
    }


    #endregion

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender || _cachedFields.Length == 0)
        {
            _cachedFields = table.GetVisibleColumns.ToArray();
        }
        base.OnAfterRender(firstRender);
    }

#if !Management
    [Parameter]
    public ChannelDeviceTreeItem SelectModel { get; set; }

    [Parameter]
    public IEnumerable<VariableRuntime>? Items { get; set; } = Enumerable.Empty<VariableRuntime>();

#endif
#if !Management

    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<VariableRuntime> tableEditDialogOption)
    {
        tableEditDialogOption.Model = tableEditDialogOption.Model.AdaptVariableRuntime();
    }
#else
    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<VariableRuntime> tableEditDialogOption)
    {
    }
#endif
    [Inject]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }
    public bool Disposed { get; set; }
    protected override async ValueTask DisposeAsync(bool disposing)
    {
#if Management
        timer.Dispose();
#endif
        Disposed = true;
        VariableRuntimeDispatchService?.UnSubscribe(Refresh);
        context?.Dispose();
        if (Module != null)
            await Module.InvokeVoidAsync("dispose", Id);

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        context = ExecutionContext.Capture();
        Localizer = App.CreateLocalizerByType(GetType());
        VariableRuntimeDispatchService.Subscribe(Refresh);

        scheduler = new SmartTriggerScheduler(Notify, TimeSpan.FromMilliseconds(1000));

#if Management
        timer = new TimerX(RunTimer, null, 2000, 2000) { Async = true };
#endif
        base.OnInitialized();
    }

    private ExecutionContext? context;

#if Management
    private void RunTimer(object? state)
    {
        scheduler.Trigger();
    }
    private TimerX timer;
#endif
    /// <summary>
    /// IntFormatter
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static Task<string> JsonFormatter(object? d)
    {
        try
        {
            var ret = "";
            if (d is TableColumnContext<VariableRuntime, object?> data && data?.Value != null)
            {
                ret = data.Value.ToSystemTextJsonString();
            }
            return Task.FromResult(ret);
        }
        catch (Exception)
        {
            return Task.FromResult("");
        }

    }
    [Inject]
    private IDispatchService<VariableRuntime> VariableRuntimeDispatchService { get; set; }
    private SmartTriggerScheduler scheduler;

    private Task Refresh(DispatchEntry<VariableRuntime> entry)
    {
        scheduler.Trigger();
        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        scheduler?.Trigger();
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

    #region 查询

    private QueryPageOptions _option = new();
    private Task<QueryData<VariableRuntime>> OnQueryAsync(QueryPageOptions options)
    {
#if Management
        var data = VariablePageService.OnVariableQueryAsync(options);

        _option = options;
        return data;
#else
        var data = Items
                .WhereIf(!string.IsNullOrWhiteSpace(options.SearchText), a => a.Name.Contains(options.SearchText))
                .GetQueryData(options);
        _option = options;
        return Task.FromResult(data);
#endif
    }

    #endregion 查询

    #region 写入变量

    private string WriteValue { get; set; }

    private async Task OnWriteVariable(VariableRuntime variableRuntime)
    {
        try
        {
            var data = await Task.Run(async () => await VariablePageService.OnWriteVariableAsync(variableRuntime.Id, WriteValue??string.Empty));
            if (!data.IsSuccess)
            {
                XTrace.WriteLine($"Write variable failed: {data.ToString()}");
                await ToastService.Warning(null, data.ErrorMessage);
            }
            else
            {
                await ToastService.Default();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    #endregion 写入变量

    #region 编辑

    private int TestVariableCount { get; set; }
    private int TestDeviceCount { get; set; }

    private string SlaveUrl { get; set; }
    private bool BusinessEnable { get; set; }

    [Inject]
    IVariablePageService VariablePageService { get; set; }

    #region 修改
    private async Task Copy(IEnumerable<Variable> variables)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["Copy"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        if (!variables.Any())
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        op.Component = BootstrapDynamicComponent.CreateComponent<VariableCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(VariableCopyComponent.OnSave), async  (int CopyCount,  string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber) =>
            {
                await Task.Run(() =>VariablePageService.CopyVariableAsync(variables.ToList(),CopyCount,CopyVariableNamePrefix,CopyVariableNameSuffixNumber,AutoRestartThread));
                await InvokeAsync(table.QueryAsync);
            }},
            {nameof(VariableCopyComponent.Model),variables },
        });

        await DialogService.Show(op);
    }

    private async Task BatchEdit(IEnumerable<Variable> variables)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        var oldModel = variables.Where(a => !a.DynamicVariable).FirstOrDefault();//默认值显示第一个
        if (oldModel == null)
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        variables = variables.Where(a => !a.DynamicVariable).AdaptListVariable();
        oldModel = oldModel.AdaptVariable();
        var model = oldModel.AdaptVariable();//默认值显示第一个
        op.Component = BootstrapDynamicComponent.CreateComponent<VariableEditComponent>(new Dictionary<string, object?>
        {
             {nameof(VariableEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(()=> VariablePageService.BatchEditVariableAsync(variables.ToList(),oldModel,model, AutoRestartThread));

                await InvokeAsync(table.QueryAsync);
            }},
            {nameof(VariableEditComponent.AutoRestartThread),AutoRestartThread },
            {nameof(VariableEditComponent.Model),model },
            {nameof(VariableEditComponent.ValidateEnable),true },
            {nameof(VariableEditComponent.BatchEditEnable),true },
        });
        await DialogService.Show(op);
    }

    private async Task<bool> Delete(IEnumerable<Variable> variables)
    {
        try
        {
            return await Task.Run(async () => await VariablePageService.DeleteVariableAsync(variables.Select(a => a.Id).ToList(), AutoRestartThread));
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    private async Task<bool> Save(Variable variable, ItemChangedType itemChangedType)
    {
        try
        {
            variable.VariablePropertyModels ??= new();
            foreach (var item in variable.VariablePropertyModels)
            {
                var result = (!PluginServiceUtil.HasDynamicProperty(item.Value.Value)) || (item.Value.ValidateForm != null && await item.Value.ValidateForm.ValidateAsync() != false);
                if (result == false)
                {
                    return false;
                }
            }

            variable.AlarmPropertys ??= new();
            if (variable.AlarmPropertysValidateForm != null && await variable.AlarmPropertysValidateForm.ValidateAsync() == false)
            {
                return false;
            }

            variable.VariablePropertys = PluginServiceUtil.SetDict(variable.VariablePropertyModels);
            variable = variable.AdaptVariable();
            return await Task.Run(() => VariablePageService.SaveVariableAsync(variable, itemChangedType, AutoRestartThread));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

    private Task<VariableRuntime> OnAdd()
    {
#if !Management
        return Task.FromResult(new VariableRuntime()
        {
            DeviceId = SelectModel?.TryGetDeviceRuntime(out var deviceRuntime) == true ? deviceRuntime?.IsCollect == true ? deviceRuntime?.Id ?? 0 : 0 : 0
        });
#else
        return Task.FromResult(new VariableRuntime());
#endif

    }

    #region 导出

    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<VariableRuntime> tableExportContext, bool all = false)
    {
        bool ret;
        if (all)
        {
            ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });
        }
        else
        {
#if !Management
            switch (SelectModel.ChannelDevicePluginType)
            {

                case ChannelDevicePluginTypeEnum.PluginName:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, PluginName = SelectModel.PluginName });
                    break;
                case ChannelDevicePluginTypeEnum.Channel:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, ChannelId = SelectModel.ChannelRuntimeId });
                    break;
                case ChannelDevicePluginTypeEnum.Device:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, DeviceId = SelectModel.DeviceRuntimeId, PluginType = SelectModel.TryGetDeviceRuntime(out var deviceRuntime) ? deviceRuntime?.PluginType : null });
                    break;
                default:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });

                    break;
            }
#else
            ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = _option });
#endif
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    async Task ExcelVariableAsync(ITableExportContext<VariableRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = GatewayLocalizer["ExcelVariable"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
#if !Management

        var models = Items
                .WhereIf(!string.IsNullOrWhiteSpace(_option.SearchText), a => a.Name.Contains(_option.SearchText)).GetData(_option, out var total).Cast<Variable>().ToList();

#else

        var models = await VariablePageService.GetVariableListAsync(_option, 2000);

#endif

        if (models.Count > 2000)
        {
            await ToastService.Warning("online Excel max data count 2000");
            return;
        }
        var uSheetDatas = await VariablePageService.ExportVariableAsync(models, _option.SortName, _option.SortOrder);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                await Task.Run(async ()=>
                {
                    await    VariablePageService.ImportVariableUSheetDatasAsync(data,AutoRestartThread);
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

    private async Task ExcelImportAsync(ITableExportContext<VariableRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = GatewayLocalizer["ImportVariable"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () => await InvokeAsync(table.QueryAsync),
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> import = (a =>
        {

            return VariablePageService.ImportVariableAsync(a, AutoRestartThread);

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

                await VariablePageService.ClearVariableAsync(AutoRestartThread);

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

    private async Task InsertTestDataAsync()
    {
        try
        {
            try
            {
                await Task.Run(() => VariablePageService.InsertTestDataAsync(TestVariableCount, TestDeviceCount, SlaveUrl, BusinessEnable, AutoRestartThread));
            }
            finally
            {
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await table.QueryAsync();
                    StateHasChanged();
                });
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }


    private async Task InsertTestDtuDataAsync()
    {
        try
        {
            try
            {
                await Task.Run(() => VariablePageService.InsertTestDtuDataAsync(TestDeviceCount, SlaveUrl, AutoRestartThread));
            }
            finally
            {
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await table.QueryAsync();
                    StateHasChanged();
                });
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }


    [Parameter]
    public bool AutoRestartThread { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Gateway.Razor._Imports>? GatewayLocalizer { get; set; }
    #endregion
}
