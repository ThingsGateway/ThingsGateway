//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Razor;

[CascadingTypeParameter(nameof(TItem))]
public partial class AdminTable<TItem> where TItem : class, new()
{

    /// <inheritdoc cref="Table{TItem}.DoubleClickToEdit"/>
    [Parameter]
    public bool DoubleClickToEdit { get; set; } = false;
    /// <inheritdoc cref="Table{TItem}.OnDoubleClickCellCallback"/>
    [Parameter]
    public Func<string, TItem, object?, Task>? OnDoubleClickCellCallback { get; set; }
    /// <inheritdoc cref="Table{TItem}.OnDoubleClickRowCallback"/>
    [Parameter]
    public Func<TItem, Task>? OnDoubleClickRowCallback { get; set; }
    /// <inheritdoc cref="Table{TItem}.OnClickRowCallback"/>
    [Parameter]
    public Func<TItem, Task>? OnClickRowCallback { get; set; }


    /// <inheritdoc cref="Table{TItem}.AllowDragColumn"/>
    [Parameter]
    public bool AllowDragColumn { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.AllowResizing"/>
    [Parameter]
    public bool AllowResizing { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.AutoGenerateColumns"/>
    [Parameter]
    public bool AutoGenerateColumns { get; set; }

    /// <inheritdoc cref="Table{TItem}.AutoRefreshInterval"/>
    [Parameter]
    public int AutoRefreshInterval { get; set; }

    /// <inheritdoc cref="Table{TItem}.BeforeRowButtonTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? BeforeRowButtonTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.ClickToSelect"/>
    [Parameter]
    public bool ClickToSelect { get; set; }

    /// <summary>
    /// <inheritdoc cref="Table{TItem}.ClientTableName"/>
    /// </summary>
    [Parameter]
    public string? ClientTableName { get; set; }

    /// <inheritdoc cref="Table{TItem}.CustomerSearchModel"/>
    [Parameter]
    public ITableSearchModel? CustomerSearchModel { get; set; }

    /// <inheritdoc cref="Table{TItem}.CustomerSearchTemplate"/>
    [Parameter]
    public RenderFragment<ITableSearchModel>? CustomerSearchTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.DisableExtendDeleteButton"/>
    [Parameter]
    public bool DisableExtendDeleteButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.DisableExtendDeleteButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? DisableExtendDeleteButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.DisableExtendEditButton"/>
    [Parameter]
    public bool DisableExtendEditButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.DisableExtendEditButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? DisableExtendEditButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.EditFooterTemplate"/>
    [Parameter]
    public RenderFragment<TItem> EditFooterTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.ScrollingDialogContent"/>
    [Parameter]
    public bool ScrollingDialogContent { get; set; }

    /// <inheritdoc cref="Table{TItem}.EditTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? EditTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.ExportButtonDropdownTemplate"/>
    [Parameter]
    public RenderFragment<ITableExportContext<TItem>> ExportButtonDropdownTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.ExportButtonText"/>
    [Parameter]
    public string? ExportButtonText { get; set; } = App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Razor._Imports))["ExportButtonText"];

    /// <inheritdoc cref="Table{TItem}.ExtendButtonColumnWidth"/>
    [Parameter]
    public int ExtendButtonColumnWidth { get; set; } = 130;

    /// <inheritdoc cref="Table{TItem}.Height"/>
    [Parameter]
    public int? Height { get; set; } = null;

    /// <inheritdoc cref="Table{TItem}.IsAutoQueryFirstRender"/>
    [Parameter]
    public bool IsAutoQueryFirstRender { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.IsAutoRefresh"/>
    [Parameter]
    public bool IsAutoRefresh { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.IsFixedHeader"/>
    [Parameter]
    public bool IsFixedHeader { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.IsMultipleSelect"/>
    [Parameter]
    public bool IsMultipleSelect { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.DataService"/>
    [Parameter]
    public IDataService<TItem> DataService { get; set; }

    /// <inheritdoc cref="Table{TItem}.CreateItemCallback"/>
    [Parameter]
    public Func<TItem> CreateItemCallback { get; set; }


    /// <inheritdoc cref="Table{TItem}.IsPagination"/>
    [Parameter]
    public bool IsPagination { get; set; }

    /// <inheritdoc cref="Table{TItem}.IsTree"/>
    [Parameter]
    public bool IsTree { get; set; }

    /// <inheritdoc cref="Table{TItem}.Items"/>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <inheritdoc cref="Table{TItem}.ModelEqualityComparer"/>
    [Parameter]
    public Func<TItem, TItem, bool>? ModelEqualityComparer { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnAfterDeleteAsync"/>
    [Parameter]
    public Func<List<TItem>, Task>? OnAfterDeleteAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnAfterModifyAsync"/>
    [Parameter]
    public Func<Task>? OnAfterModifyAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnAfterSaveAsync"/>
    [Parameter]
    public Func<TItem, Task>? OnAfterSaveAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnDeleteAsync"/>
    [Parameter]
    public Func<IEnumerable<TItem>, Task<bool>>? OnDeleteAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnQueryAsync"/>
    [Parameter]
    public Func<QueryPageOptions, Task<QueryData<TItem>>>? OnQueryAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnSaveAsync"/>
    [Parameter]
    public Func<TItem, ItemChangedType, Task<bool>>? OnSaveAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnTreeExpand"/>
    [Parameter]
    public Func<TItem, Task<IEnumerable<TableTreeNode<TItem>>>>? OnTreeExpand { get; set; }

    /// <inheritdoc cref="Table{TItem}.PageItemsSource"/>
    [Parameter]
    public IEnumerable<int>? PageItemsSource { get; set; } = new int[]
    {
        20,
        50,
        100,
        200
    };

    /// <inheritdoc cref="Table{TItem}.RowButtonTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? RowButtonTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.RowHeight"/>
    [Parameter]
    public float RowHeight { get; set; } = 38;

    /// <inheritdoc cref="Table{TItem}.ScrollMode"/>
    [Parameter]
    public ScrollMode ScrollMode { get; set; }

    /// <inheritdoc cref="Table{TItem}.SearchMode"/>
    [Parameter]
    public SearchMode SearchMode { get; set; }

    /// <inheritdoc cref="Table{TItem}.SearchModel"/>
    [Parameter]
    public TItem SearchModel { get; set; }

    /// <inheritdoc cref="Table{TItem}.SearchTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? SearchTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.SelectedRows"/>
    [Parameter]
    public List<TItem>? SelectedRows { get; set; } = new List<TItem>();

    /// <inheritdoc cref="Table{TItem}.SelectedRowsChanged"/>
    [Parameter]
    public EventCallback<List<TItem>> SelectedRowsChanged { get; set; }

    /// <inheritdoc cref="Table{TItem}.SetRowClassFormatter"/>
    [Parameter]
    public Func<TItem, string?>? SetRowClassFormatter { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowAddButton"/>
    [Parameter]
    public bool? ShowAddButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowAdvancedSearch"/>
    [Parameter]
    public bool ShowAdvancedSearch { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowCardView"/>
    [Parameter]
    public bool ShowCardView { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowColumnList"/>
    [Parameter]
    public bool ShowColumnList { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowDefaultButtons"/>
    [Parameter]
    public bool ShowDefaultButtons { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowDeleteButton"/>
    [Parameter]
    public bool? ShowDeleteButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowDeleteButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowDeleteButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowEditButton"/>
    [Parameter]
    public bool? ShowEditButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowEditButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowEditButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowEmpty"/>
    [Parameter]
    public bool ShowEmpty { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowExportButton"/>
    [Parameter]
    public bool ShowExportButton { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExportCsvButton"/>
    [Parameter]
    public bool ShowExportCsvButton { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExportPdfButton"/>
    [Parameter]
    public bool ShowExportPdfButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExtendButtons"/>
    [Parameter]
    public bool ShowExtendButtons { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExtendDeleteButton"/>
    [Parameter]
    public bool? ShowExtendDeleteButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExtendDeleteButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowExtendDeleteButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExtendEditButton"/>
    [Parameter]
    public bool? ShowExtendEditButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExtendEditButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowExtendEditButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowFilterHeader"/>
    [Parameter]
    public bool ShowFilterHeader { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowLoading"/>
    [Parameter]
    public bool ShowLoading { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowMultiFilterHeader"/>
    [Parameter]
    public bool ShowMultiFilterHeader { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.FixedExtendButtonsColumn"/>
    [Parameter]
    public bool FixedExtendButtonsColumn { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.FixedMultipleColumn"/>
    [Parameter]
    public bool FixedMultipleColumn { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.FixedDetailRowHeaderColumn"/>
    [Parameter]
    public bool FixedDetailRowHeaderColumn { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.FixedLineNoColumn"/>
    [Parameter]
    public bool FixedLineNoColumn { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowRefresh"/>
    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowResetButton"/>
    [Parameter]
    public bool ShowResetButton { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowSearch"/>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowSearchButton"/>
    [Parameter]
    public bool ShowSearchButton { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowSearch"/>
    [Parameter]
    public bool ShowSearchText { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowToolbar"/>
    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.SortString"/>
    [Parameter]
    public string? SortString { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableColumns"/>
    [NotNull]
    [Parameter]
    public RenderFragment<TItem>? TableColumns { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableToolbarBeforeTemplate"/>
    [NotNull]
    [Parameter]
    public RenderFragment? TableToolbarBeforeTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableToolbarTemplate"/>
    [NotNull]
    [Parameter]
    public RenderFragment? TableToolbarTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableExtensionToolbarBeforeTemplate"/>
    [Parameter]
    public RenderFragment? TableExtensionToolbarBeforeTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableExtensionToolbarTemplate"/>
    [Parameter]
    public RenderFragment? TableExtensionToolbarTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.TreeNodeConverter"/>
    [Parameter]
    public Func<IEnumerable<TItem>, Task<IEnumerable<TableTreeNode<TItem>>>>? TreeNodeConverter { get; set; }

    /// <inheritdoc cref="Table{TItem}.EditDialogSize"/>
    [Parameter]
    public Size EditDialogSize { get; set; } = Size.ExtraExtraLarge;

    [Inject]
    [NotNull]
    private BlazorAppContext? AppContext { get; set; }

    [NotNull]
    private string? EmptyText { get; set; } = App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Razor._Imports))["EmptyText"];

    [NotNull]
    private Table<TItem>? Instance { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    [Parameter]
    public Func<Task<TItem>> OnAdd { get; set; }

    public Task<TItem> OnAddAsync()
    {
        if (OnAdd != null)
            return OnAdd();
        return Task.FromResult(new TItem());
    }

    /// <inheritdoc cref="Table{TItem}.QueryAsync(int?)"/>
    public Task QueryAsync(int? pageIndex) => Instance.QueryAsync(pageIndex);
    /// <inheritdoc cref="Table{TItem}.QueryAsync(int?)"/>
    public Task QueryAsync() => Instance.QueryAsync();

    /// <inheritdoc cref="Table{TItem}.ToggleLoading(bool)"/>
    public ValueTask ToggleLoading(bool v) => Instance.ToggleLoading(v);

    private bool AuthorizeButton(string operate)
    {
        var url = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        return AppContext.IsHasButtonWithRole(url, operate);
    }

}
