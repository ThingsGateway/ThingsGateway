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

using Masa.Blazor;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// 个人设置
/// </summary>
public partial class UserCenter
{
    List<long> _menusChoice = new();
    readonly PasswordInfoInput _passwordInfoInput = new();
    readonly UpdateInfoInput _updateInfoInput = new();

    long DefaultMenuId { get; set; }
    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IUserCenterService UserCenterService { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        DefaultMenuId = await App.GetService<IUserCenterService>().GetLoginDefaultRazorAsync(UserManager.UserId);
        _updateInfoInput.Email = UserResoures.CurrentUser.Email;
        _updateInfoInput.Phone = UserResoures.CurrentUser.Phone;
        _menusChoice = await App.GetService<IUserCenterService>().GetLoginWorkbenchAsync();
        await base.OnParametersSetAsync();
    }

    async Task OnDefaultRazorSaveAsync()
    {
        await UserCenterService.UpdateUserDefaultRazorAsync(UserManager.UserId, DefaultMenuId);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }

    async Task OnShortcutSaveAsync()
    {
        await UserCenterService.UpdateWorkbenchAsync(_menusChoice);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
    async Task OnUpdatePasswordInfoAsync(FormContext context)
    {
        var success = context.Validate();
        if (success)
        {
            //验证成功，操作业务
            _passwordInfoInput.Id = UserResoures.CurrentUser.Id;
            await UserCenterService.EditPasswordAsync(_passwordInfoInput);
            await MainLayout.StateHasChangedAsync();
            await PopupService.EnqueueSnackbarAsync("成功，将重新登录", AlertTypes.Success);
            await Task.Delay(2000);
            NavigationManager.NavigateTo(NavigationManager.Uri);
        }
    }

    async Task OnUpdateUserInfoAsync()
    {
        await UserCenterService.UpdateUserInfoAsync(_updateInfoInput);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
}