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

using System.Net.Http.Headers;
using System.Text;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="ReadOnlyMemory{T}" /> 内容处理器
/// </summary>
public class ReadOnlyMemoryContentProcessor : HttpContentProcessorBase
{
    /// <inheritdoc />
    public override bool CanProcess(object? rawContent, string contentType) =>
        rawContent is ReadOnlyMemoryContent or ReadOnlyMemory<byte>;

    /// <inheritdoc />
    public override HttpContent? Process(object? rawContent, string contentType, Encoding? encoding)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(rawContent, contentType, encoding, out var httpContent))
        {
            return httpContent;
        }

        // 检查是否是 ReadOnlyMemory<byte> 类型
        if (rawContent is ReadOnlyMemory<byte> readOnlyMemory)
        {
            // 初始化 ReadOnlyMemoryContent 实例
            var readOnlyMemoryContent = new ReadOnlyMemoryContent(readOnlyMemory);
            readOnlyMemoryContent.Headers.ContentType = new MediaTypeHeaderValue(contentType)
            {
                CharSet = encoding?.BodyName
            };

            return readOnlyMemoryContent;
        }

        throw new InvalidOperationException(
            $"Expected a ReadOnlyMemory<byte>, but received an object of type `{rawContent.GetType()}`.");
    }
}