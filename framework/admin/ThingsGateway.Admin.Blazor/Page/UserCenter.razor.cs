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

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 个人设置
/// </summary>
public partial class UserCenter
{
    private readonly PasswordInfoInput _passwordInfoInput = new();
    private readonly UpdateInfoInput _updateInfoInput = new();
    private List<long> _menusChoice = new();
    private long _defaultMenuId { get; set; }

    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _defaultMenuId = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginDefaultRazorAsync(UserManager.UserId);
        _updateInfoInput.Email = UserResoures.CurrentUser.Email;
        _updateInfoInput.Phone = UserResoures.CurrentUser.Phone;
        _menusChoice = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync();
        await base.OnParametersSetAsync();
    }

    private async Task OnDefaultRazorSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateUserDefaultRazorAsync(UserManager.UserId, _defaultMenuId);
        await _mainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }

    private async Task OnShortcutSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateWorkbenchAsync(_menusChoice);
        await _mainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }

    private async Task OnUpdatePasswordInfoAsync(FormContext context)
    {
        var success = context.Validate();
        if (success)
        {
            //验证成功，操作业务
            _passwordInfoInput.Id = UserResoures.CurrentUser.Id;
            await _serviceScope.ServiceProvider.GetService<IUserCenterService>().EditPasswordAsync(_passwordInfoInput);
            await _mainLayout.StateHasChangedAsync();
            await PopupService.EnqueueSnackbarAsync("成功，将重新登录", AlertTypes.Success);
            await Task.Delay(2000);
            _navigationManager.NavigateTo(_navigationManager.Uri);
        }
    }

    private async Task OnUpdateUserInfoAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateUserInfoAsync(_updateInfoInput);
        await _mainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
}