
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

public class DoTask
{
    public DoTask(Func<CancellationToken, Task> doWork, ILogger logger, string taskName = null)
    {
        DoWork = doWork; Logger = logger; TaskName = taskName;
    }

    private IStringLocalizer Localizer { get; } = App.CreateLocalizerByType(typeof(DoTask))!;
    private string TaskName { get; }
    private ILogger Logger { get; }

    /// <summary>
    /// 取消令牌
    /// </summary>
    private CancellationTokenSource? _triggerCancelTokenSource;

    /// <summary>
    /// 调度取消令牌
    /// </summary>
    private CancellationToken _schedulerCancelToken;

    /// <summary>
    /// 取消令牌与调度取消令牌合集
    /// </summary>
    private CancellationTokenSource? _cancelTokenSource;

    /// <summary>
    /// 执行任务方法
    /// </summary>
    public Func<CancellationToken, Task> DoWork { get; }

    private Task PrivateTask { get; set; }

    public bool IsStoped => PrivateTask == null;

    /// <summary>
    /// 开始
    /// </summary>
    /// <param name="cancellationToken">调度取消令牌</param>
    public void Start(CancellationToken? cancellationToken = null)
    {
        _schedulerCancelToken = cancellationToken ?? CancellationToken.None;

        _triggerCancelTokenSource = new CancellationTokenSource();
        _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_triggerCancelTokenSource.Token, _schedulerCancelToken);

        // 异步执行
        PrivateTask = Task.Factory.StartNew(async () =>
        {
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                        return;
                    await DoWork(_cancelTokenSource.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
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
        }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 停止操作
    /// </summary>
    public async Task StopAsync(TimeSpan? waitTime = null)
    {
        try
        {
            _triggerCancelTokenSource?.Cancel();
            _cancelTokenSource?.Cancel();
            _triggerCancelTokenSource.Dispose();
            _cancelTokenSource?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        try
        {
            if (PrivateTask != null)
            {
                try
                {
                    if (TaskName != null)
                        Logger?.LogInformation(Localizer[$"Stoping", TaskName]);
                    await PrivateTask.WaitAsync(waitTime ?? TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    if (TaskName != null)
                        Logger?.LogInformation(Localizer[$"Stoped", TaskName]);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (TimeoutException)
                {
                    if (TaskName != null)
                        Logger?.LogWarning(Localizer[$"Timeout", TaskName]);
                }
                catch (Exception ex)
                {
                    if (TaskName != null)
                        Logger?.LogWarning(ex, Localizer[$"Error", TaskName]);
                }
                try
                {
                    PrivateTask?.Dispose();
                    PrivateTask = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
