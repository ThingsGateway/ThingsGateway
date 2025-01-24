//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------



using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using ThingsGateway.Razor;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
internal sealed class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly WebsiteOptions _websiteOptions;

    /// <inheritdoc/>
    public EmailService(IOptions<EmailOptions> emailOptions, IOptions<WebsiteOptions> websiteOptions)
    {
        _emailOptions = emailOptions.Value;
        _websiteOptions = websiteOptions.Value;
    }


    /// <inheritdoc/>
    [DisplayName("发送邮件")]
    public async Task SendEmail([Required] string content, string title = "")
    {
        var webTitle = _websiteOptions.Title;
        title = string.IsNullOrWhiteSpace(title) ? $"{webTitle} 系统邮件" : title;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOptions.DefaultFromEmail, _emailOptions.DefaultFromEmail));
        message.To.Add(new MailboxAddress(_emailOptions.DefaultToEmail, _emailOptions.DefaultToEmail));
        message.Subject = title;
        message.Body = new TextPart("html")
        {
            Text = content
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, _emailOptions.EnableSsl).ConfigureAwait(false);
        await client.AuthenticateAsync(_emailOptions.UserName, _emailOptions.Password).ConfigureAwait(false);
        await client.SendAsync(message).ConfigureAwait(false);
        await client.DisconnectAsync(true).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
