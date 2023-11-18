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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 阻塞式单项读取流。
    /// </summary>

    public abstract partial class BlockReadStream : Stream, IWrite
    {
        private readonly AutoResetEvent m_inputEvent;
        private readonly AutoResetEvent m_readEvent;
        private byte[] m_buffer;
        private bool m_dis;
        private volatile int m_length;
        private volatile int m_offset;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BlockReadStream()
        {
            this.m_readEvent = new AutoResetEvent(false);
            this.m_inputEvent = new AutoResetEvent(false);
            this.ReadTimeout = 5000;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public virtual void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 还剩余的未读取的长度
        /// </summary>
        public int CanReadLen
        {
            get
            {
                return this.m_dis ? 0 : this.m_length - this.m_offset;
            }
        }

        /// <summary>
        /// 不可使用
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// 不可使用
        /// </summary>
        public override long Length => throw new NotImplementedException();

        /// <summary>
        ///  不可使用
        /// </summary>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int ReadTimeout { get; set; }

        /// <summary>
        /// 阻塞读取。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new Exception("该流不允许读取。");
            }
            int r;
            if (this.m_length > 0)
            {
                if (this.m_length > count)
                {
                    //按count读取
                    Array.Copy(this.m_buffer, this.m_offset, buffer, offset, count);
                    this.m_length -= count;
                    this.m_offset += count;
                    r = count;
                }
                else
                {
                    //会读完本次
                    Array.Copy(this.m_buffer, this.m_offset, buffer, offset, this.m_length);
                    r = this.m_length;
                    this.Reset();
                }
            }
            else
            {
                //无数据，须等待
                if (this.m_readEvent.WaitOne(this.ReadTimeout))
                {
                    if (this.m_length == 0)
                    {
                        this.Reset();
                        r = 0;
                    }
                    else if (this.m_length > count)
                    {
                        //按count读取
                        Array.Copy(this.m_buffer, this.m_offset, buffer, offset, count);
                        this.m_length -= count;
                        this.m_offset += count;
                        r = count;
                    }
                    else
                    {
                        //会读完本次
                        Array.Copy(this.m_buffer, this.m_offset, buffer, offset, this.m_length);
                        r = this.m_length;
                        this.Reset();
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            return r;
        }

        /// <summary>
        /// 不可使用
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 不可使用
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.m_dis)
            {
                return;
            }
            this.m_dis = true;
            this.Reset();
            this.m_readEvent.SafeDispose();
            this.m_inputEvent.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 传输输入.
        /// 必须以length为0结束。读取端会超时。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected bool Input(byte[] buffer, int offset, int length)
        {
            this.m_inputEvent.Reset();
            this.m_buffer = buffer;
            this.m_offset = offset;
            this.m_length = length;
            this.m_readEvent.Set();
            return this.m_inputEvent.WaitOne(this.ReadTimeout);
        }

        private void Reset()
        {
            this.m_buffer = null;
            this.m_offset = 0;
            this.m_length = 0;
            this.m_readEvent.Reset();
            this.m_inputEvent.Set();
        }
    }
}