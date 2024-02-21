﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 会话页面
/// </summary>
public partial class Session
{
    private readonly SessionOutput _sessionOutput = new();

    private readonly SessionPageInput _sessionSearch = new();
    private SessionAnalysisOutput _sessionAnalysisOutput = new();

    private IAppDataTable _verificatinfosDatatable;

    private async Task SessionExitAsync(long id)
    {
        var confirm = await PopupService.OpenConfirmDialogAsync(AppService.I18n.T("警告"), AppService.I18n.T("强退此用户?"));
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<ISessionService>().ExitSessionAsync(id.ToInput());
        }
    }

    private async Task<SqlSugarPagedList<SessionOutput>> SessionQueryCallAsync(SessionPageInput input)
    {
        _sessionAnalysisOutput = _serviceScope.ServiceProvider.GetService<ISessionService>().Analysis();
        await InvokeAsync(StateHasChanged);
        return await _serviceScope.ServiceProvider.GetService<ISessionService>().PageAsync(input);
    }

    private async Task ShowVerificatListAsync(SessionOutput sessionOutput)
    {
        await PopupService.OpenAsync(typeof(SessionOper), new Dictionary<string, object?>()
        {
            {nameof(SessionOper.VerificatInfos),sessionOutput.VerificatSignList },
            {nameof(SessionOper.UserId),sessionOutput.Id },
});
        await _verificatinfosDatatable.QueryClickAsync();
    }
}