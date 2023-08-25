﻿#region copyright
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

using ThingsGateway.Foundation.Extension.Byte;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.DLT645;
/// <summary>
/// DLT645_2007
/// </summary>
public class DLT645_2007BitConverter : ThingsGatewayBitConverter
{
    /// <summary>
    /// DLT645_2007
    /// </summary>
    public DLT645_2007BitConverter(EndianType endianType) : base(endianType)
    {
    }

    /// <summary>
    /// DLT645协议转换double
    /// </summary>
    /// <param name="buffer">带数据项标识</param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public override double ToDouble(byte[] buffer, int offset)
    {
        return Convert.ToDouble(this.ToString(buffer, offset, buffer.Length));
    }


    /// <inheritdoc/>
    public override string ToString(byte[] buffer)
    {
        return this.ToString(buffer, 0, buffer.Length);
    }

    /// <inheritdoc/>
    public override string ToString(byte[] buffer, int offset, int length)
    {
        buffer = buffer.SelectMiddle(offset, length);
        buffer = buffer.BytesAdd(-0x33);
        var dataInfos = DLT645Helper.GetDataInfos(buffer);
        StringBuilder stringBuilder = new();
        foreach (var dataInfo in dataInfos)
        {
            //实际数据
            var content = buffer.SelectMiddle(4, dataInfo.ByteLength).Reverse().ToArray();
            if (dataInfo.IsSigned)//可能为负数
            {
                if (content[0] > 0x80)//最高位是表示正负
                {
                    content[0] = (byte)(content[0] - 0x80);
                    if (dataInfo.Digtal == 0)//无小数点
                    {
                        stringBuilder.AppendLine($"-{content.ToHexString()}");
                    }
                    else
                    {
                        stringBuilder.AppendLine((-(Convert.ToDouble(content.ToHexString()) / Math.Pow(10.0, dataInfo.Digtal))).ToString());
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
        }

        return stringBuilder.ToString();

        static void ToString(StringBuilder stringBuilder, DataInfo dataInfo, byte[] content)
        {
            if (dataInfo.Digtal < 0)
            {
                stringBuilder.AppendLine($"{Encoding.ASCII.GetString(content)}");
            }
            else if (dataInfo.Digtal == 0)//无小数点
            {
                stringBuilder.AppendLine($"{content.ToHexString()}");
            }
            else
            {
                stringBuilder.AppendLine(((Convert.ToDouble(content.ToHexString()) / Math.Pow(10.0, dataInfo.Digtal))).ToString());
            }
        }
    }


}
