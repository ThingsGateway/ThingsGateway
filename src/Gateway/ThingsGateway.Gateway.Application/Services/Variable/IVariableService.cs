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

using Microsoft.AspNetCore.Components.Forms;

using SqlSugar;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 定义了变量相关的服务接口
/// </summary>
public interface IVariableService
{
    /// <summary>
    /// 异步插入变量信息。
    /// </summary>
    /// <param name="input">要保存的设备信息。</param>
    Task AddBatchAsync(List<Variable> input);

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="models">列表</param>
    /// <param name="oldModel">旧数据</param>
    /// <param name="model">新数据</param>
    /// <returns></returns>
    Task<bool> BatchEditAsync(IEnumerable<Variable> models, Variable oldModel, Variable model);

    /// <summary>
    /// 异步清除变量数据。
    /// </summary>
    Task ClearVariableAsync(SqlSugarClient db = null);

    /// <summary>
    /// 根据设备ID异步删除变量数据。
    /// </summary>
    /// <param name="input">要删除的设备ID列表。</param>
    /// <param name="db">SqlSugar 客户端。</param>
    Task DeleteByDeviceIdAsync(IEnumerable<long> input, SqlSugarClient db);

    /// <summary>
    /// 根据ID异步删除变量数据。
    /// </summary>
    /// <param name="ids">要删除的变量ID列表。</param>
    Task<bool> DeleteVariableAsync(IEnumerable<long> ids);

    /// <summary>
    /// 异步导出变量数据到内存流中。
    /// </summary>
    /// <param name="data">要导出的变量数据。</param>
    /// <param name="deviceName">设备名称（可选）。</param>
    Task<MemoryStream> ExportMemoryStream(IEnumerable<Variable> data, string deviceName = null);

    /// <summary>
    /// 异步导出变量数据到文件流中。
    /// </summary>
    Task<Dictionary<string, object>> ExportVariableAsync(QueryPageOptions options, FilterKeyValueAction filterKeyValueAction = null);

    /// <summary>
    /// 异步获取变量的运行时信息。
    /// </summary>
    /// <param name="devId">设备ID（可选）。</param>
    Task<List<VariableRunTime>> GetVariableRuntimeAsync(long? devId = null);

    /// <summary>
    /// 异步导入变量数据。
    /// </summary>
    /// <param name="input">要导入的数据。</param>
    Task ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input);

    /// <summary>
    /// 创建n个modbus变量
    /// </summary>
    Task InsertTestDataAsync(int variableCount, int deviceCount, string slaveUrl = "127.0.0.1:502");

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询分页选项</param>
    /// <param name="businessDeviceId">业务设备id</param>
    Task<QueryData<Variable>> PageAsync(QueryPageOptions option, long? businessDeviceId);


    /// <summary>
    /// API查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<Variable>> PageAsync(VariablePageInput input);
    Task PreheatCache();

    /// <summary>
    /// 异步预览导入的数据。
    /// </summary>
    /// <param name="browserFile">要预览的文件。</param>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);

    /// <summary>
    /// 异步插入变量信息。
    /// </summary>
    /// <param name="input">要保存的设备信息。</param>
    /// <param name="type">变量变化类型。</param>
    Task<bool> SaveVariableAsync(Variable input, ItemChangedType type);
}
