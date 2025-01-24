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

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 远程请求服务构建器
/// </summary>
public interface IHttpRemoteBuilder
{
    /// <summary>
    ///     <see cref="IServiceCollection" />
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///     配置 <see cref="HttpRemoteOptions" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions> configure);
}

/// <summary>
///     <see cref="IHttpRemoteBuilder" /> 默认实现
/// </summary>
internal sealed class DefaultHttpRemoteBuilder : IHttpRemoteBuilder
{
    /// <summary>
    ///     <inheritdoc cref="DefaultHttpRemoteBuilder" />
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    public DefaultHttpRemoteBuilder(IServiceCollection services)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <inheritdoc />
    public IHttpRemoteBuilder ConfigureOptions(Action<HttpRemoteOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);

        return this;
    }
}