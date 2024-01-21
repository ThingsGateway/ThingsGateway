#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation;

namespace ThingsGateway.Components
{
    public class PlatformService : IPlatformService
    {
        private BlazorAppService AppService;

        public PlatformService(BlazorAppService appService)
        {
            AppService = appService;
        }

        public async Task OnLogExport(string logPath)
        {
            var files = TextFileReader.GetFile(logPath);
            if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
            {
                return;
            }
            //统一web下载
            foreach (var item in files)
            {
                var path = Path.GetRelativePath("wwwroot", item.FullName);
                await AppService.DownFileAsync("File", DateTimeUtil.Now.ToFileDateTimeFormat() + ".txt", new { FileName = path });//
            }
        }
    }
}