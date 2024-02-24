//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// SiemensS7DataHandleAdapter
/// </summary>
internal class SiemensS7DataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<SiemensMessage>
{
    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command, SiemensMessage item)
    {
        return command;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override SiemensMessage GetInstance()
    {
        return new SiemensMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(SiemensMessage request, byte[]? send, byte[] body, byte[] response)
    {
        var result = new OperResult<byte[], FilterResult>();
        if (response[2] * 256 + response[3] == 7)
        {
            result = new() { Content = response, Content2 = FilterResult.Success };
        }
        else
        {
            //以请求方为准，分开返回类型校验
            switch (send[17])
            {
                case 0x04:
                    result = SiemensHelper.AnalysisReadByte(send, response);
                    break;

                case 0x05:
                    result = SiemensHelper.AnalysisWrite(response);
                    break;

                default:
                    //添加返回代码校验

                    if (response[5] == 0xD0)//首次握手0XD0连接确认
                    {
                        result = new() { Content = response, Content2 = FilterResult.Success };
                    }
                    else
                    {
                        //其余情况判断错误代码
                        if (response[17] + response[18] > 0)
                        {
                            result = new($"PLC返回错误，错误类型{response[17].ToString("X2")}错误代码：{response[18].ToString("X2")}") { Content = response, Content2 = FilterResult.Success };
                        }
                        else
                        {
                            result = new() { Content = response, Content2 = FilterResult.Success };
                        }
                    }


                    break;
            }
        }
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        request.Content = result.Content;
        return result.Content2;
    }
}