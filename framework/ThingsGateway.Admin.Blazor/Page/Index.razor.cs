#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// ��ҳ
/// </summary>
public partial class Index
{
    List<SysOperateLog> SysOperateLogs;

    List<SysVisitLog> SysVisitLogs;

    [Inject]
    private InitTimezone InitTimezone { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        SysVisitLogs = (await App.GetService<IVisitLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
        SysOperateLogs = (await App.GetService<IOperateLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
        await base.OnParametersSetAsync();
    }
}