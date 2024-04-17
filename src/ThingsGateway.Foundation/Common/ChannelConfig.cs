
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation;

/// <summary>
/// 通道配置保存
/// </summary>
public class ChannelConfig : AppConfigBase
{
    public static ChannelConfig Default;

    static ChannelConfig()
    {
        Default = AppConfigBase.GetNewDefault<ChannelConfig>();
        foreach (var item in Default.ChannelDatas)
        {
            ChannelData.CreateChannel(item);
        }
    }

    /// <summary>
    /// 通道列表
    /// </summary>
    public ConcurrentList<ChannelData> ChannelDatas { get; set; } = new();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ChannelConfig() : base(($"channel.config.json"))
    {
    }
}