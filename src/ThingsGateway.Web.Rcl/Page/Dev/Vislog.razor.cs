using BlazorComponent;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    /// <summary>
    /// ������־ҳ��
    /// </summary>
    public partial class Vislog
    {
        private IAppDataTable _datatable;
        private VisitLogPageInput search = new();
        /// <summary>
        /// ��־����˵�
        /// </summary>
        public List<StringFilters> CategoryFilters { get; set; } = new();
        /// <summary>
        /// ִ�н���˵�
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
            CategoryFilters.Add(new StringFilters() { Key = T("��¼"), Value = CateGoryConst.Log_LOGIN });
            CategoryFilters.Add(new StringFilters() { Key = T("ע��"), Value = CateGoryConst.Log_LOGOUT });
            CategoryFilters.Add(new StringFilters() { Key = T("��������¼"), Value = CateGoryConst.Log_OPENAPILOGIN });
            CategoryFilters.Add(new StringFilters() { Key = T("������ע��"), Value = CateGoryConst.Log_OPENAPILOGOUT });
            ExeStatus.Add(new StringFilters() { Key = T("�ɹ�"), Value = DevLogConst.SUCCESS });
            ExeStatus.Add(new StringFilters() { Key = T("ʧ��"), Value = DevLogConst.FAIL });
            base.OnInitialized();
        }

        private async Task<SqlSugarPagedList<DevLogVisit>> QueryCall(VisitLogPageInput input)
        {
            var data = await VisitLogService.Page(input);
            return data;
        }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialog(T("ɾ��"), T("ȷ�� ?"));
            if (confirm)
            {
                await VisitLogService.Delete(CategoryFilters.Select(it => it.Value).ToArray());
                await _datatable?.QueryClick();
            }
        }
    }
}