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

using ThingsGateway.Admin.Core.Utils;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 个人设置
/// </summary>
public partial class UserCenter
{
    private UpdatePasswordInput _passwordInfoInput = new();
    private readonly UpdateInfoInput _updateInfoInput = new();
    private UpdateWorkbenchInput _menusChoice = new();
    private UpdateDefaultRazorInput _defaultMenuId { get; set; } = new();

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _defaultMenuId.DefaultRazorData = (await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync(UserManager.UserId)).DefaultRazpor;
        _updateInfoInput.Email = UserResoures.CurrentUser.Email;
        _updateInfoInput.Phone = UserResoures.CurrentUser.Phone;
        _menusChoice.WorkbenchData = (await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync(UserManager.UserId)).Shortcut;
        await base.OnParametersSetAsync();
    }

    private async Task OnDefaultRazorSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateDefaultRazorAsync(_defaultMenuId);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }

    private async Task OnShortcutSaveAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateWorkbenchAsync(_menusChoice);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }

    private async Task OnUpdatePasswordInfoAsync(FormContext context)
    {
        var success = context.Validate();
        if (success)
        {
            //验证成功，操作业务
            _passwordInfoInput.Id = UserResoures.CurrentUser.Id;
            _passwordInfoInput.Password = CryptogramUtil.Sm2Encrypt(_passwordInfoInput.Password);
            _passwordInfoInput.NewPassword = CryptogramUtil.Sm2Encrypt(_passwordInfoInput.NewPassword);
            await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdatePasswordAsync(_passwordInfoInput);
            _passwordInfoInput = new();
            await MainLayout.StateHasChangedAsync();
            await PopupService.EnqueueSnackbarAsync("成功，将重新登录", AlertTypes.Success);
            await Task.Delay(2000);
            _navigationManager.NavigateTo(_navigationManager.Uri);
        }
    }

    private async Task OnUpdateUserInfoAsync()
    {
        await _serviceScope.ServiceProvider.GetService<IUserCenterService>().UpdateUserInfoAsync(_updateInfoInput);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
    }
}