namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 通讯基类
    /// </summary>
    public abstract class BaseSerial : DependencyObject, ISerial
    {
        /// <summary>
        /// 通讯基类
        /// </summary>
        public BaseSerial()
        {
            SyncRoot = new object();
        }

        private int m_bufferLength;

        /// <summary>
        /// 数据交互缓存池限制，min=1024 byte
        /// </summary>
        public virtual int BufferLength
        {
            get => m_bufferLength;
            set
            {
                if (value < 1024)
                {
                    value = 1024 * 10;
                }
                m_bufferLength = value;
            }
        }

        /// <summary>
        /// 同步根。
        /// </summary>
        protected object SyncRoot;

        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger { get; set; }
    }
}