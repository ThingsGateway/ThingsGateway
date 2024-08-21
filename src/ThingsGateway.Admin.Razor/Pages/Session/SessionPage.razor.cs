//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SessionPage
{
    private SessionOutput? SearchModel { get; set; } = new();

    [Inject]
    [NotNull]
    private ISessionService? SessionService { get; set; }

    #region 查询

    private async Task<QueryData<SessionOutput>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SessionService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 弹出令牌信息表

    private async Task ShowVerificatList(SessionOutput sessionOutput)
    {
        if (sessionOutput.VerificatSignList?.Count > 0)
        {
            var op = new DialogOption()
            {
        IsScrolling = true,
                Title = Localizer[nameof(VerificatInfo)],
                ShowFooter = false,
                Size = Size.ExtraLarge
            };
            op.Component = BootstrapDynamicComponent.CreateComponent<VerificatListPage>(new Dictionary<string, object?>
            {
                [nameof(VerificatListPage.UserId)] = sessionOutput.Id,
                [nameof(VerificatListPage.VerificatInfos)] = sessionOutput.VerificatSignList,
            });
            await DialogService.Show(op);
        }
    }

    #endregion 弹出令牌信息表
}
