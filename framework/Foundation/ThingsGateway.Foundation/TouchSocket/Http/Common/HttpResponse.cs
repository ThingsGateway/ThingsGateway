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
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http响应
    /// </summary>
    public class HttpResponse : HttpBase
    {
        private bool m_canRead;
        private bool m_canWrite;
        private ITcpClientBase m_client;
        private byte[] m_content;
        private bool m_sentHeader;
        private long m_sentLength;

        /// <summary>
        /// Http响应
        /// </summary>
        /// <param name="client"></param>
        /// <param name="isServer"></param>
        public HttpResponse(ITcpClientBase client, bool isServer = true)
        {
            this.m_client = client;
            if (isServer)
            {
                this.m_canRead = false;
                this.m_canWrite = true;
            }
            else
            {
                this.m_canRead = true;
                this.m_canWrite = false;
            }
        }

        /// <summary>
        /// 从<see cref="HttpRequest"/>创建一个Http响应
        /// </summary>
        /// <param name="request"></param>
        public HttpResponse(HttpRequest request) : this(request.Client, true)
        {
            this.ProtocolVersion = request.ProtocolVersion;
            this.Protocols = request.Protocols;
            this.KeepAlive = request.KeepAlive;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpResponse()
        {
            this.m_canRead = false;
            this.m_canWrite = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => this.m_canRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite => this.m_canWrite;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override ITcpClientBase Client => this.m_client;

        /// <summary>
        /// 是否分块
        /// </summary>
        public bool IsChunk { get; set; }

        /// <summary>
        /// 是否代理权限验证。
        /// </summary>
        public bool IsProxyAuthenticationRequired
        {
            get
            {
                return this.StatusCode == 407;
            }
        }

        /// <summary>
        /// 是否重定向
        /// </summary>
        public bool IsRedirect
        {
            get
            {
                return this.StatusCode == 301 || this.StatusCode == 302;
            }
        }

        /// <summary>
        /// 是否已经响应数据。
        /// </summary>
        public bool Responsed { get; private set; }

        /// <summary>
        /// 状态码，默认200
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// 状态消息，默认Success
        /// </summary>
        public string StatusMessage { get; set; } = "Success";

        /// <summary>
        /// 构建数据并回应。
        /// <para>该方法仅在具有Client实例时有效。</para>
        /// </summary>
        public void Answer()
        {
            if (this.Responsed)
            {
                return;
            }
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                if (this.m_client.CanSend)
                {
                    this.m_client.DefaultSend(byteBlock);
                }
                this.Responsed = true;
            }
        }

        /// <summary>
        ///  构建响应数据。
        /// <para>当数据较大时，不建议这样操作，可直接<see cref="WriteContent(byte[], int, int)"/></para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="responsed"></param>
        public void Build(ByteBlock byteBlock, bool responsed = true)
        {
            if (this.Responsed)
            {
                throw new Exception("该对象已被响应。");
            }

            this.BuildHeader(byteBlock);
            this.BuildContent(byteBlock);
            this.Responsed = responsed;
        }

        /// <summary>
        /// 构建数据为字节数组。
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using (var byteBlock = new ByteBlock())
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 当传输模式是Chunk时，用于结束传输。
        /// </summary>
        public void Complete()
        {
            this.m_canWrite = false;
            if (this.IsChunk)
            {
                using (var byteBlock = new ByteBlock())
                {
                    byteBlock.Write(Encoding.UTF8.GetBytes($"{0:X}\r\n"));
                    byteBlock.Write(Encoding.UTF8.GetBytes("\r\n"));
                    this.m_client.DefaultSend(byteBlock);
                    this.Responsed = true;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="content"></param>
        public override void SetContent(byte[] content)
        {
            this.m_content = content;
            this.ContentLength = content.Length;
            this.ContentComplated = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool TryGetContent(out byte[] content)
        {
            if (!this.ContentComplated.HasValue)
            {
                if (!this.IsChunk && this.ContentLength == 0)
                {
                    this.m_content = new byte[0];
                    content = this.m_content;
                    return true;
                }

                try
                {
                    using (var block1 = new ByteBlock(1024 * 1024))
                    {
                        using (var block2 = new ByteBlock())
                        {
                            var buffer = block2.Buffer;
                            while (true)
                            {
                                var r = this.Read(buffer, 0, buffer.Length);
                                if (r == 0)
                                {
                                    break;
                                }
                                block1.Write(buffer, 0, r);
                            }
                            this.ContentComplated = true;
                            this.m_content = block1.ToArray();
                            content = this.m_content;
                            return true;
                        }
                    }
                }
                catch
                {
                    this.ContentComplated = false;
                    content = null;
                    return false;
                }
                finally
                {
                    this.m_canRead = false;
                }
            }
            else if (this.ContentComplated == true)
            {
                content = this.m_content;
                return true;
            }
            else
            {
                content = null;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void WriteContent(byte[] buffer, int offset, int count)
        {
            if (this.Responsed)
            {
                throw new Exception("该对象已被响应。");
            }

            if (!this.CanWrite)
            {
                throw new NotSupportedException("该对象不支持持续写入内容。");
            }

            if (!this.m_sentHeader)
            {
                using (var byteBlock = new ByteBlock())
                {
                    this.BuildHeader(byteBlock);
                    this.m_client.DefaultSend(byteBlock);
                }
                this.m_sentHeader = true;
            }
            if (this.IsChunk)
            {
                using (var byteBlock = new ByteBlock(count + 1024))
                {
                    byteBlock.Write(Encoding.UTF8.GetBytes($"{count:X}\r\n"));
                    byteBlock.Write(buffer, offset, count);
                    byteBlock.Write(Encoding.UTF8.GetBytes("\r\n"));
                    this.m_client.DefaultSend(byteBlock);
                    this.m_sentLength += count;
                }
            }
            else
            {
                if (this.m_sentLength + count <= this.ContentLength)
                {
                    this.m_client.DefaultSend(buffer, offset, count);
                    this.m_sentLength += count;
                    if (this.m_sentLength == this.ContentLength)
                    {
                        this.m_canWrite = false;
                        this.Responsed = true;
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
            this.m_client = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        protected override void LoadHeaderProterties()
        {
            var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
            if (first.Length > 0)
            {
                var ps = first[0].Split('/');
                if (ps.Length == 2)
                {
                    this.Protocols = new Protocol(ps[0]);
                    this.ProtocolVersion = ps[1];
                }
            }
            if (first.Length > 1)
            {
                _ = int.TryParse(first[1], out var code);
                this.StatusCode = code;
            }
            var msg = string.Empty;
            for (var i = 2; i < first.Length; i++)
            {
                msg += first[i] + " ";
            }
            this.StatusMessage = msg;

            var transferEncoding = this.Headers.Get(HttpHeaders.TransferEncoding);
            if ("chunked".Equals(transferEncoding, StringComparison.OrdinalIgnoreCase))
            {
                this.IsChunk = true;
            }
        }

        private void BuildContent(ByteBlock byteBlock)
        {
            if (this.ContentLength > 0)
            {
                byteBlock.Write(this.m_content);
            }
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        private void BuildHeader(ByteBlock byteBlock)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"HTTP/{this.ProtocolVersion} {this.StatusCode} {this.StatusMessage}\r\n");

            if (this.IsChunk)
            {
                this.Headers.Add(HttpHeaders.TransferEncoding, "chunked");
            }
            foreach (var headerkey in this.Headers.Keys)
            {
                stringBuilder.Append($"{headerkey}: ");
                stringBuilder.Append(this.Headers[headerkey] + "\r\n");
            }

            stringBuilder.Append("\r\n");
            byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }
    }
}