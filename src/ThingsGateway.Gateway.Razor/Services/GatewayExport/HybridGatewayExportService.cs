//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Admin.Application;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public sealed class HybridGatewayExportService(IChannelRuntimeService channelService, IDeviceRuntimeService deviceService, IVariableRuntimeService variableService, IMemoryVariableRuntimeService memoryVariableService, IImportExportService importExportService) : IGatewayExportService
{


    public async Task<bool> OnChannelExport(GatewayExportFilter exportFilter)
    {
        try
        {
            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;

            var sheets = await channelService.ExportChannelAsync(exportFilter).ConfigureAwait(false);
            var path = await importExportService.CreateFileAsync<Channel>(sheets, "Channel", false).ConfigureAwait(false);

            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Open(string path)
    {
        path = System.IO.Path.GetDirectoryName(path); // Ensure the path is absolute

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

        return true;
    }

    public async Task<bool> OnDeviceExport(GatewayExportFilter exportFilter)
    {
        try
        {
            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;
            var sheets = await deviceService.ExportDeviceAsync(exportFilter).ConfigureAwait(false);
            var path = await importExportService.CreateFileAsync<Device>(sheets, "Device", false).ConfigureAwait(false);
            Open(path);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnVariableExport(GatewayExportFilter exportFilter)
    {
        try
        {
            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;
            var sheets = await variableService.ExportVariableAsync(exportFilter).ConfigureAwait(false);
            var path = await importExportService.CreateFileAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnChannelExport(List<Channel> data)
    {
        try
        {
            var path = await channelService.ExportChannelDataFileAsync(data).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnDeviceExport(List<Device> data, string channelName, string plugin)
    {
        try
        {
            var path = await deviceService.ExportDeviceDataFileAsync(data, channelName, plugin).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnVariableExport(List<Variable> data, string devName)
    {
        try
        {
            var path = await variableService.ExportVariableDataFileAsync(data, devName).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnMemoryVariableExport(List<MemoryVariable> data, string devName)
    {
        try
        {
            var path = await memoryVariableService.ExportMemoryVariableDataFileAsync(data, devName).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnMemoryVariableExport(GatewayExportFilter exportFilter)
    {
        try
        {
            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;
            var sheets = await memoryVariableService.ExportVariableAsync(exportFilter).ConfigureAwait(false);
            var path = await importExportService.CreateFileAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
