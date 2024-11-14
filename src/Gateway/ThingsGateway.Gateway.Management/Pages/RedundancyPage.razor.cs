//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

namespace ThingsGateway.Gateway.Management;

public partial class RedundancyPage
{
    private Redundancy Redundancy { get; set; } = new();

    [Inject]
    [NotNull]
    private IStringLocalizer<Redundancy>? RedundancyLocalizer { get; set; }

    [Inject]
    [NotNull]
    private IRedundancyHostedService? RedundancyHostedService { get; set; }

    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }
    [Inject]
    [NotNull]
    private IRedundancyService? RedundancyService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        Redundancy = (await RedundancyService.GetRedundancyAsync()).Adapt<Redundancy>();
        await base.OnParametersSetAsync();
    }

    #region 修改



    private async Task OnSwitch()
    {
        try
        {
            var ret = await SwalService.ShowModal(new SwalOption()
            {
                Category = SwalCategory.Warning,
                Title = RedundancyLocalizer["Confirm"]
            });
            if (ret)
            {
                var result = await RedundancyHostedService.SwitchModeAsync();
                if (result.IsSuccess)
                    await ToastService.Success(RedundancyLocalizer[nameof(Redundancy)], $"{RazorLocalizer["Success"]}");
                else
                    await ToastService.Warning(RedundancyLocalizer[nameof(Redundancy)], $"{RazorLocalizer["Fail", result.ToString()]}");
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warning(RedundancyLocalizer[nameof(Redundancy)], $"{RazorLocalizer["Fail", ex]}");
        }
    }

    #endregion 修改

}
