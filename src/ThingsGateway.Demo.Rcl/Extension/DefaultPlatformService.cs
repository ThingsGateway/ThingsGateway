//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThingsGateway.Components
{
    public class DefaultPlatformService : IPlatformService
    {
        public Task OnLogExport(string logPath)
        {
            OpenFolder(logPath);
            return Task.CompletedTask;
        }

        private static void OpenFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("文件夹路径不能为空。", nameof(folderPath));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows 平台使用 Explorer 打开文件夹
                Process.Start("explorer.exe", folderPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS 平台使用 Finder 打开文件夹
                Process.Start("open", folderPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux 平台使用 xdg-open 打开文件夹
                Process.Start("xdg-open", folderPath);
            }
            else
            {
                throw new PlatformNotSupportedException("无法在当前操作系统上打开文件夹。");
            }
        }
    }
}