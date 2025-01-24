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
public class ModbusTcpMessage : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override int HeaderLength => 8;

    public ModbusResponse Response { get; set; } = new();

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {

        if (error)
        {
            Response.ErrorCode = byteBlock.ReadByte();
            OperCode = Response.ErrorCode;
            ErrorMessage = ModbusHelper.GetDescriptionByErrorCode(Response.ErrorCode.Value);
            ErrorType = ErrorTypeEnum.DeviceError;
        }
        else
        {
            Response.ErrorCode = null;
            if (Response.FunctionCode <= 4)
            {
                Response.Length = byteBlock.ReadByte();
            }
            else
            {
                Response.StartAddress = byteBlock.ReadUInt16();
            }
        }
        if (Response.ErrorCode != null)
        {
            return FilterResult.Success;
        }

        if (Response.FunctionCode <= 4)
        {
            OperCode = 0;
            Content = byteBlock.ToArrayTake(BodyLength - 1);
            Response.Data = Content;
            return FilterResult.Success;
        }
        else if (Response.FunctionCode == 5 || Response.FunctionCode == 6)
        {
            byteBlock.Position = HeaderLength - 1;
            Response.StartAddress = byteBlock.ReadUInt16();
            OperCode = 0;
            Content = byteBlock.ToArrayTake(BodyLength - 2);
            Response.Data = Content;
            return FilterResult.Success;
        }
        else if (Response.FunctionCode == 15 || Response.FunctionCode == 16)
        {
            byteBlock.Position = HeaderLength - 1;
            Response.StartAddress = byteBlock.ReadUInt16(EndianType.Big);
            Response.Length = byteBlock.ReadUInt16(EndianType.Big);
            OperCode = 0;
            Content = Array.Empty<byte>();
            return FilterResult.Success;
        }
        else
        {
            OperCode = 999;
            ErrorMessage = ModbusResource.Localizer["ModbusError1"];
        }
        return FilterResult.GoOn;
    }
    bool error = false;
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Sign = byteBlock.ReadUInt16(EndianType.Big);
        byteBlock.Position += 2;
        BodyLength = byteBlock.ReadUInt16(EndianType.Big) - 2;
        Response.Station = byteBlock.ReadByte();
        Response.FunctionCode = byteBlock.ReadByte();
        if ((Response.FunctionCode & 0x80) == 0)
        {
            error = false;
        }
        else
        {
            Response.FunctionCode = Response.FunctionCode.SetBit(7, false);
            error = true;
        }
        return true;

    }
}
