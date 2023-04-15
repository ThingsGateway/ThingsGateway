using BlazorComponent;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class Oplog
    {
        private IAppDataTable _datatable;
        private OperateLogPageInput search = new();
        public List<StringFilters> CategoryFilters { get; set; } = new();
        public List<StringFilters> ExeStatus { get; set; } = new();

        private void FilterHeaders(List<DataTableHeader<DevLogOperate>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.CreateTime));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.UpdateTime));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DevLogOperate.Id));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.CreateUser));
            datas.RemoveWhere(it => it.Value == nameof(DevLogVisit.UpdateUser));
            foreach (DataTableHeader<DevLogOperate> item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(DevLogOperate.ExeMessage):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(DevLogOperate.ClassName):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(DevLogOperate.ParamJson):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(DevLogOperate.ResultJson):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(DevLogOperate.Category):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.Name):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.OpIp):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.OpBrowser):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.OpOs):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.OpTime):
                        item.Sortable = true;
                        break;

                    case nameof(DevLogOperate.OpAccount):
                        item.Sortable = true;
                        break;

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }

        private void Filters(List<Filters> datas)
        {
            foreach (var item in datas)
            {
                switch (item.Key)
                {
                    case nameof(DevLogOperate.ExeMessage):
                        item.Value = false;
                        break;

                    case nameof(DevLogOperate.ClassName):
                        item.Value = false;
                        break;

                    case nameof(DevLogOperate.ParamJson):
                        item.Value = false;
                        break;

                    case nameof(DevLogOperate.ResultJson):
                        item.Value = false;
                        break;

                    case nameof(DevLogOperate.OpBrowser):
                        item.Value = false;
                        break;

                    case nameof(DevLogOperate.OpOs):
                        item.Value = false;
                        break;
                }
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CategoryFilters.Add(new StringFilters() { Key = T("操作"), Value = CateGoryConst.Log_OPERATE });
            CategoryFilters.Add(new StringFilters() { Key = T("第三方操作"), Value = CateGoryConst.Log_OPENAPIOPERATE });
            ExeStatus.Add(new StringFilters() { Key = T("成功"), Value = DevLogConst.SUCCESS });
            ExeStatus.Add(new StringFilters() { Key = T("失败"), Value = DevLogConst.FAIL });
        }

        private async Task<SqlSugarPagedList<DevLogOperate>> QueryCall(OperateLogPageInput input)
        {
            input.Account = search.Account;
            input.Category = search.Category;
            input.ExeStatus = search.ExeStatus;
            var data = await OperateLogService.Page(input);
            return data;
        }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("删除"), T("确定 ?"));
            if (confirm)
            {
                await OperateLogService.Delete(CategoryFilters.Select(it => it.Value).ToArray());
                await _datatable?.QueryClickAsync();
            }
        }
    }
}