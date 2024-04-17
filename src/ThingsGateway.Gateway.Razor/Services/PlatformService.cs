
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------





using Microsoft.JSInterop;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public class PlatformService : IPlatformService
{
    public PlatformService(IJSRuntime jSRuntime)
    {
        JSRuntime = jSRuntime;
    }

    private IJSRuntime JSRuntime { get; set; }

    public async Task OnLogExport(string logPath)
    {
        var files = TextFileReader.GetFiles(logPath);
        if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
        {
            return;
        }
        //统一web下载
        foreach (var item in files)
        {
            var path = Path.GetRelativePath("wwwroot", item.FullName);
            await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            string url = "api/file/download";
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            await ajaxJS.InvokeVoidAsync("blazor_downloadFile", url, fileName, new { FileName = path });
        }
    }
}