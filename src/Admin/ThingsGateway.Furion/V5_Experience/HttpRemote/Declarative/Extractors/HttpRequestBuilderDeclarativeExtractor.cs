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
///     HTTP 声明式 <see cref="HttpRequestBuilder" /> 自定义配置提取器
/// </summary>
internal sealed class HttpRequestBuilderDeclarativeExtractor : IFrozenHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 尝试解析单个 Action<HttpRequestBuilder> 类型参数
        if (context.Args.SingleOrDefault(u => u is Action<HttpRequestBuilder>) is Action<HttpRequestBuilder>
            requestBuilderAction)
        {
            requestBuilderAction.Invoke(httpRequestBuilder);
        }
    }

    /// <inheritdoc />
    public int Order => 1;
}