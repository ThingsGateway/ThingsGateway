//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道服务
/// </summary>
public interface IChannelService
{
    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="models">列表</param>
    /// <param name="oldModel">旧数据</param>
    /// <param name="model">新数据</param>
    /// <returns></returns>
    Task<bool> BatchEditAsync(IEnumerable<Channel> models, Channel oldModel, Channel model);

    /// <summary>
    /// 清除所有通道
    /// </summary>
    Task ClearChannelAsync();

    /// <summary>
    /// 删除通道
    /// </summary>
    /// <param name="ids">待删除通道的ID列表</param>
    Task<bool> DeleteChannelAsync(IEnumerable<long> ids);

    /// <summary>
    /// 从缓存中删除通道
    /// </summary>
    void DeleteChannelFromCache();

    /// <summary>
    /// 导出通道为文件流结果
    /// </summary>
    /// <returns>文件流结果</returns>
    Task<Dictionary<string, object>> ExportChannelAsync(QueryPageOptions options);

    /// <summary>
    /// 导出通道为内存流
    /// </summary>
    /// <param name="data">通道数据</param>
    /// <returns>内存流</returns>
    Task<MemoryStream> ExportMemoryStream(List<Channel> data);

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>通道列表</returns>
    List<Channel> GetAll();

    /// <summary>
    /// 通过配置获取通道
    /// </summary>
    /// <param name="channel">通道对象</param>
    /// <param name="config">配置信息</param>
    /// <returns>通道</returns>
    IChannel GetChannel(Channel channel, TouchSocketConfig config);

    /// <summary>
    /// 通过ID获取通道
    /// </summary>
    /// <param name="id">通道ID</param>
    /// <returns>通道对象</returns>
    Channel? GetChannelById(long id);

    /// <summary>
    /// 通过名称获取通道ID
    /// </summary>
    /// <param name="name">通道名称</param>
    /// <returns>通道ID</returns>
    long? GetIdByName(string name);

    /// <summary>
    /// 通过ID获取通道名称
    /// </summary>
    /// <param name="id">通道ID</param>
    /// <returns>通道名称</returns>
    string? GetNameById(long id);

    /// <summary>
    /// 导入通道数据
    /// </summary>
    /// <param name="input">导入数据</param>
    Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    Task<QueryData<Channel>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// API查询
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<Channel>> PageAsync(ChannelPageInput input);

    /// <summary>
    /// 预览导入数据
    /// </summary>
    /// <param name="browserFile">浏览器文件对象</param>
    /// <returns>导入预览结果</returns>
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);

    /// <summary>
    /// 保存通道
    /// </summary>
    /// <param name="input">通道对象</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveChannelAsync(Channel input, ItemChangedType type);
}
