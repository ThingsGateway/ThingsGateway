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

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// UserMenu
/// </summary>
public partial class UserMenu
{
    [Inject]
    NavigationManager NavigationManager { get; set; }
    [Inject]
    private UserResoures UserResoures { get; set; }

    [Inject]
    private AjaxService AjaxService { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private async Task LogoutAsync()
    {
        var ajaxOption = new AjaxOption
        {
            Url = "/auth/b/logout",
        };
        var str = await AjaxService.GetMessageAsync(ajaxOption);
        var ret = str?.ToJsonWithT<UnifyResult<string>>();
        if (ret?.Code != 200)
        {
            await PopupService.EnqueueSnackbarAsync("注销失败", AlertTypes.Error);
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync("注销成功", AlertTypes.Success);
            await Task.Delay(500);
            NavigationManager.NavigateTo(NavigationManager.Uri);
        }
    }
}