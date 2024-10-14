//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife.Caching;

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{
    /// <summary>
    /// 从设备地址中解析附加信息，包括 dataFormat=XX;4字节数据解析规则、encoding=XX;字符串解析规则、len=XX;读写长度、bcdFormat=XX; bcd解析规则等。
    /// 这个方法获取<see cref="IThingsGatewayBitConverter"/>，并去掉地址中的所有额外信息。
    /// 解析步骤将被缓存。
    /// </summary>
    /// <param name="registerAddress">设备地址</param>
    /// <param name="defaultBitConverter">默认的数据转换器</param>
    /// <returns><see cref="IThingsGatewayBitConverter"/> 实例</returns>
    public static IThingsGatewayBitConverter GetTransByAddress(this IThingsGatewayBitConverter defaultBitConverter, ref string? registerAddress)
    {
        if (registerAddress.IsNullOrEmpty()) return defaultBitConverter;

        var type = defaultBitConverter.GetType();
        // 尝试从缓存中获取解析结果
        var cacheKey = $"{nameof(ThingsGatewayBitConverterExtension)}_{nameof(GetTransByAddress)}_{type.FullName}_{defaultBitConverter.ToJsonString()}_{registerAddress}";
        if (MemoryCache.Instance.TryGetValue(cacheKey, out IThingsGatewayBitConverter cachedConverter))
        {
            return cachedConverter!;
        }

        // 去除设备地址两端的空格
        registerAddress = registerAddress.Trim();

        // 根据分号拆分附加信息
        var strs = registerAddress.SplitStringBySemicolon();

        DataFormatEnum? dataFormat = null;
        Encoding? encoding = null;
        int? length = null;
        bool? wstring = null;
        int? stringlength = null;
        BcdFormatEnum? bcdFormat = null;
        StringBuilder sb = new();
        foreach (var str in strs)
        {
            // 解析 dataFormat
            if (str.ToLower().StartsWith("data="))
            {
                var dataFormatName = str.Substring(5);
                try { if (Enum.TryParse<DataFormatEnum>(dataFormatName, true, out var dataFormat1)) dataFormat = dataFormat1; } catch { }
            }
            else if (str.ToLower().StartsWith("w="))
            {
                var wstringName = str.Substring(2);
                try { if (bool.TryParse(wstringName, out var wstring1)) wstring = wstring1; } catch { }
            }
            // 解析 encoding
            else if (str.ToLower().StartsWith("encoding="))
            {
                var encodingName = str.Substring(9);
                try { encoding = Encoding.GetEncoding(encodingName); } catch { }
            }
            // 解析 length
            else if (str.ToLower().StartsWith("len="))
            {
                var lenStr = str.Substring(4);
                stringlength = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            // 解析 array length
            else if (str.ToLower().StartsWith("arraylen="))
            {
                var lenStr = str.Substring(9);
                length = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            // 解析 bcdFormat
            else if (str.ToLower().StartsWith("bcd="))
            {
                var bcdName = str.Substring(4);
                try { if (Enum.TryParse<BcdFormatEnum>(bcdName, true, out var bcdFormat1)) bcdFormat = bcdFormat1; } catch { }
            }

            // 处理其他情况，将未识别的部分拼接回去
            else
            {
                if (sb.Length > 0)
                    sb.Append($";{str}");
                else
                    sb.Append($"{str}");
            }
        }

        // 更新设备地址为去除附加信息后的地址
        registerAddress = sb.ToString();

        // 如果没有解析出任何附加信息，则直接返回默认的数据转换器
        if (bcdFormat == null && length == null && stringlength == null && encoding == null && dataFormat == null && wstring == null)
        {
            return defaultBitConverter;
        }

        // 根据默认的数据转换器创建新的数据转换器实例
        var converter = (IThingsGatewayBitConverter)defaultBitConverter!.Map(type);

        // 更新新的数据转换器实例的属性值
        if (encoding != null)
        {
            converter.Encoding = encoding;
        }
        if (bcdFormat != null)
        {
            converter.BcdFormat = bcdFormat.Value;
        }
        if (length != null)
        {
            converter.ArrayLength = length.Value;
        }
        if (wstring != null)
        {
            converter.IsVariableStringLength = wstring.Value;
        }
        if (stringlength != null)
        {
            converter.StringLength = stringlength.Value;
        }
        if (dataFormat != null)
        {
            converter.DataFormat = dataFormat.Value;
        }

        // 将解析结果添加到缓存中，缓存有效期为3600秒
        MemoryCache.Instance.Set(cacheKey, converter!, 3600);
        return converter;
    }

    #region 获取对应数据类型的数据

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

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static object GetDataFormBytes(this IThingsGatewayBitConverter byteConverter, IProtocol protocol, string address, byte[] buffer, int index, DataTypeEnum dataType)
    {
        switch (dataType)
        {
            case DataTypeEnum.Boolean:
                return byteConverter.ArrayLength > 1 ?
                byteConverter.ToBoolean(buffer, index, byteConverter.ArrayLength.Value, protocol.BitReverse(address)) :
                byteConverter.ToBoolean(buffer, index, protocol.BitReverse(address));

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

    #endregion 获取对应数据类型的数据
}
