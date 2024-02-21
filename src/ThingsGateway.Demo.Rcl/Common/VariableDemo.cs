//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation;

namespace ThingsGateway.Demo;

/// <inheritdoc/>
public class VariableDemo : IVariable
{
    /// <inheritdoc/>
    public int? IntervalTime { get; set; }

    /// <inheritdoc/>
    public string? RegisterAddress { get; set; }

    /// <inheritdoc/>
    public int Index { get; set; }

    /// <inheritdoc/>
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    /// <inheritdoc/>
    public DataTypeEnum DataType { get; set; }

    /// <inheritdoc/>
    public object? Value { get; set; }

    public bool IsOnline { get; set; }

    public string? LastErrorMessage => VariableSource?.LastErrorMessage;

    public IVariableSource VariableSource { get; set; }

    public OperResult SetValue(object value, DateTime dateTime = default, bool isOnline = false)
    {
        Value = value ?? "null";
        IsOnline = isOnline;
        return new();
    }

    public Task<OperResult> SetValueToDeviceAsync(string value, string? executive = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OperResult());
    }
}