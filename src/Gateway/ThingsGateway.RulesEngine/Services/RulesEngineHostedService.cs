//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;
public class RulesLog
{
    public RulesLog(Rules rules, TextFileLogger log)
    {
        Log = log;
        Rules = rules;
    }

    public TextFileLogger Log { get; set; }
    public Rules Rules { get; set; }
}

internal sealed class RulesEngineHostedService : BackgroundService, IRulesEngineHostedService
{
    internal string LogPathFormat = "Logs/RulesEngineLog/{0}";
    private readonly ILogger _logger;
    /// <inheritdoc cref="RulesEngineHostedService"/>
    public RulesEngineHostedService(ILogger<RulesEngineHostedService> logger, IStringLocalizer<RulesEngineHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    private IStringLocalizer Localizer { get; }

    /// <summary>
    /// 重启锁
    /// </summary>
    private WaitLock RestartLock { get; } = new();
    private List<Rules> Rules { get; set; } = new();
    public Dictionary<RulesLog, BlazorDiagram> BlazorDiagrams { get; private set; } = new();

    public async Task Edit(Rules rules)
    {
        try
        {
            await Delete(new List<long>() { rules.Id }).ConfigureAwait(false);
            if (rules.Status)
            {
                var data = Init(rules);
                await Start(data.rulesLog, data.blazorDiagram, default).ConfigureAwait(false);
                var service = App.GetService<IDispatchService<Rules>>();
                service.Dispatch(new());
            }

        }
        finally
        {
        }
    }

    public async Task Delete(IEnumerable<long> ids)
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            var dels = BlazorDiagrams.Where(a => ids.Contains(a.Key.Rules.Id)).ToArray();
            foreach (var del in dels)
            {
                if (del.Value != null)
                {
                    foreach (var nodeModel in del.Value.Nodes)
                    {
                        nodeModel.TryDispose();
                    }
                    del.Value.TryDispose();
                    BlazorDiagrams.Remove(del.Key);
                }
            }
            var service = App.GetService<IDispatchService<Rules>>();
            service.Dispatch(new());
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    private (RulesLog rulesLog, BlazorDiagram blazorDiagram) Init(Rules rules)
    {
        var log = TextFileLogger.GetMultipleFileLogger(string.Format(LogPathFormat, rules.Id));
        log.LogLevel = TouchSocket.Core.LogLevel.Trace;
        BlazorDiagram blazorDiagram = new();
        RuleHelpers.Load(blazorDiagram, rules.RulesJson);
        var result = (new RulesLog(rules, log), blazorDiagram);
        BlazorDiagrams.Add(result.Item1, blazorDiagram);

        return result;
    }
    private static async Task Start(RulesLog rulesLog, BlazorDiagram item, CancellationToken cancellationToken)
    {
        var startNodes = item.Nodes.Where(a => a is StartNode);
        startNodes.ForEach(a => (a as INode).LogMessage = rulesLog.Log);
        foreach (var link in startNodes.SelectMany(a => a.PortLinks))
        {
            rulesLog.Log.Trace("Start");
            await Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput(), rulesLog.Log, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task Analysis(NodeModel targetNode, NodeInput input, ILog log, CancellationToken cancellationToken)
    {
        (targetNode as INode).LogMessage = log;
        try
        {
            if (targetNode == null)
                return;
            if (targetNode is IConditionNode conditionNode)
            {
                var next = await conditionNode.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
                if (next)
                {
                    foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                    {
                        await Analysis((link.Target.Model as PortModel)?.Parent, input, log, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else if (targetNode is IExpressionNode expressionNode)
            {
                var nodeOutput = await expressionNode.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
                foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                {
                    await Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = nodeOutput.Value, }, log, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (targetNode is IActuatorNode actuatorNode)
            {
                var nodeOutput = await actuatorNode.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
                foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                {
                    await Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = nodeOutput.Value }, log, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (targetNode is ITriggerNode triggerNode)
            {

                Func<NodeOutput, Task> func = (async a =>
                {
                    foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                    {
                        await Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = a.Value }, log, cancellationToken).ConfigureAwait(false);
                    }
                });
                await triggerNode.StartAsync(func).ConfigureAwait(false);

            }

        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            log.LogWarning(ex);
        }
    }


    #region worker服务

    private async Task BefortStart(CancellationToken cancellationToken)
    {
        AfterStop();

        Rules = App.GetService<IRulesService>().GetAll();
        BlazorDiagrams = new();
        foreach (var rules in Rules.Where(a => a.Status))
        {
            var item = Init(rules);
            await Start(item.rulesLog, item.blazorDiagram, cancellationToken).ConfigureAwait(false);
        }
        var service = App.GetService<IDispatchService<Rules>>();
        service.Dispatch(new());

        _ = Task.Factory.StartNew(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var item in BlazorDiagrams?.Values?.SelectMany(a => a.Nodes) ?? new List<NodeModel>())
                {
                    if (item is IExexcuteExpressionsBase)
                    {
                        CSharpScriptEngineExtension.SetExpire((item as TextNode).Text);
                    }
                }
                await Task.Delay(60000, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);

    }

    private void AfterStop()
    {
        foreach (var item in BlazorDiagrams.Values)
        {
            foreach (var nodeModel in item.Nodes)
            {
                nodeModel.TryDispose();
            }
        }
        BlazorDiagrams.Clear();
    }

    private CancellationTokenSource? TokenSource { get; set; }

    private void Cancel()
    {
        if (TokenSource != null)
        {
            TokenSource.Cancel();
            TokenSource.Dispose();
            TokenSource = null;
        }
    }

    internal async Task StartAsync()
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码
            TokenSource ??= new CancellationTokenSource();
            await BefortStart(TokenSource.Token).ConfigureAwait(false);
            _logger.LogInformation(Localizer["RulesEngineTaskStart"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Start"); // 记录错误日志
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    internal async Task StopAsync()
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码
            Cancel();
            AfterStop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop"); // 记录错误日志
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return StartAsync();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return StopAsync();
    }


    #endregion worker服务
}
