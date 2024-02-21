//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// ConfigUtil
/// </summary>
public static class ConfigExtension
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

            case ChannelTypeEnum.SerialPortClient:
                if (serialPortOption == null) throw new ArgumentNullException(nameof(SerialPortOption));
                return config.GetSerialPortWithOption(serialPortOption);

            case ChannelTypeEnum.UdpSession:
                if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl)) throw new ArgumentNullException(nameof(IPHost));
                return config.GetUdpSessionWithIPHost(remoteUrl, bindUrl);
        }
        return null;
    }
}