#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Newtonsoft.Json;

namespace ThingsGateway.Foundation;

/// <summary>
/// 将基数据类型转换为指定端的一个字节数组，
/// 或将一个字节数组转换为指定端基数据类型。
/// </summary>
public class ThingsGatewayBitConverter : IThingsGatewayBitConverter
{
    private readonly EndianType endianType;

    private DataFormat dataFormat;
    /// <inheritdoc/>
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
    public ThingsGatewayBitConverter(EndianType endianType, DataFormat dataFormat)
    {
        this.endianType = endianType; this.dataFormat = dataFormat;
    }

    /// <inheritdoc/>
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


    /// <inheritdoc/>
    public EndianType EndianType => endianType;
    /// <inheritdoc/>
    public bool IsStringReverseByteWord { get; set; }

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter CreateByDateFormat(DataFormat dataFormat)
    {
        ThingsGatewayBitConverter byteConverter = new ThingsGatewayBitConverter(EndianType, dataFormat)
        {
            IsStringReverseByteWord = IsStringReverseByteWord,
        };
        return byteConverter;
    }
    /// <inheritdoc/>
    public byte[] GetBytes(bool value)
    {
        return GetBytes(new bool[1]
        {
              value
        });
    }

    /// <inheritdoc/>
    public byte[] GetBytes(bool[] values)
    {
        return values?.BoolArrayToByte();
    }

    /// <inheritdoc/>
    public byte[] GetBytes(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (!IsSameOfSet())
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }
    /// <inheritdoc/>
    public byte[] GetBytes(short[] value)
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
    public byte[] GetBytes(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (!IsSameOfSet())
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }
    /// <inheritdoc/>
    public byte[] GetBytes(ushort[] value)
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
    public byte[] GetBytes(int value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(int[] value)
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
    public byte[] GetBytes(uint value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(uint[] value)
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
    public byte[] GetBytes(long value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(long[] value)
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
    public byte[] GetBytes(ulong value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(ulong[] value)
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
    public byte[] GetBytes(float value)
    {
        return ByteTransDataFormat4(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(float[] value)
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
    public byte[] GetBytes(double value)
    {
        return ByteTransDataFormat8(BitConverter.GetBytes(value));
    }
    /// <inheritdoc/>
    public byte[] GetBytes(double[] value)
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
    public byte[] GetBytes(string value)
    {
        if (value == null)
        {
            return null;
        }

        byte[] bytes = Encoding.GetBytes(value);
        return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
    }

    /// <inheritdoc/>
    public byte[] GetBytes(string value, int length)
    {
        if (value == null)
        {
            return null;
        }

        byte[] bytes = Encoding.GetBytes(value);
        return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(length) : bytes.ArrayExpandToLength(length);
    }

    /// <inheritdoc/>
    public byte[] GetBytes(string value, BcdFormat bcdFormat)
    {
        if (value == null)
        {
            return null;
        }

        byte[] bytes = DataHelper.GetBytesFromBCD(value, bcdFormat);
        return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
    }

    /// <inheritdoc/>
    public byte[] GetBytes(string value, int length, BcdFormat bcdFormat)
    {
        if (value == null)
        {
            return null;
        }

        byte[] bytes = DataHelper.GetBytesFromBCD(value, bcdFormat);
        return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(length) : bytes.ArrayExpandToLength(length);
    }

    /// <inheritdoc/>
    public bool IsSameOfSet()
    {
        return !(BitConverter.IsLittleEndian ^ (endianType == EndianType.Little));
    }

    /// <inheritdoc/>
    public string ToBcdString(byte[] buffer, BcdFormat bcdFormat)
    {
        return ToBcdString(buffer, 0, buffer.Length, bcdFormat);
    }

    /// <inheritdoc/>
    public string ToBcdString(byte[] buffer, int offset, int length, BcdFormat bcdFormat)
    {
        return IsStringReverseByteWord ? DataHelper.GetBCDValue(buffer.SelectMiddle<byte>(offset, length).BytesReverseByWord(), bcdFormat) : DataHelper.GetBCDValue(buffer.SelectMiddle<byte>(offset, length), bcdFormat);
    }

    /// <inheritdoc/>
    public bool ToBoolean(byte[] buffer, int offset)
    {
        byte[] bytes = new byte[buffer.Length];
        Array.Copy(buffer, 0, bytes, 0, buffer.Length);
        if (!IsSameOfSet() && buffer.Length > 1)
        {
            bytes = bytes.BytesReverseByWord();
        }
        return bytes.GetBoolByIndex(offset);
    }

    /// <inheritdoc/>
    public byte ToByte(byte[] buffer, int offset)
    {
        byte[] bytes = new byte[1];
        Array.Copy(buffer, offset, bytes, 0, bytes.Length);
        return bytes[0];
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public int ToInt32(byte[] buffer, int offset)
    {
        byte[] bytes = ByteTransDataFormat4(buffer, offset);

        return BitConverter.ToInt32(bytes, 0);
    }

    /// <inheritdoc/>
    public long ToInt64(byte[] buffer, int offset)
    {
        byte[] bytes = ByteTransDataFormat8(buffer, offset);
        return BitConverter.ToInt64(bytes, 0);
    }

    /// <inheritdoc/>
    public float ToSingle(byte[] buffer, int offset)
    {
        byte[] bytes = ByteTransDataFormat4(buffer, offset);

        return BitConverter.ToSingle(bytes, 0);
    }

    /// <inheritdoc/>
    public string ToString(byte[] buffer)
    {
        return ToString(buffer, 0, buffer.Length);
    }

    /// <inheritdoc/>
    public string ToString(byte[] buffer, int offset, int length)
    {
        byte[] numArray = buffer.SelectMiddle<byte>(offset, length);
        return IsStringReverseByteWord ?
            Encoding.GetString(numArray.BytesReverseByWord()).TrimEnd().Replace($"\0", "") :
            Encoding.GetString(numArray).TrimEnd().Replace($"\0", "");
        ;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public uint ToUInt32(byte[] buffer, int offset)
    {
        byte[] bytes = ByteTransDataFormat4(buffer, offset);

        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool[] ToBoolean(byte[] buffer, int offset, int len)
    {
        bool[] flagArray = new bool[len];
        for (int index = 0; index < len; ++index)
        {
            flagArray[index] = buffer.GetBoolByIndex(offset + index);
        }
        return flagArray;
    }

    /// <inheritdoc/>
    public double[] ToDouble(byte[] buffer, int offset, int len)
    {
        double[] numArray = new double[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToDouble(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public short[] ToInt16(byte[] buffer, int offset, int len)
    {
        short[] numArray = new short[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public int[] ToInt32(byte[] buffer, int offset, int len)
    {
        int[] numArray = new int[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public long[] ToInt64(byte[] buffer, int offset, int len)
    {
        long[] numArray = new long[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }
    /// <inheritdoc/>
    public float[] ToSingle(byte[] buffer, int offset, int len)
    {
        float[] numArray = new float[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToSingle(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public ushort[] ToUInt16(byte[] buffer, int offset, int len)
    {
        ushort[] numArray = new ushort[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public uint[] ToUInt32(byte[] buffer, int offset, int len)
    {
        uint[] numArray = new uint[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public ulong[] ToUInt64(byte[] buffer, int offset, int len)
    {
        ulong[] numArray = new ulong[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }
}