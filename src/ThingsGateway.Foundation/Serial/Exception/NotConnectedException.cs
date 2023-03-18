namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 未连接异常
    /// </summary>
    [Serializable]
    public class NotOpenedException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NotOpenedException()
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public NotOpenedException(string message) : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public NotOpenedException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected NotOpenedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}