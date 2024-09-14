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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Request
{
    #region Request

    /// <summary>
    /// 数据标识
    /// </summary>
    public byte[] DataId { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte[] Station { get; set; } = Array.Empty<byte>();

    #endregion Request
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Send : ISendMessage
{
    internal ControlCode ControlCode = default;

    /// <summary>
    /// 密码、操作码
    /// </summary>
    private byte[] Codes = default;

    /// <summary>
    /// 写入值
    /// </summary>
    private string[] Datas = default;

    private byte[] Fehead = default;

    public Dlt645_2007Send(Dlt645_2007Address dlt645_2007Address, ushort sign, ControlCode controlCode, byte[] fehead = default, byte[] codes = default, string[] datas = default)
    {
        Sign = sign;
        Dlt645_2007Address = dlt645_2007Address;
        ControlCode = controlCode;

        Fehead = fehead ?? Array.Empty<byte>();
        Codes = codes ?? Array.Empty<byte>();
        Datas = datas ?? Array.Empty<string>();
    }

    public int MaxLength => 300;
    public int SendHeadCodeIndex { get; private set; }
    public int Sign { get; set; }
    internal Dlt645_2007Address Dlt645_2007Address { get; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        if (Dlt645_2007Address?.DataId.Length < 4)
        {
            throw new(DltResource.Localizer["DataIdError"]);
        }
        if (Fehead.Length > 0)
        {
            byteBlock.Write(Fehead);//帧起始符
            SendHeadCodeIndex = Fehead.Length;
        }

        byteBlock.WriteByte(0x68);//帧起始符
        byteBlock.Write(Dlt645_2007Address.Station);//6个字节地址域
        byteBlock.WriteByte(0x68);//帧起始符
        byteBlock.WriteByte((byte)ControlCode);//控制码

        byteBlock.WriteByte((byte)(Dlt645_2007Address.DataId.Length));//数据域长度
        byteBlock.Write(Dlt645_2007Address.DataId);//数据域标识DI3、DI2、DI1、DI0

        byteBlock.Write(Codes);

        if (Datas.Length > 0)
        {
            var dataInfos = Dlt645Helper.GetDataInfos(Dlt645_2007Address.DataId);
            if (Datas.Length != dataInfos.Count)
            {
                throw new(DltResource.Localizer["CountError"]);
            }
            for (int i = 0; i < Datas.Length; i++)
            {
                var dataInfo = dataInfos[i];
                byte[] data;
                if (dataInfo.IsSigned)//可能为负数
                {
                    var doubleValue = Convert.ToDouble(Datas[i]);
                    if (dataInfo.Digtal != 0)//无小数点
                    {
                        doubleValue *= Math.Pow(10.0, dataInfo.Digtal);
                    }
                    data = doubleValue.ToString().HexStringToBytes().Reverse().ToArray();
                    if (doubleValue < 0)
                    {
                        data[0] = (byte)(data[0] & 0x80);
                    }
                }
                else
                {
                    if (dataInfo.Digtal < 0)
                    {
                        data = Encoding.ASCII.GetBytes(Datas[i]).Reverse().ToArray();
                    }
                    else if (dataInfo.Digtal == 0)//无小数点
                    {
                        data = Datas[i].HexStringToBytes().Reverse().ToArray();
                    }
                    else
                    {
                        data = (Convert.ToDouble(Datas[i]) * Math.Pow(10.0, dataInfo.Digtal)).ToString().HexStringToBytes().Reverse().ToArray();
                    }
                }

                byteBlock.Write(data);
            }
        }

        byteBlock[Fehead.Length + 9] = (byte)(byteBlock.Length - 10 - Fehead.Length);//数据域长度

        for (int index = Fehead.Length + 10; index < byteBlock.Length; ++index)
            byteBlock[index] += 0x33;//传输时发送方按字节进行加33H处理，接收方按字节进行减33H处理

        int num = 0;
        for (int index = Fehead.Length; index < byteBlock.Length; ++index)
            num += byteBlock[index];
        byteBlock.WriteByte((byte)num);//校验码,总加和
        byteBlock.WriteByte(0x16);//结束符
    }
}
