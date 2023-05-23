#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using BlazorComponent;

using SqlSugar;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class Session
    {
        private List<VerificatInfo> _verificatInfos;
        private IAppDataTable _verificatinfosDatatable;
        private bool IsShowVerificatSignList;
        private SessionOutput sessionOutput = new();
        private SessionPageInput sessionSearch = new();

        private async Task sessionExit(long id)
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("警告"), T("确定 ?"));
            if (confirm)
            {
                await SessionService.ExitSession(id.ToIdInput());
            }
        }

        private void sessionFilterHeaders(List<DataTableHeader<SessionOutput>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(SessionOutput.VerificatSignList));
            datas.RemoveWhere(it => it.Value == nameof(SessionOutput.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(SessionOutput.Id));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(SessionOutput.Account):
                        item.Sortable = true;
                        break;

                    case nameof(SessionOutput.LatestLoginTime):
                        item.Sortable = true;
                        break;

                    case nameof(SessionOutput.OnlineStatus):
                        item.Sortable = true;
                        break;

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<SessionOutput>> sessionQueryCall(SessionPageInput input)
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
            var send = new ExitVerificatInput()
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

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
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