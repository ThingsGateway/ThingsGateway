//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife.Collections;

namespace ThingsGateway.Foundation;

public class ConverterConfig
{
    public virtual DataFormatEnum? DataFormat { get; set; }
    public virtual Encoding? Encoding { get; set; }
    public virtual int? EncodingName
    {
        get
        {
            return Encoding?.CodePage;
        }
        set
        {
            try
            {
                if (value != null)
                    Encoding = Encoding.GetEncoding(value.Value);
                else
                    Encoding = null;
            }
            catch { Encoding = null; }
        }
    }
    public virtual bool? VariableStringLength { get; set; }
    public virtual int? Stringlength { get; set; }
    public virtual BcdFormatEnum? BcdFormat { get; set; }

    public ConverterConfig(string value)
    {
        if (value.IsNullOrEmpty()) return;
        // 去除设备地址两端的空格
        value = value.Trim();

        // 根据分号拆分附加信息
        var strs = value.SplitStringBySemicolon();
        DataFormatEnum? dataFormat = null;
        Encoding? encoding = null;
        bool? wstring = null;
        int? stringlength = null;
        BcdFormatEnum? bcdFormat = null;
        foreach (var str in strs)
        {
            // 解析 dataFormat
            if (str.StartsWith("data=", StringComparison.OrdinalIgnoreCase))
            {
                var dataFormatName = str.Substring(5);
                try { if (Enum.TryParse<DataFormatEnum>(dataFormatName, true, out var dataFormat1)) dataFormat = dataFormat1; } catch { }
            }
            else if (str.StartsWith("vsl=", StringComparison.OrdinalIgnoreCase))
            {
                var wstringName = str.Substring(4);
                try { if (bool.TryParse(wstringName, out var wstring1)) wstring = wstring1; } catch { }
            }
            // 解析 encoding
            else if (str.StartsWith("encoding=", StringComparison.OrdinalIgnoreCase))
            {
                var encodingName = str.Substring(9);
                try { encoding = Encoding.GetEncoding(encodingName); } catch { }
            }
            // 解析 length
            else if (str.StartsWith("len=", StringComparison.OrdinalIgnoreCase))
            {
                var lenStr = str.Substring(4);
                stringlength = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            // 解析 bcdFormat
            else if (str.StartsWith("bcd=", StringComparison.OrdinalIgnoreCase))
            {
                var bcdName = str.Substring(4);
                try { if (Enum.TryParse<BcdFormatEnum>(bcdName, true, out var bcdFormat1)) bcdFormat = bcdFormat1; } catch { }
            }
        }

        DataFormat = dataFormat;
        Encoding = encoding;
        VariableStringLength = wstring;
        Stringlength = stringlength;
        BcdFormat = bcdFormat;


    }
    public override string ToString()
    {
        StringBuilder stringBuilder = Pool.StringBuilder.Get();
        if (DataFormat != null)
        {
            stringBuilder.Append("format=");
            stringBuilder.Append(DataFormat.ToString());
            stringBuilder.Append(';');
        }
        if (Encoding != null)
        {
            stringBuilder.Append("encoding=");
            stringBuilder.Append(Encoding.WebName);
            stringBuilder.Append(';');
        }
        if (VariableStringLength != null)
        {
            stringBuilder.Append("vsl=");
            stringBuilder.Append(VariableStringLength.ToString());
            stringBuilder.Append(';');
        }
        if (Stringlength != null)
        {
            stringBuilder.Append("len=");
            stringBuilder.Append(Stringlength.ToString());
            stringBuilder.Append(';');
        }
        if (BcdFormat != null)
        {
            stringBuilder.Append("bcd=");
            stringBuilder.Append(BcdFormat.ToString());
            stringBuilder.Append(';');
        }
        var data = stringBuilder.ToString();

        Pool.StringBuilder.Return(stringBuilder);

        return data;
    }

}
