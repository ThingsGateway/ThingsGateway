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

using Microsoft.Net.Http.Headers;

using System.Net;
using System.Text.RegularExpressions;

using ThingsGateway.Utilities;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 远程请求模块帮助类
/// </summary>
internal static partial class Helpers
{
    /// <summary>
    ///     从互联网 URL 地址中加载流
    /// </summary>
    /// <param name="requestUri">互联网 URL 地址</param>
    /// <param name="maxResponseContentBufferSize">响应内容的最大缓存大小。默认值为：<c>100MB</c>。</param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static Stream GetStreamFromRemote(string requestUri, long maxResponseContentBufferSize = 104857600L)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);

        // 检查 URL 地址是否是互联网地址
        if (!NetworkUtility.IsWebUrl(requestUri))
        {
            throw new ArgumentException($"Invalid internet address: `{requestUri}`.", nameof(requestUri));
        }

        // 初始化 HttpClient 实例
        using var httpClient = new HttpClient();

        // 限制流大小
        httpClient.MaxResponseContentBufferSize = maxResponseContentBufferSize;

        // 启用性能优化（返回 Stream 内容时，请勿启用此配置，否则流将因压缩而变得不可读。）
        // httpClient.PerformanceOptimization();

        // 设置默认 User-Agent
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent,
            Constants.USER_AGENT_OF_BROWSER);

        try
        {
            // 发送 HTTP 远程请求
            var httpResponseMessage = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, requestUri),
                HttpCompletionOption.ResponseHeadersRead);

            // 确保请求成功
            httpResponseMessage.EnsureSuccessStatusCode();

            // 读取流并返回
            return httpResponseMessage.Content.ReadAsStream();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to load stream from internet address: `{requestUri}`.", e);
        }
    }

    /// <summary>
    ///     从 <see cref="Uri" /> 中解析文件的名称
    /// </summary>
    /// <param name="uri">
    ///     <see cref="Uri" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? GetFileNameFromUri(Uri? uri)
    {
        // 空检查
        if (uri is null)
        {
            return null;
        }

        // 获取 URL 的绝对路径
        var path = uri.AbsolutePath;

        // 使用 / 分割路径，并获取最后一个部分作为潜在的文件的名称
        var parts = path.Split('/');
        var fileName = parts.Length > 0 ? parts[^1] : string.Empty;

        // 检查文件的名称是否为空或仅由点组成
        if (string.IsNullOrEmpty(fileName) || fileName.Trim('.').Length == 0)
        {
            return string.Empty;
        }

        // 查找文件的名称中的查询字符串开始位置。如果存在查询字符串，则去除它
        var queryStartIndex = fileName.IndexOf('?');
        if (queryStartIndex != -1)
        {
            fileName = fileName[..queryStartIndex];
        }

        // 检查文件的名称是否包含有效的扩展名
        var lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex == -1 || lastDotIndex == fileName.Length - 1)
        {
            return string.Empty;
        }

        // 使用 UTF-8 解码文件的名称
        return Uri.UnescapeDataString(fileName);
    }

    /// <summary>
    ///     解析 HTTP 谓词
    /// </summary>
    /// <param name="httpMethod">HTTP 谓词</param>
    /// <returns>
    ///     <see cref="HttpMethod" />
    /// </returns>
    internal static HttpMethod ParseHttpMethod(string? httpMethod)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(httpMethod);

        return HttpMethod.Parse(httpMethod);
    }

    /// <summary>
    ///     验证字符串是否是 <c>application/x-www-form-urlencoded</c> 格式
    /// </summary>
    /// <param name="output">字符串</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsFormUrlEncodedFormat(string output)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(output);

        return FormUrlEncodedFormatRegex().IsMatch(output);
    }

    /// <summary>
    ///     检查 HTTP 状态码是否是重定向状态码
    /// </summary>
    /// <param name="statusCode">
    ///     <see cref="HttpStatusCode" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsRedirectStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.Ambiguous or HttpStatusCode.Moved or HttpStatusCode.Redirect
            or HttpStatusCode.RedirectMethod or HttpStatusCode.RedirectKeepVerb || (int)statusCode == 308;

    /// <summary>
    ///     从给定的绝对 URI 中解析出基础地址
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="Uri" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static Uri ParseBaseAddress(Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(requestUri);

        // 检查是否是绝对地址
        if (!requestUri.IsAbsoluteUri)
        {
            throw new ArgumentException("The requestUri must be an absolute URI.", nameof(requestUri));
        }

        return new Uri(
            $"{requestUri.Scheme}://{requestUri.Host}{(requestUri.IsDefaultPort ? string.Empty : $":{requestUri.Port}")}");
    }

    /// <summary>
    ///     <c>application/x-www-form-urlencoded</c> 格式正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(
        @"^(?:(?:[a-zA-Z0-9-._~]+|%(?:[0-9A-Fa-f]{2}))+=(?:[a-zA-Z0-9-._~]*|%(?:[0-9A-Fa-f]{2}))+)(?:&(?:[a-zA-Z0-9-._~]+|%(?:[0-9A-Fa-f]{2}))+=(?:[a-zA-Z0-9-._~]*|%(?:[0-9A-Fa-f]{2}))+)*$")]
    private static partial Regex FormUrlEncodedFormatRegex();
}