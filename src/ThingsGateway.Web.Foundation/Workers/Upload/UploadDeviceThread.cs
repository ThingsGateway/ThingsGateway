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

using System.Linq;
using System.Threading;

using ThingsGateway.Foundation;

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
    public static int CycleInterval { get; } = 10;
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
                    var data = new TGEasyLogger(device.Driver.NewMessage);
                    log.AddLogger(device.DeviceId.ToString(), data);
                }
                catch (Exception ex)
                {
                    device.Logger?.LogError(ex, "报文日志添加失败");
                }
                await device.BeforeActionAsync(StoppingToken.Token);
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
                                    device.FinishAction();
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
            foreach (var token in StoppingTokens)
            {
                token.Cancel();
            }
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
                UploadDeviceCores.FirstOrDefault()?.Logger?.LogError(ex, $"{UploadDeviceCores.FirstOrDefault()?.Device?.Name}上传线程停止错误");
            }
            if (taskResult != true)
            {
                foreach (var device in UploadDeviceCores)
                {
                    device.Logger?.LogInformation($"{device.Device.Name}上传线程停止超时，已强制取消");
                }
            }
            foreach (var token in StoppingTokens)
            {
                token?.SafeDispose();
            }
            DeviceTask?.SafeDispose();
            DeviceTask = null;

            StoppingTokens.Clear();
        }
    }
    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        StopThread();
        UploadDeviceCores.ForEach(a => a.SafeDispose());
        UploadDeviceCores.Clear();
    }
}
