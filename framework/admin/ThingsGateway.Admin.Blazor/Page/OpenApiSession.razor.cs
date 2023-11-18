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
/// OpenApiSession
/// </summary>
public partial class OpenApiSession
{
    private readonly OpenApiSessionOutput _sessionOutput = new();
    private readonly OpenApiSessionPageInput _sessionSearch = new();
    private bool _isShowVerificatSignList;
    private List<VerificatInfo> _verificatInfos;
    private IAppDataTable _verificatinfosDatatable;

    private async Task SessionExitAsync(long id)
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("警告", "确定 ?");
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<OpenApiSessionService>().ExitSessionAsync(id);
        }
    }

    private async Task<ISqlSugarPagedList<OpenApiSessionOutput>> SessionQueryCallAsync(OpenApiSessionPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<OpenApiSessionService>().PageAsync(input);
    }

    private async Task ShowVerificatListAsync(List<VerificatInfo> verificatInfos)
    {
        _verificatInfos = verificatInfos;
        _isShowVerificatSignList = true;
        if (_verificatinfosDatatable != null)
            await _verificatinfosDatatable.QueryClickAsync();
    }

    private async Task VerificatExitAsync(IEnumerable<VerificatInfo> verificats)
    {
        var send = new OpenApiExitVerificatInput()
        {
            VerificatIds = verificats.Select(it => it.Id).ToList(),
            Id = verificats.First().UserId
        };
        await _serviceScope.ServiceProvider.GetService<OpenApiSessionService>().ExitVerificatAsync(send);
        _verificatInfos.RemoveWhere(it => send.VerificatIds.Contains(it.Id));
    }


    private async Task<ISqlSugarPagedList<VerificatInfo>> VerificatQueryCallAsync(BasePageInput basePageInput)
    {
        await Task.CompletedTask;
        return _verificatInfos.ToPagedList(basePageInput);
    }
}