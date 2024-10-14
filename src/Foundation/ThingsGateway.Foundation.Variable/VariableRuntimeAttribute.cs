//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// 变量特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class VariableRuntimeAttribute : Attribute
{
    /// <summary>
    /// 数据类型，默认不填时会使用属性的Type
    /// </summary>
    public DataTypeEnum DataType { get; set; }

    /// <summary>
    /// 读取表达式
    /// </summary>
    public string? ReadExpressions { get; set; }

    /// <summary>
    /// 寄存器地址
    /// </summary>
    public string? RegisterAddress { get; set; }

    /// <summary>
    /// 写入表达式
    /// </summary>
    public string? WriteExpressions { get; set; }
}
