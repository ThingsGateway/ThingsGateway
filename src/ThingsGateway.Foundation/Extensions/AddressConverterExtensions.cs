
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using NewLife.Caching;

using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

public static class AddressConverterExtensions
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
        var cacheKey = $"{nameof(AddressConverterExtensions)}_{nameof(GetTransByAddress)}_{type.FullName}_{defaultBitConverter.ToJsonString()}_{registerAddress}";
        if (Cache.Default.TryGetValue(cacheKey, out IThingsGatewayBitConverter cachedConverter))
        {
            return (IThingsGatewayBitConverter)cachedConverter!.ToJsonString().FromJsonString(type);
        }

        // 去除设备地址两端的空格
        registerAddress = registerAddress.Trim();

        // 根据分号拆分附加信息
        var strs = registerAddress.SplitStringBySemicolon();

        DataFormatEnum? dataFormat = null;
        Encoding? encoding = null;
        int? length = null;
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
        if (bcdFormat == null && length == null && stringlength == null && encoding == null && dataFormat == null)
        {
            return defaultBitConverter;
        }

        // 根据默认的数据转换器创建新的数据转换器实例
        var converter = (IThingsGatewayBitConverter)defaultBitConverter!.ToJsonString().FromJsonString(type);

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
        if (stringlength != null)
        {
            converter.StringLength = stringlength.Value;
        }
        if (dataFormat != null)
        {
            converter.DataFormat = dataFormat.Value;
        }

        // 将解析结果添加到缓存中，缓存有效期为3600秒
        Cache.Default.Set(cacheKey, (IThingsGatewayBitConverter)converter!.ToJsonString().FromJsonString(type), 3600);
        return converter;
    }
}