﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Razor;

[CascadingTypeParameter(nameof(TItem))]
public partial class AdminTable<TItem> where TItem : class, new()
{
    /// <inheritdoc cref="Table{TItem}.PageItemsSource"/>
    [Parameter]
    public IEnumerable<int>? PageItemsSource { get; set; }

    /// <inheritdoc cref="Table{TItem}.ExtendButtonColumnWidth"/>
    [Parameter]
    public int ExtendButtonColumnWidth { get; set; } = 130;

    /// <inheritdoc cref="Table{TItem}.ShowMultiFilterHeader"/>
    [Parameter]
    public bool ShowMultiFilterHeader { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowFilterHeader"/>
    [Parameter]
    public bool ShowFilterHeader { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExportPdfButton"/>
    [Parameter]
    public bool ShowExportPdfButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.SortString"/>
    [Parameter]
    public string? SortString { get; set; }

    /// <inheritdoc cref="Table{TItem}.SearchModel"/>
    [Parameter]
    public TItem SearchModel { get; set; }

    /// <inheritdoc cref="Table{TItem}.ScrollMode"/>
    [Parameter]
    public ScrollMode ScrollMode { get; set; }

    /// <inheritdoc cref="Table{TItem}.SearchMode"/>
    [Parameter]
    public SearchMode SearchMode { get; set; }

    /// <inheritdoc cref="Table{TItem}.ClickToSelect"/>
    [Parameter]
    public bool ClickToSelect { get; set; }

    /// <inheritdoc cref="Table{TItem}.SelectedRowsChanged"/>
    [Parameter]
    public EventCallback<List<TItem>> SelectedRowsChanged { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowCardView"/>
    [Parameter]
    public bool ShowCardView { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.TableColumns"/>
    [NotNull]
    [Parameter]
    public RenderFragment<TItem>? TableColumns { get; set; }

    /// <inheritdoc cref="Table{TItem}.RowButtonTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? RowButtonTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.CustomerSearchTemplate"/>
    [Parameter]
    public RenderFragment<ITableSearchModel>? CustomerSearchTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.EditTemplate"/>
    [Parameter]
    public RenderFragment<TItem>? EditTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.TableToolbarTemplate"/>
    [NotNull]
    [Parameter]
    public RenderFragment? TableToolbarTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.IsPagination"/>
    [Parameter]
    public bool IsPagination { get; set; }

    /// <inheritdoc cref="Table{TItem}.IsMultipleSelect"/>
    [Parameter]
    public bool IsMultipleSelect { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.TableSize"/>
    [Parameter]
    public TableSize TableSize { get; set; } = TableSize.Normal;

    /// <inheritdoc cref="Table{TItem}.RowHeight"/>
    [Parameter]
    public float RowHeight { get; set; } = 38;

    /// <inheritdoc cref="Table{TItem}.PageItems"/>
    [Parameter]
    public int PageItems { get; set; }

    /// <inheritdoc cref="Table{TItem}.IsFixedHeader"/>
    [Parameter]
    public bool IsFixedHeader { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.IsTree"/>
    [Parameter]
    public bool IsTree { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowToolbar"/>
    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowColumnList"/>
    [Parameter]
    public bool ShowColumnList { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowRefresh"/>
    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ExportButtonDropdownTemplate"/>
    [Parameter]
    public RenderFragment<ITableExportContext<TItem>> ExportButtonDropdownTemplate { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowEmpty"/>
    [Parameter]
    public bool ShowEmpty { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowLoading"/>
    [Parameter]
    public bool ShowLoading { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowSearch"/>
    [Parameter]
    public bool ShowSearch { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowAdvancedSearch"/>
    [Parameter]
    public bool ShowAdvancedSearch { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowResetButton"/>
    [Parameter]
    public bool ShowResetButton { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowDeleteButton"/>
    [Parameter]
    public bool? ShowDeleteButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowAddButton"/>
    [Parameter]
    public bool? ShowAddButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowEditButton"/>
    [Parameter]
    public bool? ShowEditButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowDefaultButtons"/>
    [Parameter]
    public bool ShowDefaultButtons { get; set; } = true;

    /// <inheritdoc cref="Table{TItem}.ShowExtendEditButton"/>
    [Parameter]
    public bool? ShowExtendEditButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExportButton"/>
    [Parameter]
    public bool ShowExportButton { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.Items"/>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <inheritdoc cref="Table{TItem}.AllowDragColumn"/>
    [Parameter]
    public bool AllowDragColumn { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.Height"/>
    [Parameter]
    public int? Height { get; set; } = null;

    /// <inheritdoc cref="Table{TItem}.AllowResizing"/>
    [Parameter]
    public bool AllowResizing { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExportCsvButton"/>
    [Parameter]
    public bool ShowExportCsvButton { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.ShowExtendDeleteButton"/>
    [Parameter]
    public bool? ShowExtendDeleteButton { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowExtendButtons"/>
    [Parameter]
    public bool ShowExtendButtons { get; set; } = false;

    /// <inheritdoc cref="Table{TItem}.AutoGenerateColumns"/>
    [Parameter]
    public bool AutoGenerateColumns { get; set; }

    /// <inheritdoc cref="Table{TItem}.CustomerSearchModel"/>
    [Parameter]
    public ITableSearchModel? CustomerSearchModel { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnQueryAsync"/>
    [Parameter]
    public Func<QueryPageOptions, Task<QueryData<TItem>>>? OnQueryAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnTreeExpand"/>
    [Parameter]
    public Func<TItem, Task<IEnumerable<TableTreeNode<TItem>>>>? OnTreeExpand { get; set; }

    /// <inheritdoc cref="Table{TItem}.TreeNodeConverter"/>
    [Parameter]
    public Func<IEnumerable<TItem>, Task<IEnumerable<TableTreeNode<TItem>>>>? TreeNodeConverter { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnSaveAsync"/>
    [Parameter]
    public Func<TItem, ItemChangedType, Task<bool>>? OnSaveAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnDeleteAsync"/>
    [Parameter]
    public Func<IEnumerable<TItem>, Task<bool>>? OnDeleteAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.SelectedRows"/>
    [Parameter]
    public List<TItem>? SelectedRows { get; set; } = new List<TItem>();

    /// <inheritdoc cref="Table{TItem}.ShowEditButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowEditButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.ShowDeleteButtonCallback"/>
    [Parameter]
    public Func<TItem, bool>? ShowDeleteButtonCallback { get; set; }

    /// <inheritdoc cref="Table{TItem}.OnAfterModifyAsync"/>
    [Parameter]
    public Func<Task>? OnAfterModifyAsync { get; set; }

    /// <inheritdoc cref="Table{TItem}.ModelEqualityComparer"/>
    [Parameter]
    public Func<TItem, TItem, bool>? ModelEqualityComparer { get; set; }

    [NotNull]
    private Table<TItem>? Instance { get; set; }

    /// <inheritdoc cref="Table{TItem}.ToggleLoading(bool)"/>
    public ValueTask ToggleLoading(bool v) => Instance.ToggleLoading(v);

    /// <inheritdoc cref="Table{TItem}.QueryAsync(int?)"/>
    public Task QueryAsync() => Instance.QueryAsync();

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    [Inject]
    [NotNull]
    private BlazorAppContext? AppContext { get; set; }

    private bool AuthorizeButton(string operate)
    {
        var url = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        return AppContext.IsHasButtonWithRole(url, operate);
    }

    [NotNull]
    private string? EmptyText { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
}