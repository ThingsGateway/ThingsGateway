//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

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
        channel.Collects.Add(this);
        Channel = channel;
        Logger = channel.Logger;
        Channel.Starting += ChannelStarting;
        Channel.Stoped += ChannelStoped;
        Channel.Started += ChannelStarted;
        Channel.ChannelReceived += ChannelReceived;
        Channel.Config.ConfigurePlugins(ConfigurePlugins());
        channel.Setup(channel.Config.Clone());
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
    public virtual EndianType EndianType
    {
        get => ThingsGatewayBitConverter.EndianType;
        set => ThingsGatewayBitConverter.EndianType = value;
    }

    /// <inheritdoc/>
    public virtual int SendDelayTime { get; set; }

    /// <inheritdoc/>
    public virtual int Timeout { get; set; } = 3000;

    /// <inheritdoc/>
    public virtual ushort ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// <inheritdoc cref="IThingsGatewayBitConverter.IsBoolReverseByteWord"/>
    /// </summary>
    public bool IsBoolReverseByteWord
    {
        get
        {
            return ThingsGatewayBitConverter.IsBoolReverseByteWord;
        }
        set
        {
            ThingsGatewayBitConverter.IsBoolReverseByteWord = value;
        }
    }

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
    /// 通道连接成功时
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual Task ChannelStarted(IClientChannel channel)
    {
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 通道断开连接后
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected Task ChannelStoped(IClientChannel channel)
    {
        if (channel == this.Channel)
        {
            //取消全部等待池
            channel.WaitHandlePool.CancelAll();
        }
        else
        {
        }

        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 通道即将连接成功时,会设置适配器
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected virtual Task ChannelStarting(IClientChannel channel)
    {
        channel.SetDataHandlingAdapter(GetDataAdapter());
        return EasyTask.CompletedTask;
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
                clientChannel.SetDataHandlingAdapter(GetDataAdapter());
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
    public virtual int GetBitOffset(string address)
    {
        int bitIndex = 0;
        if (address?.IndexOf('.') > 0)
            bitIndex = address.SplitStringByDelimiter().Last().ToInt();
        return bitIndex;
    }

    /// <inheritdoc/>
    public virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool)
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

    /// <summary>
    /// 接收,非主动发送的情况，重写实现非主从并发通讯协议
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task ChannelReceived(IClientChannel client, ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MessageBase response)
        {
            if (!client.WaitHandlePool.SetRun(response))
            {
                //非主动发送的情况，重写实现非主从并发通讯协议
            }
        }

        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async ValueTask SendAsync(ISendMessage sendMessage, IClientChannel channel = default, CancellationToken token = default)
    {
        await Channel.ConnectAsync(ConnectTimeout, token).ConfigureAwait(false);
        if (SendDelayTime != 0)
            await Task.Delay(SendDelayTime, token).ConfigureAwait(false);

        if (token.IsCancellationRequested)
            return;

        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            await clientChannel.SendAsync(sendMessage).ConfigureAwait(false);
        }
        else
        {
            await channel.SendAsync(sendMessage).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult> SendAsync(string socketId, ISendMessage sendMessage, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceChannel)Channel).Clients.TryGetClient($"ID={socketId}", out TcpSessionClientChannel? client))
            {
                await SendAsync(sendMessage, client);
                return OperResult.Success;
            }
            else
                return new OperResult(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
        {
            await SendAsync(sendMessage);
            return OperResult.Success;
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<byte[]>> SendThenReturnAsync(ISendMessage command, CancellationToken cancellationToken, IClientChannel channel = default)
    {
        await Channel.ConnectAsync(ConnectTimeout, cancellationToken).ConfigureAwait(false);
        if (SendDelayTime != 0)
            await Task.Delay(SendDelayTime, cancellationToken).ConfigureAwait(false);
        SetDataAdapter();

        MessageBase? result;

        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            result = await GetResponsedDataAsync(command, Timeout, clientChannel, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            result = await GetResponsedDataAsync(command, Timeout, channel, cancellationToken).ConfigureAwait(false);
        }

        return new OperResult<byte[]>(result) { Content = result.Content };
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult<byte[]>> SendThenReturnAsync(byte[] sendBytes, CancellationToken cancellationToken)
    {
        return SendThenReturnAsync(new SendMessage(sendBytes), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult<byte[]>> SendThenReturnAsync(string socketId, byte[] sendBytes, CancellationToken cancellationToken)
    {
        return SendThenReturnAsync(socketId, new SendMessage(sendBytes), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<byte[]>> SendThenReturnAsync(string socketId, ISendMessage sendMessage, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceChannel)Channel).Clients.TryGetClient($"ID={socketId}", out TcpSessionClientChannel? client))
                return await SendThenReturnAsync(sendMessage, cancellationToken, client);
            else
                return (new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]));
        }
        else
            return await SendThenReturnAsync(sendMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual bool IsSingleThread { get; } = true;

    /// <summary>
    /// 实现等待数据，需要加锁
    /// </summary>
    /// <param name="item"></param>
    /// <param name="timeout"></param>
    /// <param name="clientChannel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async ValueTask<MessageBase> GetResponsedDataAsync(ISendMessage item, int timeout, IClientChannel clientChannel, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
            await clientChannel.WaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        var waitData = clientChannel.WaitHandlePool.GetWaitDataAsync(out var sign);
        try
        {
            item.Sign = sign;
            waitData.SetCancellationToken(cancellationToken);
            await clientChannel.SendAsync(item).ConfigureAwait(false);
            var waitDataStatus = await waitData.WaitAsync(timeout).ConfigureAwait(false);
            waitDataStatus.ThrowIfNotRunning();
            var response = waitData.WaitResult;
            return response;
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

        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content, GetBitOffset(address), length));
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
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
                Channel.Starting -= ChannelStarting;
                Channel.Stoped -= ChannelStoped;
                Channel.Started -= ChannelStarted;
                Channel.ChannelReceived -= ChannelReceived;
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
                Channel.Collects.Remove(this);
                if (Channel.Collects.Count == 0)
                {
                    try
                    {
                        //只关闭，不释放
                        Channel.Close();
                        if (Channel is IClientChannel client)
                            client.WaitHandlePool.SafeDispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex);
                    }
                }
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
