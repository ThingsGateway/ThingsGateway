//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
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
    public static object GetDataFormBytes(this IThingsGatewayBitConverter byteConverter, byte[] buffer, int index, DataTypeEnum dataType)
    {
        switch (dataType)
        {
            case DataTypeEnum.Boolean:
                return byteConverter.ArrayLength > 1 ?
                byteConverter.ToBoolean(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToBoolean(buffer, index);

            case DataTypeEnum.Byte:
                return
                byteConverter.ArrayLength > 1 ?
                byteConverter.ToByte(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToByte(buffer, index);

            case DataTypeEnum.Int16:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToInt16(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToInt16(buffer, index);

            case DataTypeEnum.UInt16:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToUInt16(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToUInt16(buffer, index);

            case DataTypeEnum.Int32:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToInt32(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToInt32(buffer, index);

            case DataTypeEnum.UInt32:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToUInt32(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToUInt32(buffer, index);

            case DataTypeEnum.Int64:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToInt64(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToInt64(buffer, index);

            case DataTypeEnum.UInt64:
                return
                byteConverter.ArrayLength > 1 ?
                byteConverter.ToUInt64(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToUInt64(buffer, index);

            case DataTypeEnum.Single:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToSingle(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToSingle(buffer, index);

            case DataTypeEnum.Double:
                return
                 byteConverter.ArrayLength > 1 ?
                byteConverter.ToDouble(buffer, index, byteConverter.ArrayLength.Value) :
                byteConverter.ToDouble(buffer, index);

            case DataTypeEnum.String:
            default:
                if (byteConverter.ArrayLength > 1)
                {
                    List<String> strings = new();
                    for (int i = 0; i < byteConverter.ArrayLength; i++)
                    {
                        var data = byteConverter.ToString(buffer, index + i * byteConverter.StringLength ?? 1, byteConverter.StringLength ?? 1);
                        strings.Add(data);
                    }
                    return strings.ToArray();
                }
                else
                {
                    return byteConverter.ToString(buffer, index, byteConverter.StringLength ?? 1);
                }
        }
    }

    /// <summary>
    /// 根据数据类型获取字节数组
    /// </summary>
    public static byte[] GetBytesFormData(this IThingsGatewayBitConverter byteConverter, JToken value, DataTypeEnum dataType)
    {
        if (byteConverter.ArrayLength > 1)
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(value.ToObject<Boolean[]>());

                case DataTypeEnum.Byte:
                    return value.ToObject<Byte[]>();

                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(value.ToObject<Int16[]>());

                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(value.ToObject<UInt16[]>());

                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(value.ToObject<Int32[]>());

                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(value.ToObject<UInt32[]>());

                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(value.ToObject<Int64[]>());

                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(value.ToObject<UInt64[]>());

                case DataTypeEnum.Single:
                    return byteConverter.GetBytes(value.ToObject<Single[]>());

                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(value.ToObject<Double[]>());

                case DataTypeEnum.String:
                default:
                    List<byte> bytes = new();
                    String[] strings = value.ToObject<String[]>();
                    for (int i = 0; i < byteConverter.ArrayLength; i++)
                    {
                        var data = byteConverter.GetBytes(strings[i]);
                        bytes.AddRange(data);
                    }
                    return bytes.ToArray();
            }
        }
        else
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(value.ToObject<Boolean>());

                case DataTypeEnum.Byte:
                    return byteConverter.GetBytes(value.ToObject<Byte>());

                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(value.ToObject<Int16>());

                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(value.ToObject<UInt16>());

                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(value.ToObject<Int32>());

                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(value.ToObject<UInt32>());

                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(value.ToObject<Int64>());

                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(value.ToObject<UInt64>());

                case DataTypeEnum.Single:
                    return byteConverter.GetBytes(value.ToObject<Single>());

                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(value.ToObject<Double>());

                case DataTypeEnum.String:
                default:
                    return byteConverter.GetBytes(value.ToObject<String>());
            }
        }
    }

    #endregion 获取对应数据类型的数据
}
