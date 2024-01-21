#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 首页
/// </summary>
#if Admin

[Route("/index")]
public partial class Index
#else

public partial class Index
#endif
{
    private List<SysOperateLog> _sysOperateLogs;

    private List<SysVisitLog> _sysVisitLogs;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _sysVisitLogs = (await _serviceScope.ServiceProvider.GetService<IVisitLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
        _sysOperateLogs = (await _serviceScope.ServiceProvider.GetService<IOperateLogService>().PageAsync(new() { Size = 5, Account = UserResoures.CurrentUser?.Account })).Records.ToList();
        await base.OnParametersSetAsync();
    }
}