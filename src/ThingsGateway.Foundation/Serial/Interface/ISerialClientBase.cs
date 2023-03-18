using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial
{
    public interface ISerialClientBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender
    {
        /// <summary>
        /// 缓存池大小
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
        /// </summary>
        bool CanSetDataHandlingAdapter { get; }

 
        TouchSocketConfig Config { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        SerialDataHandlingAdapter SerialDataHandlingAdapter { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        CloseEventHandler<ISerialClientBase> Closed { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="ThingsGateway.Foundation.Serial.SerialEventArgs.IsPermitOperation"/>终止断开行为。
        /// </para>
        /// </summary>
        CloseEventHandler<ISerialClientBase> Closing { get; set; }
        /// <summary>
        /// 表示是否为客户端。
        /// </summary>
        bool IsClient { get; }
        /// <summary>
        /// 主通信器
        /// </summary>
        SerialPort MainSerialPort { get; }

        /// <summary>
        /// 接收模式
        /// </summary>
        public ReceiveType ReceiveType { get; }

        /// <summary>
        /// 中断终端
        /// </summary>
        void Close();

        /// <summary>
        /// 中断终端，传递中断消息
        /// </summary>
        /// <param name="msg"></param>
        void Close(string msg);

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        void SetDataHandlingAdapter(SerialDataHandlingAdapter adapter);
    }
}