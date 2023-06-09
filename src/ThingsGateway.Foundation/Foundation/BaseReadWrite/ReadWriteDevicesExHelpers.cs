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
    /// 根据数据类型写入设备
    /// </summary>
    /// <returns></returns>
    public static Task<OperResult> WriteAsync(this IReadWriteDevice readWriteDevice, Type type, string address, string value, bool isBcd = false, CancellationToken cancellationToken = default)
    {
        if (type == typeof(bool))
            return readWriteDevice.WriteAsync(address, GetBoolValue(value), cancellationToken);
        else if (type == typeof(byte))
            return readWriteDevice.WriteAsync(address, Convert.ToByte(value), cancellationToken);
        else if (type == typeof(sbyte))
            return readWriteDevice.WriteAsync(address, Convert.ToSByte(value), cancellationToken);
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
            return readWriteDevice.WriteAsync(address, value, isBcd, cancellationToken);
        }
        return Task.FromResult(new OperResult($"{type}数据类型未实现写入"));
    }



    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    /// <param name="thingsGatewayBitConverter"></param>
    /// <param name="type"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static dynamic GetDynamicData(this IThingsGatewayBitConverter thingsGatewayBitConverter, Type type, params byte[] bytes)
    {
        if (type == typeof(bool))
            return thingsGatewayBitConverter.ToBoolean(bytes, 0);
        else if (type == typeof(byte))
            return thingsGatewayBitConverter.ToByte(bytes, 0);
        else if (type == typeof(sbyte))
            return thingsGatewayBitConverter.ToByte(bytes, 0);
        else if (type == typeof(short))
            return thingsGatewayBitConverter.ToInt16(bytes, 0);
        else if (type == typeof(ushort))
            return thingsGatewayBitConverter.ToUInt16(bytes, 0);
        else if (type == typeof(int))
            return thingsGatewayBitConverter.ToInt32(bytes, 0);
        else if (type == typeof(uint))
            return thingsGatewayBitConverter.ToUInt32(bytes, 0);
        else if (type == typeof(long))
            return thingsGatewayBitConverter.ToInt64(bytes, 0);
        else if (type == typeof(ulong))
            return thingsGatewayBitConverter.ToUInt64(bytes, 0);
        else if (type == typeof(float))
            return thingsGatewayBitConverter.ToSingle(bytes, 0);
        else if (type == typeof(double))
            return thingsGatewayBitConverter.ToDouble(bytes, 0);
        else if (type == typeof(string))
        {
            return thingsGatewayBitConverter.ToString(bytes);
        }
        return Task.FromResult(new OperResult($"{type}数据类型未实现"));
    }
}