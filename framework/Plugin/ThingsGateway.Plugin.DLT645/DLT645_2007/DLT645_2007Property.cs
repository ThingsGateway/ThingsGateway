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

namespace ThingsGateway.Plugin.DLT645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class DLT645_2007Property : CollectDriverPropertyBase
{
    /// <summary>
    /// COM口
    /// </summary>
    [DeviceProperty("COM口", "示例：COM1")]
    public override string PortName { get; set; } = "COM1";
    /// <summary>
    /// 波特率
    /// </summary>
    [DeviceProperty("波特率", "通常为：38400/19200/9600/4800")]
    public override int BaudRate { get; set; } = 9600;
    /// <summary>
    /// 数据位
    /// </summary>
    [DeviceProperty("数据位", "通常为：8/7/6")]
    public override byte DataBits { get; set; } = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    [DeviceProperty("停止位", "示例：None/One/Two/OnePointFive")]
    public override StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// 校验位
    /// </summary>
    [DeviceProperty("校验位", "示例：None/Odd/Even/Mark/Space")]
    public override Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DeviceProperty("读写超时时间", "")]
    public ushort TimeOut { get; set; } = 3000;
    /// <summary>
    /// 默认地址
    /// </summary>
    [DeviceProperty("默认地址", "")]
    public string Station { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    [DeviceProperty("密码", "")]
    public string Password { get; set; }
    /// <summary>
    /// 操作员代码
    /// </summary>
    [DeviceProperty("操作员代码", "")]
    public string OperCode { get; set; }

    /// <summary>
    /// 前导符报文头
    /// </summary>
    [DeviceProperty("前导符报文头", "")]
    public bool EnableFEHead { get; set; } = true;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [DeviceProperty("帧前时间", "某些设备性能较弱，报文间需要间隔较长时间")]
    public int FrameTime { get; set; } = 0;
    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DeviceProperty("组包缓存超时ms", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1s")]
    public int CacheTimeout { get; set; } = 1000;




    /// <summary>
    /// 共享链路
    /// </summary>
    [DeviceProperty("共享链路", "")]
    public override bool IsShareChannel { get; set; } = false;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ChannelEnum ShareChannel => ChannelEnum.SerialPort;


}
