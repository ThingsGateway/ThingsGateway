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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading.Tasks;
//using ThingsGateway.Foundation;

//namespace ThingsGateway.Foundation.Http.WebSockets
//{
//    /// <summary>
//    /// IWebSocket
//    /// </summary>
//    public interface IWebSocket
//    {
//        /// <summary>
//        /// 表示当前WebSocket是否已经完成连接。
//        /// </summary>
//        bool IsHandshaked { get; }

//        /// <summary>
//        /// 发送Close报文。
//        /// </summary>
//        /// <param name="msg"></param>
//        void Close(string msg);

//        /// <summary>
//        /// 发送Ping报文。
//        /// </summary>
//        void Ping();

//        /// <summary>
//        /// 发送Pong报文。
//        /// </summary>
//        void Pong();

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        void Send(WSDataFrame dataFrame);

//        /// <summary>
//        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
//        /// </summary>
//        /// <param name="dataFrame"></param>
//        /// <returns></returns>
//        Task SendAsync(WSDataFrame dataFrame);
//    }
//}