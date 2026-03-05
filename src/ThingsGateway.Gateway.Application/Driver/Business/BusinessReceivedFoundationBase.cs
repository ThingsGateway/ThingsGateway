//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessReceivedFoundationBase : BusinessBase, IReceivedFoundationDevice
{

    /// <summary>
    /// 底层驱动，有可能为null
    /// </summary>
    public abstract IReceivedDevice? ReceivedFoundationDevice { get; }

    public override string ToString()
    {
        return ReceivedFoundationDevice?.ToString() ?? base.ToString();
    }

    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (ReceivedFoundationDevice != null)
            await ReceivedFoundationDevice.SafeDisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public override bool IsConnected()
    {
        return ReceivedFoundationDevice?.OnLine == true;
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        if (ReceivedFoundationDevice != null)
        {
            await ReceivedFoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    protected virtual ValueTask TestOnline(object? state, CancellationToken cancellationToken)
    {
        return TestOnline(this, cancellationToken);

        static async PooledValueTask TestOnline(BusinessReceivedFoundationBase @this, CancellationToken cancellationToken)
        {
            if (@this.ReceivedFoundationDevice != null)
            {
                if (!@this.ReceivedFoundationDevice.OnLine)
                {
                    if (@this.ReceivedFoundationDevice.DisposedValue || @this.ReceivedFoundationDevice.Channel?.DisposedValue != false) return;
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (@this.ReceivedFoundationDevice.DisposedValue || @this.ReceivedFoundationDevice.Channel?.DisposedValue != false) return;

                            await @this.ReceivedFoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);

                            if (@this.CurrentDevice.DeviceStatusChangeTime < TimerX.Now.AddMinutes(-1))
                            {
                                await Task.Delay(30000, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        @this.LogMessage?.LogWarning(ex, "Connect failed");
                    }
                }
            }

        }

    }


    /// <summary>
    /// 获取任务
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作结果的枚举。</returns>
    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {

        var list = base.ProtectedGetTasks(cancellationToken);

        var testOnline = new ScheduledAsyncTask(30000, TestOnline, null, LogMessage, cancellationToken);
        list.Add(testOnline);

        return list;
    }


}
