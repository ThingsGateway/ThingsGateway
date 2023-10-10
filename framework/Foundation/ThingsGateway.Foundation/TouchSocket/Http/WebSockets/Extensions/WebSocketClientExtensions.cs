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

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WSClientExtensions
    /// </summary>
    public static class WebSocketClientExtensions
    {
        #region DependencyProperty

        private static readonly DependencyProperty<bool> IsContProperty =
            DependencyProperty<bool>.Register("IsCont", false);

        private static void SetIsCont(this IHttpClientBase client, bool value)
        {
            client.SetValue(IsContProperty, value);
        }

        private static bool GetIsCont(this IHttpClientBase client)
        {
            return client.GetValue(IsContProperty);
        }

        internal static readonly DependencyProperty<InternalWebSocket> WebSocketProperty =
           DependencyProperty<InternalWebSocket>.Register("WebSocket", null);

        /// <summary>
        /// 获取显式WebSocket终端。
        /// <para>
        /// 
        /// </para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="allowReceive"></param>
        /// <returns></returns>
        public static IWebSocket GetWebSocket(this IHttpClientBase client, bool allowReceive = true)
        {
            var websocket = client.GetValue(WebSocketProperty);
            if (websocket == null)
            {
                websocket = new InternalWebSocket(client, allowReceive);
                client.SetValue(WebSocketProperty, websocket);
            }
            return websocket;
        }

        /// <summary>
        /// 清除显式WebSocket终端。
        /// </summary>
        /// <param name="client"></param>
        public static void ClearWebSocket(this IHttpClientBase client)
        {
            client.RemoveValue(WebSocketProperty);
        }
        #endregion
        /// <summary>
        /// 发送Close报文。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static void CloseWithWS(this IHttpClientBase client, string msg)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
            {
                SendWithWS(client, frame);
            }
        }

        /// <summary>
        /// 发送Close报文。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Task CloseWithWSAsync(this IHttpClientBase client, string msg)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
            {
                return SendWithWSAsync(client, frame);
            }
        }

        /// <summary>
        /// WebSocket是否已经完成握手
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool GetHandshaked(this IHttpClientBase client)
        {
            return client.GetValue(WebSocketFeature.HandshakedProperty);
        }

        /// <summary>
        /// 获取WebSocket版本号。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetWebSocketVersion(this IHttpClientBase client)
        {
            return client.GetValue(WebSocketFeature.WebSocketVersionProperty);
        }

        /// <summary>
        /// 发送Ping报文。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static void PingWS(this IHttpClientBase client)
        {
            SendWithWS(client, new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
        }

        /// <summary>
        /// 发送Ping报文。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static Task PingWSAsync(this IHttpClientBase client)
        {
            return SendWithWSAsync(client, new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping });
        }

        /// <summary>
        /// 发送Pong报文。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static void PongWS(this IHttpClientBase client)
        {
            SendWithWS(client, new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
        }

        /// <summary>
        /// 发送Pong报文。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static Task PongWSAsync(this IHttpClientBase client)
        {
            return SendWithWSAsync(client, new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong });
        }

        /// <summary>
        /// 设置WebSocket版本号。
        /// </summary>
        public static void SetWebSocketVersion(this IHttpClientBase client, string value)
        {
            client.SetValue(WebSocketFeature.WebSocketVersionProperty, value);
        }

        #region 同步发送


        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static void SendWithWS(this IHttpClientBase client, byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                if (offset == 0)
                {
                    frame.PayloadData = new ByteBlock(buffer, length);
                }
                else
                {
                    frame.AppendBinary(buffer, offset, length);
                }
                SendWithWS(client, frame, endOfMessage);
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock"></param>
        /// <param name="endOfMessage"></param>
        public static void SendWithWS(this IHttpClientBase client, ByteBlock byteBlock, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                frame.PayloadData = byteBlock;
                SendWithWS(client, frame, endOfMessage);
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        public static void SendWithWS(this IHttpClientBase client, byte[] buffer, bool endOfMessage = true)
        {
            SendWithWS(client, buffer, 0, buffer.Length, endOfMessage);
        }

        /// <summary>
        /// 采用WebSocket协议，发送文本数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        public static void SendWithWS(this IHttpClientBase client, string text, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Text }.AppendText(text))
            {
                SendWithWS(client, frame, endOfMessage);
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送WS数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        public static void SendWithWS(this IHttpClientBase client, WSDataFrame dataFrame, bool endOfMessage = true)
        {
            var isCont = client.GetIsCont();

            WSDataType dataType;
            if (isCont)
            {
                dataType = WSDataType.Cont;
                if (endOfMessage)
                {
                    client.SetIsCont(false);
                }
            }
            else
            {
                dataType = dataFrame.Opcode;
                if (!endOfMessage)
                {
                    client.SetIsCont(true);
                }
            }
            dataFrame.Opcode = dataType;
            using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
            {
                if (client.IsClient)
                {
                    dataFrame.BuildRequest(byteBlock);
                }
                else
                {
                    dataFrame.BuildResponse(byteBlock);
                }
                client.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        #endregion 同步发送

        #region 异步发送

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static Task SendWithWSAsync(this IHttpClientBase client, byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = endOfMessage, Opcode = WSDataType.Binary })
            {
                if (offset == 0)
                {
                    frame.PayloadData = new ByteBlock(buffer, length);
                }
                else
                {
                    frame.AppendBinary(buffer, offset, length);
                }
                return SendWithWSAsync(client, frame, endOfMessage);
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送二进制流数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        public static Task SendWithWSAsync(this IHttpClientBase client, byte[] buffer, bool endOfMessage = true)
        {
            return SendWithWSAsync(client, buffer, 0, buffer.Length, endOfMessage);
        }

        /// <summary>
        /// 采用WebSocket协议，发送文本数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        public static Task SendWithWSAsync(this IHttpClientBase client, string text, bool endOfMessage = true)
        {
            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Text }.AppendText(text))
            {
                return SendWithWSAsync(client, frame, endOfMessage);
            }
        }

        /// <summary>
        /// 采用WebSocket协议，发送WS数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        public static Task SendWithWSAsync(this IHttpClientBase client, WSDataFrame dataFrame, bool endOfMessage = true)
        {
            var isCont = client.GetIsCont();

            WSDataType dataType;
            if (isCont)
            {
                dataType = WSDataType.Cont;
                if (endOfMessage)
                {
                    client.SetIsCont(false);
                }
            }
            else
            {
                dataType = dataFrame.Opcode;
                if (!endOfMessage)
                {
                    client.SetIsCont(true);
                }
            }
            dataFrame.Opcode = dataType;
            using (var byteBlock = new ByteBlock(dataFrame.GetTotalSize()))
            {
                if (client.IsClient)
                {
                    dataFrame.BuildRequest(byteBlock);
                }
                else
                {
                    dataFrame.BuildResponse(byteBlock);
                }
                return client.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        #endregion 异步发送
    }
}