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

    /// <inheritdoc/>
    public virtual EndianType EndianType { get; set; }

    /// <inheritdoc/>
    public virtual bool IsStringReverseByteWord { get; set; }

    /// <inheritdoc/>
    public virtual bool IsBoolReverseByteWord { get; set; }

    internal TouchSocketBitConverter TouchSocketBitConverter => TouchSocketBitConverter.GetBitConverter(EndianType);

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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
            byte[] bytes = TouchSocketBitConverter.GetBytes(value[index]);
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
    public virtual bool ToBoolean(byte[] buffer, int offset)
    {
        byte[] bytes;
        if (IsBoolReverseByteWord)
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
        return TouchSocketBitConverter.ToInt32(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual long ToInt64(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToInt64(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual ushort ToUInt16(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToUInt16(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual uint ToUInt32(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToUInt32(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual ulong ToUInt64(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToUInt64(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual float ToSingle(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToSingle(buffer, offset);
    }

    public virtual double ToDouble(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToDouble(buffer, offset);
    }

    /// <inheritdoc/>
    public virtual bool[] ToBoolean(byte[] buffer, int offset, int len)
    {
        bool[] result = new bool[len];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ToBoolean(buffer, offset + i);
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

    public virtual byte[] GetBytes(decimal value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(char value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(bool value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(bool[] values)
    {
        return TouchSocketBitConverter.GetBytes(values);
    }

    public virtual byte[] GetBytes(short value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(ushort value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(int value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(uint value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(long value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(ulong value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(float value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual byte[] GetBytes(double value)
    {
        return TouchSocketBitConverter.GetBytes(value);
    }

    public virtual decimal ToDecimal(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToDecimal(buffer, offset);
    }

    public virtual char ToChar(byte[] buffer, int offset)
    {
        return TouchSocketBitConverter.ToChar(buffer, offset);
    }
}
