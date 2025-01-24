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
///     HTTP 声明式 <see cref="AcceptLanguageAttribute" /> 特性提取器
/// </summary>
internal sealed class AcceptLanguageDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 检查方法或接口是否贴有 [AcceptLanguage] 特性
        if (!context.IsMethodDefined<AcceptLanguageAttribute>(out var acceptLanguageAttribute, true))
        {
            return;
        }

        // 设置客户端所偏好的自然语言和区域设置
        httpRequestBuilder.AcceptLanguage(acceptLanguageAttribute.Language);
    }
}