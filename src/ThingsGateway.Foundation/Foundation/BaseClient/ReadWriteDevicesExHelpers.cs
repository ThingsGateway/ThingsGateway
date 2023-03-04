namespace ThingsGateway.Foundation
{
    public static class ReadWriteDevicesExHelpers
    {
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

        public static Task<OperResult> WriteAsync(this IReadWriteDevice readWriteDevice, Type type, string address, string value, bool isBcd = false)
        {
            if (type == typeof(bool))
                return readWriteDevice.WriteAsync(address, GetBoolValue(value));
            else if (type == typeof(byte))
                return readWriteDevice.WriteAsync(address, Convert.ToByte(value));
            else if (type == typeof(sbyte))
                return readWriteDevice.WriteAsync(address, Convert.ToSByte(value));
            else if (type == typeof(short))
                return readWriteDevice.WriteAsync(address, Convert.ToInt16(value));
            else if (type == typeof(ushort))
                return readWriteDevice.WriteAsync(address, Convert.ToUInt16(value));
            else if (type == typeof(int))
                return readWriteDevice.WriteAsync(address, Convert.ToInt32(value));
            else if (type == typeof(uint))
                return readWriteDevice.WriteAsync(address, Convert.ToUInt32(value));
            else if (type == typeof(long))
                return readWriteDevice.WriteAsync(address, Convert.ToInt64(value));
            else if (type == typeof(ulong))
                return readWriteDevice.WriteAsync(address, Convert.ToUInt64(value));
            else if (type == typeof(float))
                return readWriteDevice.WriteAsync(address, Convert.ToSingle(value));
            else if (type == typeof(double))
                return readWriteDevice.WriteAsync(address, Convert.ToDouble(value));
            else if (type == typeof(string))
            {
                return readWriteDevice.WriteAsync(address, value, isBcd);
            }
            return Task.FromResult(new OperResult($"{type}数据类型未实现写入"));
        }




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
}