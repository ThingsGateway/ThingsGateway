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
public class ModbusSlaveProperty : BusinessPropertyBase
{
    [DynamicProperty]
    public ModbusTypeEnum ModbusType { get; set; }

    /// <summary>
    /// 默认站号
    /// </summary>
    [DynamicProperty]
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DynamicProperty]
    public DataFormatEnum DataFormat { get; set; }

    [DynamicProperty]
    public bool IsStringReverseByteWord { get; set; }

    [DynamicProperty]
    public int CheckClearTime { get; set; } = 120;

    /// <summary>
    /// 最大连接数
    /// </summary>
    [DynamicProperty]
    public int MaxClientCount { get; set; } = 60000;

    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DynamicProperty]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 多站点
    /// </summary>
    [DynamicProperty]
    public bool MulStation { get; set; } = true;

    /// <summary>
    /// 允许写入
    /// </summary>
    [DynamicProperty]
    public bool DeviceRpcEnable { get; set; } = true;

    /// <summary>
    /// 立即写入内存
    /// </summary>
    [DynamicProperty]
    public bool IsWriteMemory { get; set; } = true;

    [DynamicProperty]
    public string DtuId { get; set; } = "DtuId";

    [DynamicProperty]
    public int HeartbeatTime { get; set; } = 5;

    [DynamicProperty]
    public string HeartbeatHexString { get; set; } = "FFFF8080";
}
