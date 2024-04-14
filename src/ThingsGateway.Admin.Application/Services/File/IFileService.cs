
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Admin.Application;

public interface IFileService
{
    /// <summary>
    /// 上传文件，保存在磁盘中
    /// </summary>
    /// <param name="pPath">保存路径</param>
    /// <param name="file">文件流</param>
    /// <returns>最终全路径</returns>
    Task<string> UploadFileAsync(string pPath, IBrowserFile file);

    /// <summary>
    /// 验证文件信息
    /// </summary>
    /// <param name="file">文件流</param>
    /// <param name="maxSize">最大文件大小（单位：MB）</param>
    /// <param name="allowTypes">允许上传的文件类型</param>
    void Verification(IBrowserFile file, int maxSize = 200, string[]? allowTypes = null);

    /// <summary>
    /// 获取本地存储文件流
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="isPathFolder">路径是否包含文件名称</param>
    /// <returns>文件流</returns>
    FileStreamResult GetFileStreamResult(string path, string fileName, bool isPathFolder = false);

    /// <summary>
    /// 按字节数组转为文件流
    /// </summary>
    /// <param name="byteArray">字节数组</param>
    /// <param name="fileName">文件名称</param>
    /// <returns>文件流</returns>
    FileStreamResult GetFileStreamResult(byte[] byteArray, string fileName);
}