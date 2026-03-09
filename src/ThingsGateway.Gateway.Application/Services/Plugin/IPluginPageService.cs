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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
public interface IPluginPageService
{


    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null);
    Task<List<ChannelTypeEnum>> OnChannelTypeQueryAsync(string pluginName);

    /// <summary>
    /// 分页显示插件
    /// </summary>
    public Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null);

    /// <summary>
    /// 重载插件
    /// </summary>
    Task ReloadPluginAsync();

    ///// <summary>
    ///// 添加插件
    ///// </summary>
    ///// <param name="plugin"></param>
    ///// <returns></returns>
    //Task SavePlugin(PluginAddInput plugin);

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    Task SavePluginByPathAsync(PluginAddPathInput plugin);
}
