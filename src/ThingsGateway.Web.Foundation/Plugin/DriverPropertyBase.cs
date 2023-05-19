using System.IO.Ports;

using ThingsGateway.Foundation.Serial;

using TouchSocket.Sockets;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 共享通道
/// </summary>
public enum ShareChannelEnum
{
    /// <summary>
    /// 不支持共享
    /// </summary>
    None,
    /// <summary>
    /// 串口
    /// </summary>
    SerialClient,
    /// <summary>
    /// TCP
    /// </summary>
    TcpClient,
    /// <summary>
    /// UDP
    /// </summary>
    TGUdpSession
}
/// <summary>
/// <inheritdoc cref="DriverPropertyBase"/><br></br>
/// 1.5.0版本适配共享通道，支持自定义TCP/UDP/Serial共享<see cref="TGTcpClient"/>,<see cref="TGUdpSession"/>,<see cref="SerialClient"/>
/// </summary>
public abstract class CollectDriverPropertyBase : DriverPropertyBase
{
    /// <summary>
    /// 是否支持共享通道
    /// </summary>
    public abstract bool IsShareChannel { get; set; }

    /// <summary>
    /// 共享通道类型
    /// </summary>
    public abstract ShareChannelEnum ShareChannel { get; }

    #region Socket
    /// <summary>
    /// IP地址
    /// </summary>
    public virtual string IP { get; set; } = "127.0.0.1";
    /// <summary>
    /// 端口
    /// </summary>
    public virtual int Port { get; set; } = 502;

    #endregion

    #region Serial
    /// <summary>
    /// COM名称
    /// </summary>
    public virtual string PortName { get; set; } = "COM1";
    /// <summary>
    /// 波特率
    /// </summary>
    public virtual int BaudRate { get; set; } = 9600;
    /// <summary>
    /// 数据位
    /// </summary>
    public virtual byte DataBits { get; set; } = 8;
    /// <summary>
    /// 校验位
    /// </summary>
    public virtual Parity Parity { get; set; } = Parity.None;
    /// <summary>
    /// 停止位
    /// </summary>
    public virtual StopBits StopBits { get; set; } = StopBits.One;

    #endregion
}

/// <summary>
/// <inheritdoc cref="DriverPropertyBase"/>
/// </summary>
public abstract class UpDriverPropertyBase : DriverPropertyBase
{

}
/// <summary>
/// 上传插件配置项
/// 使用<see cref="DevicePropertyAttribute"/>特性标识
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// 使用<see cref="DevicePropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class DriverPropertyBase
{

}