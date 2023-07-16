#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Linq;

using ThingsGateway.Resources;

namespace ThingsGateway.Foundation;

/// <summary>
/// 所有数据转换类的静态辅助方法
/// </summary>
public static class ByteTransformHelpers
{
    private static MemoryCache<string, IThingsGatewayBitConverter> _cache;

    static ByteTransformHelpers()
    {
        _cache = new();
    }

    /// <inheritdoc/>
    public static OperResult<TResult> GetResultFromBytes<TResult>(
      OperResult<byte[]> result,
      Func<byte[], TResult> translator)
    {
        try
        {
            return result.IsSuccess ? OperResult.CreateSuccessResult(translator(result.Content)) : result.Copy<TResult>();
        }
        catch (Exception ex)
        {
            OperResult<TResult> resultFromBytes = new OperResult<TResult>
                (
                string.Format("{0} {1} : Length({2}) {3}", ThingsGatewayStatus.DataTransError.GetDescription(), result.Content.ToHexString(), result.Content.Length, ex.Message)
                );
            return resultFromBytes;
        }
    }

    /// <inheritdoc/>
    public static OperResult GetResultFromOther<TIn>(
              OperResult<TIn> result,
      Func<TIn, OperResult> trans)
    {
        return !result.IsSuccess ? result : trans(result.Content);
    }

    /// <inheritdoc/>
    public static OperResult<TResult> GetResultFromOther<TResult, TIn>(
      OperResult<TIn> result,
      Func<TIn, OperResult<TResult>> trans)
    {
        return !result.IsSuccess ? result.Copy<TResult>() : trans(result.Content);
    }

    /// <inheritdoc/>
    public static OperResult<TResult> GetSuccessResultFromOther<TResult, TIn>(
              OperResult<TIn> result,
      Func<TIn, TResult> trans)
    {
        return !result.IsSuccess ? result.Copy<TResult>() : OperResult.CreateSuccessResult(trans(result.Content));
    }
    /// <summary>
    ///设备地址可以带有的额外信息包含：
    /// DATA=XX;
    /// TEXT=XX;
    /// BCD=XX;
    /// LEN=XX;
    ///<br></br>
    /// 解析地址的附加<see cref="DataFormat"/> />参数方法,获取新的<see cref="IThingsGatewayBitConverter"/>，
    /// 并去掉address中的全部额外信息
    /// </summary>
    public static IThingsGatewayBitConverter GetTransByAddress(ref string address, IThingsGatewayBitConverter defaultTransform)
    {
        var hasCache = _cache.TryGetValue(address + defaultTransform.ToJson(), out IThingsGatewayBitConverter thingsGatewayBitConverter);
        if (hasCache)
        {
            return thingsGatewayBitConverter;
        }
        else
        {
            int? length = null;
            BcdFormat? bcdFormat = null;
            DataFormat? dataFormat = null;
            if (address.IsNullOrEmpty()) return defaultTransform;
            var strs = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            var format = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("DATA="))?.ToUpper();
            switch (format)
            {
                case "DATA=ABCD":
                    dataFormat = DataFormat.ABCD;
                    break;

                case "DATA=BADC":
                    dataFormat = DataFormat.BADC;
                    break;

                case "DATA=DCBA":
                    dataFormat = DataFormat.DCBA;
                    break;

                case "DATA=CDAB":
                    dataFormat = DataFormat.CDAB;
                    break;
            }

            var strencoding = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("TEXT="))?.ToUpper();
            Encoding encoding = null;
            switch (strencoding)
            {
                case "TEXT=UTF8":
                    encoding = Encoding.UTF8;
                    break;

                case "TEXT=ASCII":
                    encoding = Encoding.ASCII;
                    break;

                case "TEXT=Default":
                    encoding = Encoding.Default;
                    break;

                case "TEXT=Unicode":
                    encoding = Encoding.Unicode;
                    break;
            }

            var strlen = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("LEN="))?.ToUpper().Replace("LEN=", "");
            length = strlen.IsNullOrEmpty() ? null : Convert.ToUInt16(strlen);

            var strBCDFormat = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("BCD="))?.ToUpper();
            switch (strBCDFormat)
            {
                case "BCD=C8421":
                    bcdFormat = BcdFormat.C8421;
                    break;

                case "BCD=C2421":
                    bcdFormat = BcdFormat.C2421;
                    break;

                case "BCD=C3":
                    bcdFormat = BcdFormat.C3;
                    break;

                case "BCD=C5421":
                    bcdFormat = BcdFormat.C5421;
                    break;

                case "BCD=Gray":
                    bcdFormat = BcdFormat.Gray;
                    break;
            }

            //去除以上的额外信息
            address = String.Join(";", strs.Where(m =>
            (!m.Trim().ToUpper().Contains("DATA=")) &&
            (!m.Trim().ToUpper().Contains("TEXT=")) &&
            (!m.Trim().ToUpper().Contains("BCD=")) &&
            (!m.Trim().ToUpper().Contains("LEN="))
            ));
            IThingsGatewayBitConverter converter = defaultTransform;
            if (dataFormat != null)
            {
                converter = defaultTransform.CreateByDateFormat(dataFormat.Value);
            }
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
                converter.StringLength = length.Value;
            }
            _cache.SetCache(address + defaultTransform.ToJson(), converter);
            return converter;
        }

    }


}