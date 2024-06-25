//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Drawing;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class Dlt645_2007Message : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override int HeaderLength { get; set; } = 10;

    public Dlt645_2007Address? Request { get; set; }
    public Dlt645_2007Send? Dlt645_2007Send { get; set; }

    public Dlt645_2007Response Response { get; set; } = new();

    public override void SendInfo(ISendMessage sendMessage)
    {
        Dlt645_2007Send = (sendMessage as Dlt645_2007Send);
        Request = Dlt645_2007Send.Dlt645_2007Address;
    }

    private int HeadCodeIndex;

    /// <inheritdoc/>
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        if (Request != null)
        {
            //因为设备可能带有FE前导符开头，这里找到0x68的位置

            if (byteBlock != null)
            {
                for (int index = byteBlock.Position; index < byteBlock.Length; index++)
                {
                    if (byteBlock[index] == 0x68)
                    {
                        HeadCodeIndex = index;
                        break;
                    }
                }
            }

            //帧起始符 地址域  帧起始符 控制码 数据域长度共10个字节
            HeaderLength = HeadCodeIndex - byteBlock.Position + 10;
            BodyLength = byteBlock[HeaderLength - 9] + 2;
            return true;
        }
        else
        {
            return false;//不是主动请求的，可能是心跳DTU包，直接放弃
        }
    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        int sendHeadCodeIndex = Dlt645_2007Send.SendHeadCodeIndex;

        var pos = byteBlock.Position - HeaderLength;
        if (response[len + HeadCodeIndex - 1] == 0x16)
        {
            //检查校验码
            int sumCheck = 0;
            for (int i = headCodeIndex; i < len + headCodeIndex - 2; i++)
                sumCheck += response[i];
            if ((byte)sumCheck != response[len + headCodeIndex - 2])
            {
                //校验错误
                request.ErrorMessage = DltResource.Localizer["SumError"];
                request.OperCode = 999;
                return new AdapterResult() { FilterResult = FilterResult.Success };
            }

            if (
                (response[headCodeIndex + 1] != send[sendHeadCodeIndex + 1]) ||
                (response[headCodeIndex + 2] != send[sendHeadCodeIndex + 2]) ||
                (response[headCodeIndex + 3] != send[sendHeadCodeIndex + 3]) ||
                (response[headCodeIndex + 4] != send[sendHeadCodeIndex + 4]) ||
                (response[headCodeIndex + 5] != send[sendHeadCodeIndex + 5]) ||
                (response[headCodeIndex + 6] != send[sendHeadCodeIndex + 6])
                )//设备地址不符合时，返回错误
            {
                if (
                (send[sendHeadCodeIndex + 1] == 0xAA) &&
                (send[sendHeadCodeIndex + 2] == 0xAA) &&
                (send[sendHeadCodeIndex + 3] == 0xAA) &&
                (send[sendHeadCodeIndex + 4] == 0xAA) &&
                (send[sendHeadCodeIndex + 5] == 0xAA) &&
                (send[sendHeadCodeIndex + 6] == 0xAA)
                )//读写通讯地址例外
                {
                }
                else
                {
                    request.ErrorMessage = DltResource.Localizer["StationNotSame"];
                    request.OperCode = 999;
                    return new AdapterResult() { FilterResult = FilterResult.Success };
                }
            }

            if ((response[headCodeIndex + 8] & 0x40) == 0x40)//控制码bit6为1时，返回错误
            {
                byte byte1 = (byte)(response[headCodeIndex + 10] - 0x33);
                var error = Dlt645Helper.Get2007ErrorMessage(byte1);
                request.ErrorMessage = DltResource.Localizer["FunctionError", $"0x{response[headCodeIndex + 8]:X2}", error];
                request.OperCode = 999;
                return new AdapterResult() { FilterResult = FilterResult.Success };
            }
            if ((response[headCodeIndex + 8] != send[sendHeadCodeIndex + 8] + 0x80))//控制码不符合时，返回错误
            {
                request.ErrorMessage =
                     DltResource.Localizer["FunctionNotSame", $"0x{response[headCodeIndex + 8]:X2}", $"0x{send[sendHeadCodeIndex + 8]:X2}"];
                request.OperCode = 999;
                return new AdapterResult() { FilterResult = FilterResult.Success };
            }
            if (send[sendHeadCodeIndex + 8] == (byte)ControlCode.Read ||
    send[sendHeadCodeIndex + 8] == (byte)ControlCode.Write
    )
            {
                //数据标识不符合时，返回错误
                if (
                (response[headCodeIndex + 10] == send[sendHeadCodeIndex + 10]) &&
                (response[headCodeIndex + 11] == send[sendHeadCodeIndex + 11]) &&
                (response[headCodeIndex + 12] == send[sendHeadCodeIndex + 12]) &&
                (response[headCodeIndex + 13] == send[sendHeadCodeIndex + 13])
                )
                {
                }
                else
                {
                    request.ErrorMessage = DltResource.Localizer["DataIdNotSame"];
                    request.OperCode = 999;
                    return new AdapterResult() { FilterResult = FilterResult.Success };
                }
            }

            request.OperCode = 0;
            return new AdapterResult()
            {
                Content = response.ToArray(headCodeIndex + 10, len - 12),
                FilterResult = FilterResult.Success
            };
        }
        else
        {
            request.OperCode = 999;
            return new AdapterResult() { FilterResult = FilterResult.Success };
        }
        return FilterResult.GoOn;
    }
}
