#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using ThingsGateway.Foundation;

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
    public static int CycleInterval { get; } = 10;
    /// <summary>
    /// 初始化
    /// </summary>
    protected void InitTask()
    {
        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        DeviceTask = new Task<Task>(async () =>
        {
            await Task.Yield();
            var channelResult = CollectDeviceCores.FirstOrDefault().Driver.GetShareChannel();
            foreach (var device in CollectDeviceCores)
            {

                if (device.Driver == null)
                    return;
                try
                {
                    LoggerGroup log = CollectDeviceCores.FirstOrDefault().Driver.TouchSocketConfig.Container.Resolve<ILog>() as LoggerGroup;
                    var data = new TGEasyLogger(device.Driver.NewMessage);
                    log.AddLogger(device.DeviceId.ToString(), data);
                }
                catch (Exception ex)
                {
                    device.Logger?.LogError(ex, "报文日志添加失败");
                }
                device.IsShareChannel = CollectDeviceCores.Count > 1;
                if (channelResult.IsSuccess)
                {
                    await device.BeforeActionAsync(StoppingToken.Token, channelResult.Content);
                }
                else
                {
                    await device.BeforeActionAsync(StoppingToken.Token);
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
                        try
                        {
                            var result = await device.RunActionAsync(StoppingToken.Token);
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
                                    await device.FinishActionAsync();
                            }
                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (ObjectDisposedException)
                        {

                        }

                    }
                    else
                    {
                        if (!device.IsExited)
                            await device.FinishActionAsync();
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
            bool? taskResult = false;
            try
            {
                taskResult = DeviceTask.GetAwaiter().GetResult()?.Wait(10000);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                CollectDeviceCores.FirstOrDefault()?.Logger?.LogError(ex, $"{CollectDeviceCores.FirstOrDefault()?.Device?.Name}采集线程停止错误");
            }
            if (taskResult != true)
            {
                foreach (var device in CollectDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}采集线程停止超时，已强制取消");
                }
            }
            StoppingToken?.SafeDispose();

            DeviceTask?.SafeDispose();
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
        CollectDeviceCores.ForEach(a => a.SafeDispose());
        CollectDeviceCores.Clear();

    }
}
