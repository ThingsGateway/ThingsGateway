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

using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusMasterProperty : CollectPropertyBase
{
    [DynamicProperty("Modbus协议类型", "")]
    public ModbusTypeEnum ModbusType { get; set; }

    /// <summary>
    /// 心跳检测
    /// </summary>
    [DynamicProperty("心跳检测", "大写16进制字符串，符合心跳内容会自动回应")]
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    /// <summary>
    /// 默认站号
    /// </summary>
    [DynamicProperty("默认站号", "")]
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty("默认解析顺序", "")]
    public DataFormatEnum DataFormat { get; set; }

    [DynamicProperty("字符串按字反转", "")]
    public bool IsStringReverseByteWord { get; set; }

    /// <summary>
    /// 无交互2min时断开连接
    /// </summary>
    [DynamicProperty("无交互2min时断开连接", "")]
    public bool CheckClear { get; set; }

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
    /// 最大打包长度
    /// </summary>
    [DynamicProperty("最大打包长度", "")]
    public ushort MaxPack { get; set; } = 100;
}