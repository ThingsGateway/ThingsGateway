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

namespace ThingsGateway.Gateway.Application;

public interface IVariableService : ISugarService, ITransient
{
    /// <summary>
    /// 添加变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddAsync(VariableAddInput input);

    /// <summary>
    /// 批量添加变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddBatchAsync(List<VariableAddInput> input);

    /// <summary>
    /// 清空变量
    /// </summary>
    /// <returns></returns>
    Task ClearAsync();

    /// <summary>
    /// 复制变量
    /// </summary>
    /// <param name="variables"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    Task CopyAsync(IEnumerable<Variable> variables, int result);

    /// <summary>
    /// 删除变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 根据设备Id删除变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteByDeviceIdAsync(List<long> input);

    /// <summary>
    /// 编辑变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditAsync(VariableEditInput input);

    /// <summary>
    /// 获取变量Runtime
    /// </summary>
    /// <returns></returns>
    Task<List<VariableRunTime>> GetVariableRuntimeAsync(long? devId = null);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<Variable>> PageAsync(VariablePageInput input);

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync();

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync(VariableInput input);

    /// <summary>
    /// 导入预览
    /// </summary>
    /// <param name="browserFile"></param>
    /// <returns></returns>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);

    /// <summary>
    /// 导入
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input);

    /// <summary>
    /// 导出文件
    /// </summary>
    Task<MemoryStream> ExportMemoryStream(IEnumerable<Variable> data, string deviceName = null);
}