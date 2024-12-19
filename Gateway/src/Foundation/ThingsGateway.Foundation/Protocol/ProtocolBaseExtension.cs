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
/// 协议基类
/// </summary>
public static partial class ProtocolBaseExtension
{
    #region 读取

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Boolean>> ReadBooleanAsync(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadBooleanAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Double>> ReadDoubleAsync(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadDoubleAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int16>> ReadInt16Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt16Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int32>> ReadInt32Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt32Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int64>> ReadInt64Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadInt64Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Single>> ReadSingleAsync(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadSingleAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<String>> ReadStringAsync(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadStringAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt16>> ReadUInt16Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt16Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt32>> ReadUInt32Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt32Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt64>> ReadUInt64Async(this IProtocol protocol, string address, CancellationToken cancellationToken = default)
    {
        var result = await protocol.ReadUInt64Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    #endregion 读取
}
