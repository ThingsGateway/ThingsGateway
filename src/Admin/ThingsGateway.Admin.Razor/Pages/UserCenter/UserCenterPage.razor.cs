//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class UserCenterPage
{

    [CascadingParameter(Name = "ReloadMenu")]
    private Func<Task>? ReloadMenu { get; set; }

    [CascadingParameter(Name = "ReloadUser")]
    private Func<Task>? ReloadUser { get; set; }

    private SysUser SysUser { get; set; }

    private UpdatePasswordInput UpdatePasswordInput { get; set; } = new();

    [Inject]
    [NotNull]
    private IUserCenterService? UserCenterService { get; set; }

    private WorkbenchInfo WorkbenchInfo { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        SysUser = AppContext.CurrentUser.Adapt<SysUser>();
        SysUser.Avatar = AppContext.CurrentUser.Avatar;
        WorkbenchInfo = (await UserCenterService.GetLoginWorkbenchAsync(SysUser.Id)).Adapt<WorkbenchInfo>();

        await base.OnParametersSetAsync();
    }

    private async Task OnSavePassword(EditContext editContext)
    {
        try
        {
            await UserCenterService.UpdatePasswordAsync(UpdatePasswordInput);
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            await ToastService.Success(Localizer["UpdatePassword"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(Localizer["UpdatePassword"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    private async Task OnSaveUserInfo(EditContext editContext)
    {
        try
        {
            await UserCenterService.UpdateUserInfoAsync(SysUser);
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            await ToastService.Success(Localizer["UpdateUserInfo"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(Localizer["UpdateUserInfo"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    private async Task OnSaveWorkbench(EditContext editContext)
    {
        try
        {
            await UserCenterService.UpdateWorkbenchInfoAsync(WorkbenchInfo);
            if (ReloadMenu != null)
            {
                await ReloadMenu();
            }
            await ToastService.Success(Localizer["UpdateWorkbenchInfo"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(Localizer["UpdateWorkbenchInfo"], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }
}
