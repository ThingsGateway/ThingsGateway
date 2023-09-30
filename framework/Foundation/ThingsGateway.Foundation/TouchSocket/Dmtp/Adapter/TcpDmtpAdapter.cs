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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// TcpDmtpAdapter
    /// </summary>
    public class TcpDmtpAdapter : CustomFixedHeaderByteBlockDataHandlingAdapter<DmtpMessage>
    {
        private SpinLock m_locker = new SpinLock();

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => true;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public override int HeaderLength => 6;

        /// <inheritdoc/>
        protected override DmtpMessage GetInstance()
        {
            return new DmtpMessage();
        }

        /// <inheritdoc/>
        protected override void OnReceivedSuccess(DmtpMessage request)
        {
            request.SafeDispose();
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            if (!(requestInfo is DmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
            }
            using (var byteBlock = new ByteBlock(message.GetLength()))
            {
                message.Build(byteBlock);
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <inheritdoc/>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            if (transferBytes.Count == 0)
            {
                return;
            }

            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }

            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            var lockTaken = false;
            try
            {
                this.m_locker.Enter(ref lockTaken);
                foreach (var item in transferBytes)
                {
                    this.GoSend(item.Array, item.Offset, item.Count);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    this.m_locker.Exit(false);
                }
            }
        }
    }
}