//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public static partial class DeviceExtension
{
    #region 读取

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Boolean>> ReadBooleanAsync(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadBooleanAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Double>> ReadDoubleAsync(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadDoubleAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int16>> ReadInt16Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt16Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int32>> ReadInt32Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt32Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int64>> ReadInt64Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt64Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Single>> ReadSingleAsync(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadSingleAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<String>> ReadStringAsync(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadStringAsync(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt16>> ReadUInt16Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt16Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt32>> ReadUInt32Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt32Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt64>> ReadUInt64Async(this IDevice device, string address, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt64Async(address, 1, null, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content[0]);
    }

    #endregion 读取



    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="IVariable.Index"/>
    /// 不支持变长字符串类型变量，不能存在于变量List中
    /// </summary>
    /// <param name="device">设备</param>
    /// <param name="variables">设备变量List</param>
    /// <param name="buffer">返回的字节数组</param>
    /// <param name="exWhenAny">任意一个失败时抛出异常</param>
    /// <returns>解析结果</returns>
    public static OperResult PraseStructContent<T>(this IEnumerable<T> variables, IDevice device, byte[] buffer, bool exWhenAny) where T : IVariable
    {
        var time = DateTime.Now;
        var result = OperResult.Success;
        foreach (var variable in variables)
        {
            IThingsGatewayBitConverter byteConverter = variable.ThingsGatewayBitConverter;
            var dataType = variable.DataType;
            int index = variable.Index;
            try
            {
                var data = byteConverter.GetDataFormBytes(device, variable.RegisterAddress, buffer, index, dataType, variable.ArrayLength ?? 1);
                result = Set(variable, data);
                if (exWhenAny)
                    if (!result.IsSuccess)
                        return result;
            }
            catch (Exception ex)
            {
                return new OperResult($"Error parsing byte array, address: {variable.RegisterAddress}, array length: {buffer.Length}, index: {index}, type: {dataType}", ex);
            }
        }
        return result;
        OperResult Set(IVariable organizedVariable, object num)
        {
            return organizedVariable.SetValue(num, time);
        }
    }

    /// <summary>
    /// 当状态不是<see cref="WaitDataStatus.SetRunning"/>时返回异常。
    /// </summary>
    public static OperResult Check(this WaitDataAsync<MessageBase> waitDataAsync)
    {
        switch (waitDataAsync.Status)
        {
            case WaitDataStatus.SetRunning:
                return new();

            case WaitDataStatus.Canceled: return new(new OperationCanceledException());
            case WaitDataStatus.Overtime: return waitDataAsync.WaitResult == null ? new(new TimeoutException()) : new(waitDataAsync.WaitResult);
            case WaitDataStatus.Disposed:
            case WaitDataStatus.Default:
            default:
                {
                    return waitDataAsync.WaitResult == null ? new(new Exception(TouchSocketCoreResource.UnknownError)) : new(waitDataAsync.WaitResult);
                }
        }
    }
}
