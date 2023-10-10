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
using System.Text;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 字节块流
    /// </summary>
    [DebuggerDisplay("Len={Len},Pos={Pos},Capacity={Capacity}")]
    public sealed partial class ByteBlock : Stream, IWrite
    {
        private BytePool m_bytePool;
        private bool m_canReturn;
        private int m_dis = 1;
        private long m_length;
        private long m_position;

        /// <summary>
        ///  字节块流
        /// </summary>
        /// <param name="byteSize"></param>
        public ByteBlock(int byteSize = 1024 * 64)
        {
            this.m_bytePool = BytePool.Default;
            this.Buffer = BytePool.Default.Rent(byteSize);
            this.Using = true;
            this.m_canReturn = true;
        }

        /// <summary>
        /// 字节块流
        /// </summary>
        /// <param name="byteSize"></param>
        /// <param name="bytePool"></param>
        public ByteBlock(int byteSize, BytePool bytePool)
        {
            this.m_bytePool = bytePool;
            this.Buffer = bytePool.Rent(byteSize);
            this.Using = true;
            this.m_canReturn = true;
        }

        /// <summary>
        /// 实例化一个已知内存的对象。且该内存不会被回收。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public ByteBlock(byte[] bytes, int length)
        {
            this.Buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
            this.m_length = length;
            this.Using = true;
        }

        /// <summary>
        /// 实例化一个已知内存的对象。且该内存不会被回收。
        /// </summary>
        /// <param name="bytes"></param>
        public ByteBlock(byte[] bytes) : this(bytes, bytes.Length)
        {
        }

        /// <summary>
        /// 字节实例
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLen"/>>0时为True。
        /// </summary>
        public override bool CanRead => this.Using && this.CanReadLen > 0;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public int CanReadLen => this.Len - this.Pos;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public long CanReadLength => this.m_length - this.m_position;

        /// <summary>
        /// 支持查找
        /// </summary>
        public override bool CanSeek => this.Using;

        /// <summary>
        /// 可写入
        /// </summary>
        public override bool CanWrite => this.Using;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => this.Buffer.Length;

        /// <summary>
        /// 空闲长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public int FreeLength => this.Capacity - this.Pos;

        /// <summary>
        /// 表示持续性持有，为True时，Dispose将调用无效。
        /// </summary>
        public bool Holding { get; private set; }

        /// <summary>
        /// Int真实长度
        /// </summary>
        public int Len => (int)this.m_length;

        /// <summary>
        /// 真实长度
        /// </summary>
        public override long Length => this.m_length;

        /// <summary>
        /// int型流位置
        /// </summary>
        public int Pos
        {
            get => (int)this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 流位置
        /// </summary>
        public override long Position
        {
            get => this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 使用状态
        /// </summary>
        public bool Using { get; private set; }

        /// <summary>
        /// 直接完全释放，游离该对象，然后等待GC
        /// </summary>
        public void AbsoluteDispose()
        {
            if (Interlocked.Decrement(ref this.m_dis) == 0)
            {
                this.Dis();
            }
        }

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Clear()
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            Array.Clear(this.Buffer, 0, this.Buffer.Length);
        }

        /// <summary>
        /// 无实际效果
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(in ByteBlock byteBlock, int length)
        {
            var r = this.Read(byteBlock.Buffer, byteBlock.Pos, length);
            byteBlock.SetLength(r);
            return r;
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public override int Read(byte[] buffer, int offset, int length)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            var len = this.m_length - this.m_position > length ? length : this.CanReadLen;
            Array.Copy(this.Buffer, this.m_position, buffer, offset, len);
            this.m_position += len;
            return len;
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return this.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(out byte[] buffer, int length)
        {
            buffer = new byte[length];
            return this.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public override int ReadByte()
        {
            var value = this.Buffer[this.m_position];
            this.m_position++;
            return value;
        }

        /// <summary>
        /// 将内存块初始化到刚申请的状态。
        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Reset()
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.m_position = 0;
            this.m_length = 0;
        }

        /// <summary>
        /// 设置流位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_position = offset;
                    break;

                case SeekOrigin.Current:
                    this.m_position += offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.m_length + offset;
                    break;
            }
            return this.m_position;
        }

        /// <summary>
        /// 移动游标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public void Seek(int position)
        {
            this.Position = position;
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <returns></returns>
        public void SeekToEnd()
        {
            this.Position = this.Length;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <returns></returns>
        public void SeekToStart()
        {
            this.Position = 0;
        }

        /// <summary>
        /// 重新设置容量
        /// </summary>
        /// <param name="size">新尺寸</param>
        /// <param name="retainedData">是否保留原数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetCapacity(int size, bool retainedData = false)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.m_canReturn)
            {
                this.m_bytePool.Return(this.Buffer);
            }

            byte[] bytes;
            if (this.m_bytePool == null)
            {
                bytes = new byte[size];
                this.m_canReturn = false;
            }
            else
            {
                bytes = this.m_bytePool.Rent(size);
                this.m_canReturn = true;
            }

            if (retainedData)
            {
                Array.Copy(this.Buffer, 0, bytes, 0, this.Buffer.Length);
            }
            this.Buffer = bytes;
        }

        /// <summary>
        /// 设置持续持有属性，当为True时，调用Dispose会失效，表示该对象将长期持有，直至设置为False。
        /// 当为False时，会自动调用Dispose。
        /// </summary>
        /// <param name="holding"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetHolding(bool holding)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.Holding = holding;
            if (!holding)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void SetLength(long value)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (value > this.Buffer.Length)
            {
                throw new Exception("设置值超出容量");
            }
            this.m_length = value;
        }

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset, int length)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            var buffer = new byte[length];
            Array.Copy(this.Buffer, offset, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// 转换为有效内存
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0, this.Len);
        }

        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] ToArray(int offset)
        {
            return this.ToArray(offset, this.Len - offset);
        }

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString(0, this.Len);
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public string ToString(int offset, int length)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            else
            {
                return Encoding.UTF8.GetString(this.Buffer, offset, length);
            }
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public string ToString(int offset)
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            else
            {
                return Encoding.UTF8.GetString(this.Buffer, offset, this.Len - offset);
            }
        }

        /// <summary>
        /// 将<see cref="ByteBlock"/>中的有效数据写入到当前
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Write(ByteBlock byteBlock)
        {
            this.Write(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (this.m_position >= int.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"{nameof(this.Position)}不能大于{int.MaxValue}的值。");
            }
            if (this.Buffer.Length - this.m_position < count)
            {
                var need = this.Buffer.Length + count - ((int)(this.Buffer.Length - this.m_position));
                long lend = this.Buffer.Length;
                while (need > lend)
                {
                    lend *= 2;
                }

                if (lend > int.MaxValue)
                {
                    lend = Math.Min(need + 1024 * 1024 * 100, int.MaxValue);
                }

                this.SetCapacity((int)lend, true);
            }
            Array.Copy(buffer, offset, this.Buffer, this.m_position, count);
            this.m_position += count;
            this.m_length = Math.Max(this.m_position, this.m_length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.Holding)
            {
                return;
            }

            if (disposing)
            {
                if (Interlocked.Decrement(ref this.m_dis) == 0)
                {
                    if (this.m_canReturn)
                    {
                        this.m_bytePool.Return(this.Buffer);
                    }
                    this.Dis();
                }
            }

            base.Dispose(disposing);
        }

        private void Dis()
        {
            this.Holding = false;
            this.m_position = 0;
            this.m_length = 0;
            this.Buffer = null;
            this.m_bytePool = null;
            this.m_canReturn = false;
        }

        #region BytesPackage

        /// <summary>
        /// 从当前流位置读取一个独立的<see cref="byte"/>数组包
        /// </summary>
        public byte[] ReadBytesPackage()
        {
            var status = (byte)this.ReadByte();
            if (status == 0)
            {
                return null;
            }
            var length = this.ReadInt32();
            var data = new byte[length];
            Array.Copy(this.Buffer, this.Pos, data, 0, length);
            this.Pos += length;
            return data;
        }

        /// <summary>
        /// 尝试获取数据包信息，方便从Buffer操作数据
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool TryReadBytesPackageInfo(out int pos, out int len)
        {
            var status = (byte)this.ReadByte();
            if (status == 0)
            {
                pos = 0;
                len = 0;
                return false;
            }
            len = this.ReadInt32();
            pos = this.Pos;
            return true;
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包，值可以为null。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteBytesPackage(byte[] value, int offset, int length)
        {
            if (value == null)
            {
                this.Write((byte)0);
            }
            else
            {
                this.Write((byte)1);
                this.Write(length);
                this.Write(value, offset, length);
            }
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包。值可以为null。
        /// </summary>
        /// <param name="value"></param>
        public void WriteBytesPackage(byte[] value)
        {
            if (value == null)
            {
                this.WriteBytesPackage(value, 0, 0);
            }
            else
            {
                this.WriteBytesPackage(value, 0, value.Length);
            }
        }

        #endregion BytesPackage

        #region ByteBlock

        /// <summary>
        /// 从当前流位置读取一个<see cref="ByteBlock"/>值。
        /// <para>
        /// 注意，使用该方式读取到的内存块，会脱离释放周期，所以最好在使用完成后自行释放。
        /// </para>
        /// </summary>
        public ByteBlock ReadByteBlock()
        {
            if (this.ReadIsNull())
            {
                return default;
            }

            if (!this.TryReadBytesPackageInfo(out var pos, out var len))
            {
                return default;
            }
            var byteBlock = new ByteBlock(len);
            byteBlock.Write(this.Buffer, pos, len);
            this.m_position += len;
            return byteBlock;
        }

        /// <summary>
        /// 写入<see cref="ByteBlock"/>值
        /// </summary>
        public void WriteByteBlock(ByteBlock byteBlock)
        {
            if (byteBlock is null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteNotNull();
                this.WriteBytesPackage(byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        #endregion ByteBlock

        #region Int32

        /// <summary>
        /// 从当前流位置读取一个<see cref="int"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public int ReadInt32(bool? bigEndian = null)
        {
            int value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToInt32(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToInt32(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToInt32(this.Buffer, this.Pos); break;
            }
            this.m_position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="int"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(int value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Int32

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public short ReadInt16(bool? bigEndian = null)
        {
            short value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToInt16(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToInt16(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToInt16(this.Buffer, this.Pos); break;
            }
            this.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="short"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(short value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Int16

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public long ReadInt64(bool? bigEndian = null)
        {
            long value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToInt64(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToInt64(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToInt64(this.Buffer, this.Pos); break;
            }
            this.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="long"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(long value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Int64

        #region Boolean

        /// <summary>
        /// 从当前流位置读取一个<see cref="bool"/>值
        /// </summary>
        public bool ReadBoolean()
        {
            var value = TouchSocketBitConverter.Default.ToBoolean(this.Buffer, this.Pos);
            this.Pos += 1;
            return value;
        }

        /// <summary>
        /// 写入<see cref="bool"/>值
        /// </summary>
        /// <param name="value"></param>
        public void Write(bool value)
        {
            this.Write(TouchSocketBitConverter.Default.GetBytes(value));
        }

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 写入<see cref="byte"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Write(byte value)
        {
            this.Write(new byte[] { value }, 0, 1);
        }

        #endregion Byte

        #region String

        /// <summary>
        /// 从当前流位置读取一个<see cref="string"/>值
        /// </summary>
        public string ReadString()
        {
            var len = this.ReadInt32();
            if (len < 0)
            {
                return null;
            }
            else
            {
                var str = Encoding.UTF8.GetString(this.Buffer, this.Pos, len);
                this.Pos += len;
                return str;
            }
        }

        /// <summary>
        /// 写入<see cref="string"/>值。值可以为null，或者空。
        /// <para>注意：该操作不具备通用性，读取时必须使用ReadString。或者得先做出判断，由默认端序的int32值标识，具体如下：</para>
        /// <list type="bullet">
        /// <item>小于0，表示字符串为null</item>
        /// <item>等于0，表示字符串为""</item>
        /// <item>大于0，表示字符串在utf-8编码下的字节长度。</item>
        /// </list>
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value)
        {
            if (value == null)
            {
                this.Write(-1);
            }
            else
            {
                var buffer = Encoding.UTF8.GetBytes(value);
                this.Write(buffer.Length);
                this.Write(buffer);
            }
        }

        /// <summary>
        /// 写入<see cref="string"/>值。值必须为有效值。可通用解析。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void WriteString(string value, Encoding encoding = null)
        {
            this.Write((encoding ?? Encoding.UTF8).GetBytes(value));
        }

        #endregion String

        #region Char

        /// <summary>
        /// 从当前流位置读取一个<see cref="char"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public char ReadChar(bool? bigEndian = null)
        {
            char value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToChar(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToChar(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToChar(this.Buffer, this.Pos); break;
            }
            this.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="char"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(char value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Char

        #region Double

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public double ReadDouble(bool? bigEndian = null)
        {
            double value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToDouble(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToDouble(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToDouble(this.Buffer, this.Pos); break;
            }
            this.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="double"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(double value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Double

        #region Float

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public float ReadFloat(bool? bigEndian = null)
        {
            float value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToSingle(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToSingle(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToSingle(this.Buffer, this.Pos); break;
            }
            this.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="float"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(float value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion Float

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public ushort ReadUInt16(bool? bigEndian = null)
        {
            ushort value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToUInt16(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToUInt16(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToUInt16(this.Buffer, this.Pos); break;
            }
            this.Pos += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ushort"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(ushort value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public uint ReadUInt32(bool? bigEndian = null)
        {
            uint value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToUInt32(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToUInt32(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToUInt32(this.Buffer, this.Pos); break;
            }
            this.Pos += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="uint"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(uint value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public ulong ReadUInt64(bool? bigEndian = null)
        {
            ulong value;
            switch (bigEndian)
            {
                case true: value = TouchSocketBitConverter.BigEndian.ToUInt64(this.Buffer, this.Pos); break;
                case false: value = TouchSocketBitConverter.LittleEndian.ToUInt64(this.Buffer, this.Pos); break;
                default: value = TouchSocketBitConverter.Default.ToUInt64(this.Buffer, this.Pos); break;
            }
            this.Pos += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ulong"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bigEndian">是否为指定大端编码。允许true（大端），false（小端），null（默认端序）三种赋值。默认为null。</param>
        public void Write(ulong value, bool? bigEndian = null)
        {
            switch (bigEndian)
            {
                case true:
                    this.Write(TouchSocketBitConverter.BigEndian.GetBytes(value));
                    break;

                case false:
                    this.Write(TouchSocketBitConverter.LittleEndian.GetBytes(value));
                    break;

                default:
                    this.Write(TouchSocketBitConverter.Default.GetBytes(value));
                    break;
            }
        }

        #endregion UInt64

        #region Null

        /// <summary>
        /// 从当前流位置读取一个标识值，判断是否为null。
        /// </summary>
        public bool ReadIsNull()
        {
            var status = this.ReadByte();
            return status == 0 ? true : status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。");
        }

        /// <summary>
        /// 判断该值是否为Null，然后写入标识值
        /// </summary>
        public void WriteIsNull<T>(T t) where T : class
        {
            if (t == null)
            {
                this.WriteNull();
            }
            else
            {
                this.WriteNotNull();
            }
        }

        /// <summary>
        /// 判断该值是否为Null，然后写入标识值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public void WriteIsNull<T>(T? t) where T : struct
        {
            if (t.HasValue)
            {
                this.WriteNotNull();
            }
            else
            {
                this.WriteNull();
            }
        }


        /// <summary>
        /// 写入一个标识非Null值
        /// </summary>
        public void WriteNotNull()
        {
            this.Write((byte)1);
        }

        /// <summary>
        /// 写入一个标识Null值
        /// </summary>
        public void WriteNull()
        {
            this.Write((byte)0);
        }

        #endregion Null

        #region DateTime

        /// <summary>
        /// 从当前流位置读取一个<see cref="DateTime"/>值
        /// </summary>
        public DateTime ReadDateTime()
        {
            var value = TouchSocketBitConverter.Default.ToInt64(this.Buffer, this.Pos);
            this.Pos += 8;
            return DateTime.FromBinary(value);
        }

        /// <summary>
        /// 写入<see cref="DateTime"/>值
        /// </summary>
        /// <param name="value"></param>
        public void Write(DateTime value)
        {
            this.Write(TouchSocketBitConverter.Default.GetBytes(value.ToBinary()));
        }

        #endregion DateTime

        #region TimeSpan

        /// <summary>
        /// 从当前流位置读取一个<see cref="TimeSpan"/>值
        /// </summary>
        public TimeSpan ReadTimeSpan()
        {
            var value = TouchSocketBitConverter.Default.ToInt64(this.Buffer, this.Pos);
            this.Pos += 8;
            return TimeSpan.FromTicks(value);
        }

        /// <summary>
        /// 写入<see cref="TimeSpan"/>值
        /// </summary>
        /// <param name="value"></param>
        public void Write(TimeSpan value)
        {
            this.Write(TouchSocketBitConverter.Default.GetBytes(value.Ticks));
        }

        #endregion TimeSpan

        #region Object

        /// <summary>
        ///  从当前流位置读取一个泛型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        public T ReadObject<T>(SerializationType serializationType = SerializationType.FastBinary)
        {
            var length = this.ReadInt32();

            if (length == 0)
            {
                return default;
            }

            T obj;

            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        obj = SerializeConvert.FastBinaryDeserialize<T>(this.Buffer, this.Pos);
                    }
                    break;

                case SerializationType.Json:
                    {
                        var jsonString = Encoding.UTF8.GetString(this.Buffer, this.Pos, length);
                        obj = SerializeConvert.JsonDeserializeFromString<T>(jsonString);
                    }
                    break;

                case SerializationType.Xml:
                    {
                        var jsonString = Encoding.UTF8.GetString(this.Buffer, this.Pos, length);
                        obj = SerializeConvert.XmlDeserializeFromString<T>(jsonString);
                    }
                    break;

                case SerializationType.SystemBinary:
                    {
                        obj = SerializeConvert.BinaryDeserialize<T>(this.Buffer, this.Pos, length);
                    }
                    break;

                default:
                    throw new Exception("未定义的序列化类型");
            }

            this.Pos += length;
            return obj;
        }

        /// <summary>
        /// 写入<see cref="object"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="serializationType"></param>
        public void WriteObject(object value, SerializationType serializationType = SerializationType.FastBinary)
        {
            if (value == null)
            {
                this.Write(0);
            }
            else
            {
                byte[] data;
                switch (serializationType)
                {
                    case SerializationType.FastBinary:
                        {
                            data = SerializeConvert.FastBinarySerialize(value);
                        }
                        break;

                    case SerializationType.Json:
                        {
                            data = SerializeConvert.JsonSerializeToBytes(value);
                        }
                        break;

                    case SerializationType.Xml:
                        {
                            data = Encoding.UTF8.GetBytes(SerializeConvert.XmlSerializeToString(value));
                        }
                        break;

                    case SerializationType.SystemBinary:
                        {
                            data = SerializeConvert.BinarySerialize(value);
                        }
                        break;

                    default:
                        throw new Exception("未定义的序列化类型");
                }
                this.Write(data.Length);
                this.Write(data);
            }
        }

        #endregion Object

        #region Package

        /// <summary>
        /// 读取一个指定类型的包
        /// </summary>
        /// <typeparam name="TPackage"></typeparam>
        /// <returns></returns>
        public TPackage ReadPackage<TPackage>() where TPackage : class, IPackage, new()
        {
            if (this.ReadIsNull())
            {
                return default;
            }
            else
            {
                var package = new TPackage();
                package.Unpackage(this);
                return package;
            }
        }

        /// <summary>
        /// 以包进行写入。允许null值。
        /// 读取时调用<see cref="ReadPackage"/>，解包。或者先判断<see cref="ReadIsNull"/>，然后自行解包。
        /// </summary>
        /// <typeparam name="TPackage"></typeparam>
        /// <param name="package"></param>
        /// <returns></returns>
        public void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage
        {
            this.WriteIsNull(package);
            package?.Package(this);
        }

        #endregion Package

        #region Implicit

        /// <summary>
        /// 将<see cref="ByteBlock"/>转为<see cref="ArraySegment{T}"/>。
        /// <para>注意：实际上是产生了一个新的内存。</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public static implicit operator ArraySegment<byte>(ByteBlock byteBlock)
        {
            return new ArraySegment<byte>(byteBlock);
        }

        /// <summary>
        /// 将<see cref="ByteBlock"/>转为<see cref="byte"/>数组。
        /// <para>注意：实际上是产生了一个新的内存。</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public static implicit operator byte[](ByteBlock byteBlock)
        {
            if (byteBlock.m_dis == 0)
            {
                throw new ObjectDisposedException(nameof(byteBlock));
            }
            return byteBlock.ToArray();
        }

        #endregion Implicit
    }
}