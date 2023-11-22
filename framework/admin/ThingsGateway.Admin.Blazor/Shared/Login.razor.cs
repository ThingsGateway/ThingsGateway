#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
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
        _welcome = "��ӭʹ��" + _configTitle + "!";
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

                await PopupService.EnqueueSnackbarAsync(new("��¼����" + ": " + ret.Msg.ToString(), AlertTypes.Error));
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(new("��¼�ɹ�", AlertTypes.Success));
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

            await PopupService.EnqueueSnackbarAsync(new("��¼����", AlertTypes.Error));
        }
    }
    private async Task<string> RefreshCode()
    {
        _captchaInfo = _serviceScope.ServiceProvider.GetService<AuthService>().GetCaptchaInfo();
        return await Task.FromResult(_captchaInfo.CodeValue);
    }
}