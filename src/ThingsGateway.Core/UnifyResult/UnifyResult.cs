namespace ThingsGateway.Core
{
    /// <summary>
    /// 全局返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnifyResult<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public object Extras { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public object Msg { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
    }
}