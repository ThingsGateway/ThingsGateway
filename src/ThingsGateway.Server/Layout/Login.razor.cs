//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Common;
using ThingsGateway.DataEncryption;
using ThingsGateway.Foundation.Common.StringExtension;
using ThingsGateway.Razor;

namespace ThingsGateway.Server;

public partial class Login
{
    private string _versionString = string.Empty;
    private LoginInput loginModel = new LoginInput();

    [SupplyParameterFromQuery]
    [Parameter]
    public string? ReturnUrl { get; set; }

    [Inject]
    [NotNull]
    private AjaxService? AjaxService { get; set; }
    [Inject]
    [NotNull]
    private IAuthRazorService? AuthRazorService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<Login>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private IAppVersionService? VersionService { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    protected override Task OnInitializedAsync()
    {
        _versionString = $"v{VersionService.Version}";
        return base.OnInitializedAsync();
    }

    private void GiteeLogin()
    {
        var websiteOptions = App.GetOptions<WebsiteOptions>()!;
        if (websiteOptions.Demo)
        {
            NavigationManager.NavigateTo("/api/auth/oauth-login?scheme=Gitee", forceLoad: true);
        }
    }

    private void GithubLogin()
    {
        var websiteOptions = App.GetOptions<WebsiteOptions>()!;
        if (websiteOptions.Demo)
        {
            NavigationManager.NavigateTo("/api/auth/oauth-login?scheme=Github", forceLoad: true);
        }
    }

    [Inject]
    NavigationManager NavigationManager { get; set; }
    private async Task LoginAsync(EditContext context)
    {
        var websiteOptions = App.GetOptions<WebsiteOptions>()!;
        if (websiteOptions.Demo)
        {
            NavigationManager.NavigateTo("/api/auth/oauth-login", forceLoad: true);
        }
        else
        {
            var model = loginModel.AdaptLoginInput();
            model.Password = DESEncryption.Encrypt(model.Password);

            try
            {
                var ret = await AuthRazorService.LoginAsync(model);

                if (ret.Code != 200)
                {
                    await ToastService.Error(Localizer["LoginErrorh1"], $"{ret.Msg}");
                }
                else
                {
                    await ToastService.Information(Localizer["LoginSuccessh1"], Localizer["LoginSuccessc1"]);
                    await Task.Delay(1000);

                    if (ReturnUrl.IsNullOrWhiteSpace() || ReturnUrl == @"/")
                    {
                        await AjaxService.Goto(ReturnUrl ?? "/");
                    }
                    else
                    {
                        await AjaxService.Goto(ReturnUrl);
                    }
                }
            }
            catch
            {
                await ToastService.Error(Localizer["LoginErrorh2"], Localizer["LoginErrorc2"]);
            }
        }
    }
}
