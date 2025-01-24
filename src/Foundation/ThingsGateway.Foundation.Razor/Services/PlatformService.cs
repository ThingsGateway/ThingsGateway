//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.JSInterop;

using ThingsGateway.Extension;
using ThingsGateway.Foundation;

namespace ThingsGateway.Debug;

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
        if (!files.IsSuccess)
        {
            return;
        }
        //打开文件夹

        string url = "api/file/download";
        //统一web下载
        foreach (var item in files.Content)
        {
            await using var jSObject = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            await jSObject.InvokeVoidAsync("blazor_downloadFile", url, fileName, new { FileName = path });
        }
    }


}
