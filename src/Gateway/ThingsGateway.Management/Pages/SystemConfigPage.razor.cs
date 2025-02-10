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
using Mapster;

namespace ThingsGateway.Management;

public partial class SystemConfigPage
{
    private RedundancyOptions RedundancyOptions { get; set; } = new();

    [Inject]
    [NotNull]
    private IStringLocalizer<RedundancyOptions>? RedundancyLocalizer { get; set; }


    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }
    [Inject]
    [NotNull]
    private IRedundancyService? RedundancyService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        RedundancyOptions = (await RedundancyService.GetRedundancyAsync()).Adapt<RedundancyOptions>();
        await base.OnParametersSetAsync();
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            TabItem?.SetHeader(AppContext.TitleLocalizer["系统管理"]);

        if (firstRender && Tab != null && tabComponent != null)
        {
            tabComponent.ActiveTab(Tab.Value);
        }
        return base.OnAfterRenderAsync(firstRender);
    }
    [CascadingParameter]
    [NotNull]
    private TabItem? TabItem { get; set; }
    [Inject]
    [NotNull]
    private IRedundancyHostedService? RedundancyHostedService { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Management._Imports> ManagementLocalizer { get; set; }

    private async Task OnRestart()
    {
        var result = await SwalService.ShowModal(new SwalOption()
        {
            Category = SwalCategory.Warning,
            Title = ManagementLocalizer["Restart"]
        });
        if (result)
        {
            RestartServerHelper.RestartServer();
        }
    }


    [Parameter]
    [SupplyParameterFromQuery]
    public int? Tab { get; set; }

}
