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

using System.Text;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpContent" /> 请求内容处理器
/// </summary>
/// <remarks>用于将原始请求内容转换成 <see cref="HttpContent" /> 实例</remarks>
public interface IHttpContentProcessor
{
    /// <summary>
    ///     <inheritdoc cref="IServiceProvider" />
    /// </summary>
    IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    ///     判断当前处理器是否可以处理指定的内容类型
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    bool CanProcess(object? rawContent, string contentType);

    /// <summary>
    ///     将原始请求内容转换为 <see cref="HttpContent" /> 实例
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="encoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpContent" />
    /// </returns>
    HttpContent? Process(object? rawContent, string contentType, Encoding? encoding);
}