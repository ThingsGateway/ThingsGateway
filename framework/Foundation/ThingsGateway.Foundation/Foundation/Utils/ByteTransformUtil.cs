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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// 所有数据转换类的静态辅助方法
/// </summary>
public class ByteTransformUtil
{
    /// <inheritdoc/>
    public static OperResult<TResult> GetResultFromBytes<TResult>(OperResult<byte[]> result, Func<byte[], TResult> translator)
    {
        try
        {
            return result.IsSuccess ? OperResult.CreateSuccessResult(translator(result.Content)) : new OperResult<TResult>(result);
        }
        catch (Exception ex)
        {
            return new OperResult<TResult>(string.Format("{0} {1} : Length({2}) {3}", "转换失败", result.Content.ToHexString(), result.Content.Length, ex.Message));
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

        var strs = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

        DataFormat? dataFormat = null;
        Encoding encoding = null;
        int? length = null;
        BcdFormat? bcdFormat = null;
        StringBuilder sb = new();
        foreach (var str in strs)
        {
            if (str.StartsWith("DATA="))
            {
                Dictionary<string, DataFormat> formatMappings = new Dictionary<string, DataFormat>
    {
        { "DATA=ABCD", DataFormat.ABCD },
        { "DATA=BADC", DataFormat.BADC },
        { "DATA=DCBA", DataFormat.DCBA },
        { "DATA=CDAB", DataFormat.CDAB }
    };
                if (formatMappings.TryGetValue(str, out var data))
                    dataFormat = data;
            }
            else if (str.StartsWith("TEXT="))
            {
                var encodingMappings = new Dictionary<string, Encoding>
    {
        { "TEXT=UTF8", Encoding.UTF8 },
        { "TEXT=ASCII", Encoding.ASCII },
        { "TEXT=Default", Encoding.Default },
        { "TEXT=Unicode", Encoding.Unicode }
    };
                if (encodingMappings.TryGetValue(str, out var enc))
                    encoding = enc;
            }
            else if (str.StartsWith("LEN="))
            {
                var lenStr = str.Substring(4);
                length = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            else if (str.StartsWith("BCD="))
            {
                var bcdStr = str.Substring(4);

                var bcdMappings = new Dictionary<string, BcdFormat>
            {
                { "C8421", BcdFormat.C8421 },
                { "C2421", BcdFormat.C2421 },
                { "C3", BcdFormat.C3 },
                { "C5421", BcdFormat.C5421 },
                { "Gray", BcdFormat.Gray }
            };

                if (bcdMappings.TryGetValue(bcdStr, out var bcd))
                {
                    bcdFormat = bcd;
                }
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