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
namespace ThingsGateway.Management.Razor;

using Microsoft.JSInterop;

using System.Collections.Generic;

using ThingsGateway.Gateway.Razor;
using ThingsGateway.Management.Application;

using TouchSocket.Core;

public sealed class ManagementGatewayExportService(IJSRuntime jSRuntime, IChannelPageService channelPageService, IDevicePageService devicePageService, IVariablePageService variablePageService, IMemoryVariablePageService memoryVariablePageService) : IGatewayExportService
{


    public async Task<bool> OnChannelExport(GatewayExportFilter exportFilter)
    {
        try
        {

            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;

            var item = await channelPageService.ExportChannelFileAsync(exportFilter).ConfigureAwait(false);

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

    public async Task<bool> OnChannelExport(List<Channel> data)
    {
        try
        {


            var item = await channelPageService.ExportChannelDataFileAsync(data).ConfigureAwait(false);

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
            var item = await devicePageService.ExportDeviceFileAsync(exportFilter).ConfigureAwait(false);

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

    public async Task<bool> OnDeviceExport(List<Device> data, string channelName, string plugin)
    {
        try
        {


            var item = await devicePageService.ExportDeviceDataFileAsync(data, channelName, plugin).ConfigureAwait(false);

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


            var item = await memoryVariablePageService.ExportMemoryVariableDataFileAsync(data, devName).ConfigureAwait(false);

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

    public async Task<bool> OnMemoryVariableExport(GatewayExportFilter exportFilter)
    {
        try
        {
            var item = await memoryVariablePageService.ExportMemoryVariableFileAsync(exportFilter).ConfigureAwait(false);

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
            var item = await variablePageService.ExportVariableFileAsync(exportFilter).ConfigureAwait(false);

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

    public async Task<bool> OnVariableExport(List<Variable> data, string devName)
    {
        try
        {


            var item = await variablePageService.ExportVariableDataFileAsync(data, devName).ConfigureAwait(false);

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

