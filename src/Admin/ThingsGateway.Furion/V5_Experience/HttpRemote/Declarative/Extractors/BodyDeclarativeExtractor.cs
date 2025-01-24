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

using System.Net.Mime;
using System.Reflection;

using ThingsGateway.Extensions;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式 <see cref="BodyAttribute" /> 特性提取器
/// </summary>
internal sealed class BodyDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 查找单个贴有 [Body] 特性的参数
        var bodyParameter =
            context.UnFrozenParameters.SingleOrDefault(u => u.Key.IsDefined(typeof(BodyAttribute), true));

        // 解析参数信息
        var (parameter, value) = bodyParameter;

        // 空检查
        if (parameter is null)
        {
            return;
        }

        // 获取 BodyAttribute 实例
        var bodyAttribute = parameter.GetCustomAttribute<BodyAttribute>(true);

        // 空检查
        ArgumentNullException.ThrowIfNull(bodyAttribute);

        // 检查是否为原始字符串内容
        if (value is string stringValue && bodyAttribute.RawString)
        {
            // 空检查
            ArgumentException.ThrowIfNullOrWhiteSpace(bodyAttribute.ContentType);

            // 设置原始字符串内容
            httpRequestBuilder.SetRawStringContent(stringValue, bodyAttribute.ContentType);
        }
        else
        {
            // 设置请求内容
            httpRequestBuilder.SetContent(value, bodyAttribute.ContentType);
        }

        // 检查是否启用 StringContent 方式构建 application/x-www-form-urlencoded 请求内容
        if (httpRequestBuilder.ContentType.IsIn([MediaTypeNames.Application.FormUrlEncoded]) &&
            bodyAttribute.UseStringContent)
        {
            httpRequestBuilder.AddStringContentForFormUrlEncodedContentProcessor();
        }

        // 设置内容编码
        if (!string.IsNullOrWhiteSpace(bodyAttribute.ContentEncoding))
        {
            httpRequestBuilder.SetContentEncoding(bodyAttribute.ContentEncoding);
        }
    }
}