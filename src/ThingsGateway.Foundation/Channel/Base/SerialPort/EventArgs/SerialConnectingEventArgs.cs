
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.IO.Ports;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// Connecting
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate Task SerialConnectingEventHandler<TClient>(TClient client, SerialConnectingEventArgs e);

    /// <summary>
    /// 客户端连接事件。
    /// </summary>
    public class SerialConnectingEventArgs : MsgPermitEventArgs
    {
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        /// <param name="serialPort"></param>
        public SerialConnectingEventArgs(SerialPort serialPort)
        {
            this.SerialPort = serialPort;
            this.IsPermitOperation = true;
        }

        /// <summary>
        /// 新初始化的通信器
        /// </summary>
        public SerialPort SerialPort { get; }
    }
}