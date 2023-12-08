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

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc调用设置
    /// </summary>
    public class InvokeOption : IInvokeOption
    {
        static InvokeOption()
        {
            OnlySend = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.OnlySend
            };

            WaitSend = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.WaitSend
            };

            WaitInvoke = new InvokeOption(timeout: 5000)
            {
                FeedbackType = FeedbackType.WaitInvoke
            };
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public InvokeOption()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timeout"></param>
        public InvokeOption(int timeout)
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000ms
        /// </summary>
        public static InvokeOption OnlySend { get; }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000ms
        /// </summary>
        public static InvokeOption WaitInvoke { get; }

        /// <summary>
        /// 默认设置。
        /// Timeout=5000 ms
        /// </summary>
        public static InvokeOption WaitSend { get; }

        /// <summary>
        /// 调用反馈
        /// </summary>
        public FeedbackType FeedbackType { get; set; } = FeedbackType.WaitInvoke;

        /// <summary>
        /// 调用超时，
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// 可以取消的调用令箭
        /// </summary>
        public CancellationToken Token { get; set; } = CancellationToken.None;
    }
}