// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace ThingsGateway.Schedule;

/// <summary>
/// HTTP 作业消息
/// </summary>
[SuppressSniffer]
public sealed class HttpJobMessage
{
    /// <summary>
    /// 请求地址
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 请求报文体
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// 请求客户端名称
    /// </summary>
    public string ClientName { get; set; } = nameof(HttpJob);

    /// <summary>
    /// 确保请求成功，否则抛异常
    /// </summary>
    public bool EnsureSuccessStatusCode { get; set; } = true;

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// 是否打印 HTTP 响应内容
    /// </summary>
    /// <remarks>默认 true（打印）</remarks>
    public bool PrintResponseContent { get; set; } = true;

    /// <summary>
    /// 作业组名称
    /// </summary>
    [JsonIgnore]
    public string GroupName { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    [JsonIgnore]
    public string Description { get; set; }
}