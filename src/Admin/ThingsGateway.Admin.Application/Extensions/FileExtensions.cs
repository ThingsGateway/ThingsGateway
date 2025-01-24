// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class FileExtensions
{
    /// <summary>
    /// 存储本地文件
    /// </summary>
    /// <param name="pPath">存储的第一层目录</param>
    /// <param name="file"></param>
    /// <returns>文件全路径</returns>
    public static async Task<string> StorageLocal(this IBrowserFile file, string pPath = "imports")
    {
        string uploadFileFolder = App.WebHostEnvironment?.WebRootPath ?? "wwwroot"!;//赋值路径
        var now = CommonUtils.GetSingleId();
        var filePath = Path.Combine(uploadFileFolder, pPath);
        if (!Directory.Exists(filePath))//如果不存在就创建文件夹
            Directory.CreateDirectory(filePath);
        //var fileSuffix = Path.GetExtension(file.Name).ToLower();// 文件后缀
        var fileObjectName = $"{now}{file.Name}";//存储后的文件名
        var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路径
        fileName = fileName.Replace("\\", "/");//格式化一系
        //存储文件
        using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
        {
            using var fs = file.OpenReadStream(1024 * 1024 * 500);
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }
        return fileName;
    }
}
