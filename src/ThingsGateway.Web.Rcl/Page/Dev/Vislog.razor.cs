using BlazorComponent;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    /// <summary>
    /// 访问日志页面
    /// </summary>
    public partial class Vislog
    {
        private IAppDataTable _datatable;
        private VisitLogPageInput search = new();
        /// <summary>
        /// 日志分类菜单
        /// </summary>
        public List<StringFilters> CategoryFilters { get; set; } = new();
        /// <summary>
        /// 执行结果菜单
        /// </summary>
        public List<StringFilters> ExeStatus { get; set; } = new();

        private void FilterHeaders(List<DataTableHeader<DevLogVisit>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.CreateTime));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.UpdateTime));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.Id));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.CreateUser));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.UpdateUser));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(DevLogVisit.Category):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.Name):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.OpIp):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.OpBrowser):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.OpOs):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.OpTime):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogVisit.OpAccount):
                        item.Sortable = true;
                        break;

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }
        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            CategoryFilters.Add(new StringFilters() { Key = T("登录"), Value = CateGoryConst.Log_LOGIN });
            CategoryFilters.Add(new StringFilters() { Key = T("注销"), Value = CateGoryConst.Log_LOGOUT });
            CategoryFilters.Add(new StringFilters() { Key = T("第三方登录"), Value = CateGoryConst.Log_OPENAPILOGIN });
            CategoryFilters.Add(new StringFilters() { Key = T("第三方注销"), Value = CateGoryConst.Log_OPENAPILOGOUT });
            ExeStatus.Add(new StringFilters() { Key = T("成功"), Value = DevLogConst.SUCCESS });
            ExeStatus.Add(new StringFilters() { Key = T("失败"), Value = DevLogConst.FAIL });
            base.OnInitialized();
        }

        private async Task<SqlSugarPagedList<DevLogVisit>> QueryCall(VisitLogPageInput input)
        {
            var data = await VisitLogService.Page(input);
            return data;
        }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialog(T("删除"), T("确定 ?"));
            if (confirm)
            {
                await VisitLogService.Delete(CategoryFilters.Select(it => it.Value).ToArray());
                await _datatable?.QueryClick();
            }
        }
    }
}