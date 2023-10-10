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
using System.Diagnostics;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    ///单线程状况的流式数据处理适配器测试
    /// </summary>
    public class SingleStreamDataAdapterTester : IDisposable
    {
        private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;
        private readonly Thread m_sendThread;
        private SingleStreamDataHandlingAdapter m_adapter;
        private int m_bufferLength;
        private int m_count;
        private bool m_dispose;
        private int m_expectedCount;
        private Action<ByteBlock, IRequestInfo> m_receivedCallBack;
        private Stopwatch m_stopwatch;
        private int m_timeout;

        /// <summary>
        /// Tcp数据处理适配器测试
        /// </summary>
        protected SingleStreamDataAdapterTester()
        {
            this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            this.m_sendThread = new Thread(this.BeginSend);
            this.m_sendThread.IsBackground = true;
            this.m_sendThread.Name = "DataAdapterTesterThread";
            this.m_sendThread.Start();
        }

        /// <summary>
        /// 获取测试器
        /// </summary>
        /// <param name="adapter">待测试适配器</param>
        /// <param name="receivedCallBack">收到数据回调</param>
        /// <param name="bufferLength">缓存数据长度</param>
        /// <returns></returns>
        public static SingleStreamDataAdapterTester CreateTester(SingleStreamDataHandlingAdapter adapter, int bufferLength = 1024, Action<ByteBlock, IRequestInfo> receivedCallBack = default)
        {
            var tester = new SingleStreamDataAdapterTester();
            tester.m_adapter = adapter;
            tester.m_bufferLength = bufferLength;
            adapter.SendCallBack = tester.SendCallback;
            adapter.ReceivedCallBack = tester.OnReceived;
            tester.m_receivedCallBack = receivedCallBack;
            return tester;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.m_dispose = true;
        }

        /// <summary>
        /// 模拟测试运行发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        public TimeSpan Run(byte[] buffer, int offset, int length, int testCount, int expectedCount, int timeout)
        {
            this.m_count = 0;
            this.m_expectedCount = expectedCount;
            this.m_timeout = timeout;
            this.m_stopwatch = new Stopwatch();
            this.m_stopwatch.Start();
            Task.Run(() =>
            {
                for (var i = 0; i < testCount; i++)
                {
                    this.m_adapter.SendInput(buffer, offset, length);
                }
            });

            if (SpinWait.SpinUntil(() => this.m_count == this.m_expectedCount, this.m_timeout))
            {
                this.m_stopwatch.Stop();
                return this.m_stopwatch.Elapsed;
            }
            throw new TimeoutException();
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        public TimeSpan Run(byte[] buffer, int testCount, int expectedCount, int timeout)
        {
            return this.Run(buffer, 0, buffer.Length, testCount, expectedCount, timeout);
        }

        private void BeginSend()
        {
            while (!this.m_dispose)
            {
                if (this.tryGet(out var byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            this.m_adapter.ReceivedInput(block);
                        }
                        finally
                        {
                            block.Dispose();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void OnReceived(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.m_count++;
            this.m_receivedCallBack?.Invoke(byteBlock, requestInfo);
        }

        private void SendCallback(byte[] buffer, int offset, int length)
        {
            var asyncByte = new QueueDataBytes(new byte[length], 0, length);
            Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
            this.m_asyncBytes.Enqueue(asyncByte);
        }

        private bool tryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();
            ByteBlock block = null;
            while (true)
            {
                if (this.m_asyncBytes.TryDequeue(out var asyncByte))
                {
                    if (block == null)
                    {
                        block = BytePool.Default.GetByteBlock(this.m_bufferLength);
                        byteBlocks.Add(block);
                    }
                    var surLen = this.m_bufferLength - block.Pos;
                    if (surLen < asyncByte.Length)//不能完成写入
                    {
                        block.Write(asyncByte.Buffer, asyncByte.Offset, surLen);
                        var offset = surLen;
                        while (offset < asyncByte.Length)
                        {
                            block = this.Write(asyncByte, ref offset);
                            byteBlocks.Add(block);
                        }

                        if (byteBlocks.Count > 10)
                        {
                            break;
                        }
                    }
                    else//本次能完成写入
                    {
                        block.Write(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                        if (byteBlocks.Count > 10)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (byteBlocks.Count > 0)
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private ByteBlock Write(QueueDataBytes transferByte, ref int offset)
        {
            var block = BytePool.Default.GetByteBlock(this.m_bufferLength);
            var len = Math.Min(transferByte.Length - offset, this.m_bufferLength);
            block.Write(transferByte.Buffer, offset, len);
            offset += len;

            return block;
        }
    }
}