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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http基础头部
    /// </summary>
    public abstract class HttpBase : BlockReader, IRequestInfo
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public static readonly string ServerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly byte[] m_rnrnCode = Encoding.UTF8.GetBytes("\r\n\r\n");

        private readonly InternalHttpHeader m_headers;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpBase()
        {
            this.ReadTimeout = 1000 * 30;
            this.m_headers = new InternalHttpHeader();
        }

        /// <summary>
        /// 能否写入。
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// 客户端
        /// </summary>
        public abstract ITcpClientBase Client { get; }

        /// <summary>
        /// 内容填充完成
        /// </summary>
        public bool? ContentComplated { get; protected set; } = null;

        /// <summary>
        /// 内容长度
        /// </summary>
        public long ContentLength
        {
            get
            {
                var contentLength = this.m_headers.Get(HttpHeaders.ContentLength);
                if (contentLength.IsNullOrEmpty())
                {
                    return 0;
                }
                if (long.TryParse(contentLength, out var value))
                {
                    return value;
                }
                return 0;
            }
            set
            {
                this.m_headers.Add(HttpHeaders.ContentLength, value.ToString());
            }
        }

        /// <summary>
        /// 保持连接。
        /// <para>
        /// 一般的，当是http1.1时，如果没有显式的Connection: close，即返回true。当是http1.0时，如果没有显式的Connection: Keep-Alive，即返回false。
        /// </para>
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                var keepalive = this.Headers.Get(HttpHeaders.Connection);
                if (this.ProtocolVersion == "1.0")
                {
                    if (keepalive.IsNullOrEmpty())
                    {
                        return false;
                    }
                    else
                    {
                        return keepalive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    if (keepalive.IsNullOrEmpty())
                    {
                        return true;
                    }
                    else
                    {
                        return keepalive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            set
            {
                if (this.ProtocolVersion == "1.0")
                {
                    if (value)
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "Keep-Alive");
                    }
                    else
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "close");
                    }
                }
                else
                {
                    if (!value)
                    {
                        this.m_headers.Add(HttpHeaders.Connection, "close");
                    }
                }
            }
        }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return this.m_headers.Get(HttpHeaders.ContentType);
            }
            set
            {
                this.m_headers.Add(HttpHeaders.ContentType, value);
            }
        }

        /// <summary>
        /// 传递标识
        /// </summary>
        public object Flag { get; set; }

        /// <summary>
        /// 请求头集合
        /// </summary>
        public IHttpHeader Headers
        {
            get
            {
                return this.m_headers;
            }
        }

        /// <summary>
        /// 协议名称，默认HTTP
        /// </summary>
        public Protocol Protocols { get; set; } = Protocol.Http;

        /// <summary>
        /// HTTP协议版本，默认1.1
        /// </summary>
        public string ProtocolVersion { get; set; } = "1.1";

        /// <summary>
        /// 请求行
        /// </summary>
        public string RequestLine { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool ParsingHeader(ByteBlock byteBlock, int length)
        {
            var index = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, length, m_rnrnCode);
            if (index > 0)
            {
                var headerLength = index - byteBlock.Pos;
                this.ReadHeaders(byteBlock.Buffer, byteBlock.Pos, headerLength);
                byteBlock.Pos += headerLength;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 从Request中持续读取数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, count);
        }

        /// <summary>
        /// 从内存中读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void ReadHeaders(byte[] buffer, int offset, int length)
        {
            var data = Encoding.UTF8.GetString(buffer, offset, length);
            var rows = Regex.Split(data, "\r\n");

            //Request URL & Method & Version
            this.RequestLine = rows[0];

            //Request Headers
            this.GetRequestHeaders(rows);
            this.LoadHeaderProterties();
        }

        /// <summary>
        /// 设置一次性内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public abstract void SetContent(byte[] content);

        /// <summary>
        /// 获取一次性内容。
        /// </summary>
        /// <returns></returns>
        public abstract bool TryGetContent(out byte[] content);

        /// <summary>
        /// 持续写入内容。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public abstract void WriteContent(byte[] buffer, int offset, int count);

        internal bool InternalInput(byte[] buffer, int offset, int length)
        {
            return this.Input(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.DisposedValue && this.CanRead)
            {
                this.TryGetContent(out _);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        protected abstract void LoadHeaderProterties();

        private void GetRequestHeaders(string[] rows)
        {
            if (rows == null || rows.Length <= 0)
            {
                return;
            }

            foreach (var item in rows)
            {
                var kv = item.SplitFirst(':');
                if (kv.Length == 2)
                {
                    var key = kv[0].ToLower();
                    this.Headers.Add(key, kv[1]);
                }
            }
        }
    }
}