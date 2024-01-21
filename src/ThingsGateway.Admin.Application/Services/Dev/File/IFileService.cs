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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 文件管理服务
/// </summary>
public interface IFileService : ITransient
{
    /// <summary>
    /// 获取FileStreamResult文件流
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="fileName">文件名</param>
    /// <param name="isPathFolder">路径是否是文件夹</param>
    /// <returns></returns>
    FileStreamResult GetFileStreamResult(string path, string fileName, bool isPathFolder = false);

    /// <summary>
    /// 获取FileStreamResult文件流
    /// </summary>
    /// <param name="byteArray">文件数组</param>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    FileStreamResult GetFileStreamResult(byte[] byteArray, string fileName);

    /// <summary>
    /// 上传文件到本地返回下载url
    /// </summary>
    /// <param name="pPath">存储的第一层目录</param>
    /// <param name="file">文件</param>
    /// <returns>文件全路径</returns>
    Task<string> UploadFileAsync(string pPath, IBrowserFile file);

    /// <summary>
    /// 文件验证
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="maxSzie">文件最大体积(M)</param>
    /// <param name="allowTypes">允许的格式</param>
    void Verification(IBrowserFile file, int maxSzie = 200, string[] allowTypes = null);
}