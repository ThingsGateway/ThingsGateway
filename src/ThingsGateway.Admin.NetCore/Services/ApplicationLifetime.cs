// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Admin.NetCore;

public sealed class ApplicationLifetime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
    private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
    private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
    private readonly ILogger<ApplicationLifetime> _logger;

    public ApplicationLifetime(ILogger<ApplicationLifetime> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Triggered when the application host has fully started and is about to wait
    /// for a graceful shutdown.
    /// </summary>
    public CancellationToken ApplicationStarted => _startedSource.Token;

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// Request may still be in flight. Shutdown will block until this event completes.
    /// </summary>
    public CancellationToken ApplicationStopping => _stoppingSource.Token;

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// All requests should be complete at this point. Shutdown will block
    /// until this event completes.
    /// </summary>
    public CancellationToken ApplicationStopped => _stoppedSource.Token;

    /// <summary>
    /// Signals the ApplicationStopping event and blocks until it completes.
    /// </summary>
    public void StopApplication()
    {
        // Lock on CTS to synchronize multiple calls to StopApplication. This guarantees that the first call
        // to StopApplication and its callbacks run to completion before subsequent calls to StopApplication,
        // which will no-op since the first call already requested cancellation, get a chance to execute.
        lock (_stoppingSource)
        {
            ExecuteHandlers(_stoppingSource);
        }
    }

    /// <summary>
    /// Signals the ApplicationStarted event and blocks until it completes.
    /// </summary>
    public void NotifyStarted()
    {
        ExecuteHandlers(_startedSource);
    }

    /// <summary>
    /// Signals the ApplicationStopped event and blocks until it completes.
    /// </summary>
    public void NotifyStopped()
    {
        ExecuteHandlers(_stoppedSource);
    }

    private static void ExecuteHandlers(CancellationTokenSource cancel)
    {
        // Noop if this is already cancelled
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        // Run the cancellation token callbacks
        cancel.Cancel(throwOnFirstException: false);
    }
}
