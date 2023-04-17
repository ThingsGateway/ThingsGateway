using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Core.Utils;
using ThingsGateway.Web.Rcl.Core;

namespace ThingsGateway.Web.Rcl
{
    public partial class Login
    {
        private string CaptchaValue;

        private LoginInput loginModel = new LoginInput();

        [Inject]
        public AjaxService AjaxService { get; set; }

        [Inject]
        public IAuthService AuthService { get; set; }

        [Inject]
        public IConfigService ConfigService { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Parameter]
        public string UserLogoUrl { get; set; } = BlazorConst.ResourceUrl + "images/defaultUser.svg";

        [Parameter]
        public string Welcome { get; set; }

        private ValidCodeOutPut CaptchaInfo { get; set; }

        private string Password { get; set; }

        private string SYS_DEFAULT_REMARK { get; set; }

        private string SYS_DEFAULT_TITLE { get; set; }

        private async Task Enter(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await LoginAsync();
            }
        }
        private PImageCaptcha captcha;
        private async Task LoginAsync()
        {
            loginModel.ValidCodeReqNo = CaptchaInfo?.ValidCodeReqNo;
            loginModel.ValidCode = CaptchaValue;
            loginModel.Password = CryptogramUtil.Sm4Encrypt(Password);
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
                var ret = str.ToJsonEntity<UnifyResult<LoginOutPut>>();
                if (ret.Code != 200)
                {
                    await captcha.RefreshCode();
                    await PopupService.EnqueueSnackbarAsync(T("µÇÂ¼´íÎó") + ": " + ret.Msg.ToString(), AlertTypes.Error);
                }
                else
                {
                    await PopupService.EnqueueSnackbarAsync(T("µÇÂ¼³É¹¦"), AlertTypes.Success);
                    await Task.Delay(500);
                    await AjaxService.GotoAsync("/");
                }
            }
            else
            {
                await captcha.RefreshCode();
                await PopupService.EnqueueSnackbarAsync(@T("µÇÂ¼´íÎó"), AlertTypes.Error);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (App.HostEnvironment.IsDevelopment())
            {
                loginModel.Account = "superAdmin";
                Password = "111111";
            }
            GetCaptchaInfo();
            SYS_DEFAULT_TITLE = (await ConfigService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_TITLE)).ConfigValue;
            SYS_DEFAULT_REMARK = (await ConfigService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_REMARK))?.ConfigValue;
            _showCaptcha = (await ConfigService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_CAPTCHA_OPEN))?.ConfigValue?.ToBoolean() == true;
            Welcome = T("»¶Ó­Ê¹ÓÃ") + SYS_DEFAULT_TITLE + "!";
            await base.OnInitializedAsync();
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
}