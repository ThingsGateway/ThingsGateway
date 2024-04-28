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

using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

using System.Reflection;
using System.Text;
using System.Web;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application;

public class ImportExportService : IImportExportService
{
    private readonly IFileService _fileService;

    public ImportExportService(IFileService fileService)
    {
        _fileService = fileService;
    }

    #region 导出

    /// <summary>
    /// 导出excel文件流
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    /// <param name="input">实体对象或者IDataReader</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="isDynamicExcelColumn">动态excel列，根据实体的<see cref="IgnoreExcelAttribute"/>属性判断是否生成 </param>
    /// <returns></returns>
    public async Task<FileStreamResult> ExportAsync<T>(object input, string fileName, bool isDynamicExcelColumn = true) where T : class
    {
        var config = new OpenXmlConfiguration();
        if (isDynamicExcelColumn)
        {
            var type = typeof(T);
            var data = type.GetRuntimeProperties();
            List<DynamicExcelColumn> dynamicExcelColumns = new();
            int index = 0;
            foreach (var item in data)
            {
                var ignore = item.GetCustomAttribute<IgnoreExcelAttribute>() != null;
                //描述
                var desc = type.GetPropertyDisplayName(item.Name);
                //数据源增加
                dynamicExcelColumns.Add(new DynamicExcelColumn(item.Name) { Ignore = ignore, Index = index, Name = desc ?? item.Name, Width = 30 });
                if (!ignore)
                    index++;
            }
            config.DynamicColumns = dynamicExcelColumns.ToArray();
        }
        config.TableStyles = TableStyles.None;

        if (!fileName.Contains("."))
            fileName += ".xlsx";

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
        Directory.CreateDirectory(path);

        string searchPattern = $"*{fileName}"; // 文件名匹配模式
        string[] files = Directory.GetFiles(path, searchPattern);

        //删除同后缀的文件
        var whereFiles = files.Where(file => File.GetLastWriteTime(file) < DateTime.Now.AddMinutes(-2));

        foreach (var file in whereFiles)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
        }

        fileName = YitIdHelper.NextId() + fileName;
        var filePath = Path.Combine(path, fileName);
        using (FileStream fs = new(filePath, FileMode.Create))
        {
            await MiniExcel.SaveAsAsync(fs, input, configuration: config);
        }
        var result = _fileService.GetFileStreamResult(filePath, fileName);
        return result;
    }

    #endregion 导出

    #region 导入

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>保存全路径</returns>
    public async Task<string> UploadFileAsync(IBrowserFile file)
    {
        _fileService.Verification(file);
        return await _fileService.UploadFileAsync("imports", file);
    }

    #endregion 导入

    #region 方法

    /// <summary>
    /// 获取文件名，默认xlsx类型
    /// </summary>
    /// <param name="fileName">文件名称，不含类型名称的话默认xlsx</param>
    /// <returns></returns>
    public string GetUrlEncodeFileName(string fileName)
    {
        if (!fileName.Contains("."))
            fileName += ".xlsx";
        fileName = HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));//文件名转utf8不然前端下载会乱码
        return fileName;
    }

    #endregion 方法
}