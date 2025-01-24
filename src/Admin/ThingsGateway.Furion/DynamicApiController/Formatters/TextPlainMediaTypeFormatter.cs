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

using System.Text;

namespace Microsoft.AspNetCore.Mvc.Formatters;

/// <summary>
/// text/plain 请求 Body 参数支持
/// </summary>
[SuppressSniffer]
public sealed class TextPlainMediaTypeFormatter : TextInputFormatter
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public TextPlainMediaTypeFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    /// <summary>
    /// 重写 <see cref="ReadRequestBodyAsync(InputFormatterContext, Encoding)"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="effectiveEncoding"></param>
    /// <returns></returns>
    public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
    {
        using var reader = new StreamReader(context.HttpContext.Request.Body, effectiveEncoding);
        var stringContent = await reader.ReadToEndAsync().ConfigureAwait(false);

        return await InputFormatterResult.SuccessAsync(stringContent).ConfigureAwait(false);
    }
}