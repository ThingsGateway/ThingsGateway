using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace ThingsGateway.Web.Rcl.Core
{
    public partial class AppDataTable<TItem, SearchItem, AddItem, EditItem> : IAppDataTable
        where TItem : PrimaryIdEntity, new()
        where SearchItem : BasePageInput, new()
        where AddItem : class, new()
        where EditItem : class, new()
    {
        private MDataTable<TItem> _table;

        private Dictionary<string, string> DetailModelPairs = new Dictionary<string, string>();

        [Parameter]
        public Func<AddItem, Task> AddCall { get; set; }

        /// <summary>
        /// 获得/设置 添加模板
        /// </summary>
        [Parameter]
        public RenderFragment<AddItem> AddTemplate { get; set; }

        [Parameter]
        public string ClassString { get; set; }

        [Parameter]
        public Func<IEnumerable<TItem>, Task> DeleteCall { get; set; }

        [Parameter]
        public bool Dense { get; set; }
        /// <summary>
        /// 获得/设置 明细行模板
        /// </summary>
        [Parameter]
        public RenderFragment<TItem> DetailRowTemplate { get; set; }

        [Parameter]
        public Func<EditItem, Task> EditCall { get; set; }

        /// <summary>
        /// 获得/设置 编辑模板
        /// </summary>
        [Parameter]
        public RenderFragment<EditItem> EditTemplate { get; set; }

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

        [Parameter]
        public bool IsMenuOperTemplate { get; set; } = true;

        [Parameter]
        public bool IsPage { get; set; } = true;

        [Parameter]
        public bool IsShowOperCol { get; set; } = true;

        [Parameter]
        public bool IsShowSearchKey { get; set; } = false;
        [Parameter]
        public bool IsShowClearSearch { get; set; } = true;
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

        [Parameter]
        public IEnumerable<TItem> Items { get; set; } = new List<TItem>();

        /// <summary>
        /// 获得/设置 其他操作栏模板
        /// </summary>
        [Parameter]
        public RenderFragment<IEnumerable<TItem>> OtherToolbarTemplate { get; set; }

        [Parameter]
        public List<PageSize> PageSizeItems { get; set; } = new List<PageSize>()
        {
            new PageSize(){Key="5",Value=5},
            new PageSize(){Key="10",Value=10},
            new PageSize(){Key="50",Value=50},
            new PageSize(){Key="100",Value=100}
         };

        [Parameter]
        public Func<SearchItem, Task<SqlSugarPagedList<TItem>>> QueryCall { get; set; }

        /// <summary>
        /// 获得/设置 SearchModel 实例
        /// </summary>
        [Parameter]
        public SearchItem SearchModel
        {
            get;
            set;
        }

        /// <summary>
        /// 获得/设置 查询与操作栏模板
        /// </summary>
        [Parameter]
        public RenderFragment<SearchItem> SearchTemplate { get; set; }

        [Parameter]
        public bool ShowAddButton { get; set; }

        [Parameter]
        public bool ShowDeleteButton { get; set; }

        [Parameter]
        public bool ShowDetailButton { get; set; }

        [Parameter]
        public bool ShowEditButton { get; set; }

        [Parameter]
        public bool ShowFilter { get; set; } = true;

        [Parameter]
        public bool ShowQueryButton { get; set; }

        [Parameter]
        public bool ShowSelect { get; set; } = true;

        private AddItem AddModel { get; set; }
        public bool AddShow { get; set; }
        private bool DeleteLoading { get; set; }
        private TItem DetailModel { get; set; }
        private bool DetailShow { get; set; }
        private EditItem EditModel { get; set; }

        private bool EditShow { get; set; }

        /// <summary>
        /// 过滤实体列，
        /// </summary>
        private List<Filters> FilterHeaderString { get; set; } = new();

        private int FirstRender { get; set; }
        private List<DataTableHeader<TItem>> headers { get; set; } = new();

        private List<DataTableHeader<TItem>> Headers { get; set; } = new();

        private int Page { get; set; }

        private SqlSugarPagedList<TItem> PageItems { get; set; } = new();
        private bool QueryLoading { get; set; }

        private MForm SearchForm { get; set; }

        private IEnumerable<TItem> selectedItem { get; set; } = new List<TItem>();

 

        protected override bool ShouldRender()
        {
            return base.ShouldRender() ;
        }

        public async Task QueryClick()
        {
            try
            {
                QueryLoading = true;
                StateHasChanged();
                await Task.Yield();
                PageItems = await QueryCall.Invoke(SearchModel);
                Items = PageItems.Records;

                //var item = PageSizeItems.FirstOrDefault(it => it.Key == "ALL");
                //if (item != null)
                //{
                //    item.Value = PageItems.Total;
                //}
                //else
                //{
                //    PageSizeItems.Add(new PageSize() { Key = "ALL", Value = PageItems.Total });
                //}
                if (!IsPage)
                {
                    SearchModel.Size = PageItems.Total;
                }
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex, false);
            }
            finally
            {
                QueryLoading = false;
               await InvokeAsync( StateHasChanged);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                if (Page != SearchModel.Current)
                {
                    Page = SearchModel.Current;
                    await QueryClick();
                }
            }
            else
            {
                Page = SearchModel.Current;
                if (Items.Count() <= 0)
                    await QueryClick();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (SearchModel == null)
                SearchModel = new();
            var data = ObjectExtensions.GetAllPropsName(typeof(TItem));

            foreach (var item in data)
            {
                Headers.Add(new()
                { Text = typeof(TItem).GetDescription(item), Value = item, });
            }
            if (IsShowOperCol)
            {
                Headers.Add(new()
                { Text = T("操作"), Value = BlazorConst.TB_Actions, Width = 200 });
            }

            if (FilterHeaders != null)
            {
                FilterHeaders.Invoke(Headers);
            }

            foreach (var item in Headers)
            {
                var filter = new Filters()
                { Title = item.Text, Key = item.Value, Value = true, };
                FilterHeaderString.Add(filter);
            }
            if (Filters != null)
            {
                Filters.Invoke(FilterHeaderString);
            }

            var action = Headers.FirstOrDefault(a => a.Value == BlazorConst.TB_Actions);
            if (action != null)
                action.Width = 70 * ((ShowEditButton ? 1 : 0) + (ShowDetailButton ? 1 : 0) + (ShowDeleteButton ? 1 : 0) + (ItemColOperTemplate != null ? 1 : 0));

            FilterChanged();
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
                await AddCall(AddModel);
                await QueryClick();
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
                if (_selectedItem.Count() <= 0)
                {
                    await PopupService.EnqueueSnackbarAsync(T("选择一行后才能进行操作"));
                }
                else
                {
                    if (DeleteCall != null)
                    {
                        var confirm = await OpenConfirmDialog(T("删除"), T("确定 ?"));
                        if (confirm)
                        {
                            await DeleteCall(_selectedItem);
                            await QueryClick();
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
            if (_selectedItem.Count() > 1)
            {
                await PopupService.EnqueueSnackbarAsync(T("只能选择一行"));
            }
            else if (_selectedItem.Count() == 1)
            {
                DetailModel = _selectedItem.FirstOrDefault();
                var strs = typeof(TItem).GetAllPropsName();
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (var item in strs)
                {
                    if (item != BlazorConst.TB_Actions)
                    {
                        string value = typeof(TItem).GetPropValue(DetailModel, item);
                        keyValuePairs.Add(item, value);
                    }
                }
                DetailModelPairs = keyValuePairs;
                DetailShow = true;
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(T("选择一行后才能进行操作"));
            }
        }

        private async Task EditClick(params TItem[] _selectedItem)
        {
            if (_selectedItem.Count() > 1)
            {
                await PopupService.EnqueueSnackbarAsync(T("只能选择一行"));
            }
            else if (_selectedItem.Count() == 1)
            {
                EditModel = _selectedItem.FirstOrDefault().Adapt<EditItem>();
                EditShow = true;
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(T("选择一行后才能进行操作"));
            }
        }

        private void EditOnCancel()
        {
            EditShow = false;
        }

        private async Task EditOnSave(ModalActionEventArgs args)
        {
            try
            {
                await EditCall?.Invoke(EditModel);
                await QueryClick();
                EditShow = false;
            }
            catch (Exception ex)
            {
                args.Cancel();
                await PopupService.EnqueueSnackbarAsync(ex, false);
            }
        }
        private async Task InputEnter(int a)
        {
            if (ShowQueryButton)
            {
                SearchModel.Current = a;
                await QueryClick();
            }
        }
        private async Task Enter(KeyboardEventArgs e)
        {
            if (ShowQueryButton)
                if (e.Code == "Enter" || e.Code == "NumpadEnter")
                {
                    await QueryClick();
                }
        }

        private void FilterChanged()
        {
            headers = Headers.Where(it => FilterHeaderString.Any(a => a.Key == it.Value && a.Value == true)).ToList();
        }

        private async Task HandleOnOptionsUpdate(DataOptions dataOptions)
        {
            SearchModel.SortField = dataOptions.SortBy.FirstOrDefault();
            SearchModel.SortOrder = dataOptions.SortDesc.FirstOrDefault() ? "desc" : "asc";
            await QueryClick();
        }

        private async Task<bool> OpenConfirmDialog(string title, string content)
        {
            return await PopupService.ConfirmAsync(title, content, AlertTypes.Error);
        }

        private async Task<bool> OpenConfirmDialog(string title, string content, AlertTypes type)
        {
            return await PopupService.ConfirmAsync(title, content, type);
        }
        private int size { get;set; }
        private  async Task PageChanged(int val)
        {
            if (((float)PageItems.Total / SearchModel.Size) < SearchModel.Current)
            {
                SearchModel.Current = (int)(PageItems.Total / SearchModel.Size) + 1;
            }
            if(SearchModel.Current<=0)
            {
                SearchModel.Current = 1;
            }
                if (FirstRender >= 1&& size!= SearchModel.Size)
            {
                size = SearchModel.Size;
                await QueryClick();
            }

            FirstRender += 1;
            size = SearchModel.Size;
        }
    }
}