//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using System.Text;
using System.Web;

using Yitter.IdGenerator;

namespace ThingsGateway;

public class FileService : IFileService
{
    private IStringLocalizer<FileService> _localizer;
    public FileService(
        IStringLocalizer<FileService> localizer)
    {
        _localizer = localizer;

    }
    /// <summary>
    /// 获取本地存储文件流
    /// </summary>
    /// <param name="path">文件夹</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="isPathFolder">第一个参数是否是包含文件名称的全路径</param>
    /// <returns>文件流</returns>
    public FileStreamResult GetFileStreamResult(string path, string fileName, bool isPathFolder = false)
    {
        if (isPathFolder) path = path.CombinePathWithOs(fileName);
        fileName = HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));//文件名转utf8不然前端下载会乱码
        //文件转流
        var result = new FileStreamResult(new FileStream(path, FileMode.Open), "application/octet-stream")
        {
            FileDownloadName = fileName
        };
        return result;
    }

    /// <summary>
    /// 按字节数组转为文件流
    /// </summary>
    /// <param name="byteArray">字节数组</param>
    /// <param name="fileName">文件名称</param>
    /// <returns>文件流</returns>
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

    /// <summary>
    /// 上传文件，保存在磁盘中
    /// </summary>
    /// <param name="pPath">保存路径</param>
    /// <param name="file">文件</param>
    /// <returns>最终全路径</returns>
    public async Task<string> UploadFileAsync(  IBrowserFile file,string pPath= "imports")
    {
        return await file.StorageLocal(pPath);
    }

    /// <summary>
    /// 验证文件信息
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="maxSize">最大文件大小</param>
    /// <param name="allowTypes">扩展名称匹配</param>
    public void Verification(IBrowserFile file, int maxSize = 200, string[]? allowTypes = null)
    {
        if (file == null) throw Oops.Bah(_localizer["FileNullError"]);
        if (file.Size > maxSize * 1024 * 1024) throw Oops.Bah(_localizer["FileLengthError", maxSize]);
        var fileSuffix = Path.GetExtension(file.Name).ToLower().Split(".")[1]; // 文件后缀
        string[] allowTypeS = allowTypes == null ? ["xlsx"] : allowTypes;//允许上传的文件类型
        if (!allowTypeS.Contains(fileSuffix)) throw Oops.Bah(_localizer["FileTypeError", fileSuffix]);
    }

    #region 方法

    #endregion 方法
}
