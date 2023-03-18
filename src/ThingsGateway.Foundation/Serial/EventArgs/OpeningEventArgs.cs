using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 串口连接事件。
    /// </summary>
    public class OpeningEventArgs : OperationEventArgs
    {
        private readonly SerialPort serialPort;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket"></param>
        public OpeningEventArgs(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        /// <summary>
        /// 新初始化的通信器
        /// </summary>
        public SerialPort SerialPort => serialPort;
    }
}