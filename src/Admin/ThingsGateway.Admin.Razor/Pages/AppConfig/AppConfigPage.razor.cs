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

public partial class AppConfigPage
{
    private AppConfig AppConfig;

    [Inject]
    [NotNull]
    private IStringLocalizer<AppConfig>? AppConfigLocalizer { get; set; }

    [Inject]
    [NotNull]
    private ISysDictService? SysDictService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        AppConfig = (await SysDictService.GetAppConfigAsync()).Adapt<AppConfig>();
        await base.OnParametersSetAsync();
    }

    #region 修改

    private async Task OnSaveLogin(EditContext editContext)
    {
        try
        {
            await SysDictService.EditLoginPolicyAsync(AppConfig.LoginPolicy);
            await ToastService.Success(AppConfigLocalizer[nameof(LoginPolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(AppConfigLocalizer[nameof(LoginPolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    private async Task OnSavePagePolicy(EditContext editContext)
    {
        try
        {
            await SysDictService.EditPagePolicyAsync(AppConfig.PagePolicy);
            await ToastService.Success(AppConfigLocalizer[nameof(PagePolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(AppConfigLocalizer[nameof(PagePolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    private async Task OnSavePassword(EditContext editContext)
    {
        try
        {
            await SysDictService.EditPasswordPolicyAsync(AppConfig.PasswordPolicy);
            await ToastService.Success(AppConfigLocalizer[nameof(PasswordPolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(AppConfigLocalizer[nameof(PasswordPolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    private async Task OnSaveWebsite(EditContext editContext)
    {
        try
        {
            await SysDictService.EditWebsitePolicyAsync(AppConfig.WebsitePolicy);
            await ToastService.Success(AppConfigLocalizer[nameof(WebsitePolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");
        }
        catch (Exception ex)
        {
            await ToastService.Warning(AppConfigLocalizer[nameof(WebsitePolicy)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex.Message]}");
        }
    }

    #endregion 修改
}
