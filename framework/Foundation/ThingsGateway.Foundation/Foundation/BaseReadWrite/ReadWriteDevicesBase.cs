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

using System.ComponentModel;
using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Core;



/// <summary>
/// 读写设备基类
/// </summary>
public abstract class ReadWriteDevicesBase : IReadWrite
{

    #region 属性
    /// <inheritdoc/>
    [Description("组包缓存时间ms")]
    public int CacheTimeout { get; set; } = 1000;

    /// <inheritdoc/>
    [Description("数据解析规则")]
    public DataFormat DataFormat
    {
        get => ThingsGatewayBitConverter.DataFormat;
        set => ThingsGatewayBitConverter.DataFormat = value;
    }


    /// <inheritdoc/>
    [Description("帧前时间ms")]
    public int FrameTime { get; set; }

    /// <inheritdoc/>
    public ILog Logger { get; protected set; }

    /// <inheritdoc/>
    public ushort RegisterByteLength { get; protected set; } = 1;

    /// <inheritdoc/>
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter(EndianType.Big);

    /// <inheritdoc/>
    [Description("读写超时时间")]
    public int TimeOut { get; set; } = 3000;

    /// <inheritdoc/>
    public bool CascadeDisposal { get; set; } = true;

    /// <inheritdoc/>
    public virtual string GetAddressDescription()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("通用格式");
        stringBuilder.AppendLine("4字节转换格式");
        stringBuilder.AppendLine("  DATA=ABCD; ，代表大端格式，其中ABCD=>Big-Endian;BADC=>;Big-Endian Byte Swap;CDAB=>Little-Endian Byte Swap;DCBA=>Little-Endian");
        stringBuilder.AppendLine("字符串长度/数组长度：");
        stringBuilder.AppendLine("  LEN=1;");
        stringBuilder.AppendLine("BCD格式：");
        stringBuilder.AppendLine("  BCD=C8421;，其中有C8421;C5421;C2421;C3;Gray");
        stringBuilder.AppendLine("字符格式：");
        stringBuilder.AppendLine("  TEXT=UTF8;，其中有UTF8;ASCII;Default;Unicode");
        stringBuilder.AppendLine("");
        return stringBuilder.ToString();
    }

    /// <inheritdoc/>
    public virtual int GetBitOffset(string address)
    {
        int bitIndex = 0;
        if (address?.IndexOf('.') > 0)
            bitIndex = address.SplitDot().Last().ToInt();
        return bitIndex;
    }
    /// <inheritdoc/>
    public virtual bool IsBitReverse(string address)
    {
        return address?.IndexOf('.') > 0;
    }

    /// <summary>
    /// 获取数据类型对应的寄存器长度
    /// </summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="length">读取数量</param>
    /// <param name="typeLength">读取数据类型对应的字节长度</param>
    /// <param name="isBool">isBool</param>
    /// <returns></returns>
    protected virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool && IsBitReverse(address))
        {
            var data = Math.Ceiling((double)length / RegisterByteLength / 8);
            return (int)data;
        }
        else
        {
            return (int)result;
        }
    }
    #endregion

    #region 读取
    /// <inheritdoc/>
    public abstract OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据数据类型从设备中获取实际值，返回object
    /// </summary>
    public async Task<IOperResult<object>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => await ReadStringAsync(address, length, cancellationToken),
            DataTypeEnum.Boolean => await ReadBooleanAsync(address, length, cancellationToken),
            DataTypeEnum.Byte => await ReadAsync(address, length, cancellationToken),
            DataTypeEnum.Int16 => await ReadInt16Async(address, length, cancellationToken),
            DataTypeEnum.UInt16 => await ReadUInt16Async(address, length, cancellationToken),
            DataTypeEnum.Int32 => await ReadInt32Async(address, length, cancellationToken),
            DataTypeEnum.UInt32 => await ReadUInt32Async(address, length, cancellationToken),
            DataTypeEnum.Int64 => await ReadInt64Async(address, length, cancellationToken),
            DataTypeEnum.UInt64 => await ReadUInt64Async(address, length, cancellationToken),
            DataTypeEnum.Single => await ReadSingleAsync(address, length, cancellationToken),
            DataTypeEnum.Double => await ReadDoubleAsync(address, length, cancellationToken),
            _ => new OperResult<object>($"{dataType}数据类型未实现"),
        };
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值，返回object
    /// </summary>
    public IOperResult<object> Read(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => ReadString(address, length, cancellationToken),
            DataTypeEnum.Boolean => ReadBoolean(address, length, cancellationToken),
            DataTypeEnum.Byte => Read(address, length, cancellationToken),
            DataTypeEnum.Int16 => ReadInt16(address, length, cancellationToken),
            DataTypeEnum.UInt16 => ReadUInt16(address, length, cancellationToken),
            DataTypeEnum.Int32 => ReadInt32(address, length, cancellationToken),
            DataTypeEnum.UInt32 => ReadUInt32(address, length, cancellationToken),
            DataTypeEnum.Int64 => ReadInt64(address, length, cancellationToken),
            DataTypeEnum.UInt64 => ReadUInt64(address, length, cancellationToken),
            DataTypeEnum.Single => ReadSingle(address, length, cancellationToken),
            DataTypeEnum.Double => ReadDouble(address, length, cancellationToken),
            _ => new OperResult<object>($"{dataType}数据类型未实现"),
        };
    }

    #region 其他数据类型读取
    /// <inheritdoc/>
    public async Task<OperResult<Boolean[]>> ReadBooleanAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);

        var result = await ReadAsync(address, GetLength(address, length, RegisterByteLength, true), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToBoolean(result.Content, GetBitOffset(address), length, IsBitReverse(address)));
    }
    /// <inheritdoc/>
    public async Task<OperResult<Int16[]>> ReadInt16Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt16(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<UInt16[]>> ReadUInt16Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt16(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<Int32[]>> ReadInt32Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt32(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<UInt32[]>> ReadUInt32Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public async Task<OperResult<Int64[]>> ReadInt64Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt64(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<UInt64[]>> ReadUInt64Async(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt64(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<Single[]>> ReadSingleAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToSingle(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public async Task<OperResult<Double[]>> ReadDoubleAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToDouble(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public async Task<OperResult<String>> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = await ReadAsync(address, GetLength(address, transformParameter.Length ?? length, 1), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToString(result.Content));
    }


    #endregion

    #region 其他数据类型读取
    /// <inheritdoc/>
    public OperResult<Boolean[]> ReadBoolean(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, RegisterByteLength, true), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToBoolean(result.Content, GetBitOffset(address), length, IsBitReverse(address)));
    }
    /// <inheritdoc/>
    public OperResult<Int16[]> ReadInt16(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt16(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<UInt16[]> ReadUInt16(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 2), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt16(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<Int32[]> ReadInt32(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt32(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<UInt32[]> ReadUInt32(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt32(result.Content, 0, length));
    }

    /// <inheritdoc/>
    public OperResult<Int64[]> ReadInt64(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToInt64(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<UInt64[]> ReadUInt64(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToUInt64(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<Single[]> ReadSingle(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 4), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToSingle(result.Content, 0, length));
    }
    /// <inheritdoc/>
    public OperResult<Double[]> ReadDouble(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToDouble(result.Content, 0, length));
        //return global::ReadWriteDevicesExtensionsHelpers.OperResultFrom<double[]>(result, (Func<double[]>)(() => transformParameter.ToDouble(result.Content, 0, length)));
    }

    /// <inheritdoc/>
    public OperResult<String> ReadString(string address, int length, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        var result = Read(address, GetLength(address, transformParameter.Length ?? length, 1), cancellationToken);
        return result.OperResultFrom(() => transformParameter.ToString(result.Content));
    }


    #endregion

    #endregion

    /// <summary>
    /// 根据数据类型写入设备
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> WriteAsync(string address, string value, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (length <= 1)
            {
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, value, cancellationToken),
                    DataTypeEnum.Boolean => await WriteAsync(address, value.GetBoolValue(), cancellationToken),
                    DataTypeEnum.Byte => await WriteAsync(address, Convert.ToByte(value), cancellationToken),
                    DataTypeEnum.Int16 => await WriteAsync(address, Convert.ToInt16(value), cancellationToken),
                    DataTypeEnum.UInt16 => await WriteAsync(address, Convert.ToUInt16(value), cancellationToken),
                    DataTypeEnum.Int32 => await WriteAsync(address, Convert.ToInt32(value), cancellationToken),
                    DataTypeEnum.UInt32 => await WriteAsync(address, Convert.ToUInt32(value), cancellationToken),
                    DataTypeEnum.Int64 => await WriteAsync(address, Convert.ToInt64(value), cancellationToken),
                    DataTypeEnum.UInt64 => await WriteAsync(address, Convert.ToUInt64(value), cancellationToken),
                    DataTypeEnum.Single => await WriteAsync(address, Convert.ToSingle(value), cancellationToken),
                    DataTypeEnum.Double => await WriteAsync(address, Convert.ToDouble(value), cancellationToken),
                    _ => new OperResult($"{dataType}数据类型未实现写入"),
                };
            }
            else
            {
                return dataType switch
                {
                    DataTypeEnum.String => await WriteAsync(address, value, cancellationToken),
                    DataTypeEnum.Boolean => await WriteAsync(address, value.FromJsonString<bool[]>(), cancellationToken),
                    DataTypeEnum.Byte => await WriteAsync(address, value.FromJsonString<byte[]>(), cancellationToken),
                    DataTypeEnum.Int16 => await WriteAsync(address, value.FromJsonString<Int16[]>(), cancellationToken),
                    DataTypeEnum.UInt16 => await WriteAsync(address, value.FromJsonString<UInt16[]>(), cancellationToken),
                    DataTypeEnum.Int32 => await WriteAsync(address, value.FromJsonString<Int32[]>(), cancellationToken),
                    DataTypeEnum.UInt32 => await WriteAsync(address, value.FromJsonString<UInt32[]>(), cancellationToken),
                    DataTypeEnum.Int64 => await WriteAsync(address, value.FromJsonString<Int64[]>(), cancellationToken),
                    DataTypeEnum.UInt64 => await WriteAsync(address, value.FromJsonString<UInt64[]>(), cancellationToken),
                    DataTypeEnum.Single => await WriteAsync(address, value.FromJsonString<Single[]>(), cancellationToken),
                    DataTypeEnum.Double => await WriteAsync(address, value.FromJsonString<Double[]>(), cancellationToken),
                    _ => new OperResult($"{dataType}数据类型未实现写入"),
                };
            }

        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }

    }

    /// <summary>
    /// 根据数据类型写入设备
    /// </summary>
    /// <returns></returns>
    public OperResult Write(string address, string value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            return dataType switch
            {
                DataTypeEnum.String => Write(address, value, cancellationToken),
                DataTypeEnum.Boolean => Write(address, value.GetBoolValue(), cancellationToken),
                DataTypeEnum.Byte => Write(address, Convert.ToByte(value), cancellationToken),
                DataTypeEnum.Int16 => Write(address, Convert.ToInt16(value), cancellationToken),
                DataTypeEnum.UInt16 => Write(address, Convert.ToUInt16(value), cancellationToken),
                DataTypeEnum.Int32 => Write(address, Convert.ToInt32(value), cancellationToken),
                DataTypeEnum.UInt32 => Write(address, Convert.ToUInt32(value), cancellationToken),
                DataTypeEnum.Int64 => Write(address, Convert.ToInt64(value), cancellationToken),
                DataTypeEnum.UInt64 => Write(address, Convert.ToUInt64(value), cancellationToken),
                DataTypeEnum.Single => Write(address, Convert.ToSingle(value), cancellationToken),
                DataTypeEnum.Double => Write(address, Convert.ToDouble(value), cancellationToken),
                _ => new OperResult($"{dataType}数据类型未实现写入"),
            };
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }

    }

    #region 异步写入

    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, new bool[1] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, byte value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, short value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ushort value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, int value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, uint value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, long value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ulong value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, float value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, double value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    #endregion

    #region 连续多写

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, short[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ushort[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, int[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, uint[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, long[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ulong[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, float[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, double[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), cancellationToken);
    }

    #endregion

    #region 连续多写


    /// <inheritdoc/>
    public OperResult Write(string address, short[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, ushort[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, int[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, uint[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, long[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, ulong[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, float[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public OperResult Write(string address, double[] value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    #endregion

    #region 同步写入

    /// <inheritdoc/>
    public abstract OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual OperResult Write(string address, bool value, CancellationToken cancellationToken = default)
    {
        return Write(address, new bool[1] { value }, cancellationToken);
    }
    /// <inheritdoc/>
    public virtual OperResult Write(string address, byte value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, short value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ushort value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, int value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, uint value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, long value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, ulong value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, float value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, double value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    /// <inheritdoc/>
    public virtual OperResult Write(string address, string value, CancellationToken cancellationToken = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return Write(address, transformParameter.GetBytes(value), cancellationToken);
    }

    #endregion


    #region 连接/断开

    /// <summary>
    /// 连接操作
    /// </summary>
    public abstract void Connect(CancellationToken cancellationToken);

    /// <inheritdoc cref="Connect"/>
    public abstract Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 断开操作
    /// </summary>
    public abstract void Disconnect();

    /// <summary>
    /// 设置适配器
    /// </summary>
    public abstract void SetDataAdapter(object socketClient = null);

    /// <inheritdoc/>
    public abstract void Dispose();

    #endregion

    /// <inheritdoc/>
    public abstract List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new();

}