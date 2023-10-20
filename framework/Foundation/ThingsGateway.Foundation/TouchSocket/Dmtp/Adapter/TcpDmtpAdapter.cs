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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThingsGateway.Foundation.Core;

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// TcpDmtpAdapter
    /// </summary>
    public class TcpDmtpAdapter : CustomFixedHeaderByteBlockDataHandlingAdapter<DmtpMessage>
    {
        private readonly SemaphoreSlim m_locker = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => true;

        /// <inheritdoc/>
        public override bool CanSplicingSend => true;

        /// <inheritdoc/>
        public override int HeaderLength => 6;

        /// <summary>
        /// 最大拼接
        /// </summary>
        public const int MaxSplicing = 1024 * 64;

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
        protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
        {
            if (!(requestInfo is DmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
            }
            if (message.BodyByteBlock != null && message.BodyByteBlock.Length > this.MaxPackageSize)
            {
                throw new Exception("发送的BodyLength={requestInfo.BodyLength},大于设定的MaxPackageSize={this.MaxPackageSize}");
            }
            using (var byteBlock = new ByteBlock(message.GetLength()))
            {
                message.Build(byteBlock);
                await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }


        /// <inheritdoc/>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            if (!(requestInfo is DmtpMessage message))
            {
                throw new Exception($"无法将{nameof(requestInfo)}转换为{nameof(DmtpMessage)}");
            }
            if (message.BodyByteBlock != null && message.BodyByteBlock.Length > this.MaxPackageSize)
            {
                throw new Exception("发送的BodyLength={requestInfo.BodyLength},大于设定的MaxPackageSize={this.MaxPackageSize}");
            }
            using (var byteBlock = new ByteBlock(message.GetLength()))
            {
                message.Build(byteBlock);
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
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
            if (length > this.MaxPackageSize)
            {
                try
                {

                    await this.m_locker.WaitAsync();
                    foreach (var item in transferBytes)
                    {
                        await this.GoSendAsync(item.Array, item.Offset, item.Count);
                    }
                }
                finally
                {
                    this.m_locker.Release();
                }
            }
            else
            {
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
                }
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

            if (length > this.MaxPackageSize)
            {
                try
                {

                    this.m_locker.Wait();
                    foreach (var item in transferBytes)
                    {
                        this.GoSend(item.Array, item.Offset, item.Count);
                    }
                }
                finally
                {
                    this.m_locker.Release();
                }
            }
            else
            {
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }
    }
}