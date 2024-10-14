//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
[PluginOption(Singleton = true)]
public class DtuPlugin : PluginBase, ITcpReceivingPlugin
{
    /// <summary>
    /// 心跳16进制字符串
    /// </summary>
    public string HeartbeatHexString { get; set; }

    /// <inheritdoc/>
    public async Task OnTcpReceiving(ITcpSession client, ByteBlockEventArgs e)
    {
        var len = HeartbeatHexString.HexStringToBytes().Length;
        if (client is TcpSessionClientChannel socket && socket.Service is TcpServiceChannel tcpServiceChannel)
        {
            if (!socket.Id.StartsWith("ID="))
            {
                var id = $"ID={e.ByteBlock}";
                if (tcpServiceChannel.TryGetClient(id, out var oldClient))
                {
                    try
                    {
                        oldClient.TryShutdown();
                        await oldClient.CloseAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                    }
                }
                await socket.ResetIdAsync(id).ConfigureAwait(false);
                client.Logger?.Info(DefaultResource.Localizer["DtuConnected", id]);
                e.Handled = true;
            }

            if (!socket.Service.ClientExists(socket.Id))
            {
                try
                {
                    socket.TryShutdown();
                    await socket.CloseAsync().ConfigureAwait(false);
                }
                catch
                {
                }

                await e.InvokeNext().ConfigureAwait(false);//如果本插件无法处理当前数据，请将数据转至下一个插件。
                return;
            }

            if (len > 0)
            {
                if (HeartbeatHexString == e.ByteBlock.AsSegment(0, len).ToHexString(default))
                {
                    if (DateTime.UtcNow - socket.LastSentTime.ToUniversalTime() < TimeSpan.FromMilliseconds(200))
                    {
                        await Task.Delay(200).ConfigureAwait(false);
                    }
                    //回应心跳包
                    await socket.SendAsync(e.ByteBlock.AsSegment()).ConfigureAwait(false);
                    e.Handled = true;
                    if (socket.Logger?.LogLevel <= LogLevel.Trace)
                        socket.Logger?.Trace($"{socket}- Heartbeat");
                }
            }
        }
        await e.InvokeNext().ConfigureAwait(false);//如果本插件无法处理当前数据，请将数据转至下一个插件。
    }
}
