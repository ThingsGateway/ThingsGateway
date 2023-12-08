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

using System.Diagnostics;
using System.Net;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Udp数据处理适配器测试
    /// </summary>
    public class UdpDataAdapterTester : IDisposable
    {
        private readonly IntelligentDataQueue<QueueDataBytes> asyncBytes;
        private UdpDataHandlingAdapter adapter;
        private int count;
        private bool dispose;
        private int expectedCount;
        private Action<ByteBlock, IRequestInfo> receivedCallBack;
        private Stopwatch stopwatch;
        private int timeout;

        private UdpDataAdapterTester(int multiThread)
        {
            this.asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            for (var i = 0; i < multiThread; i++)
            {
                Task.Run(this.BeginSend);
            }
        }

        /// <summary>
        /// 获取测试器
        /// </summary>
        /// <param name="adapter">待测试适配器</param>
        /// <param name="multiThread">并发多线程数量</param>
        /// <param name="receivedCallBack">收到数据回调</param>
        /// <returns></returns>
        public static UdpDataAdapterTester CreateTester(UdpDataHandlingAdapter adapter, int multiThread, Action<ByteBlock, IRequestInfo> receivedCallBack = default)
        {
            var tester = new UdpDataAdapterTester(multiThread);
            tester.adapter = adapter;
            adapter.SendCallBack = tester.SendCallback;
            adapter.ReceivedCallBack = tester.OnReceived;
            tester.receivedCallBack = receivedCallBack;
            return tester;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.dispose = true;
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
            this.count = 0;
            this.expectedCount = expectedCount;
            this.timeout = timeout;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            Task.Run(() =>
            {
                for (var i = 0; i < testCount; i++)
                {
                    this.adapter.SendInput(null, buffer, offset, length);
                }
            });
            if (SpinWait.SpinUntil(() => this.count == this.expectedCount, this.timeout))
            {
                this.stopwatch.Stop();
                return this.stopwatch.Elapsed;
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
            while (!this.dispose)
            {
                if (this.tryGet(out var byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            this.adapter.ReceivedInput(null, block);
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

        private void OnReceived(EndPoint endPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.receivedCallBack?.Invoke(byteBlock, requestInfo);
            Interlocked.Increment(ref this.count);
        }

        private void SendCallback(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            var asyncByte = new QueueDataBytes(new byte[length], 0, length);
            Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
            this.asyncBytes.Enqueue(asyncByte);
        }

        private bool tryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();

            while (this.asyncBytes.TryDequeue(out var asyncByte))
            {
                var block = new ByteBlock(asyncByte.Length);
                block.Write(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                byteBlocks.Add(block);
            }
            return byteBlocks.Count > 0;
        }
    }
}