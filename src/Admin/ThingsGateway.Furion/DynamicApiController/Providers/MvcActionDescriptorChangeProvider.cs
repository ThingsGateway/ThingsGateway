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

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace ThingsGateway.DynamicApiController;

/// <summary>
/// MVC 控制器感知提供器
/// </summary>
[SuppressSniffer]
public class MvcActionDescriptorChangeProvider : IActionDescriptorChangeProvider
{
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationChangeToken _stoppingToken;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MvcActionDescriptorChangeProvider()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _stoppingToken = new CancellationChangeToken(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// 获取改变 ChangeToken
    /// </summary>
    /// <returns></returns>
    public IChangeToken GetChangeToken()
    {
        return _stoppingToken;
    }

    /// <summary>
    /// 通知变化
    /// </summary>
    public void NotifyChanges()
    {
        var oldCancellationTokenSource = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
        _stoppingToken = new CancellationChangeToken(_cancellationTokenSource.Token);
        oldCancellationTokenSource.Cancel();
    }
}