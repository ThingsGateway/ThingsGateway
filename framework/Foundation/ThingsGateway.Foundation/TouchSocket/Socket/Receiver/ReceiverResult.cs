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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// ReceiverResult
    /// </summary>
    public readonly struct ReceiverResult : IDisposable
    {
        private readonly Action m_disAction;

        /// <summary>
        /// SocketReceiveResult
        /// </summary>
        /// <param name="disAction"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        public ReceiverResult(Action disAction, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.m_disAction = disAction;
            this.ByteBlock = byteBlock;
            this.RequestInfo = requestInfo;
        }

        /// <summary>
        /// 字节块
        /// </summary>
        public ByteBlock ByteBlock { get; }

        /// <summary>
        /// 数据对象
        /// </summary>
        public IRequestInfo RequestInfo { get; }

        /// <summary>
        /// 连接已关闭
        /// </summary>
        public bool IsClosed => this.ByteBlock == null && this.RequestInfo == null;

        /// <inheritdoc/>
        public void Dispose()
        {
            m_disAction?.Invoke();
        }
    }
}
