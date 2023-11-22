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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Blazor;

public partial class Login
{
    private readonly LoginInput _loginModel = new();
    private PImageCaptcha _captcha;
    private string _captchaValue;
    private bool _showCaptcha;
    private bool _showPassword;
    [Inject]
    private AjaxService _ajaxService { get; set; }
    private ValidCodeOutput _captchaInfo { get; set; }
    private string _configRemark { get; set; }
    private string _configTitle { get; set; }
    [Inject]
    private NavigationManager _NavigationManager { get; set; }

    private string _password { get; set; }
    private string _userLogoUrl { get; set; } = BlazorResourceConst.ResourceUrl + "images/defaultUser.svg";
    private string _welcome { get; set; }
    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (App.WebHostEnvironment.IsDevelopment())
        {
            _loginModel.Account = "superAdmin";
            _password = "111111";
        }

        GetCaptchaInfo();
        _configTitle = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_TITLE))?.ConfigValue;
        _configRemark = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_REMARK))?.ConfigValue;
        _showCaptcha = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_CAPTCHA_OPEN))?.ConfigValue?.ToBool(false) == true;
        _welcome = "欢迎使用" + _configTitle + "!";
        await base.OnParametersSetAsync();
    }

    private async Task Enter(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await LoginAsync();
        }
    }


    private void GetCaptchaInfo()
    {
        _captchaInfo = _serviceScope.ServiceProvider.GetService<AuthService>().GetCaptchaInfo();
    }

    private async Task LoginAsync()
    {
        _loginModel.ValidCodeReqNo = _captchaInfo.ValidCodeReqNo;
        _loginModel.ValidCode = _captchaValue;
        _loginModel.Password = DESCEncryption.Encrypt(_password, DESCKeyConst.DESCKey);
        if (IsMobile)
        {
            _loginModel.Device = AuthDeviceTypeEnum.APP;
        }
        else
        {
            _loginModel.Device = AuthDeviceTypeEnum.PC;
        }

        var ajaxOption = new AjaxOption
        {
            Url = "/auth/b/login",
            Data = _loginModel,
        };
        var str = await _ajaxService.GetMessageAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.FromJsonString<UnifyResult<LoginOutput>>();
            if (ret.Code != 200)
            {
                if (_captcha != null)
                {
                    await _captcha.RefreshCode();
                }

                await PopupService.EnqueueSnackbarAsync(new("登录错误" + ": " + ret.Msg.ToString(), AlertTypes.Error));
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(new("登录成功", AlertTypes.Success));
                await Task.Delay(500);
                var userId = await _serviceScope.ServiceProvider.GetService<SysUserService>().GetIdByAccountAsync(_loginModel.Account);
                var data = await _serviceScope.ServiceProvider.GetService<UserCenterService>().GetLoginDefaultRazorAsync(userId);
                var sameLevelMenus = await _serviceScope.ServiceProvider.GetService<ResourceService>().GetaMenuAndSpaListAsync();
                if (_NavigationManager.ToAbsoluteUri(_NavigationManager.Uri).AbsolutePath == "/Login" || _NavigationManager.ToAbsoluteUri(_NavigationManager.Uri).AbsolutePath == "/")
                    await _ajaxService.GotoAsync(sameLevelMenus.FirstOrDefault(a => a.Id == data)?.Component ?? "index");
                else
                    await _ajaxService.GotoAsync(_NavigationManager.Uri);
            }
        }
        else
        {
            if (_captcha != null)
            {
                await _captcha.RefreshCode();
            }

            await PopupService.EnqueueSnackbarAsync(new("登录错误", AlertTypes.Error));
        }
    }
    private async Task<string> RefreshCode()
    {
        _captchaInfo = _serviceScope.ServiceProvider.GetService<AuthService>().GetCaptchaInfo();
        return await Task.FromResult(_captchaInfo.CodeValue);
    }
}