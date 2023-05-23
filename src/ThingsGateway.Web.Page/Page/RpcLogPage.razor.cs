#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Web.Page
{
    public partial class RpcLogPage
    {
        private IAppDataTable _datatable;
        private RpcLogPageInput search = new();

        private void FilterHeaders(List<DataTableHeader<RpcLog>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(RpcLog.Id));
            foreach (DataTableHeader<RpcLog> item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {

                    case nameof(RpcLog.ParamJson):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(RpcLog.ResultJson):
                        item.CellClass += " table-text-truncate ";
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

            }
        }


        private async Task<SqlSugarPagedList<RpcLog>> QueryCall(RpcLogPageInput input)
        {
            var data = await RpcLogService.PageAsync(input);
            return data;
        }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("ɾ��"), T("ȷ�� ?"));
            if (confirm)
            {
                await RpcLogService.DeleteAsync();
                await _datatable?.QueryClickAsync();
            }
        }
    }
}