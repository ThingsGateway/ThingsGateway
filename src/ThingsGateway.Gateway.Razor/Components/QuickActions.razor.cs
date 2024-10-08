//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

using System.Diagnostics;

using ThingsGateway.Admin.Razor;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife;

namespace ThingsGateway.Gateway.Razor;

/// <summary>
/// 快捷操作
/// </summary>
public partial class QuickActions
{
    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }

    private string? HeaderText { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<QuickActions>? Localizer { get; set; }

    [Inject]
    private IPluginService PluginService { get; set; }

    private string? ReloadPluginConfirmText { get; set; }
    private string? RestartText { get; set; }
    private string? ReloadServiceConfirmText { get; set; }
    private string? ReloadServiceText { get; set; }
    private string? TooltipText { get; set; }

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole("/gateway/devicestatus", operate);
    }

    protected override void OnInitialized()
    {
        TooltipText ??= Localizer[nameof(TooltipText)];
        HeaderText ??= Localizer[nameof(HeaderText)];

        RestartText ??= Localizer[nameof(RestartText)];
        ReloadServiceText ??= Localizer[nameof(ReloadServiceText)];
        ReloadPluginConfirmText ??= Localizer[nameof(ReloadPluginConfirmText)];
        ReloadServiceConfirmText ??= Localizer[nameof(ReloadServiceConfirmText)];
        base.OnInitialized();
    }

    private async Task OnReloadService()
    {
        try
        {
            await Task.Run(async () =>
            {
                await GlobalData.CollectDeviceHostedService.RestartAsync();
            });
        }
        finally
        {
        }
    }


    [Inject]
    private IHostApplicationLifetime ApplicationLifetime { get; set; }
    private async Task OnRestart()
    {
        try
        {
            await Task.Run(async () =>
            {

                // 启动进程
                var process = Process.GetCurrentProcess();
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = ProcessHelper.GetCommandLine(process.Id),
                    });
                ApplicationLifetime.StopApplication();
                await Task.Delay(1000);

                //if (!Runtime.IsConsole) process.CloseMainWindow();
                Environment.Exit(0);
                //process.Kill();
            });

        }
        finally
        {
        }
    }
    private async Task ToggleOpen()
    {
        await Module!.InvokeVoidAsync("toggle", Id);
    }
}
