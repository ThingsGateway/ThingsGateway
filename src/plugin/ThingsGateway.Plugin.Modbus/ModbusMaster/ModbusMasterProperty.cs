//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Gateway.Application;


namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusMasterProperty : CollectPropertyBase
{
    [DynamicProperty]
    public ModbusTypeEnum ModbusType { get; set; }

    /// <summary>
    /// 心跳检测
    /// </summary>
    [DynamicProperty]
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    /// <summary>
    /// 默认站号
    /// </summary>
    [DynamicProperty]
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 默认DtuId
    /// </summary>
    [DynamicProperty]
    public string? DtuId { get; set; } = "TEST";

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty]
    public DataFormatEnum DataFormat { get; set; }

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
    /// 发送延时ms
    /// </summary>
    [DynamicProperty]
    public int SendDelayTime { get; set; } = 0;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 最大打包长度
    /// </summary>
    [DynamicProperty]
    public ushort MaxPack { get; set; } = 100;

    /// <summary>
    /// 客户端连接滑动过期时间
    /// </summary>
    [DynamicProperty]
    public int CheckClearTime { get; set; } = 120;

    [DynamicProperty]
    public bool IsStringReverseByteWord { get; set; }

    public override int ConcurrentCount { get; set; } = 1;
}
