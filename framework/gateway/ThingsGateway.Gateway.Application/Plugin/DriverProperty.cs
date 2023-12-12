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

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 插件配置项
/// 使用<see cref="VariablePropertyAttribute"/>特性标识
/// <para></para>
/// </summary>
public abstract class VariablePropertyBase
{
}

/// <summary>
/// 插件配置项
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// <br></br>
/// 使用<see cref="DevicePropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class DriverPropertyBase
{
    /// <summary>
    /// 离线后恢复运行的间隔时间 /s，默认300s
    /// </summary>
    [DeviceProperty("离线恢复间隔", "离线后恢复运行的间隔时间s，默认300s，最大3600s")]
    public virtual int ReIntervalTime { get; set; } = 300;

    /// <summary>
    /// 失败重试次数，默认3
    /// </summary>
    [DeviceProperty("失败重试次数", "失败重试次数，默认3")]
    public virtual int RetryCount { get; set; } = 3;

    #region 共享通道配置

    /// <summary>
    /// 是否支持共享通道
    /// </summary>
    public virtual bool IsShareChannel { get; set; } = false;

    /// <summary>
    /// 共享通道类型
    /// </summary>
    public virtual ChannelEnum ShareChannel { get; } = ChannelEnum.None;

    #endregion

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
    /// COM名称
    /// </summary>
    public virtual string PortName { get; set; } = "COM1";

    /// <summary>
    /// 停止位
    /// </summary>
    public virtual StopBits StopBits { get; set; } = StopBits.One;

    #endregion
}

/// <summary>
/// 插件配置项
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// <br></br>
/// 使用<see cref="DevicePropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class UpDriverPropertyBase : DriverPropertyBase
{
    public override int ReIntervalTime { get; set; } = 300;

    public override int RetryCount { get; set; } = 3;

    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小50ms")]
    public virtual int CycleInterval { get; set; } = 1000;

}

