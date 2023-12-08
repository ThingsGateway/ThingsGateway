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
using ThingsGateway.Foundation.Resources;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http客户端
    /// </summary>
    public class HttpClient : HttpClientBase
    {
    }

    /// <summary>
    /// Http客户端基类
    /// </summary>
    public class HttpClientBase : TcpClientBase, IHttpClient
    {
        #region 字段

        private readonly SemaphoreSlim m_semaphoreForRequest = new SemaphoreSlim(1, 1);
        private readonly WaitData<HttpResponse> m_waitData = new WaitData<HttpResponse>();
        private readonly WaitDataAsync<HttpResponse> m_waitDataAsync = new WaitDataAsync<HttpResponse>();
        private bool m_getContent;

        #endregion 字段

        /// <inheritdoc/>
        public override void Connect(int timeout, CancellationToken token)
        {
            if (this.Config.GetValue(HttpConfigExtensions.HttpProxyProperty) is HttpProxy httpProxy)
            {
                var proxyHost = httpProxy.Host;
                var credential = httpProxy.Credential;
                var remoteHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                try
                {
                    this.Config.SetRemoteIPHost(proxyHost);
                    base.Connect(timeout, token);
                    var request = new HttpRequest();
                    request.InitHeaders()
                        .SetHost(remoteHost.Host)
                        .SetProxyHost(remoteHost.Host)
                        .AsMethod("CONNECT");
                    var response = this.Request(request, false, timeout, token);
                    if (response.IsProxyAuthenticationRequired)
                    {
                        if (credential is null)
                        {
                            throw new Exception("未指定代理的凭据。");
                        }
                        var authHeader = response.Headers.Get(HttpHeaders.ProxyAuthenticate);
                        if (authHeader.IsNullOrEmpty())
                        {
                            throw new Exception("未指定代理身份验证质询。");
                        }

                        var ares = new AuthenticationChallenge(authHeader, credential);

                        request.Headers.Add(HttpHeaders.ProxyAuthorization, ares.ToString());
                        if (!response.KeepAlive)
                        {
                            base.Close("代理要求关闭连接，随后重写连接。");
                            base.Connect(timeout, token);
                        }

                        response = this.Request(request, false, timeout, token);
                    }

                    if (response.StatusCode != 200)
                    {
                        throw new Exception(response.StatusMessage);
                    }
                }
                finally
                {
                    this.Config.SetRemoteIPHost(remoteHost);
                }
            }
            else
            {
                base.Connect(timeout, token);
            }
        }

        /// <inheritdoc/>
        public override async Task ConnectAsync(int timeout, CancellationToken token)
        {
            if (this.Config.GetValue(HttpConfigExtensions.HttpProxyProperty) is HttpProxy httpProxy)
            {
                var proxyHost = httpProxy.Host;
                var credential = httpProxy.Credential;
                var remoteHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                try
                {
                    this.Config.SetRemoteIPHost(proxyHost);
                    await base.ConnectAsync(timeout, token);
                    var request = new HttpRequest();
                    request.InitHeaders()
                        .SetHost(remoteHost.Host)
                        .SetProxyHost(remoteHost.Host)
                        .AsMethod("CONNECT");
                    var response = await this.RequestAsync(request, false, timeout, token);
                    if (response.IsProxyAuthenticationRequired)
                    {
                        if (credential is null)
                        {
                            throw new Exception("未指定代理的凭据。");
                        }
                        var authHeader = response.Headers.Get(HttpHeaders.ProxyAuthenticate);
                        if (authHeader.IsNullOrEmpty())
                        {
                            throw new Exception("未指定代理身份验证质询。");
                        }

                        var ares = new AuthenticationChallenge(authHeader, credential);

                        request.Headers.Add(HttpHeaders.ProxyAuthorization, ares.ToString());
                        if (!response.KeepAlive)
                        {
                            base.Close("代理要求关闭连接，随后重写连接。");
                            await base.ConnectAsync(timeout, token);
                        }

                        response = await this.RequestAsync(request, timeout: timeout);
                    }

                    if (response.StatusCode != 200)
                    {
                        throw new Exception(response.StatusMessage);
                    }
                }
                finally
                {
                    this.Config.SetRemoteIPHost(remoteHost);
                }
            }
            else
            {
                await base.ConnectAsync(timeout, token);
            }
        }

        /// <inheritdoc/>
        public HttpResponse Request(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            try
            {
                this.m_semaphoreForRequest.Wait(token);
                this.m_getContent = false;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);
                    this.DefaultSend(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }
                    switch (this.m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketHttpResource.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            throw new OperationCanceledException();
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
            }
            finally
            {
                this.m_semaphoreForRequest.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponse> RequestAsync(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            try
            {
                await this.m_semaphoreForRequest.WaitAsync(timeout, token);
                this.m_getContent = false;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);
                    await this.DefaultSendAsync(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }
                    switch (await this.m_waitDataAsync.WaitAsync(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitDataAsync.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketHttpResource.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            throw new OperationCanceledException();
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
            }
            finally
            {
                this.m_semaphoreForRequest.Release();
            }
        }

        /// <inheritdoc/>
        public HttpResponse RequestContent(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            try
            {
                this.m_semaphoreForRequest.Wait(token);
                this.m_getContent = true;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);

                    this.DefaultSend(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }

                    switch (this.m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketHttpResource.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            throw new OperationCanceledException();
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
            }
            finally
            {
                this.m_semaphoreForRequest.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponse> RequestContentAsync(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            try
            {
                await this.m_semaphoreForRequest.WaitAsync(timeout, token);
                this.m_getContent = true;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);

                    await this.DefaultSendAsync(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }

                    switch (await this.m_waitDataAsync.WaitAsync(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketHttpResource.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            throw new OperationCanceledException();
                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
            }
            finally
            {
                this.m_semaphoreForRequest.Release();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_waitData?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnConnecting(ConnectingEventArgs e)
        {
            this.Protocol = Protocol.Http;
            this.SetDataHandlingAdapter(new HttpClientDataHandlingAdapter());
            await base.OnConnecting(e);
        }

        /// <inheritdoc/>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            this.m_waitData.Cancel();
            this.m_waitDataAsync.Cancel();
            await base.OnDisconnected(e);
        }

        /// <inheritdoc/>
        protected override Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is HttpResponse response)
            {
                if (this.m_getContent)
                {
                    response.TryGetContent(out _);
                }
                this.Set(response);
            }

            return base.ReceivedData(e);
        }

        private void Reset(CancellationToken token)
        {
            this.m_waitData.Reset();
            this.m_waitDataAsync.Reset();
            this.m_waitData.SetCancellationToken(token);
            this.m_waitDataAsync.SetCancellationToken(token);
        }

        private void Set(HttpResponse response)
        {
            this.m_waitData.Set(response);
            this.m_waitDataAsync.Set(response);
        }
    }
}