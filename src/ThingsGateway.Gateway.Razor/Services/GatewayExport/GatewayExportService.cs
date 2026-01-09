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
using Microsoft.JSInterop;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

internal sealed class GatewayExportService(IJSRuntime jSRuntime, IChannelPageService channelPageService, IDevicePageService devicePageService, IVariablePageService variablePageService, IMemoryVariablePageService memoryVariablePageService) : IGatewayExportService
{

    public async Task<bool> OnChannelExport(GatewayExportFilter exportFilter)
    {
        try
        {
            await using var ajaxJS = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
            string url = "api/gatewayExport/channel";
            string fileName = $"{DateTime.Now.ToFileDateTimeFormat()}.xlsx";
            return await ajaxJS.InvokeAsync<bool>("postJson_downloadFile", url, fileName, exportFilter.ToJsonString());
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

            var item = await channelPageService.ExportChannelDataFileAsync(data);

            //打开文件夹
            string url = "api/file/download";
            //统一web下载
            await using var jSObject = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            return await jSObject.InvokeAsync<bool>("blazor_downloadFile", url, fileName, new { FileName = path });

        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> OnDeviceExport(GatewayExportFilter exportFilter)
    {
        try
        {
            await using var ajaxJS = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
            string url = "api/gatewayExport/device";
            string fileName = $"{DateTime.Now.ToFileDateTimeFormat()}.xlsx";
            return await ajaxJS.InvokeAsync<bool>("postJson_downloadFile", url, fileName, exportFilter.ToJsonString());
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

            var item = await devicePageService.ExportDeviceDataFileAsync(data, channelName, plugin);

            //打开文件夹
            string url = "api/file/download";
            //统一web下载
            await using var jSObject = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            return await jSObject.InvokeAsync<bool>("blazor_downloadFile", url, fileName, new { FileName = path });

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

            await using var ajaxJS = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
            string url = "api/gatewayExport/variable";
            string fileName = $"{DateTime.Now.ToFileDateTimeFormat()}.xlsx";
            return await ajaxJS.InvokeAsync<bool>("postJson_downloadFile", url, fileName, exportFilter.ToJsonString());
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

            await using var ajaxJS = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
            string url = "api/gatewayExport/memoryvariable";
            string fileName = $"{DateTime.Now.ToFileDateTimeFormat()}.xlsx";
            return await ajaxJS.InvokeAsync<bool>("postJson_downloadFile", url, fileName, exportFilter.ToJsonString());
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

            var item = await variablePageService.ExportVariableDataFileAsync(data, devName);

            //打开文件夹
            string url = "api/file/download";
            //统一web下载
            await using var jSObject = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            return await jSObject.InvokeAsync<bool>("blazor_downloadFile", url, fileName, new { FileName = path });

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

            var item = await memoryVariablePageService.ExportMemoryVariableDataFileAsync(data, devName);

            //打开文件夹
            string url = "api/file/download";
            //统一web下载
            await using var jSObject = await jSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            return await jSObject.InvokeAsync<bool>("blazor_downloadFile", url, fileName, new { FileName = path });

        }
        catch
        {
            return false;
        }
    }

}
