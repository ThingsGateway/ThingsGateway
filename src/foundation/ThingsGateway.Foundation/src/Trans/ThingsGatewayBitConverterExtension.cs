//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{
    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static OperResult<object> GetDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, byte[] bytes, DataTypeEnum dataType, int offset = 0)
    {
        switch (dataType)
        {
            case DataTypeEnum.String:
                return new() { Content = thingsGatewayBitConverter.ToString(bytes, offset, bytes.Length - offset) };

            case DataTypeEnum.Boolean:
                return new() { Content = thingsGatewayBitConverter.ToBoolean(bytes, offset) };

            case DataTypeEnum.Byte:
                return new() { Content = thingsGatewayBitConverter.ToByte(bytes, offset) };

            case DataTypeEnum.Int16:
                return new() { Content = thingsGatewayBitConverter.ToInt16(bytes, offset) };

            case DataTypeEnum.UInt16:
                return new() { Content = thingsGatewayBitConverter.ToUInt16(bytes, offset) };

            case DataTypeEnum.Int32:
                return new() { Content = thingsGatewayBitConverter.ToInt32(bytes, offset) };

            case DataTypeEnum.UInt32:
                return new() { Content = thingsGatewayBitConverter.ToUInt32(bytes, offset) };

            case DataTypeEnum.Int64:
                return new() { Content = thingsGatewayBitConverter.ToInt64(bytes, offset) };

            case DataTypeEnum.UInt64:
                return new() { Content = thingsGatewayBitConverter.ToUInt64(bytes, offset) };

            case DataTypeEnum.Single:
                return new() { Content = thingsGatewayBitConverter.ToSingle(bytes, offset) };

            case DataTypeEnum.Double:
                return new() { Content = thingsGatewayBitConverter.ToDouble(bytes, offset) };

            default:
                return new(string.Format(FoundationConst.DataTypeNotSupported, dataType));
        }
    }


    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static OperResult<byte[]> GetBytesFormData(this IThingsGatewayBitConverter thingsGatewayBitConverter, JToken value, DataTypeEnum dataType)
    {
        if (thingsGatewayBitConverter.ArrayLength > 1)
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<bool[]>()) };

                case DataTypeEnum.Byte:
                    return new() { Content = value.ToObject<Byte[]>() };

                case DataTypeEnum.Int16:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int16[]>()) };

                case DataTypeEnum.UInt16:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt16[]>()) };

                case DataTypeEnum.Int32:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int32[]>()) };

                case DataTypeEnum.UInt32:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt32[]>()) };

                case DataTypeEnum.Int64:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int64[]>()) };

                case DataTypeEnum.UInt64:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt64[]>()) };

                case DataTypeEnum.Single:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Single[]>()) };

                case DataTypeEnum.Double:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Double[]>()) };

                default:
                    return new(string.Format(FoundationConst.DataTypeNotSupported, dataType));
            }
        }
        else
        {
            switch (dataType)
            {
                case DataTypeEnum.String:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<String>()) };

                case DataTypeEnum.Boolean:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Boolean>()) };

                case DataTypeEnum.Byte:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Byte>()) };

                case DataTypeEnum.Int16:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int16>()) };

                case DataTypeEnum.UInt16:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt16>()) };

                case DataTypeEnum.Int32:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int32>()) };

                case DataTypeEnum.UInt32:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt32>()) };

                case DataTypeEnum.Int64:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Int64>()) };

                case DataTypeEnum.UInt64:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<UInt64>()) };

                case DataTypeEnum.Single:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Single>()) };

                case DataTypeEnum.Double:
                    return new() { Content = thingsGatewayBitConverter.GetBytes(value.ToObject<Double>()) };

                default:
                    return new(string.Format(FoundationConst.DataTypeNotSupported, dataType));
            }

        }
    }

    #endregion 获取对应数据类型的数据
}