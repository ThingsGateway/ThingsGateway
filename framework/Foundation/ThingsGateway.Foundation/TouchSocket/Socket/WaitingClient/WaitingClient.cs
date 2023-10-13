#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Sockets
{
    internal class WaitingClient<TClient> : DisposableObject, IWaitingClient<TClient> where TClient : IClient, ISender
    {
        private readonly Func<ResponsedData, bool> m_func;
        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
        private volatile bool m_breaked;
        private CancellationTokenSource m_cancellation;

        public WaitingClient(TClient client, WaitingOptions waitingOptions, Func<ResponsedData, bool> func)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.WaitingOptions = waitingOptions;
            this.m_func = func;
        }

        public WaitingClient(TClient client, WaitingOptions waitingOptions)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.WaitingOptions = waitingOptions;
        }

        public bool CanSend
        {
            get
            {
                return this.Client is ITcpClientBase tcpClient ? tcpClient.CanSend : this.Client is ISerialSessionBase serialSession ? serialSession.CanSend : this.Client is IUdpSession;
            }
        }

        public TClient Client { get; private set; }

        public WaitingOptions WaitingOptions { get; set; }

        protected override void Dispose(bool disposing)
        {
            this.Client = default;
            this.Cancel();
            base.Dispose(disposing);
        }

        private void Cancel()
        {
            this.m_cancellation.Cancel();
        }

        #region 同步Response

        public ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            try
            {
                this.m_semaphoreSlim.Wait();
                this.m_breaked = false;
                if (token.CanBeCanceled)
                {
                    using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
                    m_cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);
                }
                else
                {
                    m_cancellation = new CancellationTokenSource(timeout);
                }
                using (m_cancellation)
                {
                    if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                    {
                        using (var receiver = session.CreateReceiver())
                        {
                            session.Send(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);

                            while (true)
                            {
                                using (var receiverResult = receiver.ReadAsync(m_cancellation.Token).GetFalseAwaitResult())
                                {
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var receiver = this.Client.CreateReceiver())
                        {
                            this.Client.Send(buffer, offset, length);
                            while (true)
                            {
                                using (var receiverResult = receiver.ReadAsync(m_cancellation.Token).GetFalseAwaitResult())
                                {
                                    if (receiverResult.IsClosed)
                                    {
                                        this.m_breaked = true;
                                        this.Cancel();
                                    }
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);

                                    if (this.m_func == null)
                                    {
                                        return response;
                                    }
                                    else
                                    {
                                        if (this.m_func.Invoke(response))
                                        {
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : throw new TimeoutException();
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        public ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, 0, buffer.Length, timeout, token);
        }

        public ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion 同步Response

        #region Response异步

        public async Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            try
            {
                await this.m_semaphoreSlim.WaitAsync();
                this.m_breaked = false;
                if (token.CanBeCanceled)
                {
                    using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
                    m_cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);
                }
                else
                {
                    m_cancellation = new CancellationTokenSource(timeout);
                }
                using (m_cancellation)
                {
                    if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                    {
                        using (var receiver = session.CreateReceiver())
                        {
                            await session.SendAsync(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);

                            while (true)
                            {
                                using (var receiverResult = await receiver.ReadAsync(m_cancellation.Token))
                                {
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var receiver = this.Client.CreateReceiver())
                        {
                            await this.Client.SendAsync(buffer, offset, length);
                            while (true)
                            {
                                using (var receiverResult = await receiver.ReadAsync(m_cancellation.Token))
                                {
                                    if (receiverResult.IsClosed)
                                    {
                                        this.m_breaked = true;
                                        this.Cancel();
                                    }
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);

                                    if (this.m_func == null)
                                    {
                                        return response;
                                    }
                                    else
                                    {
                                        if (this.m_func.Invoke(response))
                                        {
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : throw new TimeoutException();
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponseAsync(buffer, 0, buffer.Length, timeout, token);
        }

        public Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponseAsync(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion Response异步

        #region 字节同步

        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, offset, length, timeout, token).Data;
        }

        public byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(buffer, 0, buffer.Length, timeout, token);
        }

        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion 字节同步

        #region 字节异步

        public async Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(buffer, offset, length, timeout, token)).Data;
        }

        public async Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(buffer, 0, buffer.Length, timeout, token)).Data;
        }

        public async Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(byteBlock.Buffer, 0, byteBlock.Len, timeout, token)).Data;
        }

        #endregion 字节异步
    }
}