//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Admin.Application.Services.Auth;
using ThingsGateway.Admin.Core.Utils;
using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Admin.Blazor;

public partial class Login
{
    private readonly LoginInput _loginModel = new();
    private PImageCaptcha _captcha;
    private string _captchaValue;
    private bool _showCaptcha;
    private bool _showPassword;

    private ValidCodeOutput _captchaInfo { get; set; }

    [Inject]
    private NavigationManager _NavigationManager { get; set; }

    [Inject]
    private UserResoures UserResoures { get; set; }

    private string _password { get; set; }
    private string _userLogoUrl { get; set; } = BlazorAppService.DefaultResourceUrl + "images/defaultUser.svg";
    private string _welcome { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        var isDemo = App.GetConfig<bool?>("Demo:IsDemo") ?? false;
        if (isDemo || App.WebHostEnvironment.IsDevelopment())
        {
            _loginModel.Account = "superAdmin";
            _password = "111111";
        }

        GetCaptchaInfo();
        _showCaptcha = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(CateGoryConst.LOGIN_POLICY, ConfigConst.LOGIN_CAPTCHA_OPEN))?.ConfigValue?.ToBoolean(false) == true;

        _welcome = "欢迎使用" + BlazorAppInfoConfigs.Current.Title + "!";
        await base.OnParametersSetAsync();
    }

    private async Task Enter(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await LoginAsync();
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }

    private void GetCaptchaInfo()
    {
        _captchaInfo = _serviceScope.ServiceProvider.GetService<AuthService>().GetCaptchaInfo();
    }

    private async Task LoginAsync()
    {
        _loginModel.ValidCodeReqNo = _captchaInfo.ValidCodeReqNo;
        _loginModel.ValidCode = _captchaValue;
        _loginModel.Password = CryptogramUtil.Sm2Encrypt(_password);
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
            Url = "/auth/login",
            Data = _loginModel,
        };
        var str = await AppService.GetMessageAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.FromJsonString<UnifyResult<LoginOutput>>();
            if (ret.Code != 200)
            {
                if (_captcha != null)
                {
                    await _captcha.RefreshCode();
                }

                await PopupService.EnqueueSnackbarAsync(new(AppService.I18n.T("登录错误") + ": " + ret.Msg.ToString(), AlertTypes.Error));
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(new(AppService.I18n.T("登录成功"), AlertTypes.Success));
                await Task.Delay(500);

                await UserResoures.InitAllAsync();

                var data = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync(ret.Data.Id);

                if (_NavigationManager.ToAbsoluteUri(_NavigationManager.Uri).AbsolutePath == "/Login" || _NavigationManager.ToAbsoluteUri(_NavigationManager.Uri).AbsolutePath == "/")
                    await AppService.GotoAsync(UserResoures.AllSameLevelMenuSpas.FirstOrDefault(a => a.Id == data.DefaultRazpor)?.Href ?? "index");
                else
                    await AppService.GotoAsync(_NavigationManager.Uri);
            }
        }
        else
        {
            if (_captcha != null)
            {
                await _captcha.RefreshCode();
            }
        }
    }

    private async Task<string> RefreshCode()
    {
        _captchaInfo = _serviceScope.ServiceProvider.GetService<AuthService>().GetCaptchaInfo();
        return await Task.FromResult(_captchaInfo.CodeValue);
    }
}