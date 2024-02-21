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

/// <summary>
/// 导入服务
/// </summary>
public interface IImportExportService : ITransient
{
    /// <summary>
    /// 导出数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input">数据</param>
    /// <param name="fileName">文件名</param>
    /// <returns>文件流</returns>
    Task<FileStreamResult> ExportAsync<T>(object input, string fileName, bool isDynamicExcelColumn = true) where T : class, new();

    /// <summary>
    /// 上传文件到临时文件夹，返回文件路径
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<string> UploadFileAsync(IBrowserFile file);
}