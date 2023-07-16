#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/dotnetchina/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
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