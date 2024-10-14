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
/// VariableClass
/// </summary>
public class VariableClass : IVariable
{
    /// <inheritdoc/>
    protected object? _value;

    /// <summary>
    /// 数据类型
    /// </summary>
    public virtual DataTypeEnum DataType { get; set; }

    /// <summary>
    /// 偏移量，注意如果是布尔类型，Index应该为bit的偏移
    /// </summary>
    public virtual int Index { get; set; }

    /// <summary>
    /// 执行间隔
    /// </summary>
    public virtual int? IntervalTime { get; set; }

    /// <inheritdoc/>
    public bool IsOnline { get; set; }

    /// <inheritdoc/>
    public string LastErrorMessage => VariableSource?.LastErrorMessage;

    /// <summary>
    /// 寄存器地址
    /// </summary>
    public virtual string? RegisterAddress { get; set; }

    /// <summary>
    /// 数据转换规则
    /// </summary>
    public virtual IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    /// <summary>
    /// 实时值
    /// </summary>
    public virtual object? Value => _value;

    /// <summary>
    /// IVariableSource
    /// </summary>
    public IVariableSource VariableSource { get; set; }

    /// <summary>
    /// 赋值变量
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="dateTime">采集时间</param>
    /// <param name="isOnline">是否在线</param>
    /// <returns></returns>
    public virtual OperResult SetValue(object? value, DateTime dateTime = default, bool isOnline = true)
    {
        IsOnline = isOnline;
        _value = value;
        return new();
    }
}
