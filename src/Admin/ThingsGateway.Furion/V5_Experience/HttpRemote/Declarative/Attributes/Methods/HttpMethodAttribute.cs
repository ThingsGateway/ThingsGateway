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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式请求方式特性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpMethodAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="HttpMethodAttribute" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    public HttpMethodAttribute(string httpMethod, string? requestUri = null)
        : this(Helpers.ParseHttpMethod(httpMethod), requestUri)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HttpMethodAttribute" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    public HttpMethodAttribute(HttpMethod httpMethod, string? requestUri = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        Method = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     请求方式
    /// </summary>
    public HttpMethod Method { get; set; }

    /// <summary>
    ///     请求地址
    /// </summary>
    public string? RequestUri { get; set; }
}