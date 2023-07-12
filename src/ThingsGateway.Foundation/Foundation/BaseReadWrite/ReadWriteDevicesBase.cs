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
using System.Linq;

namespace ThingsGateway.Foundation;

/// <summary>
/// 读写设备基类
/// </summary>
public abstract class ReadWriteDevicesBase : DisposableObject, IReadWriteDevice
{
    /// <inheritdoc/>
    [Description("数据转换")]
    public DataFormat DataFormat
    {
        get => ThingsGatewayBitConverter.DataFormat;
        set => ThingsGatewayBitConverter.DataFormat = value;
    }

    /// <inheritdoc/>
    public ILog Logger { get; protected set; }
    /// <inheritdoc/>
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter(EndianType.Big);
    /// <inheritdoc/>
    [Description("读写超时")]
    public ushort TimeOut { get; set; } = 3000;
    /// <inheritdoc/>
    public ushort RegisterByteLength { get; set; } = 1;
    /// <inheritdoc/>

    public virtual int GetBitOffset(string address)
    {
        int bitIndex = 0;
        string[] addressSplits = new string[] { address };
        if (address.IndexOf('.') > 0)
        {
            addressSplits = address.SplitDot();
            try
            {
                bitIndex = Convert.ToInt32(addressSplits.Last());
            }
            catch
            {
                return 0;
            }
        }
        return bitIndex;
    }
    /// <inheritdoc/>
    public abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default);

    /// <inheritdoc/>
    public abstract Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default);
    /// <inheritdoc/>
    public abstract OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default);


    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default);

    /// <inheritdoc/>
    public abstract Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default);

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, bool value, CancellationToken token = default)
    {
        return WriteAsync(address, new bool[1] { value }, token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, short value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, byte value, CancellationToken token = default)
    {
        return WriteAsync(address, new byte[1] { value }, token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ushort value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, int value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, uint value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, long value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, ulong value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, float value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, double value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        return WriteAsync(address, transformParameter.GetBytes(value), token);
    }

    /// <inheritdoc/>
    public virtual Task<OperResult> WriteAsync(string address, string value, CancellationToken token = default)
    {
        IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
        byte[] data = transformParameter.GetBytes(value);
        return WriteAsync(address, data, token);
    }

    /// <inheritdoc/>
    public virtual string GetAddressDescription()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("通用格式");
        stringBuilder.AppendLine("4字节转换格式");
        stringBuilder.AppendLine("DATA=ABCD;");
        stringBuilder.AppendLine("举例：");
        stringBuilder.AppendLine("DATA=ABCD; ，代表大端格式，其中ABCD=>Big-Endian;BADC=>;Big-Endian Byte Swap;CDAB=>Little-Endian Byte Swap;DCBA=>Little-Endian");
        stringBuilder.AppendLine("字符串长度：");
        stringBuilder.AppendLine("LEN=1;");
        stringBuilder.AppendLine("BCD格式：");
        stringBuilder.AppendLine("BCD=C8421;，其中有C8421;C5421;C2421;C3;Gray");
        stringBuilder.AppendLine("字符格式：");
        stringBuilder.AppendLine("TEXT=UTF8;，其中有UTF8;ASCII;Default;Unicode");
        stringBuilder.AppendLine("");
        return stringBuilder.ToString();
    }
}