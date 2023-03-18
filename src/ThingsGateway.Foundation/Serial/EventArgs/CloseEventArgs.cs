namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 断开连接事件参数
    /// </summary>
    public class CloseEventArgs : MsgEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="mes"></param>
        public CloseEventArgs(bool manual, string mes) : base(mes)
        {
            Manual = manual;
        }

        /// <summary>
        /// 是否为主动行为。
        /// </summary>
        public bool Manual { get; private set; }
    }

}