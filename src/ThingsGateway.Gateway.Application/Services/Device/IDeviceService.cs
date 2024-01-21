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

/// <summary>
/// 网关设备服务
/// </summary>
public interface IDeviceService : ISugarService, ITransient
{
    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddAsync(DeviceAddInput input);

    /// <summary>
    /// 复制设备
    /// </summary>
    /// <param name="input"></param>
    /// <param name="count">复制数量</param>
    /// <returns></returns>
    Task CopyAsync(IEnumerable<Device> input, int count);

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 删除设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task ClearAsync(PluginTypeEnum pluginType);

    /// <summary>
    /// 根据通道Id删除变量
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteByChannelIdAsync(List<BaseIdInput> input);

    /// <summary>
    /// 编辑设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditAsync(DeviceEditInput input);

    /// <summary>
    /// 编辑多个设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditAsync(List<Device> input);

    /// <summary>
    /// 根据id获取设备
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Device? GetDeviceById(long id);

    /// <summary>
    /// 通过名称获取id
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    long? GetIdByName(string name);

    /// <summary>
    /// 通过id获取名称
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    string? GetNameById(long id);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<Device>> PageAsync(DevicePageInput input);

    /// <summary>
    /// 删除设备缓存
    /// </summary>
    void DeleteDeviceFromRedis();

    /// <summary>
    /// 获取采集设备Runtime
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long? devId = null);

    /// <summary>
    /// 获取业务设备Runtime
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    Task<List<DeviceRunTime>> GetBusinessDeviceRuntimeAsync(long? devId = null);

    /// <summary>
    /// 获取全部列表
    /// </summary>
    /// <returns></returns>
    List<Device> GetCacheList();

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync(PluginTypeEnum input);

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync(DeviceInput input);

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
    Task<MemoryStream> ExportMemoryStream(IEnumerable<Device>? data, PluginTypeEnum pluginType, string channelName = null);
}