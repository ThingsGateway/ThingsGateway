//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class S7Response : S7Request
{
    /// <summary>
    /// 错误码
    /// </summary>
    public byte? ErrorCode { get; set; }
}

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class S7Message : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override int HeaderLength => 4;

    public S7Response Response { get; set; } = new();
    public SiemensAddress[] Request { get; set; }
    public S7Send? S7Send { get; set; }

    public override void SendInfo(ISendMessage sendMessage)
    {
        S7Send = (sendMessage as S7Send);
        Request = S7Send.SiemensAddress;
    }

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.Position += 2;
        BodyLength = byteBlock.ReadUInt16(EndianType.Big) - 4;
        return true;
    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        if (Response.ErrorCode.HasValue)
        {
            return FilterResult.Success;
        }
        var pos = byteBlock.Position;
        if (S7Send.Handshake)
        {
            if (byteBlock[pos + 1] == 0xD0) // 首次握手0XD0连接确认
            {
                this.OperCode = 0;
                return FilterResult.Success;
            }
            else
            {
                // 其余情况判断错误代码
                if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
                {
                    Response.ErrorCode = byteBlock[pos + 14];
                    this.OperCode = 999;
                    this.ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                    return FilterResult.Success;
                }
                else
                {
                    Content = byteBlock.ToArray(byteBlock.Length - 2, 2);
                    this.OperCode = 0;
                    return FilterResult.Success;
                }
            }
        }

        //分bit/byte解析
        if (S7Send.Read)
        {
            int length = 0;
            int itemLen = Request.Length;//驱动只会读取一个项

            //添加错误代码校验
            // 其余情况判断错误代码
            if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
            {
                Response.ErrorCode = byteBlock[pos + 14];
                this.OperCode = 999;
                this.ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                return FilterResult.Success;
            }
            else
            {
                if (byteBlock[pos + 16] != itemLen)
                {
                    this.OperCode = 999;
                    this.ErrorMessage = SiemensS7Resource.Localizer["DataLengthError"];
                    return FilterResult.Success;
                }

                if (byteBlock.Length < pos + 18)
                {
                    this.OperCode = 999;
                    this.ErrorMessage = SiemensS7Resource.Localizer["DataLengthError"];
                    return FilterResult.Success;
                }
                if (byteBlock[pos + 17] != byte.MaxValue)
                {
                    this.OperCode = 999;
                    this.ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[pos + 17], SiemensHelper.GetCpuError(byteBlock[pos + 17])];
                    return FilterResult.Success;
                }

                //解析读取字节数组
                for (int index = 0; index < itemLen; index++)
                {
                    var address = Request[index];
                    length += address.Length;
                }
                using ValueByteBlock data = new(length);
                var dataIndex = pos + 17;
                for (int index = 0; index < itemLen; index++)
                {
                    if (byteBlock[dataIndex] != byte.MaxValue)
                    {
                        this.OperCode = 999;
                        this.ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[pos + 17], SiemensHelper.GetCpuError(byteBlock[pos + 17])];
                        return FilterResult.Success;
                    }

                    var address = Request[index];
                    if (byteBlock[dataIndex + 1] == 4)//Bit:3;Byte:4;Counter或者Timer:9
                    {
                        data.Write(byteBlock.Span.Slice(dataIndex + 4, address.Length));
                        dataIndex += address.Length + 3;
                    }
                    else if (byteBlock[dataIndex + 1] == 9)//Counter或者Timer:9
                    {
                        int num = (byteBlock[dataIndex + 2] * 256) + byteBlock[dataIndex + 3];
                        if (num % 3 == 0)
                        {
                            for (int indexCT = 0; indexCT < num / 3; indexCT++)
                            {
                                data.Write(byteBlock.Span.Slice(dataIndex + 5 + (3 * indexCT), 2));
                            }
                        }
                        else
                        {
                            for (int indexCT = 0; indexCT < num / 5; indexCT++)
                            {
                                data.Write(byteBlock.Span.Slice(dataIndex + 7 + (5 * indexCT), 2));
                            }
                        }
                        dataIndex += num + 4;
                    }
                }

                this.OperCode = 0;
                Content = data.ToArray();
                return FilterResult.Success;
            }
        }
        else
        {
            if (byteBlock[pos + 13] + byteBlock[pos + 14] > 0) // 如果错误代码不为0
            {
                Response.ErrorCode = byteBlock[pos + 14];
                this.OperCode = 999;
                this.ErrorMessage = SiemensS7Resource.Localizer["ReturnError", byteBlock[pos + 13].ToString("X2"), byteBlock[pos + 14].ToString("X2")];
                return FilterResult.Success;
            }

            if (byteBlock.Length < pos + 18)
            {
                this.OperCode = 999;
                this.ErrorMessage = SiemensS7Resource.Localizer["DataLengthError"];
                return FilterResult.Success;
            }
            if (byteBlock[pos + 17] != byte.MaxValue)
            {
                this.OperCode = 999;
                this.ErrorMessage = SiemensS7Resource.Localizer["ValidateDataError", byteBlock[pos + 17], SiemensHelper.GetCpuError(byteBlock[pos + 17])];
                return FilterResult.Success;
            }
            return FilterResult.Success;
        }
    }
}
