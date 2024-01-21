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

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Blazor;

public partial class SessionOper
{
    private IAppDataTable _verificatinfosDatatable;

    [Parameter]
    public List<VerificatInfo> VerificatInfos { get; set; }

    [Parameter]
    public long UserId { get; set; }

    private async Task VerificatExitAsync(IEnumerable<VerificatInfo> verificats)
    {
        var send = new ExitVerificatInput()
        {
            Verificats = verificats.Select(it => it.Id).ToList(),
            Id = UserId
        };
        await _serviceScope.ServiceProvider.GetService<ISessionService>().ExitVerificatAsync(send);
        VerificatInfos.RemoveWhere(it => send.Verificats.Contains(it.Id));
    }

    private Task<SqlSugarPagedList<VerificatInfo>> VerificatQueryCallAsync(BasePageInput basePageInput)
    {
        return Task.FromResult(VerificatInfos.ToPagedList(basePageInput));
    }
}