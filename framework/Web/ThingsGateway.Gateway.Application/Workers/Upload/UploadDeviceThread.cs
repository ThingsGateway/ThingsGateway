#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.Logging;



namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 上传设备线程管理
/// </summary>
public class UploadDeviceThread : IAsyncDisposable
{

    /// <summary>
    /// 线程
    /// </summary>
    protected Task DeviceTask;

    /// <summary>
    /// 启停锁
    /// </summary>
    protected EasyLock easyLock = new();

    /// <summary>
    /// CancellationTokenSources
    /// </summary>
    private ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    /// <summary>
    /// 默认等待间隔时间
    /// </summary>
    public static int CycleInterval { get; } = 10;

    /// <summary>
    /// 上传设备List，在CollectDeviceThread开始前应该初始化内容
    /// </summary>
    public ConcurrentList<UploadDeviceCore> UploadDeviceCores { get; private set; } = new();
    /// <summary>
    /// 停止采集前，提前取消Token
    /// </summary>
    public virtual async Task BeforeStopThreadAsync()
    {
        try
        {
            await easyLock.WaitAsync();

            if (DeviceTask == null)
            {
                return;
            }
            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.Cancel();
            }
        }
        finally
        {
            easyLock.Release();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await StopThreadAsync();
    }

    /// <summary>
    /// 开始上传
    /// </summary>
    public virtual async Task StartThreadAsync()
    {
        try
        {
            await easyLock.WaitAsync();

            StoppingTokens.Add(new());
            //初始化上传线程
            await InitTaskAsync();
            if (DeviceTask.Status == TaskStatus.Created)
                DeviceTask?.Start();
        }
        finally
        {
            easyLock.Release();
        }
    }
    /// <summary>
    /// 停止上传
    /// </summary>
    public virtual async Task StopThreadAsync()
    {
        try
        {
            await easyLock.WaitAsync();

            if (DeviceTask == null)
            {
                return;
            }
            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.Cancel();
            }
            try
            {
                await DeviceTask.WaitAsync(CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (TimeoutException)
            {
                foreach (var device in UploadDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}上传线程停止超时，已强制取消");
                    await device.FinishActionAsync();
                }
            }
            catch (Exception ex)
            {
                UploadDeviceCores.FirstOrDefault()?.Logger?.LogError(ex, $"{UploadDeviceCores.FirstOrDefault()?.Device?.Name}上传线程停止错误");
            }
            foreach (CancellationTokenSource cancellationToken in StoppingTokens)
            {
                cancellationToken?.SafeDispose();
            }
            DeviceTask?.SafeDispose();
            DeviceTask = null;
            StoppingTokens.Clear();
        }
        finally
        {
            easyLock.Release();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected async Task InitTaskAsync()
    {
        var stoppingToken = StoppingTokens.Last().Token;
        DeviceTask = await Task.Factory.StartNew(async () =>
        {
            try
            {
                //await Task.Yield();
                if (UploadDeviceCores.FirstOrDefault().Driver == null)
                {
                    return;
                }

                LoggerGroup log = UploadDeviceCores.FirstOrDefault().Driver.LogMessage;
                foreach (var device in UploadDeviceCores)
                {
                    if (device.Driver == null)
                    {
                        continue;
                    }

                    //添加通道报文到每个设备
                    var data = new EasyLogger(device.Driver.NewMessage) { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace };
                    log.AddLogger(data);
                    await device.BeforeActionAsync(stoppingToken);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var device in UploadDeviceCores)
                    {
                        try
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;
                            //初始化成功才能执行
                            if (device.IsInitSuccess)
                            {

                                var result = await device.RunActionAsync(stoppingToken);
                                if (result == ThreadRunReturn.None)
                                {
                                    await Task.Delay(CycleInterval);
                                }
                                else if (result == ThreadRunReturn.Continue)
                                {
                                    await Task.Delay(1000);
                                }
                                else if (result == ThreadRunReturn.Break)
                                {
                                    //当线程返回Break，直接跳出循环
                                    break;
                                }


                            }
                            else
                            {
                                await Task.Delay(1000);
                            }
                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (ObjectDisposedException)
                        {

                        }
                        catch (Exception ex)
                        {
                            log.Exception(ex);
                        }
                    }
                }
                //注意插件结束函数不能使用取消传播作为条件
                await Stop(stoppingToken);
            }
            finally
            {
                //await Task.Yield();
                await Stop(stoppingToken);
            }
            async Task Stop(CancellationToken stoppingToken)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    foreach (var device in UploadDeviceCores)
                    {
                        //如果插件还没释放，执行一次结束函数
                        if (!device.Driver.DisposedValue)
                            await device.FinishActionAsync();
                    }
                    UploadDeviceCores.Clear();
                }
            }
        }
 , TaskCreationOptions.LongRunning);


    }
}
