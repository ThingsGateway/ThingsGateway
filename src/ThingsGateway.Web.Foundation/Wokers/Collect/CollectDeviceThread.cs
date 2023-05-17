using Microsoft.Extensions.Logging;


using System.Linq;
using System.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 采集设备线程管理
/// </summary>
public class CollectDeviceThread : IDisposable
{
    private IServiceScopeFactory _scopeFactory;
    /// <summary>
    /// <inheritdoc cref="CollectDeviceThread"/>
    /// </summary>
    public CollectDeviceThread(IServiceScopeFactory scopeFactory, string changelID)
    {
        _scopeFactory = scopeFactory;
        ChangelID = changelID;
    }
    /// <summary>
    /// 采集设备List
    /// </summary>
    public ConcurrentList<CollectDeviceCore> CollectDeviceCores { get; private set; } = new();
    /// <summary>
    /// 链路标识
    /// </summary>
    public readonly string ChangelID;
    /// <summary>
    /// CancellationTokenSources
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    /// <summary>
    /// 默认等待间隔时间
    /// </summary>
    public static int CycleInterval { get; } = 100;
    /// <summary>
    /// 初始化
    /// </summary>
    protected void InitTask()
    {
        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        DeviceTask = new Task<Task>(async () =>
        {
            await Task.Yield();//
            var channelResult = CollectDeviceCores.FirstOrDefault().Driver.GetShareChannel();
            foreach (var device in CollectDeviceCores)
            {

                if (device.Driver == null)
                    return;
                try
                {
                    LoggerGroup log = CollectDeviceCores.FirstOrDefault().Driver.TouchSocketConfig.Container.Resolve<ILog>() as LoggerGroup;
                    var data = new EasyLogger(device.Driver.NewMessage);
                    log.AddLogger(device.DeviceId.ToString(), data);
                }
                catch (Exception ex)
                {
                    device.Logger?.LogError(ex, "报文日志添加失败");
                }
                device.IsShareChannel = CollectDeviceCores.Count > 1;
                if (channelResult.IsSuccess)
                {
                    await device.BeforeActionAsync(channelResult.Content);
                }
                else
                {
                    await device.BeforeActionAsync();
                }
            }
            while (!CollectDeviceCores.All(a => a.IsExited))
            {
                if (CollectDeviceCores.All(a => a.DisposedValue))
                {
                    break;
                }
                foreach (var device in CollectDeviceCores)
                {
                    if (device.IsInitSuccess)
                    {
                        if (CollectDeviceCores.Count > 1) device.Driver.InitDataAdapter();
                        var result = await device.RunActionAsync(StoppingToken);
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
                            if (!device.IsExited)
                                device.FinishAction();
                        }
                        if (CollectDeviceCores.Count > 1)
                            await Task.Delay(1000);//对于共享链路设备需延时，避免物理链路比如LORA等设备寻找失败
                    }
                    else
                    {
                        if (!device.IsExited)
                            device.FinishAction();
                    }
                }

            }

        }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 线程
    /// </summary>
    protected Task<Task> DeviceTask;

    /// <summary>
    /// 开始采集
    /// </summary>
    public virtual void StartThread()
    {
        lock (this)
        {
            StoppingTokens.Add(new());
            //初始化采集线程
            InitTask();
            DeviceTask?.Start();
        }
    }

    /// <summary>
    /// 停止采集
    /// </summary>
    public virtual void StopThread()
    {
        lock (this)
        {
            if (DeviceTask == null)
            {
                return;
            }
            CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
            StoppingToken?.Cancel();
            StoppingToken?.SafeDispose();
            if (DeviceTask.GetAwaiter().GetResult()?.Wait(10000) != true)
            {
                foreach (var device in CollectDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}采集线程停止超时，已强制取消");
                }
            }
            DeviceTask?.Dispose();
            DeviceTask = null;
            if (StoppingToken != null)
            {
                StoppingTokens.Remove(StoppingToken);
            }
        }
    }
    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        StopThread();
        CollectDeviceCores.ForEach(a => a.Dispose());
        CollectDeviceCores.Clear();
    }
}
