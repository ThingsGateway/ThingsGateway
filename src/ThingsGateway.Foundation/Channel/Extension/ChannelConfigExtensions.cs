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
public static class ChannelConfigExtensions
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
            foreach (var func in funcs)
            {
                await func.Invoke(clientChannel, e).ConfigureAwait(false);
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
            foreach (var func in funcs)
            {
                var handled = await func.Invoke(clientChannel).ConfigureAwait(false);
                if (handled)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 获取一个新的通道。传入通道类型，远程服务端地址，绑定地址，串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelType">通道类型</param>
    /// <param name="remoteUrl">远端IP端口配置</param>
    /// <param name="bindUrl">本地IP端口配置</param>
    /// <param name="serialPortOption">串口配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IChannel? GetChannel(this TouchSocketConfig config, ChannelTypeEnum channelType, string? remoteUrl = default, string? bindUrl = default, SerialPortOption? serialPortOption = default)
    {
        config.ThrowIfNull(nameof(TouchSocketConfig));
        channelType.ThrowIfNull(nameof(ChannelTypeEnum));


        switch (channelType)
        {
            case ChannelTypeEnum.TcpClient:
                remoteUrl.ThrowIfNull(nameof(IPHost));
                return config.GetTcpClientWithIPHost(remoteUrl, bindUrl);

            case ChannelTypeEnum.TcpService:
                bindUrl.ThrowIfNull(nameof(IPHost));
                return config.GetTcpServiceWithBindIPHost(bindUrl);

            case ChannelTypeEnum.SerialPort:
                serialPortOption.ThrowIfNull(nameof(SerialPortOption));
                return config.GetSerialPortWithOption(serialPortOption);

            case ChannelTypeEnum.UdpSession:
                if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl))
                    throw new ArgumentNullException(nameof(IPHost));
                return config.GetUdpSessionWithIPHost(remoteUrl, bindUrl);
        }
        return default;
    }

    /// <summary>
    /// 获取一个新的串口通道。传入串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="serialPortOption">串口配置</param>
    /// <returns></returns>
    public static SerialPortChannel GetSerialPortWithOption(this TouchSocketConfig config, SerialPortOption serialPortOption)
    {
        serialPortOption.ThrowIfNull(nameof(SerialPortOption));
        config.SetSerialPortOption(serialPortOption);

        //载入配置
        SerialPortChannel serialPortChannel = new SerialPortChannel();
        serialPortChannel.Setup(config);

        return serialPortChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp客户端通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="remoteUrl">远端IP端口配置</param>
    /// <param name="bindUrl">本地IP端口配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TcpClientChannel GetTcpClientWithIPHost(this TouchSocketConfig config, string remoteUrl, string? bindUrl = default)
    {
        remoteUrl.ThrowIfNull(nameof(IPHost));
        config.SetRemoteIPHost(remoteUrl);
        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);

        //载入配置
        TcpClientChannel tcpClientChannel = new TcpClientChannel();
        tcpClientChannel.Setup(config);
        return tcpClientChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp服务会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="bindUrl">本地IP端口配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TcpServiceChannel GetTcpServiceWithBindIPHost(this TouchSocketConfig config, string bindUrl)
    {
        bindUrl.ThrowIfNull(nameof(IPHost));

        var urls = bindUrl.SplitStringBySemicolon();
        config.SetListenIPHosts(IPHost.ParseIPHosts(urls));
        //载入配置
        TcpServiceChannel tcpServiceChannel = new TcpServiceChannel();
        tcpServiceChannel.Setup(config);
        return tcpServiceChannel;
    }

    /// <summary>
    /// 获取一个新的Udp会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="remoteUrl">远端IP端口配置</param>
    /// <param name="bindUrl">本地IP端口配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static UdpSessionChannel GetUdpSessionWithIPHost(this TouchSocketConfig config, string? remoteUrl, string? bindUrl)
    {
        if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl))
            throw new ArgumentNullException(nameof(IPHost));

        if (!string.IsNullOrEmpty(remoteUrl))
            config.SetRemoteIPHost(remoteUrl);

        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);
        else
            config.SetBindIPHost(new IPHost(0));

        //载入配置
        UdpSessionChannel udpSessionChannel = new UdpSessionChannel();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            config.UseUdpConnReset();
        }
#endif
        udpSessionChannel.Setup(config);
        return udpSessionChannel;
    }
}
