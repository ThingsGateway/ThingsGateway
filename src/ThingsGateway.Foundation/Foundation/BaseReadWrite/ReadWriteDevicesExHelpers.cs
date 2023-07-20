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


namespace ThingsGateway.Foundation;

/// <summary>
/// 读写扩展方法
/// </summary>
public static class ReadWriteDevicesExHelpers
{
    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetBoolValue(this string value)
    {
        if (value == "1")
            return true;
        if (value == "0")
            return false;
        value = value.ToUpper();
        if (value == "TRUE")
            return true;
        if (value == "FALSE")
            return false;
        if (value == "ON")
            return true;
        return !(value == "OFF") && bool.Parse(value);
    }

    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsBoolValue(this string value)
    {
        if (value == "1")
            return true;
        if (value == "0")
            return true;
        value = value.ToUpper();
        if (value == "TRUE")
            return true;
        if (value == "FALSE")
            return true;
        if (value == "ON")
            return true;
        if (value == "OFF")
            return true;
        return bool.TryParse(value, out bool result);
    }



    /// <summary>
    /// 根据数据类型写入设备，只支持C#内置数据类型，但不包含<see cref="decimal"/>和<see cref="char"/>和<see cref="sbyte"/>
    /// </summary>
    /// <returns></returns>
    public static Task<OperResult> WriteAsync(this IReadWriteDevice readWriteDevice, string address, Type type, string value, CancellationToken cancellationToken = default)
    {
        if (type == typeof(bool))
            return readWriteDevice.WriteAsync(address, GetBoolValue(value), cancellationToken);
        else if (type == typeof(byte))
            return readWriteDevice.WriteAsync(address, Convert.ToByte(value), cancellationToken);
        else if (type == typeof(short))
            return readWriteDevice.WriteAsync(address, Convert.ToInt16(value), cancellationToken);
        else if (type == typeof(ushort))
            return readWriteDevice.WriteAsync(address, Convert.ToUInt16(value), cancellationToken);
        else if (type == typeof(int))
            return readWriteDevice.WriteAsync(address, Convert.ToInt32(value), cancellationToken);
        else if (type == typeof(uint))
            return readWriteDevice.WriteAsync(address, Convert.ToUInt32(value), cancellationToken);
        else if (type == typeof(long))
            return readWriteDevice.WriteAsync(address, Convert.ToInt64(value), cancellationToken);
        else if (type == typeof(ulong))
            return readWriteDevice.WriteAsync(address, Convert.ToUInt64(value), cancellationToken);
        else if (type == typeof(float))
            return readWriteDevice.WriteAsync(address, Convert.ToSingle(value), cancellationToken);
        else if (type == typeof(double))
            return readWriteDevice.WriteAsync(address, Convert.ToDouble(value), cancellationToken);
        else if (type == typeof(string))
        {
            return readWriteDevice.WriteAsync(address, value, cancellationToken);
        }
        return Task.FromResult(new OperResult($"{type}数据类型未实现写入"));
    }


    #region 从设备中获取对应数据类型的数据
    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<bool>> GetBoolDataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var result = await readWriteDevice.ReadAsync(address, 1, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetBoolDataFormBytes(address, result.Content, readWriteDevice.GetBitOffset(address)));
        }
        else
        {
            return OperResult.CreateFailedResult<bool>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<byte>> GetByteDataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var result = await readWriteDevice.ReadAsync(address, 1, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetByteDataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<byte>(result);
        }
    }
    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<short>> GetInt16DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(2 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(2 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetInt16DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<short>(result);
        }
    }
    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<ushort>> GetUInt16DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(2 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(2 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetUInt16DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<ushort>(result);
        }
    }
    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<int>> GetInt32DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetInt32DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<int>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<uint>> GetUInt32DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetUInt32DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<uint>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<long>> GetInt64DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetInt64DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<long>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<ulong>> GetUInt64DataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetUInt64DataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<ulong>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<float>> GetSingleDataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetSingleDataFormBytes(address, result.Content));

        }
        else
        {
            return OperResult.CreateFailedResult<float>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<double>> GetDoubleDataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetDoubleDataFormBytes(address, result.Content));
        }
        else
        {
            return OperResult.CreateFailedResult<double>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<string>> GetStringDataFormDevice(this IReadWriteDevice readWriteDevice, string address, CancellationToken cancellationToken = default)
    {
        var converter = ByteTransformHelpers.GetTransByAddress(ref address, readWriteDevice.ThingsGatewayBitConverter);
        var result = await readWriteDevice.ReadAsync(address, converter.StringLength, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(readWriteDevice.ThingsGatewayBitConverter.GetStringDataFormBytes(address, result.Content));
        }
        else
        {
            return OperResult.CreateFailedResult<string>(result);
        }
    }

    /// <summary>
    /// 根据数据类型从设备中获取实际值
    /// </summary>
    public static async Task<OperResult<object>> GetDynamicDataFormDevice(this IReadWriteDevice readWriteDevice, string address, Type type, CancellationToken cancellationToken = default)
    {
        int length;
        if (type == typeof(bool))
            length = 1;
        else if (type == typeof(byte))
            length = 1;
        else if (type == typeof(short))
            length = ((int)(2 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(2 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(ushort))
            length = ((int)(2 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(2 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(int))
            length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(uint))
            length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(long))
            length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(ulong))
            length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(float))
            length = ((int)(4 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(4 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(double))
            length = ((int)(8 / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(8 / readWriteDevice.RegisterByteLength));
        else if (type == typeof(string))
        {
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, readWriteDevice.ThingsGatewayBitConverter);
            length = ((int)(byteConverter.StringLength / readWriteDevice.RegisterByteLength)) == 0 ? 1 : ((int)(byteConverter.StringLength / readWriteDevice.RegisterByteLength));
        }
        else
        {
            return new OperResult<object>($"{type}数据类型未实现");
        }
        var result = await readWriteDevice.ReadAsync(address, length, cancellationToken);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult<object>(readWriteDevice.ThingsGatewayBitConverter.GetDynamicDataFormBytes(address, type, result.Content));
        }
        else
        {
            return OperResult.CreateFailedResult<object>(result);
        }
    }
    #endregion


    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static object GetDynamicDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, Type type, byte[] bytes, int offset = 0)
    {
        if (address == null)
            address = string.Empty;
        if (type == typeof(bool))
            return thingsGatewayBitConverter.GetBoolDataFormBytes(address, bytes, offset);
        else if (type == typeof(byte))
            return thingsGatewayBitConverter.GetByteDataFormBytes(address, bytes, offset);
        else if (type == typeof(short))
            return thingsGatewayBitConverter.GetInt16DataFormBytes(address, bytes, offset);
        else if (type == typeof(ushort))
            return thingsGatewayBitConverter.GetUInt16DataFormBytes(address, bytes, offset);
        else if (type == typeof(int))
            return thingsGatewayBitConverter.GetInt32DataFormBytes(address, bytes, offset);
        else if (type == typeof(uint))
            return thingsGatewayBitConverter.GetUInt32DataFormBytes(address, bytes, offset);
        else if (type == typeof(long))
            return thingsGatewayBitConverter.GetInt64DataFormBytes(address, bytes, offset);
        else if (type == typeof(ulong))
            return thingsGatewayBitConverter.GetUInt64DataFormBytes(address, bytes, offset);
        else if (type == typeof(float))
            return thingsGatewayBitConverter.GetSingleDataFormBytes(address, bytes, offset);
        else if (type == typeof(double))
            return thingsGatewayBitConverter.GetDoubleDataFormBytes(address, bytes, offset);
        else if (type == typeof(string))
        {
            return thingsGatewayBitConverter.GetStringDataFormBytes(address, bytes, offset);
        }
        return Task.FromResult(new OperResult($"{type}数据类型未实现"));
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static bool GetDynamicDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToBoolean(bytes, offset);
    }

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static bool GetBoolDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToBoolean(bytes, offset);
    }

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static byte GetByteDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToByte(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static short GetInt16DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToInt16(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static ushort GetUInt16DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToUInt16(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static int GetInt32DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToInt32(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static uint GetUInt32DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToUInt32(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static long GetInt64DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToInt64(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static ulong GetUInt64DataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToUInt64(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static float GetSingleDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToSingle(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static double GetDoubleDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToDouble(bytes, offset);
    }
    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static string GetStringDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, int offset = 0)
    {
        var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, thingsGatewayBitConverter);
        return byteConverter.ToString(bytes, offset, bytes.Length - offset);
    }
    #endregion
}