//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.SiemensS7;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Siemens;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7MasterProperty : CollectPropertyBase
{
    [DynamicProperty("S7类型", "S200,S200Smart,S300,S400,S1200,S1500")]
    public SiemensTypeEnum SiemensType { get; set; }

    /// <summary>
    /// Rack
    /// </summary>
    [DynamicProperty("机架号", "为0时不写入")] public byte Rack { get; set; } = 0;

    /// <summary>
    /// Slot
    /// </summary>
    [DynamicProperty("槽位号", "为0时不写入")] public byte Slot { get; set; } = 0;

    /// <summary>
    /// LocalTSAP
    /// </summary>
    [DynamicProperty("LocalTSAP", "为0时不写入，通常默认0即可")] public int LocalTSAP { get; set; } = 0;

    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DynamicProperty("读写超时时间", "")]
    public ushort Timeout { get; set; } = 3000;

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DynamicProperty("连接超时时间", "")]
    public ushort ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [DynamicProperty("发送延时时间", "某些设备性能较弱，报文间需要间隔较长时间")]
    public int SendDelayTime { get; set; } = 0;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1000ms")]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty("默认解析顺序", "")]
    public DataFormatEnum DataFormat { get; set; }

    public override int ConcurrentCount { get; set; } = 1;
}