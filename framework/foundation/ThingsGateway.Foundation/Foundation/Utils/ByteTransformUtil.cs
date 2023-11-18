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

using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// 所有数据转换类的静态辅助方法
/// </summary>
public class ByteTransformUtil
{
    /// <summary>
    /// 转换对应类型
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <param name="translator"></param>
    /// <returns></returns>
    public static OperResult<TResult> GetResultFromBytes<TResult>(OperResult<byte[]> result, Func<byte[], TResult> translator)
    {
        try
        {
            return result.IsSuccess ? OperResult.CreateSuccessResult(translator(result.Content)) : new OperResult<TResult>(result);
        }
        catch (Exception ex)
        {
            return new OperResult<TResult>(string.Format("{0} {1} : Length({2}) {3}", "转换失败", result.Content.ToHexString(), result.Content.Length, ex));
        }
    }

    /// <summary>
    ///设备地址可以带有的额外信息包含：
    /// DATA=XX;
    /// TEXT=XX;
    /// BCD=XX;
    /// LEN=XX;
    ///<br></br>
    /// 解析地址的附加参数方法,获取<see cref="IThingsGatewayBitConverter"/>，并去掉address中的全部额外信息
    /// </summary>
    public static IThingsGatewayBitConverter GetTransByAddress(ref string address, IThingsGatewayBitConverter defaultTransform)
    {
        if (address.IsNullOrEmpty()) return defaultTransform;
        address = address.Trim().ToUpper();


        var strs = address.SplitStringBySemicolon();

        DataFormat? dataFormat = null;
        Encoding encoding = null;
        int? length = null;
        BcdFormat? bcdFormat = null;
        StringBuilder sb = new();
        foreach (var str in strs)
        {
            if (str.StartsWith("DATA="))
            {
                var dataFormatName = str.Remove(5);
                try { if (Enum.TryParse<DataFormat>(dataFormatName, out var dataFormat1)) dataFormat = dataFormat1; } catch { }
            }
            else if (str.StartsWith("TEXT="))
            {
                var encodingName = str.Remove(5);
                try { encoding = Encoding.GetEncoding(encodingName); } catch { }
            }
            else if (str.StartsWith("LEN="))
            {
                var lenStr = str.Substring(4);
                length = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            else if (str.StartsWith("BCD="))
            {
                var bcdName = str.Remove(4);
                try { if (Enum.TryParse<BcdFormat>(bcdName, out var bcdFormat1)) bcdFormat = bcdFormat1; } catch { }
            }
            else
            {
                if (sb.Length > 0)
                    sb.Append($";{str}");
                else
                    sb.Append($"{str}");
            }
        }

        address = sb.ToString();

        if (bcdFormat == null && length == null && encoding == null && dataFormat == null)
        {
            return defaultTransform;
        }

        var converter = defaultTransform.CopyNew();

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
            converter.Length = length.Value;
        }
        if (dataFormat != null)
        {
            converter.DataFormat = dataFormat.Value;
        }
        return converter;
    }


}