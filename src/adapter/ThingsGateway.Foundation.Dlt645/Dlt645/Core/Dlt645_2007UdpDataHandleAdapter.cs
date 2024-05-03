
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


namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007UdpDataHandleAdapter
/// </summary>
internal class Dlt645_2007UdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<Dlt645_2007Message>
{
    /// <summary>
    /// 增加FE FE FE FE的报文头部
    /// </summary>
    public string FEHead { get; set; }

    /// <inheritdoc/>
    public override void PackCommand(ISendMessage item)
    {
        if (!FEHead.IsNullOrWhiteSpace())
        {
            Dlt645Helper.AddFE(item, FEHead);
        }
    }



    protected override byte[] UnpackResponse(Dlt645_2007Message request)
    {
        var result = Dlt645Helper.GetResponse(request);
        return result.Bytes;
    }
}
