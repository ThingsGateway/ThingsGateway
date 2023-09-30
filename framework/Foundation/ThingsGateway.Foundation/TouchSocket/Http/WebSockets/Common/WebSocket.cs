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

//using System.Threading.Tasks;
//using ThingsGateway.Foundation;

//namespace ThingsGateway.Foundation.Http.WebSockets
//{
//    public class WebSocket : IWebSocket
//    {
//        private readonly IHttpClientBase m_client;
//        private readonly bool m_isServer;

//        public WebSocket(IHttpClientBase client, bool isServer)
//        {
//            this.m_client = client;
//            this.m_isServer = isServer;
//        }

//        public bool IsHandshaked => this.m_client.GetHandshaked();

//        public void Close(string msg)
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Close }.AppendText(msg))
//            {
//                this.Send(frame);
//            }
//        }

//        public void Ping()
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Ping })
//            {
//                this.Send(frame);
//            }
//        }

//        public void Pong()
//        {
//            using (var frame = new WSDataFrame() { FIN = true, Opcode = WSDataType.Pong })
//            {
//                this.Send(frame);
//            }
//        }

//        public void Send(WSDataFrame dataFrame)
//        {
//            using (var byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024))
//            {
//                if (this.m_isServer)
//                {
//                    dataFrame.BuildResponse(byteBlock);
//                }
//                else
//                {
//                    dataFrame.BuildRequest(byteBlock);
//                }

//                this.m_client.DefaultSend(byteBlock.Buffer, 0, byteBlock.Len);
//            }
//        }

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        public Task SendAsync(WSDataFrame dataFrame)
//        {
//            using (var byteBlock = new ByteBlock(dataFrame.PayloadLength + 1024))
//            {
//                if (this.m_isServer)
//                {
//                    dataFrame.BuildResponse(byteBlock);
//                }
//                else
//                {
//                    dataFrame.BuildRequest(byteBlock);
//                }

//                return this.m_client.DefaultSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
//            }
//        }
//    }
//}