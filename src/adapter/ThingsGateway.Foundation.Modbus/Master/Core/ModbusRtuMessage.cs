//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRtuMessage : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override int HeaderLength => 3;

    public ModbusAddress? Request { get; set; }

    public ModbusResponse Response { get; set; } = new();

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        if (Response.ErrorCode != null)
        {
            if (Request != null)
            {
                if (Request.Station == Response.Station)
                {
                    return FilterResult.Success;
                }
                else
                {
                    return FilterResult.GoOn;
                }
            }
        }

        var pos = byteBlock.Position - HeaderLength;
        var crcLen = 0;

        if (Response.FunctionCode <= 4)
        {
            OperCode = 0;
            Content = byteBlock.ToArrayTake(BodyLength - 2);
            Response.Data = Content;
            crcLen = 3 + Response.Length;
        }
        else if (Response.FunctionCode == 5 || Response.FunctionCode == 6)
        {
            byteBlock.Position = HeaderLength - 1;
            Response.StartAddress = byteBlock.ReadUInt16(EndianType.Big);
            OperCode = 0;
            Content = byteBlock.ToArrayTake(BodyLength - 4);
            Response.Data = Content;
            crcLen = 6;
        }
        else if (Response.FunctionCode == 15 || Response.FunctionCode == 16)
        {
            byteBlock.Position = HeaderLength - 1;
            Response.StartAddress = byteBlock.ReadUInt16(EndianType.Big);
            Response.Length = byteBlock.ReadUInt16(EndianType.Big);
            OperCode = 0;
            Content = Array.Empty<byte>();
            crcLen = 6;
        }
        else
        {
            OperCode = 999;
            ErrorMessage = ModbusResource.Localizer["ModbusError1"];
            return FilterResult.GoOn;
        }
        if (crcLen > 0)
        {
            var crc = CRC16Utils.Crc16Only(byteBlock.Span.Slice(pos, crcLen));

            //Crc
            var checkCrc = byteBlock.Span.Slice(pos + crcLen, 2).ToArray();
            if (crc.SequenceEqual(checkCrc))
            {
                //验证发送/返回站号与功能码
                //站号验证
                if (Request != null)
                {
                    if (Request.Station != Response.Station)
                    {
                        OperCode = 999;
                        Response.ErrorCode = 1;
                        ErrorMessage = ModbusResource.Localizer["StationNotSame", Request.Station, Response.Station];
                        return FilterResult.GoOn;
                    }
                    if (Response.FunctionCode > 4 ? Request.WriteFunctionCode != Response.FunctionCode : Request.FunctionCode != Response.FunctionCode)
                    {
                        OperCode = 999;
                        Response.ErrorCode = 1;
                        ErrorMessage = ModbusResource.Localizer["FunctionNotSame", Request.FunctionCode, Response.FunctionCode];
                        return FilterResult.GoOn;
                    }
                }
                return FilterResult.Success;
            }
        }
        return FilterResult.GoOn;
    }

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Response.Station = byteBlock.ReadByte();
        bool error = false;
        var code = byteBlock.ReadByte();
        if ((code & 0x80) == 0)
        {
            Response.FunctionCode = code;
        }
        else
        {
            code = code.SetBit(7, false);
            Response.FunctionCode = code;
            error = true;
        }

        if (error)
        {
            Response.ErrorCode = byteBlock.ReadByte();
            OperCode = 999;
            ErrorMessage = ModbusHelper.GetDescriptionByErrorCode(Response.ErrorCode.Value);
            BodyLength = 2;
            return true;
        }
        else
        {
            Response.ErrorCode = null;

            if (Response.FunctionCode == 5 || Response.FunctionCode == 6)
            {
                BodyLength = 5;
                return true;
            }
            else if (Response.FunctionCode == 15 || Response.FunctionCode == 16)
            {
                BodyLength = 5;
                return true;
            }
            else if (Response.FunctionCode <= 4)
            {
                Response.Length = byteBlock.ReadByte();
                BodyLength = Response.Length + 2; //数据区+crc
                return true;
            }
        }
        return false;
    }

    public override void SendInfo(ISendMessage sendMessage)
    {
        Request = ((ModbusRtuSend)sendMessage).ModbusAddress;
    }
}
