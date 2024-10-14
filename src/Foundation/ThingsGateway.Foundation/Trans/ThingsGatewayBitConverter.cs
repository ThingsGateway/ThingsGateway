//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
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
    public DataFormatEnum DataFormat { get; set; }

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
        EndianType = endianType;
    }

    /// <inheritdoc/>
    public virtual EndianType EndianType { get; }

    /// <inheritdoc/>
    public virtual bool IsStringReverseByteWord { get; set; }

    /// <inheritdoc/>
    public virtual bool IsVariableStringLength { get; set; }

    internal TouchSocketBitConverter TouchSocketBitConverter => TouchSocketBitConverter.GetBitConverter(EndianType);

    static ThingsGatewayBitConverter()
    {
        BigEndian = new ThingsGatewayBitConverter(EndianType.Big);
        LittleEndian = new ThingsGatewayBitConverter(EndianType.Little);
    }

    /// <summary>
    /// 以大端
    /// </summary>
    public static readonly ThingsGatewayBitConverter BigEndian;

    /// <summary>
    /// 以小端
    /// </summary>
    public static readonly ThingsGatewayBitConverter LittleEndian;

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter GetByDataFormat(DataFormatEnum dataFormat)
    {
        var data = new ThingsGatewayBitConverter(EndianType);
        data.Encoding = Encoding;
        data.DataFormat = dataFormat;
        data.BcdFormat = BcdFormat;
        data.StringLength = StringLength;
        data.ArrayLength = ArrayLength;
        data.IsStringReverseByteWord = IsStringReverseByteWord;

        return data;
    }

    #region GetBytes

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

    /// <inheritdoc/>
    public virtual byte[] GetBytes(short[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 2);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ushort[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 2);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(int[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(uint[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(long[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ulong[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(float[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(double[] value)
    {
        using ValueByteBlock byteBlock = new ValueByteBlock(value.Length * 4);
        for (int index = 0; index < value.Length; ++index)
        {
            byte[] bytes = GetBytes(value[index]);
            byteBlock.Write(bytes);
        }
        return byteBlock.ToArray();
    }

    #endregion GetBytes

    /// <inheritdoc/>
    public virtual string ToString(byte[] buffer, int offset, int len)
    {
        if (BcdFormat != null)
        {
            return IsStringReverseByteWord ? DataTransUtil.GetBcdValue(new ReadOnlySpan<byte>(buffer, offset, len).ToArray().BytesReverseByWord(), BcdFormat.Value) : DataTransUtil.GetBcdValue(new ReadOnlySpan<byte>(buffer, offset, len), BcdFormat.Value);
        }
        else
        {
            return IsStringReverseByteWord ?
                Encoding.GetString(new ReadOnlySpan<byte>(buffer, offset, len).ToArray().BytesReverseByWord()).TrimEnd().Replace($"\0", "") :
                Encoding.GetString(buffer, offset, len).TrimEnd().Replace($"\0", "");
        }
    }

    /// <inheritdoc/>
    public virtual bool ToBoolean(byte[] buffer, int offset, bool isReverse)
    {
        byte[] bytes;
        if (isReverse)
            bytes = buffer.BytesReverseByWord();
        else
            bytes = buffer.CopyArray();
        return bytes.GetBoolByIndex(offset);
    }

    /// <inheritdoc/>
    public virtual byte ToByte(byte[] buffer, int offset)
    {
        return buffer[offset];
    }

    /// <inheritdoc/>
    public virtual short ToInt16(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToInt16(buffer, offset);
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<int>(p);
                }
                else
                {
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<int>(p);
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<long>(p);
                }
                else
                {
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<long>(p);
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual ushort ToUInt16(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToUInt16(buffer, offset);
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<uint>(p);
                }
                else
                {
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<uint>(p);
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<ulong>(p);
                }
                else
                {
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<ulong>(p);
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<float>(p);
                }
                else
                {
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<float>(p);
                    ByteTransDataFormat4_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<double>(p);
                }
                else
                {
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<double>(p);
                    ByteTransDataFormat8_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual bool[] ToBoolean(byte[] buffer, int offset, int len, bool isReverse = false)
    {
        bool[] result = new bool[len];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ToBoolean(buffer, offset + i, isReverse);
        }
        return result;
    }

    /// <inheritdoc/>
    public virtual byte[] ToByte(byte[] buffer, int offset, int length)
    {
        byte[] bytes = new byte[length];
        Array.Copy(buffer, offset, bytes, 0, bytes.Length);
        return bytes;
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

    /// <inheritdoc/>
    public virtual byte[] GetBytes(decimal value)
    {
        var bytes = new byte[16];
        Unsafe.As<byte, decimal>(ref bytes[0]) = value;
        if (DataFormat != DataFormatEnum.DCBA)
        {
            ByteTransDataFormat16_Net6(ref bytes[0]);
        }
        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(char value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(bool value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(bool[] values)
    {
        return TouchSocketBitConverter.GetBytes(values);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(short value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ushort value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat4(bytes, 0);

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(uint value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat4(bytes, 0);

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(long value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat8(bytes, 0);

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ulong value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat8(bytes, 0);

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(float value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat4(bytes, 0);

        return bytes;
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(double value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (DataFormat != DataFormatEnum.DCBA)
            bytes = ByteTransDataFormat8(bytes, 0);

        return bytes;
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
                if (DataFormat == DataFormatEnum.DCBA)
                {
                    return Unsafe.Read<decimal>(p);
                }
                else
                {
                    ByteTransDataFormat16_Net6(ref buffer[offset]);
                    var v = Unsafe.Read<decimal>(p);
                    ByteTransDataFormat16_Net6(ref buffer[offset]);
                    return v;
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual char ToChar(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToChar(buffer, offset);
    }

    #region Tool

    /// <summary>反转多字节的数据信息</summary>
    /// <param name="value">数据字节</param>
    /// <param name="offset">起始索引，默认值为0</param>
    /// <returns>实际字节信息</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] ByteTransDataFormat4(byte[] value, int offset)

    {
        var numArray = new byte[4];
        switch (DataFormat)
        {
            case DataFormatEnum.ABCD:
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] ByteTransDataFormat8(byte[] value, int offset)
    {
        var numArray = new byte[8];
        switch (DataFormat)
        {
            case DataFormatEnum.ABCD:
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

    #endregion Tool

    #region Tool

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
                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        var span = new Span<byte>(p, 16);
                        span.Reverse();
                        break;

                    case DataFormatEnum.DCBA:
                        return;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }

    #endregion Tool
}
