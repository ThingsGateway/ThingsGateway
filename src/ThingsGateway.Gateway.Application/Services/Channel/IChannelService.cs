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

using System.Data;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道服务
/// </summary>
public interface IChannelService : ISugarService, ITransient
{
    /// <summary>
    /// 添加通道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AddAsync(ChannelAddInput input);

    /// <summary>
    /// 获取通道
    /// </summary>
    /// <param name="id">通道id</param>
    /// <param name="config">底层配置</param>
    /// <returns></returns>
    IChannel GetChannel(Channel channel, TouchSocketConfig config);

    /// <summary>
    /// 复制通道
    /// </summary>
    /// <param name="input"></param>
    /// <param name="count">复制数量</param>
    /// <returns></returns>
    Task CopyAsync(IEnumerable<Channel> input, int count);

    /// <summary>
    /// 删除通道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 编辑通道
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditAsync(ChannelEditInput input);

    /// <summary>
    /// 获取通道列表，会从缓存中读取
    /// </summary>
    /// <returns></returns>
    List<Channel> GetCacheList();

    /// <summary>
    /// 根据id获取通道
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Channel? GetChannelById(long id);

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
    Task<SqlSugarPagedList<Channel>> PageAsync(ChannelPageInput input);

    /// <summary>
    /// 删除通道缓存
    /// </summary>
    void DeleteChannelFromRedis();

    /// <summary>
    /// 清空
    /// </summary>
    /// <returns></returns>
    Task ClearAsync();

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync(IDataReader? input = null);

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<FileStreamResult> ExportFileAsync(ChannelInput input);

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
    /// 导出
    /// </summary>
    /// <param name="channels"></param>
    /// <returns></returns>
    Task<MemoryStream> ExportMemoryStream(List<Channel> data);
}