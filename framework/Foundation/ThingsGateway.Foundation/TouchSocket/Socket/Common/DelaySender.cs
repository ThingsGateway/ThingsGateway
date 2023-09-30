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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 延迟发送器
    /// </summary>
    public sealed class DelaySender : DisposableObject
    {
        private readonly IntelligentDataQueue<QueueDataBytes> m_queueDatas;
        private readonly Action<byte[], int, int> m_action;
        private AsyncAutoResetEvent m_resetEvent = new AsyncAutoResetEvent(false);
        /// <summary>
        /// 延迟发送器
        /// </summary>
        /// <param name="delaySenderOption"></param>
        /// <param name="action"></param>
        public DelaySender(DelaySenderOption delaySenderOption, Action<byte[], int, int> action)
        {
            this.DelayLength = delaySenderOption.DelayLength;
            this.m_queueDatas = new IntelligentDataQueue<QueueDataBytes>(delaySenderOption.QueueLength);
            Task.Run(this.BeginSend);
            this.m_action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// 延迟包最大尺寸。
        /// </summary>
        public int DelayLength { get; private set; }


        /// <summary>
        /// 发送
        /// </summary>
        public void Send(in QueueDataBytes dataBytes)
        {
            this.m_queueDatas.Enqueue(dataBytes);
            this.m_resetEvent.Set();
        }

        private async Task BeginSend()
        {
            while (!this.DisposedValue)
            {
                var buffer = BytePool.Default.Rent(this.DelayLength);
                try
                {
                    if (this.TryGet(buffer, out var asyncByte))
                    {
                        this.m_action.Invoke(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                    }
                    else
                    {
                        await m_resetEvent.WaitOneAsync();
                    }
                }
                catch
                {

                }
                finally
                {
                    BytePool.Default.Return(buffer);
                }
            }
        }


        private bool TryGet(byte[] buffer, out QueueDataBytes asyncByteDe)
        {
            var len = 0;
            var surLen = buffer.Length;
            while (true)
            {
                if (this.m_queueDatas.TryPeek(out var asyncB))
                {
                    if (surLen > asyncB.Length)
                    {
                        if (this.m_queueDatas.TryDequeue(out var asyncByte))
                        {
                            Array.Copy(asyncByte.Buffer, asyncByte.Offset, buffer, len, asyncByte.Length);
                            len += asyncByte.Length;
                            surLen -= asyncByte.Length;
                        }
                    }
                    else if (asyncB.Length > buffer.Length)
                    {
                        if (len > 0)
                        {
                            break;
                        }
                        else
                        {
                            asyncByteDe = asyncB;
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (len > 0)
                    {
                        break;
                    }
                    else
                    {
                        asyncByteDe = default;
                        return false;
                    }
                }
            }
            asyncByteDe = new QueueDataBytes(buffer, 0, len);
            return true;
        }
    }
}