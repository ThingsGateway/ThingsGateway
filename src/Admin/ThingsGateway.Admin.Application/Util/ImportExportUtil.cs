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

using ThingsGateway.NewLife;

namespace ThingsGateway.Admin.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class ImportExportUtil
{
    public static string GetFileDir(ref string fileName)
    {
        if (!fileName.Contains('.'))
            fileName += ".xlsx";

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
        Directory.CreateDirectory(path);

        string searchPattern = $"*{fileName}"; // 文件名匹配模式
        string[] files = Directory.GetFiles(path, searchPattern);

        //删除同后缀的文件
        var whereFiles = files.Where(file => File.GetLastWriteTime(file) < DateTime.Now.AddMinutes(-2));

        foreach (var file in whereFiles)
        {
            FileUtil.DeleteFile(file);
        }

        return path;
    }

}
