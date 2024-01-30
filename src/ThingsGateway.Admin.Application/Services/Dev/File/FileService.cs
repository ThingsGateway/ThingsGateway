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

using Furion.FriendlyException;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using System.Text;
using System.Web;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IFileService"/>
/// </summary>
public class FileService : IFileService
{
    /// <inheritdoc/>
    public async Task<string> UploadFileAsync(string pPath, IBrowserFile file)
    {
        return await StorageLocal(pPath, file);
    }

    /// <inheritdoc/>
    public void Verification(IBrowserFile file, int maxSzie = 200, string[] allowTypes = null)
    {
        if (file == null) throw Oops.Bah("文件不能为空");
        if (file.Size > maxSzie * 1024 * 1024) throw Oops.Bah($"文件大小不允许超过{maxSzie}M");
        var fileSuffix = Path.GetExtension(file.Name).ToLower().Split(".")[1]; // 文件后缀
        string[] allowTypeS = allowTypes == null ? new string[] { "xlsx" } : allowTypes;//允许上传的文件类型
        if (!allowTypeS.Contains(fileSuffix)) throw Oops.Bah(errorMessage: "文件格式错误");
    }

    /// <inheritdoc/>
    public FileStreamResult GetFileStreamResult(string path, string fileName, bool isPathFolder = false)
    {
        if (isPathFolder) path = path.CombinePath(fileName);
        fileName = HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));//文件名转utf8不然前端下载会乱码
        //文件转流
        var result = new FileStreamResult(new FileStream(path, FileMode.Open), "application/octet-stream")
        {
            FileDownloadName = fileName
        };
        return result;
    }

    /// <inheritdoc/>
    public FileStreamResult GetFileStreamResult(byte[] byteArray, string fileName)
    {
        fileName = HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));//文件名转utf8不然前端下载会乱码
        //文件转流
        var result = new FileStreamResult(new MemoryStream(byteArray), "application/octet-stream")
        {
            FileDownloadName = fileName
        };
        return result;
    }

    #region 方法

    /// <summary>
    /// 存储本地文件
    /// </summary>
    /// <param name="pPath">存储的第一层目录</param>
    /// <param name="file"></param>
    /// <returns>文件全路径</returns>
    private async Task<string> StorageLocal(string pPath, IBrowserFile file)
    {
        string uploadFileFolder = App.WebHostEnvironment.WebRootPath;//赋值路径
        var now = DateTime.Now.ToString("d");
        var filePath = Path.Combine(uploadFileFolder, pPath);
        if (!Directory.Exists(filePath))//如果不存在就创建文件夹
            Directory.CreateDirectory(filePath);
        var fileSuffix = Path.GetExtension(file.Name).ToLower();// 文件后缀
        var fileObjectName = $"{file}{now}";//存储后的文件名
        var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路局
        fileName = fileName.Replace("\\", "/");//格式化一系
        //存储文件
        using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
        {
            using var fs = file.OpenReadStream(1024 * 1024 * 200);
            await fs.CopyToAsync(stream);
        }
        return fileName;
    }

    #endregion 方法
}