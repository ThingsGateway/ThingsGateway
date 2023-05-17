using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 上传设备线程管理
/// </summary>
public class UploadDeviceThread : IDisposable
{
    private IServiceScopeFactory _scopeFactory;
    /// <summary>
    /// <inheritdoc cref="UploadDeviceThread"/>
    /// </summary>
    public UploadDeviceThread(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    /// <summary>
    /// 上传设备List
    /// </summary>
    public ConcurrentList<UploadDeviceCore> UploadDeviceCores { get; private set; } = new();
    /// <summary>
    /// CancellationTokenSources
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    
    /// <summary>
    /// 默认等待间隔时间
    /// </summary>
    public static int CycleInterval { get; } =500;
    /// <summary>
    /// 初始化
    /// </summary>
    protected void InitTask()
    {
        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        DeviceTask = new Task<Task>(async () =>
        {
            await Task.Yield();//
            foreach (var device in UploadDeviceCores)
            {
                if (device.Driver == null)
                    return;
                try
                {
                    LoggerGroup log = UploadDeviceCores.FirstOrDefault().Driver.TouchSocketConfig.Container.Resolve<ILog>() as LoggerGroup;
                    var data = new EasyLogger(device.Driver.NewMessage);
                    log.AddLogger(device.DeviceId.ToString(), data);
                }
                catch (Exception ex)
                {
                    device.Logger?.LogError(ex, "报文日志添加失败");
                }
                await device.BeforeActionAsync();
            }
            while (!UploadDeviceCores.All(a => a.IsExited))
            {
                if (UploadDeviceCores.All(a => a.DisposedValue))
                {
                    break;
                }
                foreach (var device in UploadDeviceCores)
                {
                    if (device.IsInitSuccess)
                    {
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
    /// 开始
    /// </summary>
    public virtual void StartThread()
    {
        lock (this)
        {
            StoppingTokens.Add(new());
            InitTask();
            DeviceTask?.Start();
        }
    }

    /// <summary>
    /// 停止
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
                foreach (var device in UploadDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}上传线程停止超时，已强制取消");
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
        UploadDeviceCores.ForEach(a => a.Dispose());
        UploadDeviceCores.Clear();
    }
}
