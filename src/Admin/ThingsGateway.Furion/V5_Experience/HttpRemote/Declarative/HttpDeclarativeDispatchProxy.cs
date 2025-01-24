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

using System.Reflection;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式远程请求代理类
/// </summary>
public class HttpDeclarativeDispatchProxy : DispatchProxyAsync
{
    /// <inheritdoc cref="IHttpRemoteService" />
    public IHttpRemoteService RemoteService { get; internal set; } = null!;

    /// <inheritdoc />
    public override object Invoke(MethodInfo method, object[] args) => RemoteService.Declarative(method, args)!;

    /// <inheritdoc />
    public override async Task InvokeAsync(MethodInfo method, object[] args) =>
        _ = await InvokeAsyncT<VoidContent>(method, args).ConfigureAwait(false);

    /// <inheritdoc />
    public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args) =>
        (await RemoteService.DeclarativeAsync<T>(method, args).ConfigureAwait(false))!;
}