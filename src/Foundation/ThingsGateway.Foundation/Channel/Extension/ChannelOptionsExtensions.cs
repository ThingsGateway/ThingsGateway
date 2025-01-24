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

using TouchSocket.SerialPorts;

namespace ThingsGateway.Foundation;


/// <summary>
/// 通道扩展
/// </summary>
public static class ChannelOptionsExtensions
{
    /// <summary>
    /// 触发通道接收事件
    /// </summary>
    /// <param name="clientChannel">通道</param>
    /// <param name="e">接收数据</param>
    /// <param name="funcs">事件</param>
    /// <returns></returns>
    internal static async Task OnChannelReceivedEvent(this IClientChannel clientChannel, ReceivedDataEventArgs e, ChannelReceivedEventHandler funcs)
    {
        clientChannel.ThrowIfNull(nameof(IClientChannel));
        e.ThrowIfNull(nameof(ReceivedDataEventArgs));
        funcs.ThrowIfNull(nameof(ChannelReceivedEventHandler));

        if (funcs.Count > 0)
        {
            for (int i = 0; i < funcs.Count; i++)
            {
                var func = funcs[i];
                await func.Invoke(clientChannel, e, i == funcs.Count - 1).ConfigureAwait(false);
                if (e.Handled)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 触发通道事件
    /// </summary>
    /// <param name="clientChannel">通道</param>
    /// <param name="funcs">事件</param>
    /// <returns></returns>
    internal static async Task OnChannelEvent(this IClientChannel clientChannel, ChannelEventHandler funcs)
    {
        clientChannel.ThrowIfNull(nameof(IClientChannel));
        funcs.ThrowIfNull(nameof(ChannelEventHandler));

        if (funcs.Count > 0)
        {
            for (int i = 0; i < funcs.Count; i++)
            {
                var func = funcs[i];
                var handled = await func.Invoke(clientChannel, i == funcs.Count - 1).ConfigureAwait(false);
                if (handled)
                {
                    break;
                }
            }
            foreach (var func in funcs)
            {

            }
        }
    }

    /// <summary>
    /// 获取一个新的通道。传入通道类型，远程服务端地址，绑定地址，串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IChannel? GetChannel(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        config.ThrowIfNull(nameof(TouchSocketConfig));
        channelOptions.ThrowIfNull(nameof(IChannelOptions));
        var channelType = channelOptions.ChannelType;
        channelType.ThrowIfNull(nameof(ChannelTypeEnum));
        switch (channelType)
        {
            case ChannelTypeEnum.TcpClient:
                return config.GetTcpClientWithIPHost(channelOptions);

            case ChannelTypeEnum.TcpService:
                return config.GetTcpServiceWithBindIPHost(channelOptions);

            case ChannelTypeEnum.SerialPort:
                return config.GetSerialPortWithOption(channelOptions);

            case ChannelTypeEnum.UdpSession:
                return config.GetUdpSessionWithIPHost(channelOptions);
            case ChannelTypeEnum.Other:
                channelOptions.Config = config;
                OtherChannel otherChannel = new OtherChannel(channelOptions);
                return otherChannel;
        }
        return default;
    }

    /// <summary>
    /// 获取一个新的串口通道。传入串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">串口配置</param>
    /// <returns></returns>
    public static SerialPortChannel GetSerialPortWithOption(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var serialPortOption = channelOptions.Map<SerialPortOption>();
        serialPortOption.ThrowIfNull(nameof(SerialPortOption));
        channelOptions.Config = config;
        config.SetSerialPortOption(serialPortOption);

        //载入配置
        SerialPortChannel serialPortChannel = new SerialPortChannel(channelOptions);
        return serialPortChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp客户端通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TcpClientChannel GetTcpClientWithIPHost(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var remoteUrl = channelOptions.RemoteUrl;
        var bindUrl = channelOptions.BindUrl;
        remoteUrl.ThrowIfNull(nameof(remoteUrl));
        channelOptions.Config = config;
        config.SetRemoteIPHost(remoteUrl);
        if (!string.IsNullOrWhiteSpace(bindUrl))
            config.SetBindIPHost(bindUrl);

        //载入配置
        TcpClientChannel tcpClientChannel = new TcpClientChannel(channelOptions);
        return tcpClientChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp服务会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TcpServiceChannel GetTcpServiceWithBindIPHost(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var bindUrl = channelOptions.BindUrl;
        bindUrl.ThrowIfNull(nameof(bindUrl));
        channelOptions.Config = config;

        var urls = bindUrl.SplitStringBySemicolon();
        config.SetListenIPHosts(IPHost.ParseIPHosts(urls));
        //载入配置
        TcpServiceChannel tcpServiceChannel = new TcpServiceChannel(channelOptions);
        return tcpServiceChannel;
    }

    /// <summary>
    /// 获取一个新的Udp会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static UdpSessionChannel GetUdpSessionWithIPHost(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var remoteUrl = channelOptions.RemoteUrl;
        var bindUrl = channelOptions.BindUrl;
        if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl))
            throw new ArgumentNullException(nameof(IPHost));
        channelOptions.Config = config;

        if (!string.IsNullOrEmpty(remoteUrl))
            config.SetRemoteIPHost(remoteUrl);

        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);
        else
            config.SetBindIPHost(new IPHost(0));

        //载入配置
        UdpSessionChannel udpSessionChannel = new UdpSessionChannel(channelOptions);
#if NETSTANDARD || NET6_0_OR_GREATER
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            config.UseUdpConnReset();
        }
#endif
        return udpSessionChannel;
    }
}
