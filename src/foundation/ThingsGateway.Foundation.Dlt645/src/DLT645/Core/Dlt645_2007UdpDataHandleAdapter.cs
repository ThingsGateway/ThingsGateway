//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007UdpDataHandleAdapter
/// </summary>
internal class Dlt645_2007UdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<Dlt645_2007Message>
{
    /// <summary>
    /// 增加FE FE FE FE的报文头部
    /// </summary>
    [Description("前导符报文头")]
    public bool EnableFEHead { get; set; }

    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command, Dlt645_2007Message item)
    {
        //打包时加上4个FE字节
        if (EnableFEHead)
        {
            return DataTransUtil.SpliceArray(new byte[4] { 0xFE, 0xFE, 0xFE, 0xFE }, command);
        }
        return command;
    }

    /// <inheritdoc/>
    protected override Dlt645_2007Message GetInstance()
    {
        return new Dlt645_2007Message();
    }

    protected override OperResult<byte[]> UnpackResponse(Dlt645_2007Message request, byte[]? send, byte[] response)
    {
        //因为设备可能带有FE前导符开头，这里找到0x68的位置
        int headCodeIndex = -1;
        if (response != null)
        {
            for (int index = 0; index < response.Length; index++)
            {
                if (response[index] == 0x68)
                {
                    headCodeIndex = index;
                    break;
                }
            }
        }
        int sendHeadCodeIndex = 0;
        if (send != null)
        {
            for (int index = 0; index < send.Length; index++)
            {
                if (send[index] == 0x68)
                {
                    sendHeadCodeIndex = index;
                    break;
                }
            }
        }

        //帧起始符 地址域  帧起始符 控制码 数据域长度共10个字节
        if (headCodeIndex < 0 || headCodeIndex + 10 > response.Length)
            return request;

        var len = 10 + response[headCodeIndex + 9] + 2;

        if (response.Length - headCodeIndex < len)
        {
            return request;
        }
        if (response.Length - headCodeIndex >= len && response[len + headCodeIndex - 1] == 0x16)
        {
            //检查校验码
            int sumCheck = 0;
            for (int i = headCodeIndex; i < len + headCodeIndex - 2; i++)
                sumCheck += response[i];
            if ((byte)sumCheck != response[len + headCodeIndex - 2])
            {
                //校验错误
                request.ErrorMessage = "和校验错误";
                request.OperCode = 999;
                return request;
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
                    request.ErrorMessage = "返回地址不符合规则";
                    request.OperCode = 999;
                    return request;
                }
            }

            if ((response[headCodeIndex + 8] != send[sendHeadCodeIndex + 8] + 0x80))//控制码不符合时，返回错误
            {
                request.ErrorMessage = $"返回控制码：0x{response[headCodeIndex + 8]:X2}，请求控制码：0x{send[sendHeadCodeIndex + 8]:X2}，不符合规则";
                request.OperCode = 999;
                return request;
            }

            if ((response[headCodeIndex + 8] & 0x40) == 0x40)//控制码bit6为1时，返回错误
            {
                byte byte1 = (byte)(response[headCodeIndex + 10] - 0x33);
                var error = Dlt645Helper.Get2007ErrorMessage(byte1);
                request.ErrorMessage = $"异常控制码：0x{response[headCodeIndex + 8]:X2}，错误信息：{error}";
                request.OperCode = 999;
                return request;
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
                    request.ErrorMessage = "返回数据标识不符合规则";
                    request.OperCode = 999;
                    return request;
                }
            }

            request.Content = response.RemoveBegin(headCodeIndex + 10).RemoveLast(response.Length + 2 - len - headCodeIndex);
            request.OperCode = 0;
            return request;
        }
        else
        {
            request.OperCode = 999;
            return request;
        }
    }
}