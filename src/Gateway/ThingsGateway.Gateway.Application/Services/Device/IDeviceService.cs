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
/// 网关设备服务
/// </summary>
/// <summary>
/// 设备服务接口，定义了设备相关的操作。
/// </summary>
internal interface IDeviceService
{
    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="models">列表</param>
    /// <param name="oldModel">旧数据</param>
    /// <param name="model">新数据</param>
    /// <returns></returns>
    Task<bool> BatchEditAsync(IEnumerable<Device> models, Device oldModel, Device model);

    /// <summary>
    /// 根据通道ID异步删除设备信息。
    /// </summary>
    /// <param name="ids">待删除设备的通道ID集合</param>
    /// <param name="db">数据库连接</param>
    Task DeleteByChannelIdAsync(IEnumerable<long> ids, SqlSugarClient db);

    /// <summary>
    /// 异步删除设备信息。
    /// </summary>
    /// <param name="ids">待删除设备的ID集合</param>
    /// <returns>删除是否成功的异步任务</returns>
    Task<bool> DeleteDeviceAsync(IEnumerable<long> ids);

    /// <summary>
    /// 删除设备缓存信息。
    /// </summary>
    void DeleteDeviceFromCache();

    /// <summary>
    /// 导出设备信息到文件流。
    /// </summary>
    /// <returns>导出的文件流</returns>
    Task<Dictionary<string, object>> ExportDeviceAsync(ExportFilter exportFilter);

    /// <summary>
    /// 导出设备信息到内存流。
    /// </summary>
    /// <param name="data">设备信息</param>
    /// <param name="channelName">通道名称（可选）</param>
    /// <returns>导出的内存流</returns>
    Task<MemoryStream> ExportMemoryStream(IEnumerable<Device>? data, string channelName = null);

    /// <summary>
    /// 获取所有设备信息。
    /// </summary>
    /// <returns>所有设备信息</returns>
    Task<List<Device>> GetAllAsync(SqlSugarClient db = null);

    /// <summary>
    /// 根据ID获取设备信息。
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>设备信息</returns>
    Task<Device?> GetDeviceByIdAsync(long id);

    /// <summary>
    /// 导入设备信息。
    /// </summary>
    /// <param name="input">导入的数据</param>
    /// <returns>异步任务</returns>
    Task<HashSet<long>> ImportDeviceAsync(Dictionary<string, ImportPreviewOutputBase> input);

    /// <summary>
    /// 异步分页查询设备信息。
    /// </summary>
    /// <param name="exportFilter">查询条件</param>
    /// <returns>查询结果</returns>
    Task<QueryData<Device>> PageAsync(ExportFilter exportFilter);

    /// <summary>
    /// 预览导入设备信息。
    /// </summary>
    /// <param name="browserFile">待导入的文件</param>
    /// <returns>导入预览结果</returns>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);

    /// <summary>
    /// 异步保存设备信息。
    /// </summary>
    /// <param name="input">设备信息</param>
    /// <param name="type">保存类型</param>
    /// <returns>保存是否成功的异步任务</returns>
    Task<bool> SaveDeviceAsync(Device input, ItemChangedType type);


    /// <summary>
    /// 保存是否输出日志和日志等级
    /// </summary>
    Task UpdateLogAsync(long deviceId, bool logEnable, TouchSocket.Core.LogLevel logLevel);

}
