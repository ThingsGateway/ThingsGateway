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

using BlazorComponent;

using SqlSugar;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class OpenApiSession
    {
        private List<VerificatInfo> _verificatInfos;
        private IAppDataTable _verificatinfosDatatable;
        private bool IsShowVerificatSignList;
        private OpenApiSessionOutput sessionOutput = new();
        private OpenApiSessionPageInput sessionSearch = new();

        private async Task sessionExit(long id)
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("����"), T("ȷ�� ?"));
            if (confirm)
            {
                await SessionService.ExitSession(id.ToIdInput());
            }
        }

        private void sessionFilterHeaders(List<DataTableHeader<OpenApiSessionOutput>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(OpenApiSessionOutput.VerificatSignList));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiSessionOutput.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiSessionOutput.Id));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(OpenApiSessionOutput.Account):
                        item.Sortable = true;
                        break;

                    case nameof(OpenApiSessionOutput.LatestLoginTime):
                        item.Sortable = true;
                        break;

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<OpenApiSessionOutput>> sessionQueryCall(OpenApiSessionPageInput input)
        {
            var data = await SessionService.Page(input);
            return data;
        }

        private async Task showVerificatList(List<VerificatInfo> verificatInfos)
        {
            _verificatInfos = verificatInfos;
            IsShowVerificatSignList = true;
            if (_verificatinfosDatatable != null)
                await _verificatinfosDatatable.QueryClickAsync();
        }

        private async Task verificatExit(IEnumerable<VerificatInfo> verificats)
        {
            var send = new OpenApiExitVerificatInput()
            {
                VerificatIds = verificats.Select(it => it.Id).ToList(),
                Id = verificats.First().UserId
            };
            await SessionService.ExitVerificat(send);
            _verificatInfos.RemoveWhere(it => send.VerificatIds.Contains(it.Id));
        }

        private void verificatFilterHeaders(List<DataTableHeader<VerificatInfo>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(VerificatInfo.ClientIds));
            datas.RemoveWhere(it => it.Value == nameof(VerificatInfo.OnlineNum));
            datas.RemoveWhere(it => it.Value == nameof(VerificatInfo.IsOnline));
            datas.RemoveWhere(it => it.Value == nameof(VerificatInfo.UserId));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(VerificatInfo.Device):
                        item.Sortable = true;
                        break;

                    case nameof(VerificatInfo.IsOnline):
                        item.Sortable = true;
                        break;

                    case nameof(VerificatInfo.VerificatRemain):
                        item.Sortable = true;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<VerificatInfo>> verificatQueryCall(BasePageInput basePageInput)
        {
            return await _verificatInfos.ToPagedListAsync(basePageInput);
        }
    }
}