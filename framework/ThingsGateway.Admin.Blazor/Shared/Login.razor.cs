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

using Furion.DataEncryption;

using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;


using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 登录页面
/// </summary>
public partial class Login
{
    private string CaptchaValue;
    bool _showPassword;
    bool _showCaptcha;
    private readonly LoginInput loginModel = new();


    [Inject]
    AjaxService AjaxService { get; set; }


    [Inject]
    IAuthService AuthService { get; set; }



    string UserLogoUrl { get; set; } = BlazorResourceConst.ResourceUrl + "images/defaultUser.svg";

    string Welcome { get; set; }

    private ValidCodeOutput CaptchaInfo { get; set; }

    private string Password { get; set; }

    private string CONFIG_REMARK { get; set; }

    private string CONFIG_TITLE { get; set; }

    private async Task Enter(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await LoginAsync();
        }
    }
    private PImageCaptcha captcha;
    [Inject]
    IUserCenterService UserCenterService { get; set; }
    [Inject]
    IResourceService ResourceService { get; set; }
    [Inject]
    ISysUserService SysUserService { get; set; }
    private async Task LoginAsync()
    {
        loginModel.ValidCodeReqNo = CaptchaInfo.ValidCodeReqNo;
        loginModel.ValidCode = CaptchaValue;
        loginModel.Password = DESCEncryption.Encrypt(Password, DESCKeyConst.DESCKey);
        if (IsMobile)
        {
            loginModel.Device = AuthDeviceTypeEnum.APP;
        }
        else
        {
            loginModel.Device = AuthDeviceTypeEnum.PC;
        }

        var ajaxOption = new AjaxOption { Url = "/auth/b/login", Data = loginModel, };
        var str = await AjaxService.GetMessageAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.ToJsonWithT<UnifyResult<LoginOutput>>();
            if (ret.Code != 200)
            {
                if (captcha != null)
                {
                    await captcha.RefreshCode();
                }
                await PopupService.EnqueueSnackbarAsync(new("登录错误" + ": " + ret.Msg.ToString(), AlertTypes.Error));
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(new("登录成功", AlertTypes.Success));
                await Task.Delay(500);
                var userId = await SysUserService.GetIdByAccountAsync(loginModel.Account);
                var data = await UserCenterService.GetLoginDefaultRazorAsync(userId);
                var sameLevelMenus = await ResourceService.GetaMenuAndSpaListAsync();
                if (NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/Login" || NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/")
                    await AjaxService.GotoAsync(sameLevelMenus.FirstOrDefault(a => a.Id == data)?.Component ?? "index");
                else
                    await AjaxService.GotoAsync(NavigationManager.Uri);
            }
        }
        else
        {
            if (captcha != null)
            {
                await captcha.RefreshCode();
            }
            await PopupService.EnqueueSnackbarAsync(new("登录错误", AlertTypes.Error));
        }
    }
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (App.HostEnvironment.IsDevelopment())
        {
            loginModel.Account = "superAdmin";
            Password = "111111";
        }
        GetCaptchaInfo();
        CONFIG_TITLE = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_TITLE))?.ConfigValue;
        CONFIG_REMARK = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_REMARK))?.ConfigValue;
        _showCaptcha = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_CAPTCHA_OPEN))?.ConfigValue?.ToBoolean() == true;
        Welcome = "欢迎使用" + CONFIG_TITLE + "!";
        await base.OnParametersSetAsync();
    }

    private void GetCaptchaInfo()
    {
        CaptchaInfo = AuthService.GetCaptchaInfo();
    }

    private Task<string> RefreshCode()
    {
        CaptchaInfo = AuthService.GetCaptchaInfo();
        return Task.FromResult(CaptchaInfo.CodeValue);
    }
}