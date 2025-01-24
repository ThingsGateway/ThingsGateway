//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class S7Message : MessageBase, IResultMessage
{
    /// <summary>
    /// 错误码
    /// </summary>
    public byte? Error { get; set; }

    /// <inheritdoc/>
    public override int HeaderLength => 4;

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.Position += 2;
        BodyLength = byteBlock.ReadUInt16(EndianType.Big) - 4;
        return true;
    }
    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.Position;
        if (byteBlock[pos + 1] == 0xD0) // 首次握手0XD0连接确认
        {
            OperCode = 0;
            return FilterResult.Success;
        }
        else if (byteBlock[pos + 15] == 0xF0) // PDU
        {
            // 其余情况判断错误代码
            if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                return FilterResult.Success;
            }
            else
            {
                Content = byteBlock.ToArray(byteBlock.Length - 2, 2);
                OperCode = 0;
                return FilterResult.Success;
            }
        }

        //分bit/byte解析
        else if (byteBlock[pos + 15] == 0x04) // Read
        {
            int length = byteBlock[pos + 17];
            int itemLen = byteBlock[pos + 16];

            //添加错误代码校验
            // 其余情况判断错误代码
            if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                return FilterResult.Success;
            }
            else
            {
                if (byteBlock.Length < pos + 18)
                {
                    OperCode = 999;
                    ErrorMessage = SiemensS7Resource.Localizer["DataLengthError"];
                    return FilterResult.Success;
                }
                if (byteBlock[pos + 17] != byte.MaxValue)
                {
                    OperCode = 999;
                    ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[pos + 17], SiemensHelper.GetCpuError(byteBlock[pos + 17])];
                    return FilterResult.Success;
                }

                using ValueByteBlock data = new(length);
                var dataIndex = pos + 17;
                for (int index = 0; index < itemLen; index++)
                {
                    if (byteBlock[dataIndex] != byte.MaxValue)
                    {
                        OperCode = 999;
                        ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[dataIndex], SiemensHelper.GetCpuError(byteBlock[dataIndex])];
                        return FilterResult.Success;
                    }

                    if (byteBlock[dataIndex + 1] == 4)//Bit:3;Byte:4;Counter或者Timer:9
                    {
                        byteBlock.Position = dataIndex + 2;
                        var byteLength = byteBlock.ReadUInt16(EndianType.Big) / 8;
                        data.Write(byteBlock.Span.Slice(dataIndex + 4, byteLength));
                        dataIndex += byteLength + 4;
                    }
                    else if (byteBlock[dataIndex + 1] == 9)//Counter或者Timer:9
                    {
                        byteBlock.Position = dataIndex + 2;
                        var byteLength = byteBlock.ReadUInt16(EndianType.Big);
                        if (byteLength % 3 == 0)
                        {
                            for (int indexCT = 0; indexCT < byteLength / 3; indexCT++)
                            {
                                data.Write(byteBlock.Span.Slice(dataIndex + 5 + (3 * indexCT), 2));
                            }
                        }
                        else
                        {
                            for (int indexCT = 0; indexCT < byteLength / 5; indexCT++)
                            {
                                data.Write(byteBlock.Span.Slice(dataIndex + 7 + (5 * indexCT), 2));
                            }
                        }
                        dataIndex += byteLength + 4;
                    }
                }

                OperCode = 0;
                Content = data.ToArray();
                return FilterResult.Success;
            }
        }
        else if (byteBlock[pos + 15] == 0x05) // Write
        {
            int itemLen = byteBlock[pos + 16];
            if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                return FilterResult.Success;
            }
            if (byteBlock.Length < pos + 18)
            {
                OperCode = 999;
                ErrorMessage = SiemensS7Resource.Localizer["DataLengthError"];
                return FilterResult.Success;
            }
            for (int i = 0; i < itemLen; i++)
            {
                if (byteBlock[pos + 17 + i] != byte.MaxValue)
                {
                    OperCode = 999;
                    ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[pos + 17 + i], SiemensHelper.GetCpuError(byteBlock[pos + 17 + i])];
                    return FilterResult.Success;
                }
            }

            {
                OperCode = 0;
                return FilterResult.Success;
            }
        }

        OperCode = 999;
        ErrorMessage = "Unsupport function code";
        return FilterResult.Success;
    }


}
