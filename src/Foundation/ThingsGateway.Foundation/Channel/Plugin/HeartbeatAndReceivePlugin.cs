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

namespace ThingsGateway.Foundation;

[PluginOption(Singleton = true)]
internal sealed class HeartbeatAndReceivePlugin : PluginBase, ITcpConnectedPlugin, ITcpReceivingPlugin
{
    public string DtuId
    {
        get
        {
            return _dtuId;
        }
        set
        {
            _dtuId = value;
            DtuIdByte = new ArraySegment<byte>(Encoding.UTF8.GetBytes(value));
        }
    }
    private string _dtuId;
    private ArraySegment<byte> DtuIdByte;

    /// <summary>
    /// 心跳字符串
    /// </summary>
    public string Heartbeat
    {
        get
        {
            return _heartbeat;
        }
        set
        {
            _heartbeat = value;
            HeartbeatByte = new ArraySegment<byte>(Encoding.UTF8.GetBytes(value));
        }
    }
    private string _heartbeat;
    private ArraySegment<byte> HeartbeatByte;


    public int HeartbeatTime { get; set; } = 3;

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return;//此处可判断，如果为服务器，则不用使用心跳。
        }

        if (DtuId.IsNullOrWhiteSpace()) return;

        if (client is ITcpClient tcpClient)
        {
            await tcpClient.SendAsync(DtuIdByte).ConfigureAwait(false);

            _ = Task.Factory.StartNew(async () =>
             {
                 var failedCount = 0;
                 while (client.Online)
                 {
                     await Task.Delay(HeartbeatTime).ConfigureAwait(false);
                     if (!client.Online)
                     {
                         return;
                     }

                     try
                     {
                         if (DateTime.UtcNow - tcpClient.LastSentTime.ToUniversalTime() < TimeSpan.FromMilliseconds(200))
                         {
                             await Task.Delay(200).ConfigureAwait(false);
                         }

                         await tcpClient.SendAsync(HeartbeatByte).ConfigureAwait(false);
                         tcpClient.Logger?.Trace($"{tcpClient}- Heartbeat");
                         failedCount = 0;
                     }
                     catch
                     {
                         failedCount++;
                     }
                     if (failedCount > 3)
                     {
                         await client.CloseAsync("The automatic heartbeat has failed more than 3 times and has been disconnected.").ConfigureAwait(false);
                     }
                 }
             });
        }

        await e.InvokeNext().ConfigureAwait(false);
    }

    public async Task OnTcpReceiving(ITcpSession client, ByteBlockEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return;//此处可判断，如果为服务器，则不用使用心跳。
        }

        if (DtuId.IsNullOrWhiteSpace()) return;

        if (client is ITcpClient tcpClient)
        {
            var len = HeartbeatByte.Count;
            if (len > 0)
            {
                if (HeartbeatByte.SequenceEqual(e.ByteBlock.AsSegment(0, len)))
                {
                    e.Handled = true;
                }
            }
            await e.InvokeNext().ConfigureAwait(false);//如果本插件无法处理当前数据，请将数据转至下一个插件。
        }
    }
}
