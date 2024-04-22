
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
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
        Channel.Received += Received;
        Channel.Config.ConfigurePlugins(ConfigurePlugins());
        channel.Setup(channel.Config);
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
    public virtual DataFormatEnum? DataFormat
    {
        get => ThingsGatewayBitConverter.DataFormat;
        set => ThingsGatewayBitConverter.DataFormat = value;
    }

    /// <inheritdoc/>
    public virtual int SendDelayTime { get; set; }

    /// <inheritdoc/>
    public virtual int Timeout { get; set; } = 3000;

    /// <summary>
    /// 连接超时时间
    /// </summary>
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
    public virtual IChannel Channel { get; }

    /// <inheritdoc/>
    public virtual ILog? Logger { get; protected set; }

    /// <inheritdoc/>
    public virtual int RegisterByteLength { get; protected set; } = 1;

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter(EndianType.Big);

    /// <inheritdoc/>
    public bool OnLine => Channel.CanSend;

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
        //取消全部等待池
        WaitHandlePool.CancelAll();

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
    public virtual bool BitReverse(string address)
    {
        return address?.IndexOf('.') > 0;
    }

    /// <inheritdoc/>
    public virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool && BitReverse(address))
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
    /// 等待池
    /// </summary>
    protected WaitHandlePool<MessageBase> WaitHandlePool = new();

    /// <summary>
    /// 接收,非主动发送的情况，重写实现非主从并发通讯协议
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Received(IClientChannel client, ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MessageBase response)
        {
            if (!WaitHandlePool.SetRun(response))
            {
                //非主动发送的情况，重写实现非主从并发通讯协议
            }
        }

        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void DefaultSend(byte[] command, IClientChannel channel = default)
    {
        Channel.Connect(ConnectTimeout);
        var item = command;
        if (SendDelayTime != 0)
            Thread.Sleep(SendDelayTime);
        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            clientChannel.DefaultSend(item);
        }
        else
        {
            channel.DefaultSend(item);
        }
    }

    /// <inheritdoc/>
    public virtual void Send(byte[] command, IClientChannel channel = default)
    {
        Channel.Connect(ConnectTimeout);
        var item = command;
        if (SendDelayTime != 0)
            Thread.Sleep(SendDelayTime);
        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            clientChannel.Send(item);
        }
        else
        {
            channel.Send(item);
        }
    }

    /// <inheritdoc/>
    public virtual async Task SendAsync(byte[] command, IClientChannel channel = default, CancellationToken cancellationToken = default)
    {
        var item = command;
        await Channel.ConnectAsync(ConnectTimeout, cancellationToken);
        if (SendDelayTime != 0)
            await Task.Delay(SendDelayTime, cancellationToken);
        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            await clientChannel.SendAsync(item);
        }
        else
        {
            await channel.SendAsync(item);
        }
    }

    /// <inheritdoc/>
    public virtual OperResult<byte[]> SendThenReturn(ISendMessage command, CancellationToken cancellationToken, IClientChannel channel = default)
    {
        var item = command;
        Channel.Connect(ConnectTimeout, cancellationToken);
        if (SendDelayTime != 0)
            Thread.Sleep(SendDelayTime);
        SetDataAdapter();
        MessageBase? result;

        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }

            result = GetResponsedData(item, Timeout, clientChannel, cancellationToken);
        }
        else
        {
            result = GetResponsedData(item, Timeout, channel, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<byte[]>> SendThenReturnAsync(ISendMessage command, CancellationToken cancellationToken, IClientChannel channel = default)
    {
        var item = command;
        await Channel.ConnectAsync(ConnectTimeout, cancellationToken);
        if (SendDelayTime != 0)
            await Task.Delay(SendDelayTime, cancellationToken);
        SetDataAdapter();
        MessageBase? result;

        if (channel == default)
        {
            if (Channel is not IClientChannel clientChannel) { throw new ArgumentNullException(nameof(channel)); }
            result = await GetResponsedDataAsync(item, Timeout, clientChannel, cancellationToken);
        }
        else
        {
            result = await GetResponsedDataAsync(item, Timeout, channel, cancellationToken);
        }

        return result;
    }

    public virtual OperResult<byte[]> SendThenReturn(string socketId, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={socketId}", out TgSocketClient? client))
                return SendThenReturn(new SendMessage(commandResult), cancellationToken, client);
            else
                return new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
            return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    public virtual Task<OperResult<byte[]>> SendThenReturnAsync(string socketId, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={socketId}", out TgSocketClient? client))
                return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken, client);
            else
                return Task.FromResult(new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]));
        }
        else
            return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }
    public virtual OperResult<byte[]> SendThenReturn(byte[] commandResult, CancellationToken cancellationToken)
    {
        return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    public virtual Task<OperResult<byte[]>> SendThenReturnAsync(byte[] commandResult, CancellationToken cancellationToken)
    {
        return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }

    public virtual OperResult Send(string socketId, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={socketId}", out TgSocketClient? client))
            {
                Send(commandResult, client);
                return new();
            }
            else
                return new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
        {
            Send(commandResult);
            return new();
        }
    }

    /// <summary>
    /// 是否需要并发锁，默认为true，对于工业主从协议，通常是必须的
    /// </summary>
    public virtual bool IsSingleThread { get; } = true;

    /// <summary>
    /// 实现等待数据，需要加锁
    /// </summary>
    /// <param name="item"></param>
    /// <param name="timeout"></param>
    /// <param name="clientChannel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<MessageBase> GetResponsedDataAsync(ISendMessage item, int timeout, IClientChannel clientChannel, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
            await clientChannel.WaitLock.WaitAsync(cancellationToken);
        var waitData = WaitHandlePool.GetWaitDataAsync(out var sign);
        try
        {
            item.Sign = sign;
            waitData.SetCancellationToken(cancellationToken);
            await clientChannel.SendAsync(item);
            var waitDataStatus = await waitData.WaitAsync(timeout);
            waitDataStatus.ThrowIfNotRunning();
            var response = waitData.WaitResult;
            return response;
        }
        finally
        {
            WaitHandlePool.Destroy(waitData);
            if (IsSingleThread)
                clientChannel.WaitLock.Release();
        }
    }

    /// <summary>
    /// 实现等待数据，需要加锁
    /// </summary>
    /// <param name="item"></param>
    /// <param name="timeout"></param>
    /// <param name="clientChannel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual MessageBase GetResponsedData(ISendMessage item, int timeout, IClientChannel clientChannel, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
            clientChannel.WaitLock.Wait(cancellationToken);
        var waitData = WaitHandlePool.GetWaitData(out var sign);
        try
        {
            item.Sign = sign;
            waitData.SetCancellationToken(cancellationToken);
            clientChannel.Send(item);
            var waitDataStatus = waitData.Wait(timeout);
            waitDataStatus.ThrowIfNotRunning();
            var response = waitData.WaitResult;
            return response;
        }
        finally
        {
            WaitHandlePool.Destroy(waitData);
            if (IsSingleThread)
                clientChannel.WaitLock.Release();
        }
    }

    #endregion 设备异步返回

    #region 动态类型读写

    /// <inheritdoc/>
    public virtual async Task<IOperResult<Array>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => await ReadStringAsync(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Boolean => await ReadBooleanAsync(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Byte => await ReadAsync(address, length, cancellationToken),
            DataTypeEnum.Int16 => await ReadInt16Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt16 => await ReadUInt16Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Int32 => await ReadInt32Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt32 => await ReadUInt32Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Int64 => await ReadInt64Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt64 => await ReadUInt64Async(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Single => await ReadSingleAsync(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Double => await ReadDoubleAsync(address, length, cancellationToken: cancellationToken),
            _ => new OperResult<Array>(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
        };
    }

    /// <inheritdoc/>
    public virtual IOperResult<Array> Read(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => ReadString(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Boolean => ReadBoolean(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Byte => Read(address, length, cancellationToken),
            DataTypeEnum.Int16 => ReadInt16(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt16 => ReadUInt16(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Int32 => ReadInt32(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt32 => ReadUInt32(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Int64 => ReadInt64(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.UInt64 => ReadUInt64(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Single => ReadSingle(address, length, cancellationToken: cancellationToken),
            DataTypeEnum.Double => ReadDouble(address, length, cancellationToken: cancellationToken),
            _ => new OperResult<Array>(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
        };
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult> WriteAsync(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (value is JArray jArray)
            {
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, jArray.ToObject<String[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Boolean => await WriteAsync(address, jArray.ToObject<Boolean[]>(), cancellationToken),
                    DataTypeEnum.Byte => await WriteAsync(address, jArray.ToObject<Byte[]>(), cancellationToken),
                    DataTypeEnum.Int16 => await WriteAsync(address, jArray.ToObject<Int16[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt16 => await WriteAsync(address, jArray.ToObject<UInt16[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Int32 => await WriteAsync(address, jArray.ToObject<Int32[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt32 => await WriteAsync(address, jArray.ToObject<UInt32[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Int64 => await WriteAsync(address, jArray.ToObject<Int64[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt64 => await WriteAsync(address, jArray.ToObject<UInt64[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Single => await WriteAsync(address, jArray.ToObject<Single[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Double => await WriteAsync(address, jArray.ToObject<Double[]>(), cancellationToken: cancellationToken),
                    _ => new OperResult(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
                };
            }
            else
            {
                var bitConverter = ThingsGatewayBitConverter.GetTransByAddress(ref address);
                if (bitConverter.ArrayLength > 1)
                {
                    return new("The array length is explicitly configured in the variable address, but the written value is not an array");
                }
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, value.ToObject<String>(), bitConverter, cancellationToken),
                    DataTypeEnum.Boolean => await WriteAsync(address, value.ToObject<Boolean>(), bitConverter, cancellationToken),
                    DataTypeEnum.Byte => await WriteAsync(address, value.ToObject<Byte>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int16 => await WriteAsync(address, value.ToObject<Int16>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt16 => await WriteAsync(address, value.ToObject<UInt16>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int32 => await WriteAsync(address, value.ToObject<Int32>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt32 => await WriteAsync(address, value.ToObject<UInt32>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int64 => await WriteAsync(address, value.ToObject<Int64>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt64 => await WriteAsync(address, value.ToObject<UInt64>(), bitConverter, cancellationToken),
                    DataTypeEnum.Single => await WriteAsync(address, value.ToObject<Single>(), bitConverter, cancellationToken),
                    DataTypeEnum.Double => await WriteAsync(address, value.ToObject<Double>(), bitConverter, cancellationToken),
                    _ => new OperResult(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
                };
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (value is JArray jArray)
            {
                return dataType switch
                {
                    DataTypeEnum.String => Write(address, jArray.ToObject<String[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Boolean => Write(address, jArray.ToObject<Boolean[]>(), cancellationToken),
                    DataTypeEnum.Byte => Write(address, jArray.ToObject<Byte[]>(), cancellationToken),
                    DataTypeEnum.Int16 => Write(address, jArray.ToObject<Int16[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt16 => Write(address, jArray.ToObject<UInt16[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Int32 => Write(address, jArray.ToObject<Int32[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt32 => Write(address, jArray.ToObject<UInt32[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Int64 => Write(address, jArray.ToObject<Int64[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.UInt64 => Write(address, jArray.ToObject<UInt64[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Single => Write(address, jArray.ToObject<Single[]>(), cancellationToken: cancellationToken),
                    DataTypeEnum.Double => Write(address, jArray.ToObject<Double[]>(), cancellationToken: cancellationToken),
                    _ => new OperResult(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
                };
            }
            else
            {
                var bitConverter = ThingsGatewayBitConverter.GetTransByAddress(ref address);
                if (bitConverter.ArrayLength > 1)
                {
                    return new("The array length is explicitly configured in the variable address, but the written value is not an array");
                }
                return dataType switch
                {
                    DataTypeEnum.String => Write(address, value.ToObject<String>(), bitConverter, cancellationToken),
                    DataTypeEnum.Boolean => Write(address, value.ToObject<Boolean>(), bitConverter, cancellationToken),
                    DataTypeEnum.Byte => Write(address, value.ToObject<Byte>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int16 => Write(address, value.ToObject<Int16>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt16 => Write(address, value.ToObject<UInt16>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int32 => Write(address, value.ToObject<Int32>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt32 => Write(address, value.ToObject<UInt32>(), bitConverter, cancellationToken),
                    DataTypeEnum.Int64 => Write(address, value.ToObject<Int64>(), bitConverter, cancellationToken),
                    DataTypeEnum.UInt64 => Write(address, value.ToObject<UInt64>(), bitConverter, cancellationToken),
                    DataTypeEnum.Single => Write(address, value.ToObject<Single>(), bitConverter, cancellationToken),
                    DataTypeEnum.Double => Write(address, value.ToObject<Double>(), bitConverter, cancellationToken),
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
    public abstract OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task<OperResult<Boolean[]>> ReadBooleanAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);

        var result = await ReadAsync(address, GetLength(address, length, RegisterByteLength, true), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content, GetBitOffset(address), length, BitReverse(address)));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<Int16[]>> ReadInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<UInt16[]>> ReadUInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<Int32[]>> ReadInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<UInt32[]>> ReadUInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<Int64[]>> ReadInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<UInt64[]>> ReadUInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<Single[]>> ReadSingleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToSingle(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<Double[]>> ReadDoubleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToDouble(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult<String[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new(DefaultResource.Localizer["StringAddressError"]);
        var len = bitConverter.StringLength * length;

        var result = await ReadAsync(address, GetLength(address, len.Value, 1), cancellationToken);
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

    /// <inheritdoc/>
    public virtual OperResult<Boolean[]> ReadBoolean(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, RegisterByteLength, true), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content, GetBitOffset(address), length, BitReverse(address)));
    }

    /// <inheritdoc/>
    public virtual OperResult<Int16[]> ReadInt16(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<UInt16[]> ReadUInt16(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt16(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<Int32[]> ReadInt32(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<UInt32[]> ReadUInt32(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<Int64[]> ReadInt64(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<UInt64[]> ReadUInt64(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToUInt64(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<Single[]> ReadSingle(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToSingle(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<Double[]> ReadDouble(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => bitConverter.ToDouble(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public virtual OperResult<String[]> ReadString(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new(DefaultResource.Localizer["StringAddressError"]);
        var len = bitConverter.StringLength * length;

        var result = Read(address, GetLength(address, len.Value, 1), cancellationToken);
        return result.OperResultFrom((Func<string[]>)(() =>
        {
            List<string> strings = new();
            for (int i = 0; i < length; i++)
            {
                var data = bitConverter.ToString(result.Content, (int)(i * bitConverter.StringLength.Value), (int)bitConverter.StringLength.Value);
                strings.Add(data);
            }
            return strings.ToArray();
        })
        );
    }

    #endregion 读取

    #region 写入

    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, new bool[1] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var data = bitConverter.GetBytes(value);
        return WriteAsync(address, data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length), cancellationToken);
    }

    /// <inheritdoc/>
    public abstract OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual OperResult Write(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        return Write(address, new bool[1] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var data = bitConverter.GetBytes(value);
        return Write(address, data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length), cancellationToken);
    }

    #endregion 写入

    #region 写入数组

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return WriteAsync(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<OperResult> WriteAsync(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new(DefaultResource.Localizer["StringAddressError"]);
        List<byte> bytes = new();
        value.ForEach((Action<string>)(a =>
        {
            var data = bitConverter.GetBytes(a);
            bytes.AddRange(data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length));
        }));
        return await WriteAsync(address, bytes.ToArray(), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        return Write(address, bitConverter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.StringLength == null) return new(DefaultResource.Localizer["StringAddressError"]);
        List<byte> bytes = new();
        value.ForEach((Action<string>)(a =>
        {
            var data = bitConverter.GetBytes(a);
            bytes.AddRange(data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length));
        }));
        return Write(address, bytes.ToArray(), cancellationToken);
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
                Channel.Received -= Received;
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
                Channel.Collects.Remove(this);
                if (Channel.Collects.Count == 0)
                {
                    try
                    {
                        //只关闭，不释放
                        Channel.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex);
                    }
                }
            }
        }
        WaitHandlePool.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public virtual Action<IPluginManager> ConfigurePlugins()
    {
        return a => { };
    }
}
