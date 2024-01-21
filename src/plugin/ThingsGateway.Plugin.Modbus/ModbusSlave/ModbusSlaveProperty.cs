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
public class ModbusSlaveProperty : BusinessPropertyBase
{
    [DynamicProperty("Modbus协议类型", "")]
    public ModbusTypeEnum ModbusType { get; set; }

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

    /// <summary>
    /// 无交互2min时断开连接
    /// </summary>
    [DynamicProperty("无交互2min时断开连接", "")]
    public bool CheckClear { get; set; }

    /// <summary>
    /// 最大连接数
    /// </summary>
    [DynamicProperty("最大连接数", "")]
    public int MaxClientCount { get; set; } = 60000;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1000ms")]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 多站点
    /// </summary>
    [DynamicProperty("多站点", "")]
    public bool MulStation { get; set; } = true;

    /// <summary>
    /// 允许写入
    /// </summary>
    [DynamicProperty("允许写入", "")]
    public bool DeviceRpcEnable { get; set; }

    /// <summary>
    /// 立即写入内存
    /// </summary>
    [DynamicProperty("立即写入内存", "")]
    public bool IsWriteMemory { get; set; }
}