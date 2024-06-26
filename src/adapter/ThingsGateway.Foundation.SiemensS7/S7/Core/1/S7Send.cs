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
internal class S7Send : ISendMessage
{
    public int Sign { get; set; }

    public int MaxLength => 2048;

    private SiemensAddress? SiemensAddress;
    private bool? Read;
    private bool? IsBit;
    private bool? Handshake;
    private byte[]? HandshakeBytes;

    public S7Send(SiemensAddress? siemensAddress = null, bool? read = null, bool? isBit = null, bool? handshake = null, byte[]? handshakeBytes = default)
    {
        SiemensAddress = siemensAddress;
        Read = read;
        IsBit = isBit;
        Handshake = handshake;
        HandshakeBytes = handshakeBytes;
    }

    internal void GetReadCommand<TByteBlock>(ref TByteBlock valueByteBlock, SiemensAddress[] siemensAddress) where TByteBlock : IByteBlock
    {
        byte len = (byte)siemensAddress.Length;
        ushort telegramLen = (ushort)(len * 12 + 19);
        ushort parameterLen = (ushort)(len * 12 + 2);
        //TPKT
        valueByteBlock.WriteByte(3);//版本
        valueByteBlock.WriteByte(0);
        valueByteBlock.WriteUInt16(telegramLen);//长度，item.len*12+19
        //COTP信息
        valueByteBlock.WriteByte(2);//长度
        valueByteBlock.WriteByte(0xf0);//pdu类型
        valueByteBlock.WriteByte(0x80);//目标引用
        //header
        valueByteBlock.WriteByte(0x32);//协议id
        valueByteBlock.WriteByte(0x01);//请求
        valueByteBlock.WriteUInt16(0x00);//冗余识别
        valueByteBlock.WriteUInt16(0x01);//数据引用
        valueByteBlock.WriteUInt16(parameterLen);//参数长度，item.len*12+2
        valueByteBlock.WriteUInt16(0x00);//数据长度，data.len+4 ,写入时填写，读取时为0
        valueByteBlock.WriteByte(0x04);//功能码，4 Read Var, 5 Write Var
        valueByteBlock.WriteByte(len);//Item数量

        //通信项构建
        for (int index = 0; index < len; index++)
        {
            valueByteBlock.WriteByte(0x12);//Var 规范
            valueByteBlock.WriteByte(0x0a);//剩余的字节长度
            valueByteBlock.WriteByte(0x10);//Syntax ID

            if (siemensAddress[index].DataCode == (byte)S7WordLength.Counter || siemensAddress[index].DataCode == (byte)S7WordLength.Timer)
            {
                valueByteBlock.WriteByte(siemensAddress[index].DataCode);//数据类型
            }
            else
            {
                valueByteBlock.WriteByte((byte)S7WordLength.Byte);//数据类型
            }
            valueByteBlock.WriteUInt16((ushort)siemensAddress[index].Length);//读取长度
            valueByteBlock.WriteUInt16(siemensAddress[index].DbBlock);//DB编号
            valueByteBlock.WriteByte(siemensAddress[index].DataCode);//数据块类型
            valueByteBlock.WriteByte((byte)(siemensAddress[index].AddressStart / 256 / 256 % 256));//数据块偏移量
            valueByteBlock.WriteByte((byte)(siemensAddress[index].AddressStart / 256 % 256));//数据块偏移量
            valueByteBlock.WriteByte((byte)(siemensAddress[index].AddressStart % 256));//数据块偏移量
        }
    }

    internal static void GetWriteByteCommand<TByteBlock>(ref TByteBlock valueByteBlock, SiemensAddress address, bool isBit) where TByteBlock : IByteBlock
    {
        var data = address.Data;
        byte len = (byte)data.Length;
        ushort telegramLen = (ushort)(16 + 19 + len);
        ushort parameterLen = 12 + 2;
        //TPKT
        valueByteBlock.WriteByte(3);//版本
        valueByteBlock.WriteByte(0);
        valueByteBlock.WriteUInt16(telegramLen);//长度，item.len*12+19
        //COTP信息
        valueByteBlock.WriteByte(2);//长度
        valueByteBlock.WriteByte(0xf0);//pdu类型
        valueByteBlock.WriteByte(0x80);//目标引用
        //header
        valueByteBlock.WriteByte(0x32);//协议id
        valueByteBlock.WriteByte(0x01);//请求
        valueByteBlock.WriteUInt16(0x00);//冗余识别
        valueByteBlock.WriteUInt16(0x01);//数据引用
        valueByteBlock.WriteUInt16(parameterLen);//参数长度，item.len*12+2
        valueByteBlock.WriteUInt16((ushort)(4 + len));//数据长度，data.len+4 ,写入时填写，读取时为0
        valueByteBlock.WriteByte(0x05);//功能码，4 Read Var, 5 Write Var
        valueByteBlock.WriteByte(1);//Item数量

        //写入Item与读取大致相同
        valueByteBlock.WriteByte(0x12);//Var 规范
        valueByteBlock.WriteByte(0x0a);//剩余的字节长度
        valueByteBlock.WriteByte(0x10);//Syntax ID
        valueByteBlock.WriteByte(isBit ? (byte)S7WordLength.Bit : (byte)S7WordLength.Byte);//数据类型
        valueByteBlock.WriteUInt16((ushort)len);//长度
        valueByteBlock.WriteUInt16(address.DbBlock);//DB编号
        valueByteBlock.WriteByte(address.DataCode);//数据块类型
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256 / 256));//数据块偏移量
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256));//数据块偏移量
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) % 256));//数据块偏移量

        //后面跟的是写入的数据信息
        valueByteBlock.WriteByte(0);
        valueByteBlock.WriteByte((byte)(isBit ? 3 : 4));//Bit:3;Byte:4;Counter或者Timer:9
        valueByteBlock.WriteUInt16((ushort)(isBit ? len : len * 8));
        valueByteBlock.Write(data);
    }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        if (Handshake == true)
        {
            byteBlock.Write(HandshakeBytes);
            return;
        }
        if (Read == true)
        {
            GetReadCommand(ref byteBlock, [SiemensAddress]);
        }
        else
        {
            GetWriteByteCommand(ref byteBlock, SiemensAddress, IsBit ?? false);
        }
    }
}
