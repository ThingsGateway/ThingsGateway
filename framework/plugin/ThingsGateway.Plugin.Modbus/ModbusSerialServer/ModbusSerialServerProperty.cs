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

namespace ThingsGateway.Plugin.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSerialServerProperty : DriverPropertyBase
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
    /// 默认站号
    /// </summary>
    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    /// <summary>
    /// 多站点
    /// </summary>
    [DeviceProperty("多站点", "")]
    public bool MulStation { get; set; } = true;
    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    /// <summary>
    /// 允许写入
    /// </summary>
    [DeviceProperty("允许写入", "")]
    public bool DeviceRpcEnable { get; set; }
    /// <summary>
    /// 立即写入内存
    /// </summary>
    [DeviceProperty("立即写入内存", "")]
    public bool WriteMemory { get; set; }
    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DeviceProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1000ms")]
    public int CacheTimeout { get; set; } = 1000;
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 100;
}
