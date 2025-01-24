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

using ThingsGateway.Extensions;

namespace ThingsGateway.HttpRemote;

/// <inheritdoc cref="IHttpContentConverterFactory" />
internal sealed class HttpContentConverterFactory : IHttpContentConverterFactory
{
    /// <summary>
    ///     <see cref="IHttpContentConverter{TResult}" /> 字典集合
    /// </summary>
    internal readonly Dictionary<Type, IHttpContentConverter> _converters;

    /// <summary>
    ///     <inheritdoc cref="HttpContentConverterFactory" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="converters"><see cref="IHttpContentConverter{TResult}" /> 数组</param>
    public HttpContentConverterFactory(IServiceProvider serviceProvider, IHttpContentConverter[]? converters)
    {
        ServiceProvider = serviceProvider;

        // 初始化响应内容转换器
        _converters = new Dictionary<Type, IHttpContentConverter>
        {
            [typeof(HttpResponseMessageConverter)] = new HttpResponseMessageConverter(),
            [typeof(StringContentConverter)] = new StringContentConverter(),
            [typeof(ByteArrayContentConverter)] = new ByteArrayContentConverter(),
            [typeof(StreamContentConverter)] = new StreamContentConverter(),
            [typeof(VoidContentConverter)] = new VoidContentConverter()
        };

        // 添加自定义 IHttpContentConverter 数组
        _converters.TryAdd(converters, value => value.GetType());
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public TResult? Read<TResult>(HttpResponseMessage httpResponseMessage, IHttpContentConverter[]? converters = null,
        CancellationToken cancellationToken = default) =>
        GetConverter<TResult>(converters).Read(httpResponseMessage, cancellationToken);

    /// <inheritdoc />
    public object? Read(Type resultType, HttpResponseMessage httpResponseMessage,
        IHttpContentConverter[]? converters = null,
        CancellationToken cancellationToken = default) =>
        GetConverter(resultType, converters).Read(resultType, httpResponseMessage, cancellationToken);

    /// <inheritdoc />
    public async Task<TResult?> ReadAsync<TResult>(HttpResponseMessage httpResponseMessage,
        IHttpContentConverter[]? converters = null,
        CancellationToken cancellationToken = default) =>
        await GetConverter<TResult>(converters).ReadAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<object?> ReadAsync(Type resultType, HttpResponseMessage httpResponseMessage,
        IHttpContentConverter[]? converters = null,
        CancellationToken cancellationToken = default) =>
        await GetConverter(resultType, converters).ReadAsync(resultType, httpResponseMessage, cancellationToken).ConfigureAwait(false);

    /// <summary>
    ///     获取 <see cref="IHttpContentConverter{TResult}" /> 实例
    /// </summary>
    /// <param name="converters"><see cref="IHttpContentConverter{TResult}" /> 数组</param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="IHttpContentConverter{TResult}" />
    /// </returns>
    internal IHttpContentConverter<TResult> GetConverter<TResult>(params IHttpContentConverter[]? converters)
    {
        // 初始化新的 IHttpContentConverter 字典集合
        var unionConverters = new Dictionary<Type, IHttpContentConverter>(_converters);

        // 添加自定义 IHttpContentConverter 数组
        unionConverters.TryAdd(converters, value => value.GetType());

        // 查找可以处理目标类型的响应内容转换器
        var typeConverter = unionConverters.Values.OfType<IHttpContentConverter<TResult>>().LastOrDefault();

        // 如果未找到，则调用 IObjectContentConverterFactory 实例的 GetConverter<TResult> 返回
        var converter = typeConverter ?? ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
            .GetConverter<TResult>();

        // 设置服务提供器
        converter.ServiceProvider ??= ServiceProvider;

        return converter;
    }

    /// <summary>
    ///     获取 <see cref="IHttpContentConverter" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="converters"><see cref="IHttpContentConverter{TResult}" /> 数组</param>
    /// <returns>
    ///     <see cref="IHttpContentConverter" />
    /// </returns>
    internal IHttpContentConverter GetConverter(Type resultType, params IHttpContentConverter[]? converters)
    {
        // 初始化新的 IHttpContentConverter 字典集合
        var unionConverters = new Dictionary<Type, IHttpContentConverter>(_converters);

        // 添加自定义 IHttpContentConverter 数组
        unionConverters.TryAdd(converters, value => value.GetType());

        // 查找可以处理目标类型的响应内容转换器
        var typeConverter = unionConverters.Values.OfType(typeof(IHttpContentConverter<>).MakeGenericType(resultType))
            .Cast<IHttpContentConverter>().LastOrDefault();

        // 如果未找到，则调用 IObjectContentConverterFactory 实例的 GetConverter 返回
        var converter = typeConverter ?? ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
            .GetConverter(resultType);

        // 设置服务提供器
        converter.ServiceProvider ??= ServiceProvider;

        return converter;
    }
}