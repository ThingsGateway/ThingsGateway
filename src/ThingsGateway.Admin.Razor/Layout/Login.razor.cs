
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using NewLife.Extension;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core;
using ThingsGateway.Core.Json.Extension;

namespace ThingsGateway.Admin.Razor;

public partial class Login
{
    private LoginInput loginModel = new LoginInput();

    private AuthDeviceTypeEnum authDeviceTypeEnum;

    [SupplyParameterFromQuery]
    [Parameter]
    public string? ReturnUrl { get; set; }

    [Inject]
    [NotNull]
    private AjaxService? AjaxService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<Login>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    private Task OnChanged(BreakPoint breakPoint)
    {
        authDeviceTypeEnum = breakPoint > BreakPoint.Medium ? AuthDeviceTypeEnum.PC : AuthDeviceTypeEnum.APP;
        return Task.CompletedTask;
    }

    private async Task LoginAsync(EditContext context)
    {
        var model = loginModel.Adapt<LoginInput>();
        model.Password = DESCEncryption.Encrypt(model.Password);
        model.Device = authDeviceTypeEnum;

        var ajaxOption = new AjaxOption
        {
            Url = "/api/auth/login",
            Method = "POST",
            Data = model,
        };
        using var str = await AjaxService.InvokeAsync(ajaxOption);
        if (str != null)
        {
            var ret = str.RootElement.GetRawText().FromSystemTextJsonString<UnifyResult<LoginOutput>>();
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
                    await AjaxService.Goto(ret.Data!.DefaultRazor ?? ReturnUrl ?? "/");
                }
                else
                {
                    await AjaxService.Goto(ReturnUrl);
                }
            }
        }
        else
        {
            await ToastService.Error(Localizer["LoginErrorh2"], Localizer["LoginErrorc2"]);
        }
    }
}