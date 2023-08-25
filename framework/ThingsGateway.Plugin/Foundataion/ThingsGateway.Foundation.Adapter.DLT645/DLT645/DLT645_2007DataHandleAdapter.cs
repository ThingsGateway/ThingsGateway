#region copyright
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

using System.ComponentModel;

using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.DLT645;

/// <summary>
/// DLT645_2007DataHandleAdapter
/// </summary>
public class DLT645_2007DataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<DLT645_2007Message>
{
    /// <summary>
    /// 增加FE FE FE FE的报文头部
    /// </summary>
    [Description("前导符报文头")]
    public bool EnableFEHead { get; set; }

    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command)
    {
        //打包时加上4个FE字节
        if (EnableFEHead)
        {
            return DataTransUtil.SpliceArray(new byte[4] { 0xFE, 0xFE, 0xFE, 0xFE }, command);
        }
        return command;
    }

    /// <inheritdoc/>
    protected override DLT645_2007Message GetInstance()
    {
        return new DLT645_2007Message();
    }


    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(DLT645_2007Message request, byte[] send, byte[] body, byte[] response)
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

        //帧起始符 地址域  帧起始符 控制码 数据域长度共10个字节
        if (headCodeIndex < 0 || headCodeIndex + 10 > response.Length)
            return FilterResult.Cache;


        var len = 10 + response[headCodeIndex + 9] + 2;

        if (response.Length - headCodeIndex < len)
        {
            return FilterResult.Cache;
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
                request.Message = "和校验错误";
                request.ResultCode = ResultCode.Fail;
                return FilterResult.Success;
            }
            if ((response[headCodeIndex + 8] & 0x40) == 0x40)//控制码bit6为1时，返回错误
            {
                byte byte1 = (byte)(response[headCodeIndex + 10] - 0x33);
                var error = DLT645Helper.Get2007ErrorMessage(byte1);
                request.Message = "异常控制码：" + $"0x{response[headCodeIndex + 8]:X2}，错误信息：{error}";
                request.ResultCode = ResultCode.Fail;
                return FilterResult.Success;
            }
            else
            {
                request.Content = response.RemoveBegin(headCodeIndex + 10).RemoveLast(response.Length + 2 - len - headCodeIndex);
                request.ResultCode = ResultCode.Success;
                return FilterResult.Success;
            }

        }
        else
        {
            request.ResultCode = ResultCode.Error;
            return FilterResult.Success;

        }
    }

}
