
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Newtonsoft.Json;

using System.Runtime.CompilerServices;
using System.Text;

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation;

/// <summary>
/// 将基数据类型转换为指定端的一个字节数组，
/// 或将一个字节数组转换为指定端基数据类型。
/// </summary>
public partial class ThingsGatewayBitConverter : IThingsGatewayBitConverter
{
    /// <inheritdoc/>
#if NET6_0_OR_GREATER

    [System.Text.Json.Serialization.JsonConverter(typeof(EncodingConverter))]
    [JsonConverter(typeof(NewtonsoftEncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;
#else

    [JsonConverter(typeof(NewtonsoftEncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;

#endif

    /// <inheritdoc/>
    public virtual BcdFormatEnum? BcdFormat { get; set; }

    /// <inheritdoc/>
    public virtual int? StringLength { get; set; }

    /// <inheritdoc/>
    public virtual int? ArrayLength { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ThingsGatewayBitConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="endianType"></param>
    public ThingsGatewayBitConverter(EndianType endianType)
    {
        this.EndianType = endianType;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ThingsGatewayBitConverter(EndianType endianType, DataFormatEnum dataFormat)
    {
        this.EndianType = endianType; this.DataFormat = dataFormat;
    }

    /// <inheritdoc/>
    public virtual DataFormatEnum? DataFormat { get; set; }

    /// <inheritdoc/>
    public virtual EndianType EndianType { get; init; }

    /// <inheritdoc/>
    public virtual bool IsStringReverseByteWord { get; set; }


    #region new

    #region Tool

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat2_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
            }
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat4_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);

                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        Unsafe.WriteUnaligned(p, d);
                        Unsafe.WriteUnaligned(p + 1, c);
                        Unsafe.WriteUnaligned(p + 2, b);
                        Unsafe.WriteUnaligned(p + 3, a);
                        break;

                    case DataFormatEnum.BADC:
                        Unsafe.WriteUnaligned(p, c);
                        Unsafe.WriteUnaligned(p + 1, d);
                        Unsafe.WriteUnaligned(p + 2, a);
                        Unsafe.WriteUnaligned(p + 3, b);
                        break;

                    case DataFormatEnum.CDAB:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        break;
                    case DataFormatEnum.DCBA:
                        return;
                }
            }
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat8_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);
                var e = Unsafe.ReadUnaligned<byte>(p + 4);
                var f = Unsafe.ReadUnaligned<byte>(p + 5);
                var g = Unsafe.ReadUnaligned<byte>(p + 6);
                var h = Unsafe.ReadUnaligned<byte>(p + 7);

                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        Unsafe.WriteUnaligned(p, h);
                        Unsafe.WriteUnaligned(p + 1, g);
                        Unsafe.WriteUnaligned(p + 2, f);
                        Unsafe.WriteUnaligned(p + 3, e);
                        Unsafe.WriteUnaligned(p + 4, d);
                        Unsafe.WriteUnaligned(p + 5, c);
                        Unsafe.WriteUnaligned(p + 6, b);
                        Unsafe.WriteUnaligned(p + 7, a);
                        break;

                    case DataFormatEnum.BADC:
                        Unsafe.WriteUnaligned(p, g);
                        Unsafe.WriteUnaligned(p + 1, h);
                        Unsafe.WriteUnaligned(p + 2, e);
                        Unsafe.WriteUnaligned(p + 3, f);
                        Unsafe.WriteUnaligned(p + 4, c);
                        Unsafe.WriteUnaligned(p + 5, d);
                        Unsafe.WriteUnaligned(p + 6, a);
                        Unsafe.WriteUnaligned(p + 7, b);
                        break;

                    case DataFormatEnum.CDAB:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        Unsafe.WriteUnaligned(p + 4, f);
                        Unsafe.WriteUnaligned(p + 5, e);
                        Unsafe.WriteUnaligned(p + 6, h);
                        Unsafe.WriteUnaligned(p + 7, g);
                        break;

                    case DataFormatEnum.DCBA:
                        break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat16_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {

                switch (this.DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        var span = new Span<byte>(p, 16);
                        span.Reverse();
                        break;
                    case DataFormatEnum.DCBA:
                        return;
                    default:
                    case DataFormatEnum.BADC:
                    case DataFormatEnum.CDAB:
                        throw new NotSupportedException();
                }
            }
        }
    }

    #endregion Tool

    #endregion

    #region GetBytes 

    /// <inheritdoc/>
    public virtual byte[] GetBytes(decimal value)
    {
        var bytes = DecimalConver.ToBytes(value);
        if (!this.IsSameOfSet())
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(char value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!this.IsSameOfSet())
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(bool value)
    {
        return GetBytes(new bool[1]
        {
              value
        });
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(bool[] values)
    {
        return values.BoolArrayToByte();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (!IsSameOfSet())
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(short[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (!IsSameOfSet())
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ushort[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(int value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(int[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(uint value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(uint[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(long value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(long[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ulong value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ulong[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(float value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(float[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(double value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(double[] value)
    {
        byte[] numArray = new byte[value.Length * 2];
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            bytes.CopyTo(numArray, 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }
        if (StringLength != null)
        {
            if (BcdFormat != null)
            {
                byte[] bytes = DataTransUtil.GetBytesFromBCD(value, BcdFormat.Value);
                return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(StringLength.Value) : bytes.ArrayExpandToLength(StringLength.Value);
            }
            else
            {
                byte[] bytes = Encoding.GetBytes(value);
                return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(StringLength.Value) : bytes.ArrayExpandToLength(StringLength.Value);
            }
        }
        else
        {
            if (BcdFormat != null)
            {
                byte[] bytes = DataTransUtil.GetBytesFromBCD(value, BcdFormat.Value);
                return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
            }
            else
            {
                byte[] bytes = Encoding.GetBytes(value);
                return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
            }
        }
    }

    #endregion

    /// <inheritdoc/>
    public virtual bool IsSameOfSet()
    {
        return !(BitConverter.IsLittleEndian ^ (EndianType == EndianType.Little));
    }

    /// <inheritdoc/>
    public virtual bool ToBoolean(byte[] buffer, int offset, bool isReverse = false)
    {
        byte[] bytes = new byte[buffer.Length];
        Array.Copy(buffer, 0, bytes, 0, buffer.Length);
        if (isReverse && !IsSameOfSet())
            bytes = buffer.BytesReverseByWord();
        return bytes.GetBoolByIndex(offset);
    }

    /// <inheritdoc/>
    public virtual byte ToByte(byte[] buffer, int offset)
    {
        if (buffer.Length - offset >= 2)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            return bytes[0];
        }
        else
        {
            byte[] bytes = new byte[1];
            Array.Copy(buffer, offset, bytes, 0, bytes.Length);
            return bytes[0];
        }
    }

    /// <inheritdoc/>
    public virtual byte[] ToByte(byte[] buffer, int offset, int length)
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
    public virtual double ToDouble(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<double>(p);
                }
                else
                {
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<double>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual short ToInt16(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<short>(p);
                }
                else
                {
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<short>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }
    /// <inheritdoc/>
    public virtual int ToInt32(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<int>(p);
                }
                else
                {
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<int>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual long ToInt64(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<long>(p);
                }
                else
                {
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<long>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual float ToSingle(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<float>(p);
                }
                else
                {
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<float>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual string ToString(byte[] buffer)
    {
        return ToString(buffer, 0, buffer.Length);
    }

    /// <inheritdoc/>
    public virtual string ToString(byte[] buffer, int offset, int length)
    {
        byte[] numArray = buffer.SelectMiddle(offset, length);
        if (BcdFormat != null)
        {
            return IsStringReverseByteWord ? DataTransUtil.GetBCDValue(buffer.SelectMiddle(offset, length).BytesReverseByWord(), BcdFormat.Value) : DataTransUtil.GetBCDValue(buffer.SelectMiddle(offset, length), BcdFormat.Value);
        }
        else
        {
            return IsStringReverseByteWord ?
                Encoding.GetString(numArray.BytesReverseByWord()).TrimEnd().Replace($"\0", "") :
                Encoding.GetString(numArray).TrimEnd().Replace($"\0", "");
        }
    }

    /// <inheritdoc/>
    public virtual ushort ToUInt16(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<ushort>(p);
                }
                else
                {
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<ushort>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual uint ToUInt32(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<uint>(p);
                }
                else
                {
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<uint>(p);
                    this.ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual ulong ToUInt64(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<ulong>(p);
                }
                else
                {
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<ulong>(p);
                    this.ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual char ToChar(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<char>(p);
                }
                else
                {
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<char>(p);
                    this.ByteTransDataFormat2_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }


    /// <inheritdoc/>
    public virtual decimal ToDecimal(byte[] buffer, int offset)
    {
        if (buffer.Length - offset < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                if (this.IsSameOfSet())
                {
                    return Unsafe.Read<decimal>(p);
                }
                else
                {
                    this.ByteTransDataFormat16_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<decimal>(p);
                    this.ByteTransDataFormat16_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
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
            case DataFormatEnum.ABCD:
            case null:
                numArray[0] = value[offset + 3];
                numArray[1] = value[offset + 2];
                numArray[2] = value[offset + 1];
                numArray[3] = value[offset];
                break;

            case DataFormatEnum.BADC:
                numArray[0] = value[offset + 2];
                numArray[1] = value[offset + 3];
                numArray[2] = value[offset];
                numArray[3] = value[offset + 1];
                break;

            case DataFormatEnum.CDAB:
                numArray[0] = value[offset + 1];
                numArray[1] = value[offset];
                numArray[2] = value[offset + 3];
                numArray[3] = value[offset + 2];
                break;

            case DataFormatEnum.DCBA:
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
            case DataFormatEnum.ABCD:
            case null:
                numArray[0] = value[offset + 7];
                numArray[1] = value[offset + 6];
                numArray[2] = value[offset + 5];
                numArray[3] = value[offset + 4];
                numArray[4] = value[offset + 3];
                numArray[5] = value[offset + 2];
                numArray[6] = value[offset + 1];
                numArray[7] = value[offset];
                break;

            case DataFormatEnum.BADC:
                numArray[0] = value[offset + 6];
                numArray[1] = value[offset + 7];
                numArray[2] = value[offset + 4];
                numArray[3] = value[offset + 5];
                numArray[4] = value[offset + 2];
                numArray[5] = value[offset + 3];
                numArray[6] = value[offset];
                numArray[7] = value[offset + 1];
                break;

            case DataFormatEnum.CDAB:
                numArray[0] = value[offset + 1];
                numArray[1] = value[offset];
                numArray[2] = value[offset + 3];
                numArray[3] = value[offset + 2];
                numArray[4] = value[offset + 5];
                numArray[5] = value[offset + 4];
                numArray[6] = value[offset + 7];
                numArray[7] = value[offset + 6];
                break;

            case DataFormatEnum.DCBA:
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

    /// <inheritdoc/>
    public virtual bool[] ToBoolean(byte[] buffer, int offset, int len, bool isReverse = false)
    {
        byte[] bytes = new byte[buffer.Length];
        Array.Copy(buffer, 0, bytes, 0, buffer.Length);
        bool[] result = new bool[len];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ToBoolean(buffer, offset + i, isReverse);
        }
        return result;
    }

    /// <inheritdoc/>
    public virtual double[] ToDouble(byte[] buffer, int offset, int len)
    {
        double[] numArray = new double[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToDouble(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual short[] ToInt16(byte[] buffer, int offset, int len)
    {
        short[] numArray = new short[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual int[] ToInt32(byte[] buffer, int offset, int len)
    {
        int[] numArray = new int[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual long[] ToInt64(byte[] buffer, int offset, int len)
    {
        long[] numArray = new long[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual float[] ToSingle(byte[] buffer, int offset, int len)
    {
        float[] numArray = new float[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToSingle(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual ushort[] ToUInt16(byte[] buffer, int offset, int len)
    {
        ushort[] numArray = new ushort[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual uint[] ToUInt32(byte[] buffer, int offset, int len)
    {
        uint[] numArray = new uint[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual ulong[] ToUInt64(byte[] buffer, int offset, int len)
    {
        ulong[] numArray = new ulong[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }
    /// <inheritdoc/>
    public virtual decimal[] ToDecimal(byte[] buffer, int offset, int len)
    {
        decimal[] numArray = new decimal[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToDecimal(buffer, offset + 8 * index);
        }
        return numArray;
    }
}
