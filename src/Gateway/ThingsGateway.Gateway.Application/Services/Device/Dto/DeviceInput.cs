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

using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备分页查询DTO
/// </summary>
public class DevicePageInput : BasePageInput
{
    /// <inheritdoc/>
    public long? ChannelId { get; set; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    public PluginTypeEnum PluginType { get; set; }
}



public class DeviceSearchInput : ITableSearchModel
{
    /// <inheritdoc/>
    public long? ChannelId { get; set; }

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? PluginName { get; set; }
    /// <inheritdoc/>
    public PluginTypeEnum PluginType { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(Name), () => new SearchFilterAction(nameof(Device.Name), Name));
        ret.AddIF(!string.IsNullOrEmpty(PluginName), () => new SearchFilterAction(nameof(Device.PluginName), PluginName));
        ret.AddIF(ChannelId > 0, () => new SearchFilterAction(nameof(Device.ChannelId), ChannelId, FilterAction.Equal));
        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Name = null;
        PluginName = null;
        ChannelId = null;
    }
}
