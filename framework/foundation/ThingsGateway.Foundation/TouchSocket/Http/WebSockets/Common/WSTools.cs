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

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WSTools
    /// </summary>
    public static class WSTools
    {
        /// <summary>
        /// 应答。
        /// </summary>
        public const string acceptMask = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// 构建数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="dataFrame"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool Build(ByteBlock byteBlock, WSDataFrame dataFrame, byte[] buffer, int offset, int length)
        {
            int payloadLength;

            byte[] extLen;

            if (length < 126)
            {
                payloadLength = length;
                extLen = new byte[0];
            }
            else if (length < 65536)
            {
                payloadLength = 126;
                extLen = TouchSocketBitConverter.BigEndian.GetBytes((ushort)length);
            }
            else
            {
                payloadLength = 127;
                extLen = TouchSocketBitConverter.BigEndian.GetBytes((ulong)length);
            }

            var header = dataFrame.FIN ? 1 : 0;
            header = (header << 1) + (dataFrame.RSV1 ? 1 : 0);
            header = (header << 1) + (dataFrame.RSV2 ? 1 : 0);
            header = (header << 1) + (dataFrame.RSV3 ? 1 : 0);
            header = (header << 4) + (ushort)dataFrame.Opcode;

            header = dataFrame.Mask ? (header << 1) + 1 : (header << 1) + 0;

            header = (header << 7) + payloadLength;

            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes((ushort)header));

            if (payloadLength > 125)
            {
                byteBlock.Write(extLen, 0, extLen.Length);
            }

            if (dataFrame.Mask)
            {
                byteBlock.Write(dataFrame.MaskingKey, 0, 4);
            }

            if (payloadLength > 0)
            {
                if (dataFrame.Mask)
                {
                    if (byteBlock.Capacity < byteBlock.Pos + length)
                    {
                        byteBlock.SetCapacity(byteBlock.Pos + length, true);
                    }
                    WSTools.DoMask(byteBlock.Buffer, byteBlock.Pos, buffer, offset, length, dataFrame.MaskingKey);
                    byteBlock.SetLength(byteBlock.Pos + length);
                }
                else
                {
                    byteBlock.Write(buffer, offset, length);
                }
            }
            return true;
        }

        /// <summary>
        /// 计算Base64值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CalculateBase64Key(string str)
        {
            return (str + acceptMask).ToSha1(Encoding.UTF8).ToBase64();
        }

        /// <summary>
        /// 获取Base64随即字符串。
        /// </summary>
        /// <returns></returns>
        public static string CreateBase64Key()
        {
            var src = new byte[16];
            new Random().NextBytes(src);
            return Convert.ToBase64String(src);
        }

        /// <summary>
        /// 掩码运算
        /// </summary>
        /// <param name="storeBuf"></param>
        /// <param name="sOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="masks"></param>
        public static void DoMask(byte[] storeBuf, int sOffset, byte[] buffer, int offset, int length, byte[] masks)
        {
            for (var i = 0; i < length; i++)
            {
                storeBuf[sOffset + i] = (byte)(buffer[offset + i] ^ masks[i % 4]);
            }
        }

        /// <summary>
        /// 获取WS的请求头
        /// </summary>
        /// <param name="host"></param>
        /// <param name="url"></param>
        /// <param name="version"></param>
        /// <param name="base64Key"></param>
        /// <returns></returns>
        public static HttpRequest GetWSRequest(string host, string url, string version, out string base64Key)
        {
            var request = new HttpRequest();
            request.SetUrl(url);
            request.Headers.Add(HttpHeaders.Host, host);
            request.Headers.Add(HttpHeaders.Connection, "upgrade");
            request.Headers.Add(HttpHeaders.Upgrade, "websocket");
            request.Headers.Add("Sec-WebSocket-Version", $"{version}");
            base64Key = CreateBase64Key();
            request.Headers.Add("Sec-WebSocket-Key", base64Key);
            request.AsGet();
            return request;
        }

        /// <summary>
        /// 获取响应
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool TryGetResponse(HttpRequest request, HttpResponse response)
        {
            var upgrade = request.Headers.Get(HttpHeaders.Upgrade);
            if (string.IsNullOrEmpty(upgrade))
            {
                return false;
            }
            var connection = request.Headers.Get(HttpHeaders.Connection);
            if (string.IsNullOrEmpty(connection))
            {
                return false;
            }
            var secWebSocketKey = request.Headers.Get("sec-websocket-key");
            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }

            response.StatusCode = 101;
            response.StatusMessage = "switching protocols";
            response.Headers.Add(HttpHeaders.Connection, "upgrade");
            response.Headers.Add(HttpHeaders.Upgrade, "websocket");
            response.Headers.Add("sec-websocket-accept", CalculateBase64Key(secWebSocketKey));
            return true;
        }
    }
}