﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Mapster;

using Microsoft.JSInterop;

using ThingsGateway.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

internal sealed class GatewayExportService : IGatewayExportService
{
    public GatewayExportService(IJSRuntime jSRuntime)
    {
        JSRuntime = jSRuntime;
    }

    private IJSRuntime JSRuntime { get; set; }

    public async Task OnChannelExport(QueryPageOptions dtoObject)
    {
        await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
        string url = "api/gatewayExport/channel";
        string fileName = DateTime.Now.ToFileDateTimeFormat();
        await ajaxJS.InvokeVoidAsync("postJson_downloadFile", url, fileName, new ExportDto() { FilterKeyValueAction = dtoObject.ToFilter(), QueryPageOptions = dtoObject.Adapt<QueryPageOptionsDto>() }.ToJsonString());
    }

    public async Task OnDeviceExport(QueryPageOptions dtoObject, bool collect)
    {
        await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
        string url = collect ? "api/gatewayExport/collectdevice" : "api/gatewayExport/businessdevice";
        string fileName = DateTime.Now.ToFileDateTimeFormat();
        await ajaxJS.InvokeVoidAsync("postJson_downloadFile", url, fileName, new ExportDto() { FilterKeyValueAction = dtoObject.ToFilter(), QueryPageOptions = dtoObject.Adapt<QueryPageOptionsDto>() }.ToJsonString());
    }

    public async Task OnVariableExport(QueryPageOptions dtoObject)
    {
        await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
        string url = "api/gatewayExport/variable";
        string fileName = DateTime.Now.ToFileDateTimeFormat();
        await ajaxJS.InvokeVoidAsync("postJson_downloadFile", url, fileName, new ExportDto() { FilterKeyValueAction = dtoObject.ToFilter(), QueryPageOptions = dtoObject.Adapt<QueryPageOptionsDto>() }.ToJsonString());

    }
}
