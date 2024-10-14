//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public abstract class ProtocolBase : DisposableObject, IProtocol
{
    /// <inheritdoc/>
    public ProtocolBase(IChannel channel)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));
        lock (channel)
        {
            channel.Collects.Add(this);
            Channel = channel;
            Logger = channel.Logger;
            Channel.Starting.Add(ChannelStarting);
            Channel.Stoped.Add(ChannelStoped);
            Channel.Stoping.Add(ChannelStoping);
            Channel.Started.Add(ChannelStarted);
            Channel.ChannelReceived.Add(ChannelReceived);
            Channel.Config.ConfigurePlugins(ConfigurePlugins());
        }
    }

    /// <inheritdoc/>
    ~ProtocolBase()
    {
        this.SafeDispose();
    }

    #region 属性

    /// <inheritdoc/>
    public virtual int CacheTimeout { get; set; } = 1000;

    /// <inheritdoc/>
    public virtual int SendDelayTime { get; set; }

    /// <inheritdoc/>
    public virtual int Timeout { get; set; } = 3000;

    /// <inheritdoc/>
    public virtual ushort ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// <inheritdoc cref="IThingsGatewayBitConverter.IsStringReverseByteWord"/>
    /// </summary>
    public bool IsStringReverseByteWord
    {
        get
        {
            return ThingsGatewayBitConverter.IsStringReverseByteWord;
        }
        set
        {
            ThingsGatewayBitConverter.IsStringReverseByteWord = value;
        }
    }

    /// <inheritdoc/>
    public virtual DataFormatEnum DataFormat
    {
        get => ThingsGatewayBitConverter.DataFormat;
        set => ThingsGatewayBitConverter.DataFormat = value;
    }

    /// <inheritdoc/>
    public virtual IChannel Channel { get; }

    /// <inheritdoc/>
    public virtual ILog? Logger { get; protected set; }

    /// <inheritdoc/>
    public virtual int RegisterByteLength { get; protected set; } = 1;

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter();

    /// <inheritdoc/>
    public bool OnLine => Channel.Online;

    #endregion 属性

    #region 适配器

    /// <inheritdoc/>
    public abstract DataHandlingAdapter GetDataAdapter();

    /// <summary>
    /// 通道连接成功时，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual Task<bool> ChannelStarted(IClientChannel channel)
    {
        return Task.FromResult(false);
    }
    /// <summary>
    /// 通道断开连接前，如果通道存在其他设备并且不希望其他设备处理时，返回null
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual Task<bool> ChannelStoping(IClientChannel channel)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// 通道断开连接后，如果通道存在其他设备并且不希望其他设备处理时，返回null
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected Task<bool> ChannelStoped(IClientChannel channel)
    {
        try
        {
            channel.WaitHandlePool.CancelAll();
        }
        catch
        {
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// 通道即将连接成功时，会设置适配器，如果通道存在其他设备并且不希望其他设备处理时，返回null
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual Task<bool> ChannelStarting(IClientChannel channel)
    {
        channel.SetDataHandlingAdapter(GetDataAdapter());
        return Task.FromResult(false);
    }

    /// <summary>
    /// 检测通道是否存在其他设备，如果有的话会重新设置，没有则无任何操作
    /// </summary>
    protected virtual void SetDataAdapter()
    {
        if (Channel.Collects.Count > 1)
        {
            if (Channel is IClientChannel clientChannel)
            {
                var dataHandlingAdapter = GetDataAdapter();
                if (dataHandlingAdapter.GetType() != clientChannel.ReadOnlyDataHandlingAdapter?.GetType())
                    clientChannel.SetDataHandlingAdapter(dataHandlingAdapter);
            }
        }
    }

    #endregion 适配器

    #region 变量地址解析

    /// <inheritdoc/>
    public abstract List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime) where T : IVariableSource, new();

    /// <inheritdoc/>
    public virtual string GetAddressDescription()
    {
        return DefaultResource.Localizer["DefaultAddressDes"];
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public int GetBitOffsetDefault(string address)
    {
        return GetBitOffset(address) ?? 0;
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public virtual int? GetBitOffset(string address)
    {
        int? bitIndex = null;
        if (address?.IndexOf('.') > 0)
            bitIndex = address.SplitStringByDelimiter().Last().ToInt();
        return bitIndex;
    }

    /// <inheritdoc/>
    public virtual bool BitReverse(string address)
    {
        return address?.IndexOf('.') > 0;
    }

    /// <inheritdoc/>
    public virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool && GetBitOffset(address) != null)
        {
            var data = Math.Ceiling((double)length / RegisterByteLength / 8);
            return (int)data;
        }
        else
        {
            return (int)result;
        }
    }

    #endregion 变量地址解析

    #region 设备异步返回

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Channel.ConnectAsync(ConnectTimeout, cancellationToken);
    }

    /// <inheritdoc/>
    public Task CloseAsync(string msg = default)
    {
        return Channel.CloseAsync(msg);
    }

    /// <summary>
    /// 接收,非主动发送的情况，重写实现非主从并发通讯协议，如果通道存在其他设备并且不希望其他设备处理时，设置<see cref="TouchSocketEventArgs.Handled"/> 为true
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task ChannelReceived(IClientChannel client, ReceivedDataEventArgs e)
    {

        if (e.RequestInfo is MessageBase response)
        {
            try
            {
                if (!client.WaitHandlePool.SetRun(response))
                {
                    //非主动发送的情况，重写实现非主从并发通讯协议
                }
                else
                {
                    e.Handled = true;
                    //Logger?.LogTrace($"MessageBase.Sign : {response.Sign}");
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, $"Response {response.Sign}");
            }
        }

        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult> SendAsync(ISendMessage sendMessage, IClientChannel channel = default, CancellationToken token = default)
    {
        try
        {
            try
            {
                if (!Channel.Online)
                    await Channel.ConnectAsync(ConnectTimeout, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Task.Delay(200, token).ConfigureAwait(false);
                return new(ex);
            }

            if (SendDelayTime != 0)
                await Task.Delay(SendDelayTime, token).ConfigureAwait(false);

            if (token.IsCancellationRequested)
                return new(new OperationCanceledException());


            if (channel == default)
            {
                if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
                try
                {
                    if (IsSingleThread)
                        await clientChannel.WaitLock.WaitAsync(token).ConfigureAwait(false);
                    await clientChannel.SendAsync(sendMessage).ConfigureAwait(false);
                }
                finally
                {
                    if (IsSingleThread)
                        clientChannel.WaitLock.Release();
                }
            }
            else
            {
                try
                {
                    if (IsSingleThread)
                        await channel.WaitLock.WaitAsync(token).ConfigureAwait(false);
                    await channel.SendAsync(sendMessage).ConfigureAwait(false);
                }
                finally
                {
                    if (IsSingleThread)
                        channel.WaitLock.Release();
                }
            }
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult> SendAsync(ISendMessage sendMessage, string socketId, CancellationToken cancellationToken)
    {
        try
        {
            var channelResult = await GetChannelAsync(socketId).ConfigureAwait(false);
            if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);

            return await SendAsync(sendMessage, channelResult.Content, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<IClientChannel>> GetChannelAsync(string socketId)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceChannel)Channel).TryGetClient($"ID={socketId}", out TcpSessionClientChannel? client))
            {
                return new OperResult<IClientChannel>() { Content = client };
            }
            else
            {
                await Task.Delay(1000).ConfigureAwait(false);
                return (new OperResult<IClientChannel>(DefaultResource.Localizer["DtuNoConnectedWaining", socketId]));
            }
        }
        else
            return new OperResult<IClientChannel>() { Content = (IClientChannel)Channel };
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<byte[]>> SendThenReturnAsync(ISendMessage sendMessage, string socketId, WaitDataAsync<MessageBase> waitData = default, CancellationToken cancellationToken = default)
    {
        var channelResult = await GetChannelAsync(socketId).ConfigureAwait(false);
        if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
        return await SendThenReturnAsync(sendMessage, channelResult.Content, waitData, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<byte[]>> SendThenReturnAsync(ISendMessage command, IClientChannel channel = default, WaitDataAsync<MessageBase> waitData = default, CancellationToken cancellationToken = default)
    {
        try
        {
            try
            {
                if (!Channel.Online)
                    await Channel.ConnectAsync(ConnectTimeout, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Task.Delay(200, cancellationToken).ConfigureAwait(false);
                return new(ex);
            }
            if (SendDelayTime != 0)
                await Task.Delay(SendDelayTime, cancellationToken).ConfigureAwait(false);

            MessageBase? result;

            if (channel == default)
            {
                if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
                if (waitData == default)
                {
                    waitData = clientChannel.WaitHandlePool.GetWaitDataAsync(out var sign);
                    command.Sign = sign;
                }
                result = await GetResponsedDataAsync(command, clientChannel, waitData, Timeout, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (waitData == default)
                {
                    waitData = channel.WaitHandlePool.GetWaitDataAsync(out var sign);
                    command.Sign = sign;
                }
                result = await GetResponsedDataAsync(command, channel, waitData, Timeout, cancellationToken).ConfigureAwait(false);
            }

            return new OperResult<byte[]>(result) { Content = result.Content };
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public virtual bool IsSingleThread { get; } = true;

    /// <summary>
    /// 发送并等待数据
    /// </summary>
    protected virtual async ValueTask<MessageBase> GetResponsedDataAsync(ISendMessage command, IClientChannel clientChannel, WaitDataAsync<MessageBase> waitData = default, int timeout = 3000, CancellationToken cancellationToken = default)
    {
        if (IsSingleThread)
            await clientChannel.WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (waitData == default)
        {
            waitData = clientChannel.WaitHandlePool.GetWaitDataAsync(out var sign);
            command.Sign = sign;
        }
        try
        {
            SetDataAdapter();
            waitData.SetCancellationToken(cancellationToken);

            //Logger?.LogTrace($"Command.Sign : {command.Sign}");

            await clientChannel.SendAsync(command).ConfigureAwait(false);
            var waitDataStatus = await waitData.WaitAsync(timeout).ConfigureAwait(false);
            var result = waitDataStatus.Check();
            if (result.IsSuccess)
            {
                var response = waitData.WaitResult;
                return response;
            }
            else
            {
                throw result.Exception ?? new(TouchSocketCoreResource.UnknownError);
            }
        }
        finally
        {
            clientChannel.WaitHandlePool.Destroy(waitData);
            if (IsSingleThread)
                clientChannel.WaitLock.Release();
        }
    }

    #endregion 设备异步返回

    #region 动态类型读写

    /// <inheritdoc/>
    public virtual async ValueTask<IOperResult<Array>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => await ReadStringAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Boolean => await ReadBooleanAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Byte => await ReadAsync(address, length, cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Int16 => await ReadInt16Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt16 => await ReadUInt16Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Int32 => await ReadInt32Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt32 => await ReadUInt32Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Int64 => await ReadInt64Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt64 => await ReadUInt64Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Single => await ReadSingleAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Double => await ReadDoubleAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            _ => new OperResult<Array>(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
        };
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult> WriteAsync(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (value is JArray jArray)
            {
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, jArray.ToObject<String[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Boolean => await WriteAsync(address, jArray.ToObject<Boolean[]>(), cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Byte => await WriteAsync(address, jArray.ToObject<Byte[]>(), cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int16 => await WriteAsync(address, jArray.ToObject<Int16[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt16 => await WriteAsync(address, jArray.ToObject<UInt16[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int32 => await WriteAsync(address, jArray.ToObject<Int32[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt32 => await WriteAsync(address, jArray.ToObject<UInt32[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int64 => await WriteAsync(address, jArray.ToObject<Int64[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt64 => await WriteAsync(address, jArray.ToObject<UInt64[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Single => await WriteAsync(address, jArray.ToObject<Single[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Double => await WriteAsync(address, jArray.ToObject<Double[]>(), cancellationToken: cancellationToken).ConfigureAwait(false),
                    _ => new OperResult(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
                };
            }
            else
            {
                var bitConverter = ThingsGatewayBitConverter.GetTransByAddress(ref address);
                if (bitConverter.ArrayLength > 1)
                {
                    return new OperResult("The array length is explicitly configured in the variable address, but the written value is not an array");
                }
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, value.ToObject<String>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Boolean => await WriteAsync(address, value.ToObject<Boolean>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Byte => await WriteAsync(address, value.ToObject<Byte>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int16 => await WriteAsync(address, value.ToObject<Int16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt16 => await WriteAsync(address, value.ToObject<UInt16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int32 => await WriteAsync(address, value.ToObject<Int32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt32 => await WriteAsync(address, value.ToObject<UInt32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int64 => await WriteAsync(address, value.ToObject<Int64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt64 => await WriteAsync(address, value.ToObject<UInt64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Single => await WriteAsync(address, value.ToObject<Single>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Double => await WriteAsync(address, value.ToObject<Double>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    _ => new OperResult(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
                };
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 动态类型读写

    #region 读取

    /// <inheritdoc/>
    public abstract ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Boolean[]>> ReadBooleanAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);

        var result = await ReadAsync(address, GetLength(address, length, RegisterByteLength, true), cancellationToken).ConfigureAwait(false);

        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content, GetBitOffsetDefault(address), length, BitReverse(address)));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int16[]>> ReadInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt16[]>> ReadUInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int32[]>> ReadInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt32[]>> ReadUInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int64[]>> ReadInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt64[]>> ReadUInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Single[]>> ReadSingleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToSingle(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Double[]>> ReadDoubleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToDouble(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<String[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new OperResult<String[]>(DefaultResource.Localizer["StringAddressError"]);
        var len = bitConverter.StringLength * length;

        var result = await ReadAsync(address, GetLength(address, len.Value, 1), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() =>
        {
            List<String> strings = new();
            for (int i = 0; i < length; i++)
            {
                var data = bitConverter.ToString(result.Content, i * bitConverter.StringLength.Value, bitConverter.StringLength.Value);
                strings.Add(data);
            }
            return strings.ToArray();
        }
        );
    }

    #endregion 读取

    #region 写入

    /// <inheritdoc/>
    public abstract ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract ValueTask<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, new bool[1] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, new byte[] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var data = bitConverter.GetBytes(value);
        return WriteAsync(address, data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length), cancellationToken);
    }

    #endregion 写入

    #region 写入数组

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult> WriteAsync(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new OperResult(DefaultResource.Localizer["StringAddressError"]);
        List<byte> bytes = new();
        foreach (var a in value)
        {
            var data = bitConverter.GetBytes(a);
            bytes.AddRange(data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length));
        }
        return await WriteAsync(address, bytes.ToArray(), cancellationToken).ConfigureAwait(false);
    }

    #endregion 写入数组

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (Channel != null)
        {
            lock (Channel)
            {

                if (Channel.Collects.Count == 1)
                {
                    try
                    {
                        //只关闭，不释放
                        Channel.SafeClose();
                        if (Channel is IClientChannel client)
                        {
                            client.WaitHandlePool.SafeDispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogWarning(ex);
                    }
                }
                if (Channel is TcpServiceChannel tcpServiceChannel)
                {
                    tcpServiceChannel.Clients.ForEach(a =>
                    {
                        a.WaitHandlePool.SafeDispose();
                    });
                    tcpServiceChannel.SafeClose();
                }
                Channel.Starting.Remove(ChannelStarting);
                Channel.Stoped.Remove(ChannelStoped);
                Channel.Started.Remove(ChannelStarted);
                Channel.Stoping.Remove(ChannelStoping);
                Channel.ChannelReceived.Remove(ChannelReceived);
                Channel.Collects.Remove(this);
            }
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public virtual Action<IPluginManager> ConfigurePlugins()
    {
        return a => { };
    }
}
