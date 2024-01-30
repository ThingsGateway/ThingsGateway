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

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class ChannelData : IValidatableObject, IChannelData
{
    /// <inheritdoc/>
    public long Id { get; set; } = IncrementCount.GetCurrentValue();

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public ChannelTypeEnum ChannelType { get; set; }

    /// <inheritdoc/>
    public string? RemoteUrl { get; set; }

    /// <inheritdoc/>
    public string? BindUrl { get; set; }

    /// <inheritdoc/>
    public string? PortName { get; set; } = "COM1";

    /// <inheritdoc/>
    public int? BaudRate { get; set; } = 9600;

    /// <inheritdoc/>
    public int? DataBits { get; set; } = 8;

    /// <inheritdoc/>
    public Parity? Parity { get; set; } = System.IO.Ports.Parity.None;

    /// <inheritdoc/>
    public StopBits? StopBits { get; set; } = System.IO.Ports.StopBits.One;

    /// <inheritdoc/>
    public bool? DtrEnable { get; set; } = true;

    /// <inheritdoc/>
    public bool? RtsEnable { get; set; } = true;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChannelType == ChannelTypeEnum.TcpClient)
        {
            if (string.IsNullOrEmpty(RemoteUrl))
                yield return new ValidationResult("必填", new[] { nameof(RemoteUrl) });

            if (RemoteUrl != null && !RemoteUrl.Contains(':'))
                yield return new ValidationResult("地址格式不合法，显式填写端口", new[] { nameof(RemoteUrl) });

            string testStr = GetUri(RemoteUrl ?? string.Empty);
            if (!IPHost.TryCreate(testStr, UriKind.RelativeOrAbsolute, out Uri _))
            {
                yield return new ValidationResult("地址格式不合法", new[] { nameof(RemoteUrl) });
            }
        }
        else if (ChannelType == ChannelTypeEnum.TcpService)
        {
            if (string.IsNullOrEmpty(BindUrl))
                yield return new ValidationResult("必填", new[] { nameof(BindUrl) });

            if (BindUrl != null && !BindUrl.Contains(':'))
                yield return new ValidationResult("地址格式不合法，显式填写端口", new[] { nameof(BindUrl) });

            string testStr = GetUri(BindUrl ?? string.Empty);
            if (!IPHost.TryCreate(testStr, UriKind.RelativeOrAbsolute, out Uri _))
            {
                yield return new ValidationResult("地址格式不合法", new[] { nameof(BindUrl) });
            }
        }
        else if (ChannelType == ChannelTypeEnum.SerialPortClient)
        {
            if (string.IsNullOrEmpty(PortName))
                yield return new ValidationResult("必填", new[] { nameof(PortName) });
            if (BaudRate == null)
                yield return new ValidationResult("必填", new[] { nameof(BaudRate) });
            if (DataBits == null)
                yield return new ValidationResult("必填", new[] { nameof(DataBits) });
            if (Parity == null)
                yield return new ValidationResult("必填", new[] { nameof(Parity) });
            if (StopBits == null)
                yield return new ValidationResult("必填", new[] { nameof(StopBits) });
        }
    }

    private string GetUri(string uri)
    {
        string testStr;
        if (TouchSocketUtility.IsURL(uri))
        {
            testStr = uri;
        }
        else
        {
            testStr = $"tcp://{uri}"; ;
        }

        return testStr;
    }

    /// <summary>
    /// TouchSocketConfig
    /// </summary>
    [JsonIgnore]
    public TouchSocketConfig TouchSocketConfig;

    /// <summary>
    /// Channel
    /// </summary>
    [JsonIgnore]
    public IChannel Channel;

    private static IncrementCount IncrementCount = new(long.MaxValue);

    /// <summary>
    /// 创建通道
    /// </summary>
    /// <param name="channelData"></param>
    public static void CreateChannel(ChannelData channelData)
    {
        channelData.TouchSocketConfig ??= new TouchSocket.Core.TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(channelData.Id.GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);
        channelData.TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));

        if (channelData.Channel != null)
        {
            channelData.Channel.Dispose();
        }
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