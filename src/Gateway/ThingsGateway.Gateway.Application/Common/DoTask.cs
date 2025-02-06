//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using ThingsGateway.NewLife;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public class DoTask
{
    /// <summary>
    /// 取消令牌
    /// </summary>
    private CancellationTokenSource? _cancelTokenSource;

    public DoTask(Func<CancellationToken, ValueTask> doWork, ILog logger, string taskName = null)
    {
        DoWork = doWork; Logger = logger; TaskName = taskName;
    }

    /// <summary>
    /// 执行任务方法
    /// </summary>
    public Func<CancellationToken, ValueTask> DoWork { get; }
    private ILog Logger { get; }
    private Task PrivateTask { get; set; }
    private string TaskName { get; }

    /// <summary>
    /// 开始
    /// </summary>
    /// <param name="cancellationToken">调度取消令牌</param>
    public void Start(CancellationToken? cancellationToken = null)
    {
        try
        {
            WaitLock.Wait();

            if (cancellationToken != null && cancellationToken.Value.CanBeCanceled)
            {
                _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value);
            }
            else
            {
                _cancelTokenSource = new CancellationTokenSource();
            }

            // 异步执行
            PrivateTask = Task.Run(Do);
        }
        finally
        {
            WaitLock.Release();
        }
    }

    private async Task Do()
    {
        while (!_cancelTokenSource.IsCancellationRequested)
        {
            try
            {
                if (_cancelTokenSource.IsCancellationRequested)
                    return;
                await DoWork(_cancelTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "DoWork");
            }
        }
    }

    private WaitLock WaitLock = new();
    /// <summary>
    /// 停止操作
    /// </summary>
    public async Task StopAsync(TimeSpan? waitTime = null)
    {
        try
        {
            await WaitLock.WaitAsync().ConfigureAwait(false);

            try
            {
                _cancelTokenSource?.Cancel();
                _cancelTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "Cancel error");
            }

            if (PrivateTask != null)
            {
                try
                {
                    if (TaskName != null)
                        Logger?.LogInformation($"{TaskName} Stoping");
                    if (waitTime != null)
                        await PrivateTask.WaitAsync(waitTime.Value).ConfigureAwait(false);
                    if (TaskName != null)
                        Logger?.LogInformation($"{TaskName} Stoped");
                }
                catch (ObjectDisposedException)
                {
                }
                catch (TimeoutException)
                {
                    if (TaskName != null)
                        Logger?.LogWarning($"{TaskName} Stop timeout, exiting wait block");
                }
                catch (Exception ex)
                {
                    if (TaskName != null)
                        Logger?.LogWarning(ex, $"{TaskName} Stop error");
                }
                PrivateTask = null;

            }
        }
        finally
        {
            WaitLock.Release();
        }
    }
}
