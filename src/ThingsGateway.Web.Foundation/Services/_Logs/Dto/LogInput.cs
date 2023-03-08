using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{

    public class RuntimeLogPageInput : BasePageInput
    {
        /// <summary>
        /// 日志源
        /// </summary>
        [Description("日志源")]
        public string Source { get; set; }
        /// <summary>
        /// 日志等级
        /// </summary>
        [Description("日志等级")]
        public string Level { get; set; }

    }
    public class RpcLogPageInput : BasePageInput
    {
        /// <summary>
        /// 操作源
        /// </summary>
        [Description("操作源")]
        public string Source { get; set; }
        /// <summary>
        /// 操作源
        /// </summary>
        [Description("操作对象")]
        public string Object { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        [Description("方法")]
        public string Method { get; set; }
    }
}