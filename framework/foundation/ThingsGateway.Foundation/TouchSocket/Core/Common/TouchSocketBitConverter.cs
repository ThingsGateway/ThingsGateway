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
using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public class TouchSocketBitConverter
    {
        /// <summary>
        /// 以大端
        /// </summary>
        public static TouchSocketBitConverter BigEndian;

        /// <summary>
        /// 以小端
        /// </summary>
        public static TouchSocketBitConverter LittleEndian;

        static TouchSocketBitConverter()
        {
            BigEndian = new TouchSocketBitConverter(EndianType.Big);
            LittleEndian = new TouchSocketBitConverter(EndianType.Little);
            DefaultEndianType = EndianType.Little;
        }

        /// <summary>
        /// 以默认小端，可通过<see cref="TouchSocketBitConverter.DefaultEndianType"/>重新指定默认端。
        /// </summary>
        public static TouchSocketBitConverter Default { get; private set; }

        private static EndianType m_defaultEndianType;

        /// <summary>
        /// 默认大小端切换。
        /// </summary>
        public static EndianType DefaultEndianType
        {
            get => m_defaultEndianType;
            set
            {
                m_defaultEndianType = value;
                switch (value)
                {
                    case EndianType.Little:
                        Default = LittleEndian;
                        break;

                    case EndianType.Big:
                        Default = BigEndian;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public TouchSocketBitConverter(EndianType endianType)
        {
            this.EndianType = endianType;
        }

        /// <summary>
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType { get; private set; }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSameOfSet()
        {
            return !(BitConverter.IsLittleEndian ^ (this.EndianType == EndianType.Little));
            //return true;
        }

        #region ushort

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的2字节转换为UInt16数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ushort ToUInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, 2);
                Array.Reverse(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
        }

        #endregion ushort

        #region ulong

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ulong ToUInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt64(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, 8);
                Array.Reverse(bytes);
                return BitConverter.ToUInt64(bytes, 0);
            }
        }

        #endregion ulong

        #region bool

        /// <summary>
        /// 转换为指定端1字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///  转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ToBoolean(byte[] buffer, int offset)
        {
            return BitConverter.ToBoolean(buffer, offset);
        }

        #endregion bool

        #region char

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(char value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Char数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char ToChar(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToChar(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToChar(bytes, 0);
            }
        }

        #endregion char

        #region short

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Short数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public short ToInt16(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt16(buffer, offset);
            }
            else
            {
                var bytes = new byte[2];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        #endregion short

        #region int

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的int数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ToInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt32(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        #endregion int

        #region long

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的long数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public long ToInt64(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToInt64(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToInt64(bytes, 0);
            }
        }

        #endregion long

        #region uint

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的Uint数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint ToUInt32(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToUInt32(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        #endregion uint

        #region float

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的float数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public float ToSingle(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToSingle(buffer, offset);
            }
            else
            {
                var bytes = new byte[4];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToSingle(bytes, 0);
            }
        }

        #endregion float

        #region long

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的double数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public double ToDouble(byte[] buffer, int offset)
        {
            if (this.IsSameOfSet())
            {
                return BitConverter.ToDouble(buffer, offset);
            }
            else
            {
                var bytes = new byte[8];
                Array.Copy(buffer, offset, bytes, 0, bytes.Length);
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
        }

        #endregion long

        #region decimal

        /// <summary>
        /// 转换为指定端16字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(decimal value)
        {
            var bytes = DecimalConver.ToBytes(value);
            if (!this.IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        ///  转换为指定端模式的<see cref="decimal"/>数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public decimal ToDecimal(byte[] buffer, int offset)
        {
            var bytes = new byte[16];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (this.IsSameOfSet())
            {
                return DecimalConver.FromBytes(bytes);
            }
            else
            {
                Array.Reverse(bytes);
                return DecimalConver.FromBytes(bytes);
            }
        }

        #endregion decimal
    }
}