//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation;

/// <summary>
/// 测试通道
/// </summary>
public class OtherChannel : SetupConfigObject, IClientChannel
{
    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => m_dataHandlingAdapter;

    public OtherChannel(IChannelOptions channelOptions)
    {
        ChannelOptions = channelOptions;

        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    public override TouchSocketConfig Config => base.Config ?? ChannelOptions.Config;

    /// <inheritdoc/>
    public int MaxSign { get => WaitHandlePool.MaxSign; set => WaitHandlePool.MaxSign = value; }

    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; set; } = new();

    /// <inheritdoc/>
    public IChannelOptions ChannelOptions { get; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelOptions.ChannelType;

    /// <inheritdoc/>
    public ConcurrentList<IDevice> Collects { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoping { get; set; } = new();
    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();

    /// <inheritdoc/>
    public WaitLock WaitLock => ChannelOptions.WaitLock;

    /// <inheritdoc/>
    public ConcurrentDictionary<long, Func<IClientChannel, ReceivedDataEventArgs, bool, Task>> ChannelReceivedWaitDict { get; } = new();

    private readonly WaitLock _connectLock = new WaitLock();

    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);
    }
    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器实例为null，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放，如果是，则抛出异常。
        ThrowIfDisposed();
        // 检查adapter参数是否为null，如果是，则抛出ArgumentNullException异常。
        if (adapter is null)
        {
            throw new ArgumentNullException(nameof(adapter));
        }

        // 如果当前实例的配置不为空，则将配置应用到适配器上。
        if (Config != null)
        {
            adapter.Config(Config);
        }

        // 设置适配器的日志记录器和加载、接收数据的回调方法。
        adapter.Logger = Logger;
        adapter.OnLoaded(this);
        adapter.ReceivedAsyncCallBack = PrivateHandleReceivedData;
        //adapter.SendCallBack = this.ProtectedDefaultSend;
        adapter.SendAsyncCallBack = ProtectedDefaultSendAsync;

        // 将提供的适配器实例设置为当前实例的数据处理适配器。
        m_dataHandlingAdapter = adapter;
    }

    private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        LastReceivedTime = DateTime.Now;
        await this.OnChannelReceivedEvent(new ReceivedDataEventArgs(byteBlock, requestInfo), ChannelReceived).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步发送数据，保护方法。
    /// </summary>
    /// <param name="memory">待发送的字节数据内存。</param>
    /// <returns>异步任务。</returns>
    protected Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory)
    {
        LastSentTime = DateTime.Now;
        return Task.CompletedTask;
    }

    public Protocol Protocol => new Protocol("Other");

    public DateTime LastReceivedTime { get; private set; }

    public DateTime LastSentTime { get; private set; }

    public bool IsClient => true;

    public bool Online => true;

    public Task CloseAsync(string msg)
    {
        return Task.CompletedTask;
    }

    public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public async Task SendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        // 检查数据处理适配器是否存在且支持拼接发送
        if (m_dataHandlingAdapter == null || !m_dataHandlingAdapter.CanSplicingSend)
        {
            // 如果不支持拼接发送，则计算所有字节片段的总长度
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            // 使用计算出的总长度创建一个连续的内存块
            using (var byteBlock = new ByteBlock(length))
            {
                // 将每个字节片段写入连续的内存块
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(new ReadOnlySpan<byte>(item.Array, item.Offset, item.Count));
                }
                // 根据数据处理适配器的存在与否，选择不同的发送方式
                if (m_dataHandlingAdapter == null)
                {
                    // 如果没有数据处理适配器，则使用默认方式发送
                    await ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                else
                {
                    // 如果有数据处理适配器，则通过适配器发送
                    await m_dataHandlingAdapter.SendInputAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }
        else
        {
            // 如果数据处理适配器支持拼接发送，则直接发送字节列表
            await m_dataHandlingAdapter.SendInputAsync(transferBytes).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    public Task SendAsync(ReadOnlyMemory<byte> memory)
    {
        if (m_dataHandlingAdapter == null)
        {
            return ProtectedDefaultSendAsync(memory);
        }
        else
        {
            // 否则，使用适配器的发送方法进行数据发送。
            return m_dataHandlingAdapter.SendInputAsync(memory);
        }
    }

    public Task SendAsync(IRequestInfo requestInfo)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        ThrowIfCannotSendRequestInfo();

        // 使用数据处理适配器异步发送输入请求
        return m_dataHandlingAdapter.SendInputAsync(requestInfo);
    }
    private void ThrowIfCannotSendRequestInfo()
    {
        if (m_dataHandlingAdapter == null || !m_dataHandlingAdapter.CanSendRequestInfo)
        {
            throw new NotSupportedException($"当前适配器为空或者不支持对象发送。");
        }
    }

}
