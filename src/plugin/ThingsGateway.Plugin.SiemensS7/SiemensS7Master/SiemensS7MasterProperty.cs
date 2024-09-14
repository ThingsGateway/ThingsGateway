//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.SiemensS7;
using ThingsGateway.Gateway.Application;


namespace ThingsGateway.Plugin.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7MasterProperty : CollectPropertyBase
{
    [DynamicProperty]
    public SiemensTypeEnum SiemensS7Type { get; set; }

    /// <summary>
    /// 最大打包长度
    /// </summary>
    [DynamicProperty]
    public ushort MaxPack { get; set; } = 200;

    /// <summary>
    /// Rack
    /// </summary>
    [DynamicProperty]
    public byte Rack { get; set; } = 0;

    /// <summary>
    /// Slot
    /// </summary>
    [DynamicProperty]
    public byte Slot { get; set; } = 0;

    /// <summary>
    /// LocalTSAP
    /// </summary>
    [DynamicProperty]
    public int LocalTSAP { get; set; } = 0;

    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DynamicProperty]
    public ushort Timeout { get; set; } = 3000;

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DynamicProperty]
    public ushort ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [DynamicProperty]
    public int SendDelayTime { get; set; } = 0;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty]
    public DataFormatEnum DataFormat { get; set; }

    public override int ConcurrentCount { get; set; } = 1;
}
