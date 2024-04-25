
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core;

namespace ThingsGateway.Admin.Razor;

public partial class UserLogin
{
    [Parameter]
    [EditorRequired]
    public LoginInput Model { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<EditContext, Task> OnLogin { [return: NotNull] get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<UserLogin>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
#if DEBUG
            Model.Account = "SuperAdmin";
            Model.Password = "111111";
#else
        if (WebsiteOption.Value.Demo)
        {
            Model.Account = "SuperAdmin";
            Model.Password = "111111";
        }
#endif

    }
}
