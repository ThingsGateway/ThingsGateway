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
///     管理 <see cref="HttpClient" /> 实例及及其释放操作
/// </summary>
internal sealed class HttpClientPooling
{
    /// <summary>
    ///     <inheritdoc cref="HttpClientPooling" />
    /// </summary>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    /// <param name="release">用于释放 <see cref="HttpClient" /> 实例的方法委托</param>
    internal HttpClientPooling(HttpClient httpClient, Action<HttpClient>? release)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpClient);

        Instance = httpClient;
        Release = release;
    }

    /// <summary>
    ///     <see cref="HttpClient" />
    /// </summary>
    internal HttpClient Instance { get; }

    /// <summary>
    ///     用于释放 <see cref="HttpClient" /> 实例的方法委托
    /// </summary>
    internal Action<HttpClient>? Release { get; }
}