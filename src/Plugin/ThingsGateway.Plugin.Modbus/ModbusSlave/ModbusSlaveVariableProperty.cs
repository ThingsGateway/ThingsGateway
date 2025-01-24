//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSlaveVariableProperty : VariablePropertyBase
{
    /// <summary>
    /// 从站变量地址
    /// </summary>
    [DynamicProperty]
    [Required]
    public string ServiceAddress { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [DynamicProperty]
    public DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;

    /// <summary>
    /// 允许写入
    /// </summary>
    [DynamicProperty]
    public bool VariableRpcEnable { get; set; }
}
