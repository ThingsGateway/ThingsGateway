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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007
/// </summary>
public class Dlt645_2007BitConverter : ThingsGatewayBitConverter
{
    public Dlt645_2007BitConverter()
    {
    }

    public Dlt645_2007BitConverter(EndianType endianType) : base(endianType)
    {
    }

    public override IThingsGatewayBitConverter GetByDataFormat(DataFormatEnum dataFormat)
    {
        var data = new Dlt645_2007BitConverter(EndianType);
        data.Encoding = Encoding;
        data.DataFormat = dataFormat;
        data.BcdFormat = BcdFormat;
        data.StringLength = StringLength;
        data.ArrayLength = ArrayLength;
        data.IsStringReverseByteWord = IsStringReverseByteWord;

        return data;
    }

    /// <summary>
    /// Dlt645协议转换double
    /// </summary>
    /// <param name="buffer">带数据项标识</param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public override double ToDouble(byte[] buffer, int offset)
    {
        return Convert.ToDouble(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override short ToInt16(byte[] buffer, int offset)
    {
        return Convert.ToInt16(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override int ToInt32(byte[] buffer, int offset)
    {
        return Convert.ToInt32(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override long ToInt64(byte[] buffer, int offset)
    {
        return Convert.ToInt64(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override float ToSingle(byte[] buffer, int offset)
    {
        return Convert.ToSingle(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override string ToString(byte[] buffer, int offset, int length)
    {
        var data = new ReadOnlySpan<byte>(buffer, offset, buffer.Length - offset).BytesAdd(-0x33);
        var dataInfos = Dlt645Helper.GetDataInfos(data);
        StringBuilder stringBuilder = new();
        int index = 0;
        foreach (var dataInfo in dataInfos)
        {
            //实际数据
            var content = data.Slice(4 + index, dataInfo.ByteLength).ToArray().Reverse().ToArray();
            if (dataInfo.IsSigned)//可能为负数
            {
                if (content[0] > 0x80)//最高位是表示正负
                {
                    content[0] = (byte)(content[0] - 0x80);
                    if (dataInfo.Digtal == 0)//无小数点
                    {
                        stringBuilder.Append($"{(index != 0 ? "," : "")}-{content.ToHexString()}");
                    }
                    else
                    {
                        stringBuilder.Append($"{(index != 0 ? "," : "")}{(-(Convert.ToDouble(content.ToHexString()) / Math.Pow(10.0, dataInfo.Digtal))).ToString()}");
                    }
                }
                else
                {
                    ToString(stringBuilder, dataInfo, content);
                }
            }
            else
            {
                ToString(stringBuilder, dataInfo, content);
            }
            index += dataInfo.ByteLength;
        }

        return stringBuilder.ToString();

        void ToString(StringBuilder stringBuilder, Dlt645DataInfo dataInfo, byte[] content)
        {
            if (dataInfo.Digtal < 0)
            {
                stringBuilder.Append($"{(index != 0 ? "," : "")}{Encoding.ASCII.GetString(content)}");
            }
            else if (dataInfo.Digtal == 0)//无小数点
            {
                stringBuilder.Append($"{(index != 0 ? "," : "")}{content.ToHexString()}");
            }
            else
            {
                stringBuilder.Append($"{(index != 0 ? "," : "")}{((Convert.ToDouble(content.ToHexString()) / Math.Pow(10.0, dataInfo.Digtal))).ToString()}");
            }
        }
    }

    /// <inheritdoc/>
    public override ushort ToUInt16(byte[] buffer, int offset)
    {
        return Convert.ToUInt16(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override uint ToUInt32(byte[] buffer, int offset)
    {
        return Convert.ToUInt32(ToString(buffer, offset, buffer.Length));
    }

    /// <inheritdoc/>
    public override ulong ToUInt64(byte[] buffer, int offset)
    {
        return Convert.ToUInt64(ToString(buffer, offset, buffer.Length));
    }
}
