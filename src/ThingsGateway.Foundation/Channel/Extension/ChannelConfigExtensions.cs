//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;

using TouchSocket.SerialPorts;

namespace ThingsGateway.Foundation;

public static class ChannelConfigExtensions
{
    /// <summary>
    /// 获取配置。可以指定日志等级以及日志方法
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="action"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static TouchSocketConfig GetConfigWithLog(this TouchSocketConfig config, LogLevel logLevel, Action<LogLevel, object, string, Exception> action)
    {
        config ??= new TouchSocketConfig();
        var LogMessage = new LoggerGroup() { LogLevel = logLevel };
        LogMessage.AddLogger(new EasyLogger(action) { LogLevel = logLevel });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        return config;
    }

    /// <summary>
    /// 获取通道
    /// </summary>
    public static IChannel GetChannel(this TouchSocketConfig config, ChannelTypeEnum channel, string remoteUrl, string bindUrl, SerialPortOption serialPortOption)
    {
        switch (channel)
        {
            case ChannelTypeEnum.TcpClient:
                if (string.IsNullOrEmpty(remoteUrl)) throw new ArgumentNullException(nameof(IPHost));
                return config.GetTcpClientWithIPHost(remoteUrl, bindUrl);

            case ChannelTypeEnum.TcpService:
                if (string.IsNullOrEmpty(bindUrl)) throw new ArgumentNullException(nameof(IPHost));
                return config.GetTcpServiceWithBindIPHost(bindUrl);

            case ChannelTypeEnum.SerialPort:
                if (serialPortOption == null) throw new ArgumentNullException(nameof(SerialPortOption));
                return config.GetSerialPortWithOption(serialPortOption);

            case ChannelTypeEnum.UdpSession:
                if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl)) throw new ArgumentNullException(nameof(IPHost));
                return config.GetUdpSessionWithIPHost(remoteUrl, bindUrl);
        }
        return null;
    }

    /// <summary>
    /// 获取一个新的Tcp客户端通道。传入远程服务端地址
    /// </summary>
    /// <returns></returns>
    public static TcpClientChannel GetTcpClientWithIPHost(this TouchSocketConfig config, string remoteUrl, string? bindUrl = default)
    {
        if (remoteUrl == null)
            throw new ArgumentNullException(nameof(IPHost));
        config.SetRemoteIPHost(remoteUrl);
        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);
        //载入配置
        TcpClientChannel tcpClientChannel = new TcpClientChannel();
        tcpClientChannel.Setup(config);
        return tcpClientChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp服务端通道。传入绑定地址
    /// </summary>
    /// <returns></returns>
    public static TcpServiceChannel GetTcpServiceWithBindIPHost(this TouchSocketConfig config, string bindUrl)
    {
        if (bindUrl == null) throw new ArgumentNullException(nameof(IPHost));
        var urls = bindUrl.SplitStringBySemicolon();
        config.SetListenIPHosts(IPHost.ParseIPHosts(urls));
        //载入配置
        TcpServiceChannel tcpServiceChannel = new TcpServiceChannel();
        tcpServiceChannel.Setup(config);
        return tcpServiceChannel;
    }

    /// <summary>
    /// 获取一个新的Udp通道。传入默认远程服务端地址，绑定地址
    /// </summary>
    public static UdpSessionChannel GetUdpSessionWithIPHost(this TouchSocketConfig config, string? remoteUrl, string? bindUrl)
    {
        if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl)) throw new ArgumentNullException(nameof(IPHost));
        if (!string.IsNullOrEmpty(remoteUrl))
            config.SetRemoteIPHost(remoteUrl);
        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);
        else
            config.SetBindIPHost(new IPHost(0));

        //载入配置
        UdpSessionChannel udpSessionChannel = new UdpSessionChannel();
        udpSessionChannel.Setup(config);
        return udpSessionChannel;
    }

    /// <summary>
    /// 获取一个新的串口通道。传入串口配置信息
    /// </summary>
    /// <param name="config"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static SerialPortChannel GetSerialPortWithOption(this TouchSocketConfig config, SerialPortOption value)
    {
        if (value == null) throw new ArgumentNullException(nameof(SerialPortOption));
        config.SetSerialPortOption(value);

        //载入配置
        SerialPortChannel serialPortChannel = new SerialPortChannel();
        serialPortChannel.Setup(config);

        return serialPortChannel;
    }
}
