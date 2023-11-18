#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using BlazorComponent;

using Mapster;

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using System.Reflection;

namespace ThingsGateway.Components;
/// <summary>
/// 通用表格
/// </summary>
/// <typeparam name="TItem"></typeparam>
/// <typeparam name="SearchItem"></typeparam>
/// <typeparam name="AddItem"></typeparam>
/// <typeparam name="EditItem"></typeparam>
public partial class AppDataTable<TItem, SearchItem, AddItem, EditItem> : IAppDataTable
    where TItem : IPrimaryIdEntity, new()
    where SearchItem : IBasePageInput, new()
    where AddItem : class, new()
    where EditItem : class, new()
{

    private MDataTable<TItem> _table;

    private Dictionary<string, string> DetailModelPairs = new();

    /// <summary>
    /// Width
    /// </summary>
    [Parameter]
    public StringNumber AddWidth { get; set; } = 600;
    /// <summary>
    /// Width
    /// </summary>
    [Parameter]
    public StringNumber EditWidth { get; set; } = 600;
    /// <summary>
    /// Width
    /// </summary>
    [Parameter]
    public StringNumber DetailWidth { get; set; } = 600;
    /// <summary>
    /// 添加项委托
    /// </summary>
    [Parameter]
    public Func<AddItem, Task> AddCallAsync { get; set; }

    /// <summary>
    /// 获得/设置 添加模板
    /// </summary>
    [Parameter]
    public RenderFragment<AddItem> AddTemplate { get; set; }

    /// <summary>
    /// MSheet.Class
    /// </summary>
    [Parameter]
    public string ClassString { get; set; }
    /// <summary>
    /// MSheet.Style
    /// </summary>
    [Parameter]
    public string StyleString { get; set; }
    /// <summary>
    /// 删除项委托
    /// </summary>
    [Parameter]
    public Func<IEnumerable<TItem>, Task> DeleteCallAsync { get; set; }
    /// <summary>
    /// 表格紧凑
    /// </summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// 编辑项委托
    /// </summary>
    [Parameter]
    public Func<EditItem, Task> EditCallAsync { get; set; }

    /// <summary>
    /// 获得/设置 编辑模板
    /// </summary>
    [Parameter]
    public RenderFragment<EditItem> EditTemplate { get; set; }
    /// <summary>
    /// 获得/设置 详情模板
    /// </summary>
    [Parameter]
    public RenderFragment<(DataTableHeader<TItem>, string)> Detailemplate { get; set; }

    /// <summary>
    /// 表头过滤，返回DataTableHeader列表，传输参数已包含全部初始表头与表头标题
    /// </summary>
    [Parameter]
    public Action<List<DataTableHeader<TItem>>> FilterHeaders { get; set; }

    /// <summary>
    /// 表头过滤之后执行的方法，返回Filter值，ture则显示，false则隐藏
    /// </summary>
    [Parameter]
    public Action<List<Filters>> Filters { get; set; }

    /// <summary>
    /// 获得/设置 Table Header 模板
    /// </summary>
    [Parameter]
    public RenderFragment<DataTableHeader> HeaderTemplate { get; set; }

    /// <summary>
    /// 右侧操作栏以菜单形式显示
    /// </summary>
    [Parameter]
    public bool IsMenuOperTemplate { get; set; } = true;
    /// <summary>
    /// 是否分页
    /// </summary>
    [Parameter]
    public bool IsPage { get; set; } = true;
    /// <summary>
    /// 是否显示添加按钮
    /// </summary>
    [Parameter]
    public bool IsShowAddButton { get; set; }

    /// <summary>
    /// 是否显示清空搜索
    /// </summary>
    [Parameter]
    public bool IsShowClearSearch { get; set; } = true;
    /// <summary>
    /// 是否显示删除按钮
    /// </summary>
    [Parameter]
    public bool IsShowDeleteButton { get; set; }

    /// <summary>
    /// 是否显示详情按钮
    /// </summary>
    [Parameter]
    public bool IsShowDetailButton { get; set; }

    /// <summary>
    /// 是否显示编辑按钮
    /// </summary>
    [Parameter]
    public bool IsShowEditButton { get; set; }

    /// <summary>
    /// 是否显示过滤
    /// </summary>
    [Parameter]
    public bool IsShowFilter { get; set; } = true;

    /// <summary>
    /// 是否显示右侧操作栏
    /// </summary>
    [Parameter]
    public bool IsShowOperCol { get; set; } = true;
    /// <summary>
    /// 是否显示查询按钮
    /// </summary>
    [Parameter]
    public bool IsShowQueryButton { get; set; }

    /// <summary>
    /// 是否显示搜索关键字
    /// </summary>
    [Parameter]
    public bool IsShowSearchKey { get; set; } = false;
    /// <summary>
    /// 是否显示表格多项选择
    /// </summary>
    [Parameter]
    public bool IsShowSelect { get; set; } = true;

    /// <summary>
    /// 是否显示顶部操作工具栏
    /// </summary>
    [Parameter]
    public bool IsShowToolbar { get; set; } = true;

    /// <summary>
    /// 获得/设置 Table Oper 模板
    /// </summary>
    [Parameter]
    public RenderFragment<ItemColProps<TItem>> ItemColOperTemplate { get; set; }

    /// <summary>
    /// 获得/设置 Table Cols 模板
    /// </summary>
    [Parameter]
    public RenderFragment<ItemColProps<TItem>> ItemColTemplate { get; set; }

    /// <summary>
    /// 独立设置 Table Cols 模板，需自行实现DateTime类型的时区转换
    /// </summary>
    [Parameter]
    public RenderFragment<ItemColProps<TItem>> ItemColWithDTTemplate { get; set; }


    /// <summary>
    /// 当前显示项目
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> Items { get; set; } = new List<TItem>();

    /// <summary>
    /// 获得/设置 其他操作栏模板
    /// </summary>
    [Parameter]
    public RenderFragment<IEnumerable<TItem>> OtherToolbarTemplate { get; set; }

    /// <summary>
    /// 分页选择项目
    /// </summary>
    [Parameter]
    public List<PageSize> PageSizeItems { get; set; } = new List<PageSize>()
    {
        new PageSize(){Key="5",Value=5},
        new PageSize(){Key="10",Value=10},
        new PageSize(){Key="50",Value=50},
        new PageSize(){Key="100",Value=100}
     };

    /// <summary>
    /// 查询项委托
    /// </summary>
    [Parameter]
    public Func<SearchItem, Task<ISqlSugarPagedList<TItem>>> QueryCallAsync { get; set; }

    /// <summary>
    /// 获得/设置 SearchModel 实例
    /// </summary>
    [Parameter]
    public SearchItem SearchModel { get; set; }

    /// <summary>
    /// 获得/设置 查询与操作栏模板
    /// </summary>
    [Parameter]
    public RenderFragment<SearchItem> SearchTemplate { get; set; }
    private AddItem AddModel { get; set; }
    private bool AddShow { get; set; }
    private bool DeleteLoading { get; set; }
    private TItem DetailModel { get; set; }
    private bool DetailShow { get; set; }
    private EditItem EditModel { get; set; }

    private bool EditShow { get; set; }

    private List<Filters> FilterHeaderString { get; set; } = new();

    private List<DataTableHeader<TItem>> headers = new();

    private List<DataTableHeader<TItem>> Headers { get; set; } = new();

    [Inject]
    private InitTimezone InitTimezone { get; set; }

    private int Page { get; set; }

    private ISqlSugarPagedList<TItem> PageItems { get; set; } = new SqlSugarPagedList<TItem>();
    private bool QueryLoading { get; set; }

    private MForm SearchForm { get; set; }

    private IEnumerable<TItem> selectedItem = new List<TItem>();

    private int size;

    /// <inheritdoc/>
    public async Task QueryClickAsync()
    {
        try
        {
            QueryLoading = true;
            StateHasChanged();
            PageItems = await QueryCallAsync.Invoke(SearchModel);
            Items = PageItems.Records;
            if (!IsPage)
            {
                SearchModel.Size = PageItems.Total;
            }
            selectedItem = new List<TItem>();
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        finally
        {
            QueryLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            if (Page != SearchModel.Current)
            {
                Page = SearchModel.Current;
                await QueryClickAsync();
            }
        }
        else
        {
            Page = SearchModel.Current;
            size = SearchModel.Size;
            if (!Items.Any())
                await QueryClickAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        SearchModel ??= new();

        var propertyInfos = typeof(TItem).GetProperties();
        var data = propertyInfos
            .Select(a => new
            {
                propertyInfo = a,
                datatable = a.GetCustomAttribute<DataTableAttribute>()
            })
            .Where(a => a.datatable?.IsShow == true)
            .OrderBy(a => a.datatable.Order).ToList();

        //根据特性过滤掉固定不需要的列
        foreach (var item in data)
        {
            Headers.Add(new()
            {
                Text = typeof(TItem).GetDescription(item.propertyInfo.Name),
                Value = item.propertyInfo.Name,
                CellClass = " table-minwidth " + item.datatable.CellClass,
                Sortable = item.datatable.Sortable,
                Align = DataTableHeaderAlign.Start,
                Fixed = (item.datatable.FixedLeft) ? DataTableFixed.Left : DataTableFixed.None,
                Filterable = !item.datatable.DefaultFilter
            });
        }
        if (IsShowOperCol)
        {
            Headers.Add(new()
            {
                Text = "操作",
                Value = BlazorResourceConst.DataTableActions,
                Width = 200,
                Fixed = DataTableFixed.Right,
                Sortable = false,
            });
        }

        //外部控制列是否存在
        FilterHeaders?.Invoke(Headers);

        //初始化过滤显示列表
        foreach (var item in Headers)
        {
            var filter = new Filters()
            { Title = item.Text, Key = item.Value, Value = item.Filterable, };
            FilterHeaderString.Add(filter);
        }

        //初始化Filter值
        Filters?.Invoke(FilterHeaderString);

        //定义操作列宽度
        var action = Headers.FirstOrDefault(a => a.Value == BlazorResourceConst.DataTableActions);
        if (action != null)
            action.Width = 70 * ((IsShowEditButton ? 1 : 0) + (IsShowDetailButton ? 1 : 0) + (IsShowDeleteButton ? 1 : 0) + (ItemColOperTemplate != null ? 1 : 0));

        FilterChanged();//过滤
        await base.OnInitializedAsync();
    }


    private void AddClick()
    {
        AddModel = new();
        AddShow = true;
    }

    private void AddOnCancel()
    {
        AddShow = false;
    }

    private async Task AddOnSave(ModalActionEventArgs args)
    {
        try
        {
            if (AddCallAsync != null)
            {
                await AddCallAsync.Invoke(AddModel);
            }
            await QueryClickAsync();
            AddShow = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }

    private void ClearClick()
    {
        SearchForm.Reset();
    }

    private async Task DeleteClick(params TItem[] _selectedItem)
    {
        DeleteLoading = true;
        StateHasChanged();
        try
        {
            if (_selectedItem.Length <= 0)
            {
                await PopupService.EnqueueSnackbarAsync("选择一行后才能进行操作");
            }
            else
            {
                if (DeleteCallAsync != null)
                {
                    var confirm = await PopupService.ConfirmAsync("删除", "确定 ?", AlertTypes.Warning);
                    if (confirm)
                    {
                        await DeleteCallAsync(_selectedItem);
                        await QueryClickAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        finally
        {
            DeleteLoading = false;
            StateHasChanged();
        }
    }

    private async Task DetailClick(params TItem[] _selectedItem)
    {
        if (_selectedItem.Length > 1)
        {
            await PopupService.EnqueueSnackbarAsync("只能选择一行");
        }
        else if (_selectedItem.Length == 1)
        {
            DetailModel = _selectedItem.FirstOrDefault();
            var strs = typeof(TItem).GetPropertyNames();
            Dictionary<string, string> keyValuePairs = new();
            foreach (var item in strs)
            {
                if (item != BlazorResourceConst.DataTableActions)
                {
                    var value = typeof(TItem).GetMemberInfoValue(DetailModel, item);
                    if (value is DateTime dt2)
                    {
                        value = dt2.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset);
                    }
                    keyValuePairs.Add(item, value?.ToString());
                }
            }
            DetailModelPairs = keyValuePairs;
            DetailShow = true;
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync("选择一行后才能进行操作");
        }
    }
    private async Task EditClick(params TItem[] _selectedItem)
    {
        if (_selectedItem.Length > 1)
        {
            await PopupService.EnqueueSnackbarAsync("只能选择一行");
        }
        else if (_selectedItem.Length == 1)
        {
            EditModel = _selectedItem.FirstOrDefault().Adapt<EditItem>();
            EditShow = true;
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync("选择一行后才能进行操作");
        }
    }

    private void EditOnCancel()
    {
        EditShow = false;
    }

    private async Task EditOnSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            if (EditCallAsync != null)
            {
                await EditCallAsync.Invoke(EditModel);
            }
            await QueryClickAsync();
            EditShow = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }

    private async Task Enter(KeyboardEventArgs e)
    {
        if (IsShowQueryButton)
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await QueryClickAsync();
            }
    }

    private void FilterChanged()
    {
        headers = Headers.Where(it => FilterHeaderString.Any(a => a.Key == it.Value && a.Value == true)).ToList();
    }

    private async Task HandleOnOptionsUpdate(DataOptions dataOptions)
    {
        SearchModel.SortField = dataOptions.SortBy.ToList();
        SearchModel.SortDesc = dataOptions.SortDesc.ToList();
        await QueryClickAsync();
    }

    private async Task PageChanged(int val)
    {
        if (SearchModel.Current <= 0)
        {
            SearchModel.Current = 1;
        }
        if (size != SearchModel.Size)
        {
            size = SearchModel.Size;
            await QueryClickAsync();
        }

    }
}