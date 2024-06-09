//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

internal class HeartbeatAndReceivePlugin : PluginBase, ITcpConnectedPlugin, ITcpReceivingPlugin
{
    private IDtuClient DtuClient;

    public HeartbeatAndReceivePlugin(IDtuClient dtuClient)
    {
        this.DtuClient = dtuClient;
    }

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return;//此处可判断，如果为服务器，则不用使用心跳。
        }

        if (DtuClient.DtuId.IsNullOrWhiteSpace()) return;

        if (client is ITcpClient tcpClient)
        {
            await tcpClient.SendAsync(DtuClient.DtuId.ToUTF8Bytes()).ConfigureAwait(false);

            _ = Task.Run(async () =>
             {
                 var failedCount = 0;
                 while (true)
                 {
                     await Task.Delay(this.DtuClient.HeartbeatTime * 1000).ConfigureAwait(false);
                     if (!client.Online)
                     {
                         return;
                     }

                     try
                     {
                         await tcpClient.SendAsync(this.DtuClient.HeartbeatHexString.HexStringToBytes()).ConfigureAwait(false);
                         failedCount = 0;
                     }
                     catch
                     {
                         failedCount++;
                     }
                     if (failedCount > 3)
                     {
                         await client.CloseAsync("The automatic heartbeat has failed more than 3 times and has been disconnected.").ConfigureFalseAwait();
                     }
                 }
             });
        }

        await e.InvokeNext();
    }

    public async Task OnTcpReceiving(ITcpSession client, ByteBlockEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return;//此处可判断，如果为服务器，则不用使用心跳。
        }

        if (DtuClient.DtuId.IsNullOrWhiteSpace()) return;

        if (client is ITcpClient tcpClient)
        {
            var len = DtuClient.HeartbeatHexString.HexStringToBytes().Length;
            if (len > 0)
            {
                if (DtuClient.HeartbeatHexString == e.ByteBlock.AsSegment(0, len).ToHexString(default))
                {
                    e.Handled = true;
                }
            }
            await e.InvokeNext().ConfigureAwait(false);//如果本插件无法处理当前数据，请将数据转至下一个插件。
        }
    }
}
