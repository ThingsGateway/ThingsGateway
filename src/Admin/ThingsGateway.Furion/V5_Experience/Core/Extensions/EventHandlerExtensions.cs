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

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="EventHandler{TEventArgs}" /> 拓展类
/// </summary>
internal static class EventHandlerExtensions
{
    /// <summary>
    ///     尝试执行事件处理程序
    /// </summary>
    /// <param name="handler">
    ///     <see cref="EventHandler{TEventArgs}" />
    /// </param>
    /// <param name="sender">
    ///     <see cref="object" />
    /// </param>
    /// <param name="args">
    ///     <typeparamref name="TEventArgs" />
    /// </param>
    /// <typeparam name="TEventArgs">事件参数</typeparam>
    internal static void TryInvoke<TEventArgs>(this EventHandler<TEventArgs>? handler, object? sender, TEventArgs args)
    {
        // 空检查
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(sender, args);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }
}