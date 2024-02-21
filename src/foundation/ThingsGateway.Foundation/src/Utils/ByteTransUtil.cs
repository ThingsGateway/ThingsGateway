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

/// <summary>
/// 数据转换方法
/// </summary>
public class ByteTransUtil
{
    /// <summary>
    /// 转换对应类型
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <param name="translator"></param>
    /// <returns></returns>
    public static OperResult<TResult> GetResultFromBytes<TResult>(OperResult<byte[]> result, Func<byte[]?, TResult> translator)
    {
        try
        {
            return result.IsSuccess ? new() { Content = translator(result.Content) } : new OperResult<TResult>(result);
        }
        catch (Exception ex)
        {
            return new OperResult<TResult>(string.Format(FoundationConst.TransBytesError, result.Content?.ToHexString(), result.Content?.Length), ex);
        }
    }

    /// <summary>
    ///设备地址可以带有的额外信息包含：<br></br>
    /// dataFormat=XX;4字节数据解析规则<br></br>
    /// encoding=XX;字符串解析规则<br></br>
    /// len=XX;读写长度<br></br>
    /// bcdFormat=XX; bcd解析规则<br></br>
    ///<br></br>
    /// 以上地址信息以英文分号分割，此方法解析地址的附加参数方法，获取<see cref="IThingsGatewayBitConverter"/>，并去掉address中的全部额外信息<br></br>
    /// 方法中对应解析步骤会进行缓存
    /// </summary>
    public static IThingsGatewayBitConverter GetTransByAddress(ref string registerAddress, IThingsGatewayBitConverter defaultBitConverter)
    {
        if (registerAddress.IsNullOrEmpty()) return defaultBitConverter;

        // 尝试从缓存中获取解析结果
        var cacheKey = $"{nameof(ByteTransUtil)}_{nameof(GetTransByAddress)}_{defaultBitConverter.GetType().FullName}_{defaultBitConverter.ToJsonString()}_{registerAddress}";
        if (Cache.Default.TryGetValue(cacheKey, out IThingsGatewayBitConverter cachedConverter))
        {
            return (IThingsGatewayBitConverter)cachedConverter!.ToJsonString().FromJsonString(defaultBitConverter.GetType());
        }

        registerAddress = registerAddress.Trim();

        var strs = registerAddress.SplitStringBySemicolon();

        DataFormatEnum? dataFormat = null;
        Encoding? encoding = null;
        int? length = null;
        int? stringlength = null;
        BcdFormatEnum? bcdFormat = null;
        StringBuilder sb = new();
        foreach (var str in strs)
        {
            if (str.ToLower().StartsWith("data="))
            {
                var dataFormatName = str.Substring(5);
                try { if (Enum.TryParse<DataFormatEnum>(dataFormatName, true, out var dataFormat1)) dataFormat = dataFormat1; } catch { }
            }
            else if (str.ToLower().StartsWith("encoding="))
            {
                var encodingName = str.Substring(9);
                try { encoding = Encoding.GetEncoding(encodingName); } catch { }
            }
            else if (str.ToLower().StartsWith("len="))
            {
                var lenStr = str.Substring(4);
                stringlength = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            else if (str.ToLower().StartsWith("arraylen="))
            {
                var lenStr = str.Substring(9);
                length = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            else if (str.ToLower().StartsWith("bcd="))
            {
                var bcdName = str.Substring(4);
                try { if (Enum.TryParse<BcdFormatEnum>(bcdName, true, out var bcdFormat1)) bcdFormat = bcdFormat1; } catch { }
            }
            else
            {
                if (sb.Length > 0)
                    sb.Append($";{str}");
                else
                    sb.Append($"{str}");
            }
        }

        registerAddress = sb.ToString();

        if (bcdFormat == null && length == null && stringlength == null && encoding == null && dataFormat == null)
        {
            return defaultBitConverter;
        }

        var converter = (IThingsGatewayBitConverter)defaultBitConverter!.ToJsonString().FromJsonString(defaultBitConverter.GetType());

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

        // 将解析结果添加到缓存中
        Cache.Default.Set(cacheKey, (IThingsGatewayBitConverter)converter!.ToJsonString().FromJsonString(defaultBitConverter.GetType()), 3600);
        return converter;
    }
}