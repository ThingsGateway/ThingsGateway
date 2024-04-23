
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public static class ProtocolBaseExtension
{
    #region 读取

    /// <inheritdoc/>
    public static async Task<OperResult<Boolean>> ReadBooleanAsync(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadBooleanAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<Int16>> ReadInt16Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt16Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<UInt16>> ReadUInt16Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt16Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<Int32>> ReadInt32Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt32Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<UInt32>> ReadUInt32Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt32Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<Int64>> ReadInt64Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt64Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<UInt64>> ReadUInt64Async(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt64Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<Single>> ReadSingleAsync(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadSingleAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<Double>> ReadDoubleAsync(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadDoubleAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async Task<OperResult<String>> ReadStringAsync(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadStringAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Boolean> ReadBoolean(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadBoolean(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Int16> ReadInt16(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadInt16(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<UInt16> ReadUInt16(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadUInt16(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Int32> ReadInt32(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadInt32(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<UInt32> ReadUInt32(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadUInt32(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Int64> ReadInt64(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadInt64(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<UInt64> ReadUInt64(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadUInt64(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Single> ReadSingle(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadSingle(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<Double> ReadDouble(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadDouble(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static OperResult<String> ReadString(this IProtocol protocol, string address, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        var result = protocol.ReadString(address, 1, bitConverter, cancellationToken);
        return result.OperResultFrom(() => result.Content[0]);
    }

    #endregion 读取
}
