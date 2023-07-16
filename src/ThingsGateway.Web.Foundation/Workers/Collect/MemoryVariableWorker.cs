#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 实时数据库后台服务
/// </summary>
public class MemoryVariableWorker : BackgroundService
{
    private static IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MemoryVariableWorker> _logger;
    private GlobalDeviceData _globalDeviceData;
    private IVariableService _variableService;
    /// <inheritdoc cref="MemoryVariableWorker"/>
    public MemoryVariableWorker(ILogger<MemoryVariableWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _globalDeviceData = scopeFactory.CreateScope().ServiceProvider.GetService<GlobalDeviceData>();
        _variableService = scopeFactory.CreateScope().ServiceProvider.GetService<IVariableService>();
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");

    #region worker服务
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("中间变量服务启动");
        await base.StartAsync(cancellationToken);
    }
    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("中间变量服务停止");
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
        }
    }


    #endregion

    #region core
    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    private Task<Task> MemoryWorkerTask;
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        MemoryWorkerTask = new Task<Task>(async () =>
        {
            await Task.Yield();//
            _logger?.LogInformation($"中间变量计算线程开始");

            try
            {
                var data = await _variableService.GetMemoryVariableRuntimeAsync();
                _globalDeviceData.MemoryVariables = new(data);
                StatuString = OperResult.CreateSuccessResult();
                while (!StoppingToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(500, StoppingToken.Token);

                        if (StoppingToken.Token.IsCancellationRequested)
                            break;
                        var isSuccess = true;
                        foreach (var item in _globalDeviceData.MemoryVariables)
                        {
                            //变量内部已经做了表达式转换，直接赋值0
                            var operResult = item.SetValue(0);
                            if (!operResult.IsSuccess)
                            {
                                if (StatuString.IsSuccess)
                                    _logger?.LogWarning(operResult.Message, ToString());
                                isSuccess = false;
                                StatuString = operResult;
                            }
                        }
                        if (isSuccess)
                            StatuString = OperResult.CreateSuccessResult();
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, $"历史数据循环异常");
                        StatuString = new OperResult(ex);
                    }
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
                _logger?.LogError(ex, $"中间变量计算线程循环异常");
            }
        }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 重新启动服务
    /// </summary>
    public void Restart()
    {
        Stop();
        Start();
    }

    internal void Start()
    {
        StoppingTokens.Add(new());
        Init();
        MemoryWorkerTask.Start();
    }

    internal void Stop()
    {
        CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
        StoppingToken?.Cancel();

        _logger?.LogInformation($"中间变量计算线程停止中");
        var hisHisResult = MemoryWorkerTask?.GetAwaiter().GetResult();
        bool? taskResult = false;
        try
        {
            taskResult = hisHisResult?.Wait(10000);
        }
        catch (ObjectDisposedException)
        {

        }
        catch (Exception ex)
        {
            _logger?.LogInformation(ex, "等待线程停止错误");
        }
        if (taskResult == true)
        {
            _logger?.LogInformation($"中间变量计算线程已停止");
        }
        else
        {
            _logger?.LogInformation($"历史数据线程停止超时，已强制取消");
        }
        MemoryWorkerTask?.SafeDispose();
        StoppingToken?.SafeDispose();
        StoppingTokens.Remove(StoppingToken);

    }



    #endregion
}


