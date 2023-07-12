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

using System.IO.Ports;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;

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
    UdpSession
}


/// <summary>
/// 共享通道
/// </summary>
public enum RedundantEnum
{
    /// <summary>
    /// 主站
    /// </summary>
    Primary,
    /// <summary>
    /// 备用
    /// </summary>
    Standby,
}

/// <summary>
/// <inheritdoc cref="DriverPropertyBase"/><br></br>
/// 1.5.0版本适配共享通道，支持自定义TCP/UDP/Serial共享<see cref="TGTcpClient"/>,<see cref="TGUdpSession"/>,<see cref="SerialClient"/>
/// </summary>
public abstract class CollectDriverPropertyBase : DriverPropertyBase
{
    #region 共享通道配置
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

    #endregion
}

/// <summary>
/// <inheritdoc cref="DriverPropertyBase"/>
/// </summary>
public abstract class UpDriverPropertyBase : DriverPropertyBase
{

}
/// <summary>
/// 插件配置项
/// 使用<see cref="DevicePropertyAttribute"/>特性标识
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// 使用<see cref="DevicePropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class DriverPropertyBase
{

}