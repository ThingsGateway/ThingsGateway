//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

/// <inheritdoc/>
public partial class UserLogin
{
    /// <inheritdoc/>
    [Parameter]
    [EditorRequired]
    public LoginInput Model { get; set; }

    /// <inheritdoc/>
    [Parameter]
    [EditorRequired]
    public Func<EditContext, Task> OnLogin { [return: NotNull] get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<UserLogin>? Localizer { get; set; }
    [Inject]
    [NotNull]
    private IOptions<TenantOptions>? TenantOption { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    private List<SelectedItem> Items { get; set; }

    [Inject]
    private ISysOrgService SysOrgService { get; set; }
    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        var tenantList = await SysOrgService.GetTenantListAsync();
        Items = OrgUtil.BuildOrgSelectList(tenantList).ToList();
        Model.TenantId = tenantList.FirstOrDefault().Id;        //默认第一个

        await base.OnInitializedAsync();

        if (App.HostEnvironment.IsDevelopment())
        {
            Model.Account = "SuperAdmin";
            Model.Password = "111111";
        }
        else if (WebsiteOption.Value.Demo)
        {
            Model.Account = "SuperAdmin";
            Model.Password = "111111";
        }
    }
}
