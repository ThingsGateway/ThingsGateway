using Newtonsoft.Json;

using System.Text;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public class ThingsGatewayBitConverter : IThingsGatewayBitConverter
    {
        private readonly EndianType endianType;

        private DataFormat dataFormat;
        [JsonIgnore]
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public ThingsGatewayBitConverter(EndianType endianType)
        {
            this.endianType = endianType;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public ThingsGatewayBitConverter(EndianType endianType, DataFormat dataFormat)
        {
            this.endianType = endianType; this.dataFormat = dataFormat;
        }

        public DataFormat DataFormat
        {
            get
            {
                return dataFormat;
            }
            set
            {
                dataFormat = value;
            }
        }

        /// <summary>
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType => endianType;

        public bool IsStringReverseByteWord { get; set; }

        public virtual IThingsGatewayBitConverter CreateByDateFormat(DataFormat dataFormat)
        {
            ThingsGatewayBitConverter byteConverter = new ThingsGatewayBitConverter(EndianType, dataFormat)
            {
                IsStringReverseByteWord = IsStringReverseByteWord,
            };
            return byteConverter;
        }

        public byte[] GetBytes(bool value)
        {
            return GetBytes(new bool[1]
            {
                  value
            });
        }

        public byte[] GetBytes(bool[] values)
        {
            return values?.BoolArrayToByte();
        }

        public byte[] GetBytes(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        public byte[] GetBytes(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        public byte[] GetBytes(int value)
        {
            return ByteTransDataFormat4(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(uint value)
        {
            return ByteTransDataFormat4(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(long value)
        {
            return ByteTransDataFormat8(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(ulong value)
        {
            return ByteTransDataFormat8(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(float value)
        {
            return ByteTransDataFormat4(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(double value)
        {
            return ByteTransDataFormat8(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(string value)
        {
            if (value == null)
            {
                return null;
            }

            byte[] bytes = Encoding.GetBytes(value);
            return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
        }

        public byte[] GetBytes(string value, int length)
        {
            if (value == null)
            {
                return null;
            }

            byte[] bytes = Encoding.GetBytes(value);
            return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(length) : bytes.ArrayExpandToLength(length);
        }

        public byte[] GetBytes(string value, BcdFormat bcdFormat)
        {
            if (value == null)
            {
                return null;
            }

            byte[] bytes = DataHelper.GetBytesFromBCD(value, bcdFormat);
            return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
        }

        public byte[] GetBytes(string value, int length, BcdFormat bcdFormat)
        {
            if (value == null)
            {
                return null;
            }

            byte[] bytes = DataHelper.GetBytesFromBCD(value, bcdFormat);
            return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(length) : bytes.ArrayExpandToLength(length);
        }

        public bool IsSameOfSet()
        {
            return !(BitConverter.IsLittleEndian ^ (endianType == EndianType.Little));
        }

        public string ToBcdString(byte[] buffer, BcdFormat bcdFormat)
        {
            return ToBcdString(buffer, 0, buffer.Length, bcdFormat);
        }

        public string ToBcdString(byte[] buffer, int offset, int length, BcdFormat bcdFormat)
        {
            return IsStringReverseByteWord ? DataHelper.GetBCDValue(buffer.SelectMiddle<byte>(offset, length).BytesReverseByWord(), bcdFormat) : DataHelper.GetBCDValue(buffer.SelectMiddle<byte>(offset, length), bcdFormat);
        }

        public bool ToBoolean(byte[] buffer, int offset)
        {
            byte[] bytes = new byte[buffer.Length];
            Array.Copy(buffer, 0, bytes, 0, buffer.Length);
            //TODO:待验证
            //if (!IsSameOfSet())
            //{
            //    bytes = bytes.BytesReverseByWord();
            //}
            return bytes.GetBoolByIndex(offset);
        }

        public byte ToByte(byte[] buffer, int offset)
        {
            byte[] bytes = new byte[1];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            return bytes[0];
        }

        public byte[] ToByte(byte[] buffer, int offset, int length)
        {
            byte[] bytes = new byte[length];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
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
            byte[] bytes = ByteTransDataFormat8(buffer, offset);

            return BitConverter.ToDouble(bytes, 0);
        }

        public short ToInt16(byte[] buffer, int offset)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        public int ToInt32(byte[] buffer, int offset)
        {
            byte[] bytes = ByteTransDataFormat4(buffer, offset);

            return BitConverter.ToInt32(bytes, 0);
        }

        public long ToInt64(byte[] buffer, int offset)
        {
            byte[] bytes = ByteTransDataFormat8(buffer, offset);
            return BitConverter.ToInt64(bytes, 0);
        }

        public float ToSingle(byte[] buffer, int offset)
        {
            byte[] bytes = ByteTransDataFormat4(buffer, offset);

            return BitConverter.ToSingle(bytes, 0);
        }

        public string ToString(byte[] buffer)
        {
            return ToString(buffer, 0, buffer.Length);
        }

        public string ToString(byte[] buffer, int offset, int length)
        {
            byte[] numArray = buffer.SelectMiddle<byte>(offset, length);
            return IsStringReverseByteWord ?
                Encoding.GetString(numArray.BytesReverseByWord()).TrimEnd().Replace($"\0", "") :
                Encoding.GetString(numArray).TrimEnd().Replace($"\0", "");
            ;
        }

        public ushort ToUInt16(byte[] buffer, int offset)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, 2);
            if (!IsSameOfSet())
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt16(bytes, 0);
        }

        public uint ToUInt32(byte[] buffer, int offset)
        {
            byte[] bytes = ByteTransDataFormat4(buffer, offset);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong ToUInt64(byte[] buffer, int offset)
        {
            byte[] bytes = ByteTransDataFormat8(buffer, offset);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>反转多字节的数据信息</summary>
        /// <param name="value">数据字节</param>
        /// <param name="offset">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        protected byte[] ByteTransDataFormat4(byte[] value, int offset = 0)
        {
            byte[] numArray = new byte[4];
            switch (DataFormat)
            {
                case DataFormat.ABCD:
                    numArray[0] = value[offset + 3];
                    numArray[1] = value[offset + 2];
                    numArray[2] = value[offset + 1];
                    numArray[3] = value[offset];
                    break;

                case DataFormat.BADC:
                    numArray[0] = value[offset + 2];
                    numArray[1] = value[offset + 3];
                    numArray[2] = value[offset];
                    numArray[3] = value[offset + 1];
                    break;

                case DataFormat.CDAB:
                    numArray[0] = value[offset + 1];
                    numArray[1] = value[offset];
                    numArray[2] = value[offset + 3];
                    numArray[3] = value[offset + 2];
                    break;

                case DataFormat.DCBA:
                    numArray[0] = value[offset];
                    numArray[1] = value[offset + 1];
                    numArray[2] = value[offset + 2];
                    numArray[3] = value[offset + 3];
                    break;
            }
            return numArray;
        }

        /// <summary>反转多字节的数据信息</summary>
        /// <param name="value">数据字节</param>
        /// <param name="offset">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        protected byte[] ByteTransDataFormat8(byte[] value, int offset = 0)
        {
            byte[] numArray = new byte[8];
            switch (DataFormat)
            {
                case DataFormat.ABCD:
                    numArray[0] = value[offset + 7];
                    numArray[1] = value[offset + 6];
                    numArray[2] = value[offset + 5];
                    numArray[3] = value[offset + 4];
                    numArray[4] = value[offset + 3];
                    numArray[5] = value[offset + 2];
                    numArray[6] = value[offset + 1];
                    numArray[7] = value[offset];
                    break;

                case DataFormat.BADC:
                    numArray[0] = value[offset + 6];
                    numArray[1] = value[offset + 7];
                    numArray[2] = value[offset + 4];
                    numArray[3] = value[offset + 5];
                    numArray[4] = value[offset + 2];
                    numArray[5] = value[offset + 3];
                    numArray[6] = value[offset];
                    numArray[7] = value[offset + 1];
                    break;

                case DataFormat.CDAB:
                    numArray[0] = value[offset + 1];
                    numArray[1] = value[offset];
                    numArray[2] = value[offset + 3];
                    numArray[3] = value[offset + 2];
                    numArray[4] = value[offset + 5];
                    numArray[5] = value[offset + 4];
                    numArray[6] = value[offset + 7];
                    numArray[7] = value[offset + 6];
                    break;

                case DataFormat.DCBA:
                    numArray[0] = value[offset];
                    numArray[1] = value[offset + 1];
                    numArray[2] = value[offset + 2];
                    numArray[3] = value[offset + 3];
                    numArray[4] = value[offset + 4];
                    numArray[5] = value[offset + 5];
                    numArray[6] = value[offset + 6];
                    numArray[7] = value[offset + 7];
                    break;
            }
            return numArray;
        }
    }
}