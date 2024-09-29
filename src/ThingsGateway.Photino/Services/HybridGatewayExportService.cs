//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Application;

internal class HybridGatewayExportService : IGatewayExportService
{

    private readonly IChannelService _channelService;
    private readonly IDeviceService _deviceService;
    private readonly IVariableService _variableService;
    private readonly IImportExportService _importExportService;


    public HybridGatewayExportService(
        IChannelService channelService,
        IDeviceService deviceService,
        IVariableService variableService,
        IImportExportService importExportService
        )
    {
        _channelService = channelService;
        _deviceService = deviceService;
        _variableService = variableService;
        _importExportService = importExportService;

    }

    public async Task OnChannelExport(QueryPageOptions dtoObject)
    {
        dtoObject.IsPage = false;
        dtoObject.IsVirtualScroll = false;

        var sheets = await _deviceService.ExportDeviceAsync(dtoObject, PluginTypeEnum.Business).ConfigureAwait(false);
        var path = await _importExportService.CreateFileAsync<Device>(sheets, "BusinessDevice", false).ConfigureAwait(false);

        Open(path);
    }

    private static void Open(string path)
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
    }

    public async Task OnDeviceExport(QueryPageOptions dtoObject, bool collect)
    {
        dtoObject.IsPage = false;
        dtoObject.IsVirtualScroll = false;
        var sheets = await _deviceService.ExportDeviceAsync(dtoObject, collect ? PluginTypeEnum.Collect : PluginTypeEnum.Business).ConfigureAwait(false);
        var path = await _importExportService.CreateFileAsync<Device>(sheets, collect ? "CollectDevice" : "BusinessDevice", false).ConfigureAwait(false);
        Open(path);


    }

    public async Task OnVariableExport(QueryPageOptions dtoObject)
    {
        dtoObject.IsPage = false;
        dtoObject.IsVirtualScroll = false;
        var sheets = await _variableService.ExportVariableAsync(dtoObject).ConfigureAwait(false);
        var path = await _importExportService.CreateFileAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
        Open(path);
    }
}
