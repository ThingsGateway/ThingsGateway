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
        private readonly object m_requestLocker = new object();
        private readonly WaitData<HttpResponse> m_waitData;
        private bool m_getContent;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpClientBase()
        {
            this.m_waitData = new WaitData<HttpResponse>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override ITcpClient Connect(int timeout = 5000)
        {
            if (this.Config.GetValue(HttpConfigExtensions.HttpProxyProperty) is HttpProxy httpProxy)
            {
                var proxyHost = httpProxy.Host;
                var credential = httpProxy.Credential;
                var remoteHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                try
                {
                    this.Config.SetRemoteIPHost(proxyHost);
                    base.Connect(timeout);
                    var request = new HttpRequest();
                    request.InitHeaders()
                        .SetHost(remoteHost.Host)
                        .SetUrl(remoteHost.Host, true)
                        .AsMethod("CONNECT");
                    var response = this.Request(request, timeout: timeout);
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
                            base.Connect(timeout);
                        }

                        response = this.Request(request, timeout: timeout);
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
                base.Connect(timeout);
            }
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <param name="onlyRequest"><inheritdoc/></param>
        /// <param name="timeout"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns></returns>
        public HttpResponse Request(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken cancellationToken = default)
        {
            lock (this.m_requestLocker)
            {
                this.m_getContent = false;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.m_waitData.Reset();
                    this.m_waitData.SetCancellationToken(cancellationToken);

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
                            return default;

                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onlyRequest"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public HttpResponse RequestContent(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken cancellationToken = default)
        {
            lock (this.m_requestLocker)
            {
                this.m_getContent = true;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.m_waitData.Reset();
                    this.m_waitData.SetCancellationToken(cancellationToken);

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
                            return default;

                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketHttpResource.UnknownError.GetDescription());
                    }
                }
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
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (requestInfo is HttpResponse response)
            {
                if (this.m_getContent)
                {
                    response.TryGetContent(out _);
                }
                this.m_waitData.Set(response);
            }

            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ConnectingEventArgs e)
        {
            this.Protocol = Protocol.Http;
            this.SetDataHandlingAdapter(new HttpClientDataHandlingAdapter());
            base.OnConnecting(e);
        }
    }
}