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

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRtuOverTcpProperty : DriverPropertyBase
{
    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 502;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }

    /// <summary>
    /// 默认站号
    /// </summary>
    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DeviceProperty("连接超时时间", "")]
    public ushort ConnectTimeOut { get; set; } = 3000;

    /// <summary>
    /// 最大打包长度
    /// </summary>
    [DeviceProperty("最大打包长度", "")]
    public ushort MaxPack { get; set; } = 100;

    /// <summary>
    /// CRC检测
    /// </summary>
    [DeviceProperty("CRC检测", "")]
    public bool Crc16CheckEnable { get; set; } = true;

    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DeviceProperty("读写超时时间", "")]
    public ushort TimeOut { get; set; } = 3000;

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [DeviceProperty("帧前时间ms", "某些设备性能较弱，报文间需要间隔较长时间")]
    public int FrameTime { get; set; } = 0;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DeviceProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1000ms")]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 共享链路
    /// </summary>
    [DeviceProperty("共享链路", "")]
    public override bool IsShareChannel { get; set; } = false;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ChannelEnum ShareChannel => ChannelEnum.TcpClient;
}