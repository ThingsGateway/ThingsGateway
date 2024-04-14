
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class ChannelData : IChannelData
{
    /// <inheritdoc/>
    public long Id { get; set; } = IncrementCount.GetCurrentValue();

    /// <summary>
    /// 通道名称
    /// </summary>
    [Required]
    public virtual string Name { get; set; }

    /// <inheritdoc/>
    public virtual ChannelTypeEnum ChannelType { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// LogEnable
    /// </summary>
    public bool LogEnable { get; set; }

    /// <summary>
    /// 远程地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    public string? RemoteUrl { get; set; }

    /// <summary>
    /// 本地地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    public string? BindUrl { get; set; }

    /// <summary>
    /// COM
    /// </summary>
    public string? PortName { get; set; }

    /// <summary>
    /// 波特率
    /// </summary>
    public int? BaudRate { get; set; }

    /// <summary>
    /// 数据位
    /// </summary>
    public int? DataBits { get; set; }

    /// <summary>
    /// 校验位
    /// </summary>
    public Parity? Parity { get; set; } = System.IO.Ports.Parity.None;

    /// <summary>
    /// 停止位
    /// </summary>
    public StopBits? StopBits { get; set; } = System.IO.Ports.StopBits.One;

    /// <summary>
    /// DtrEnable
    /// </summary>
    public bool? DtrEnable { get; set; }

    /// <summary>
    /// RtsEnable
    /// </summary>
    public bool? RtsEnable { get; set; }

    /// <summary>
    /// TouchSocketConfig
    /// </summary>
#if NET6_0_OR_GREATER

    [System.Text.Json.Serialization.JsonIgnore]
#endif

    [JsonIgnore]
    public TouchSocketConfig TouchSocketConfig;

    /// <summary>
    /// Channel
    /// </summary>
#if NET6_0_OR_GREATER

    [System.Text.Json.Serialization.JsonIgnore]
#endif

    [JsonIgnore]
    public IChannel Channel;

    private static IncrementCount IncrementCount = new(long.MaxValue);

    /// <summary>
    /// 创建通道
    /// </summary>
    /// <param name="channelData"></param>
    public static void CreateChannel(ChannelData channelData)
    {

        if (channelData.Channel != null)
        {
            channelData.Channel.Close();
        }
        channelData.TouchSocketConfig ??= new TouchSocket.Core.TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(channelData.Id.GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);
        channelData.TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));

        switch (channelData.ChannelType)
        {
            case ChannelTypeEnum.TcpClient:
                channelData.Channel = channelData.TouchSocketConfig.GetTcpClientWithIPHost(channelData.RemoteUrl, channelData.BindUrl);
                break;

            case ChannelTypeEnum.TcpService:
                channelData.Channel = channelData.TouchSocketConfig.GetTcpServiceWithBindIPHost(channelData.BindUrl);
                break;

            case ChannelTypeEnum.SerialPortClient:
                channelData.Channel = channelData.TouchSocketConfig.GetSerialPortWithOption(channelData.Map<SerialPortOption>());
                break;

            case ChannelTypeEnum.UdpSession:
                channelData.Channel = channelData.TouchSocketConfig.GetUdpSessionWithIPHost(channelData.RemoteUrl, channelData.BindUrl);
                break;
        }
    }
}