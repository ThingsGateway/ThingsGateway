//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

public interface IImportExportService
{
    /// <summary>
    /// 导出excel文件流
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    /// <param name="input">实体对象或者IDataReader</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="isDynamicExcelColumn">动态excel列，根据实体的<see cref="IgnoreExcelAttribute"/>属性判断是否生成 </param>
    /// <returns>导出的文件流</returns>
    Task<FileStreamResult> ExportAsync<T>(object input, string fileName, bool isDynamicExcelColumn = true) where T : class;

    /// <summary>
    /// 获取文件名，默认xlsx类型
    /// </summary>
    /// <param name="fileName">文件名称，不含类型名称的话默认xlsx</param>
    /// <returns>编码后的文件名</returns>
    string GetUrlEncodeFileName(string fileName);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>保存全路径</returns>
    Task<string> UploadFileAsync(IBrowserFile file);
}
