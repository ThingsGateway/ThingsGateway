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

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

using ThingsGateway.Extensions;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     URL 编码的表单内容处理器
/// </summary>
/// <remarks>
///     解决 <see cref="FormUrlEncodedContent" /> 无法设置编码问题。<see cref="FormUrlEncodedContent" /> 的编码格式不是 <c>utf-8</c>，而是
///     <c>Encoding.Latin1</c>。
/// </remarks>
public class StringContentForFormUrlEncodedContentProcessor : FormUrlEncodedContentProcessor
{
    /// <inheritdoc />
    public override HttpContent? Process(object? rawContent, string contentType, Encoding? encoding)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(rawContent, contentType, encoding, out var httpContent))
        {
            return httpContent;
        }

        // 如果原始内容是字符串类型且不是有效的 application/x-www-form-urlencoded 格式
        if (rawContent is string rawString && !Helpers.IsFormUrlEncodedFormat(rawString))
        {
            throw new FormatException("The content must contain only form url encoded string.");
        }

        // 将原始请求内容转换为字符串
        var content = rawContent as string ?? GetContentString(
            // 将原始请求类型转换为字符串字典类型
            rawContent.ObjectToDictionary()!
                .ToDictionary(u => u.Key.ToCultureString(CultureInfo.InvariantCulture)!,
                    u => u.Value?.ToCultureString(CultureInfo.InvariantCulture)
                )
        );

        // 初始化 StringContent 实例
        var stringContent = new StringContent(content, encoding,
            new MediaTypeHeaderValue(contentType) { CharSet = encoding?.BodyName });

        return stringContent;
    }

    /// <summary>
    ///     获取 URL 编码的表单内容格式
    /// </summary>
    /// <param name="nameValueCollection">键值对集合</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetContentString(params IEnumerable<KeyValuePair<string, string?>> nameValueCollection)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(nameValueCollection);

        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 生成 {key}={value}&... 格式
        foreach (var nameValue in nameValueCollection)
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append('&');
            }

            stringBuilder.Append(Encode(nameValue.Key));
            stringBuilder.Append('=');
            stringBuilder.Append(Encode(nameValue.Value));
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     对数据进行 URL 编码
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string Encode(string? data) =>
        string.IsNullOrEmpty(data) ? string.Empty : Uri.EscapeDataString(data).Replace("%20", "+");
}