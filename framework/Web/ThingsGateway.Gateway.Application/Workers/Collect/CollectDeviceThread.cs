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
/// 采集设备线程管理
/// </summary>
public class CollectDeviceThread : IAsyncDisposable
{
    /// <summary>
    /// 链路标识
    /// </summary>
    public readonly string ChangelID;

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
    /// <inheritdoc/>
    /// </summary>
    public CollectDeviceThread(string changelID)
    {
        ChangelID = changelID;
    }
    /// <summary>
    /// 默认等待间隔时间
    /// </summary>
    public static int CycleInterval { get; } = 10;

    /// <summary>
    /// 采集设备List，在CollectDeviceThread开始前应该初始化内容
    /// </summary>
    public ConcurrentList<CollectDeviceCore> CollectDeviceCores { get; private set; } = new();
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
    /// 开始采集
    /// </summary>
    public virtual async Task StartThreadAsync()
    {
        try
        {
            await easyLock.WaitAsync();

            StoppingTokens.Add(new());
            //初始化采集线程
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
    /// 停止采集
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
                foreach (var device in CollectDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}采集线程停止超时，已强制取消");
                    await device.FinishActionAsync();
                }
            }
            catch (Exception ex)
            {
                CollectDeviceCores.FirstOrDefault()?.Logger?.LogError(ex, $"{CollectDeviceCores.FirstOrDefault()?.Device?.Name}采集线程停止错误");
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
                var channelResult = CollectDeviceCores.FirstOrDefault().Driver.GetShareChannel();
                LoggerGroup log = CollectDeviceCores.FirstOrDefault().Driver.LogMessage;
                foreach (var device in CollectDeviceCores)
                {
                    if (device.Driver == null)
                    {
                        continue;
                    }

                    //添加通道报文到每个设备
                    var data = new EasyLogger(device.Driver.NewMessage) { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace };
                    log.AddLogger(data);
                    //传入是否共享通道
                    device.IsShareChannel = CollectDeviceCores.Count > 1;
                    if (channelResult.IsSuccess)
                    {
                        await device.BeforeActionAsync(stoppingToken, channelResult.Content);
                    }
                    else
                    {
                        await device.BeforeActionAsync(stoppingToken);
                    }
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var device in CollectDeviceCores)
                    {
                        try
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;
                            //初始化成功才能执行
                            if (device.IsInitSuccess)
                            {
                                //如果是共享通道类型，需要每次转换时切换适配器
                                if (device.IsShareChannel) device.Driver.InitDataAdapter();

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
                                await Task.Delay(1000, stoppingToken);
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
                    foreach (var device in CollectDeviceCores)
                    {
                        //如果插件还没释放，执行一次结束函数
                        if (!device.Driver.DisposedValue)
                            await device.FinishActionAsync();
                    }
                    CollectDeviceCores.Clear();
                }
            }
        }
 , TaskCreationOptions.LongRunning);
    }
}
