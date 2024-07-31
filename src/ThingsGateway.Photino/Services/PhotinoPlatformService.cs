//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.Gateway.Razor;
using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

public class PhotinoPlatformService : IPlatformService
{
    private DownloadService DownloadService { get; set; }
    public PhotinoPlatformService(DownloadService downloadService)
    {
        DownloadService = downloadService;
    }


    public async Task OnChannelExport(QueryPageOptions options)
    {
        var service = NetCoreApp.RootServices.GetRequiredService<IChannelService>();
        var data=await service.PageAsync(options);
        if (data.Items.Count() > 0)
        {
            using var memoryStream = await NetCoreApp.RootServices.GetRequiredService<IChannelService>().ExportMemoryStream(data.Items.ToList());
            await DownloadService.DownloadFromStreamAsync($"channel{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
        }
    }

    public async Task OnDeviceExport(QueryPageOptions options, bool collect)
    {
        var service = NetCoreApp.RootServices.GetRequiredService<IDeviceService>();
        var data = await service.PageAsync(options, collect ? PluginTypeEnum.Collect : PluginTypeEnum.Business);
        if (data.Items.Count() > 0)
        {
            using var memoryStream = await NetCoreApp.RootServices.GetRequiredService<IDeviceService>().ExportMemoryStream(data.Items.ToList(), collect?PluginTypeEnum.Collect:PluginTypeEnum.Business);
            await DownloadService.DownloadFromStreamAsync($"device{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
        }
    }

    public Task OnLogExport(string logPath)
    {
        OpenFolder(logPath);
        return Task.CompletedTask;
    }

    public async Task OnVariableExport(QueryPageOptions options)
    {
        var service = NetCoreApp.RootServices.GetRequiredService<IVariableService>();
        var data = await service.PageAsync(options);
        if (data.Items.Count() > 0)
        {
            using var memoryStream = await NetCoreApp.RootServices.GetRequiredService<IVariableService>().ExportMemoryStream(data.Items.ToList());
            await DownloadService.DownloadFromStreamAsync($"variable{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
        }

    }

    private void OpenFolder(string path)
    {
        // Normalize the path for the current operating system
        path = System.IO.Path.GetFullPath(path); // Ensure the path is absolute

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            System.Diagnostics.Process.Start("xdg-open", path);
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        {
            System.Diagnostics.Process.Start("open", path);
        }
    }
}
