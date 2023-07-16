#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using SqlSugar;

namespace ThingsGateway.Web.Page
{
    public partial class Index
    {
        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;

        [Inject]
        private ThingsGateway.Web.Foundation.IRpcLogService RpcLogService { get; set; }

        [Inject]
        private ThingsGateway.Web.Foundation.IBackendLogService BackendLogService { get; set; }

        [Inject]
        private IVisitLogService VisitLogService { get; set; }

        List<DevLogVisit> DevLogVisits;
        List<DevLogOperate> DevLogOps;
        List<ThingsGateway.Web.Foundation.RpcLog> RpcLogs;
        List<ThingsGateway.Web.Foundation.BackendLog> BackendLogs;
        [Inject]
        private IOperateLogService OperateLogService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (UserManager.SuperAdmin)
            {
                RpcLogs = (await RpcLogService.PageAsync(new() { Size = 5 })).Records.ToList();
                BackendLogs = (await BackendLogService.PageAsync(new() { Size = 5 })).Records.ToList();
            }

            DevLogVisits = (await VisitLogService.Page(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
            DevLogOps = (await OperateLogService.Page(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
            await base.OnInitializedAsync();
        }
    }
}