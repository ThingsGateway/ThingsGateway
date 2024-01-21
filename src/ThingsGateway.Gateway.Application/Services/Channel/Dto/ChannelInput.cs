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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道添加DTO
/// </summary>
public class ChannelAddInput : Channel, IValidatableObject
{
    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override ChannelTypeEnum ChannelType { get; set; }

    /// <inheritdoc/>
    public override bool Enable { get; set; } = true;

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
}

/// <summary>
/// 通道编辑DTO
/// </summary>
public class ChannelEditInput : ChannelAddInput
{
    /// <inheritdoc/>
    public override bool Enable { get; set; }
}

/// <summary>
/// 通道分页查询DTO
/// </summary>
public class ChannelPageInput : BasePageInput
{
    /// <inheritdoc/>
    [Description("通道名称")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Description("通道类型")]
    public ChannelTypeEnum? ChannelType { get; set; }
}

/// <summary>
/// 通道查询DTO
/// </summary>
public class ChannelInput
{
    /// <inheritdoc/>
    [Description("通道名称")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Description("通道类型")]
    public ChannelTypeEnum? ChannelType { get; set; }

    /// <summary>
    /// 全部
    /// </summary>
    public bool All { get; set; }
}