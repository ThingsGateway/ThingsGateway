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

namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 设备线程管理
/// </summary>
public class DeviceThread
{
    /// <summary>
    /// 线程最小等待间隔时间
    /// </summary>
    public const int CycleInterval = 10;

    public DeviceThread(string changelID)
    {
        ChangelID = changelID;
    }

    public string ChangelID { get; private set; }
    /// <summary>
    /// 插件集合
    /// </summary>
    public ConcurrentList<DriverBase> DriverBases { get; private set; } = new();

    /// <summary>
    /// 启停锁
    /// </summary>
    public EasyLock EasyLock { get; set; } = new();
    /// <summary>
    /// 设备单线程
    /// </summary>
    protected Task _driverTask { get; set; }

    private ConcurrentList<CancellationTokenSource> _cancellationTokenSources { get; set; } = new();

    /// <summary>
    /// 停止插件前，执行取消传播
    /// </summary>
    public virtual void BeforeStopThread()
    {
        lock (_cancellationTokenSources)
        {
            foreach (var cancellationToken in _cancellationTokenSources)
            {
                cancellationToken?.Cancel();
                cancellationToken?.SafeDispose();
            }
            _cancellationTokenSources.Clear();
        }

    }

    /// <summary>
    /// 开始上传
    /// </summary>
    public virtual async Task StartThreadAsync()
    {
        try
        {
            await EasyLock.WaitAsync();
            _cancellationTokenSources.Add(new());
            //初始化上传线程
            await InitTaskAsync();
            if (_driverTask.Status == TaskStatus.Created)
                _driverTask?.Start();
        }
        finally
        {
            EasyLock.Release();
        }
    }

    /// <summary>
    /// 停止上传
    /// </summary>
    public virtual async Task StopThreadAsync()
    {

        if (_driverTask == null)
        {
            return;
        }
        try
        {
            BeforeStopThread();

            await EasyLock.WaitAsync();
            await _driverTask.WaitAsync(CancellationToken.None);
            _driverTask?.SafeDispose();
            _driverTask = null;
        }
        finally
        {
            EasyLock.Release();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected async Task InitTaskAsync()
    {
        var stoppingToken = _cancellationTokenSources.Last().Token;
        _driverTask = await Task.Factory.StartNew(async () =>
        {
            try
            {
                LoggerGroup log = DriverBases.FirstOrDefault().LogMessage;
                var channelResult = DriverBases.FirstOrDefault().DriverPropertys.GetShareChannel(DriverBases.FirstOrDefault().FoundataionConfig);
                foreach (var driver in DriverBases)
                {
                    //添加通道报文到每个设备
                    var data = new EasyLogger(driver.AddMessageItem) { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace };
                    log.AddLogger(data);
                    await driver.BeforStartAsync(channelResult, stoppingToken);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var device in DriverBases)
                    {
                        try
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;
                            //初始化成功才能执行
                            if (device.IsInitSuccess)
                            {
                                var result = await device.ExecuteAsync(stoppingToken);
                                if (result == ThreadRunReturn.None)
                                {
                                    //4.0.0.7版本添加离线恢复的间隔时间
                                    if (device.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && device is not UpLoadBase)
                                        await Task.Delay(Math.Min(device.DriverPropertys.ReIntervalTime, DeviceWorker.CheckIntervalTime / 2) * 1000 - CycleInterval, stoppingToken);
                                    else
                                        await Task.Delay(CycleInterval, stoppingToken);
                                }
                                else if (result == ThreadRunReturn.Continue)
                                {
                                    await Task.Delay(1000, stoppingToken);
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
                    }

                    if (stoppingToken.IsCancellationRequested)
                        break;
                }
                //注意插件结束函数不能使用取消传播作为条件
                await Stop(stoppingToken);
            }
            finally
            {
                await Stop(stoppingToken);
            }
            async Task Stop(CancellationToken stoppingToken)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    foreach (var device in DriverBases)
                    {
                        //如果插件还没释放，执行一次结束函数
                        if (!device.DisposedValue)
                            await device.AfterStopAsync();
                    }
                    DriverBases.Clear();
                }
            }
        }
 , TaskCreationOptions.LongRunning);


    }
}
