//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ThingsGateway.Blazor.Diagrams.Core;
using ThingsGateway.Blazor.Diagrams.Core.Models;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

internal sealed class RulesEngineHostedService : BackgroundService, IRulesEngineHostedService
{
    internal const string LogPathFormat = "Logs/RulesEngineLog/{0}";
    internal const string LogDir = "Logs/RulesEngineLog";
    private readonly ILogger _logger;
    private IDispatchService<Rules> dispatchService;

    /// <inheritdoc cref="RulesEngineHostedService"/>
    public RulesEngineHostedService(ILogger<RulesEngineHostedService> logger, IStringLocalizer<RulesEngineHostedService> localizer)
    {
        dispatchService = App.GetService<IDispatchService<Rules>>();
        _logger = logger;
        Localizer = localizer;
    }

    private IStringLocalizer Localizer { get; }

    /// <summary>
    /// 重启锁
    /// </summary>
    private WaitLock RestartLock { get; } = new(nameof(RulesEngineHostedService));
    private List<Rules> Rules { get; set; } = new();
    public NonBlockingDictionary<RulesLog, Diagram> Diagrams { get; private set; } = new();
    public Task<RulesLog> GetRulesLogAsync(long rulesId)
    {
        var data = Diagrams.Select(a => a.Key).FirstOrDefault(a => a.Rules.Id == rulesId);
        return Task.FromResult(data);
    }


    public Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId)
    {
        var data = Diagrams.Select(a => a.Key).FirstOrDefault(a => a.Rules.Id == rulesId);
        return Task.FromResult(data?.Log?.LogLevel ?? TouchSocket.Core.LogLevel.Info);
    }

    public Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel)
    {
        var data = Diagrams.Select(a => a.Key).FirstOrDefault(a => a.Rules.Id == rulesId);
        if (data?.Log != null)
            data.Log.LogLevel = logLevel;

        return Task.CompletedTask;
    }

    public Task<string> RulesLogPathAsync(long rulesId)
    {
        var data = Diagrams.Select(a => a.Key).FirstOrDefault(a => a.Rules.Id == rulesId);
        return Task.FromResult(data?.Log?.LogPath);
    }

    public Task<Rules> GetRuleRuntimesAsync(long rulesId)
    {
        var data = Diagrams.Select(a => a.Key).FirstOrDefault(a => a.Rules.Id == rulesId);
        return Task.FromResult(data?.Rules);
    }

    public async Task EditRuleRuntimesAsync(Rules rules)
    {
        await DeleteRuleRuntimesAsync(new List<long>() { rules.Id }).ConfigureAwait(false);
        if (rules.Status)
        {
            var data = Init(rules);
            TaskStart(data.rulesLog, data.blazorDiagram, TokenSource.Token);
            dispatchService.Dispatch(null);
        }
    }

    public async Task DeleteRuleRuntimesAsync(List<long> ids)
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            var dels = Diagrams.Where(a => ids.Contains(a.Key.Rules.Id)).ToArray();
            foreach (var del in dels)
            {
                if (del.Value != null)
                {
                    foreach (var nodeModel in del.Value.Nodes)
                    {
                        nodeModel.TryDispose();
                    }
                    del.Value.TryDispose();
                    Diagrams.TryRemove(del.Key, out _);
                }
            }
            dispatchService.Dispatch(null);
        }
        finally
        {
            RestartLock.Release(); // 释放锁
        }
    }

    private (RulesLog rulesLog, DefaultDiagram blazorDiagram) Init(Rules rules)
    {
#pragma warning disable CA1863
        var log = TextFileLogger.GetMultipleFileLogger(string.Format(LogPathFormat, rules.Name.SanitizeFileName()));
#pragma warning restore CA1863
        log.LogLevel = TouchSocket.Core.LogLevel.Trace;
        DefaultDiagram blazorDiagram = new();
        RuleHelpers.Load(blazorDiagram, rules.RulesJson ?? new());
        var result = (new RulesLog(rules, log), blazorDiagram);
        Diagrams.TryAdd(result.Item1, blazorDiagram);

        return result;
    }
    private static void TaskStart(RulesLog rulesLog, DefaultDiagram item, CancellationToken cancellationToken)
    {
        rulesLog.Log.Trace("Start");
        var startNodes = item.Nodes.Where(a => a is IStartNode);
        foreach (var link in startNodes.SelectMany(a => a.PortLinks))
        {
            _ = Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput(), rulesLog, cancellationToken);
        }
    }

    private static Task Analysis(NodeModel targetNode, NodeInput input, RulesLog rulesLog, CancellationToken cancellationToken)
    {
        return Analysis(targetNode, input, rulesLog, cancellationToken);


        static async PooledTask Analysis(NodeModel targetNode, NodeInput input, RulesLog rulesLog, CancellationToken cancellationToken)
        {
            if (targetNode is INode node)
            {
                node.Logger = rulesLog.Log;
                node.RulesEngineName = rulesLog.Rules.Name;
            }

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
                            await RulesEngineHostedService.Analysis((link.Target.Model as PortModel)?.Parent, input, rulesLog, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else if (targetNode is IExpressionNode expressionNode)
                {
                    var nodeOutput = await expressionNode.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
                    if (nodeOutput.IsSuccess)
                    {
                        foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                        {
                            await RulesEngineHostedService.Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = nodeOutput.Content.Value, }, rulesLog, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else if (targetNode is IActuatorNode actuatorNode)
                {
                    var nodeOutput = await actuatorNode.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
                    if (nodeOutput.IsSuccess)
                    {
                        foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                        {
                            await RulesEngineHostedService.Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = nodeOutput.Content.Value }, rulesLog, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else if (targetNode is ITriggerNode triggerNode)
                {
                    Func<NodeOutput, CancellationToken, Task> func = (async (a, token) =>
                    {
                        foreach (var link in targetNode.PortLinks.Where(a => ((a.Target.Model as PortModel)?.Parent) != targetNode))
                        {
                            await RulesEngineHostedService.Analysis((link.Target.Model as PortModel)?.Parent, new NodeInput() { Value = a.Value }, rulesLog, token).ConfigureAwait(false);
                        }
                    });
                    await triggerNode.StartAsync(func, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                rulesLog.Log?.LogWarning(ex);
            }

            return;
        }
    }

    #region worker服务

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

    private void Clear()
    {
        foreach (var item in Diagrams.Values)
        {
            foreach (var nodeModel in item.Nodes)
            {
                nodeModel.TryDispose();
            }
        }
        Diagrams.Clear();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        await RestartRuleRuntimeAsync().ConfigureAwait(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var item in Diagrams?.Values?.SelectMany(a => a.Nodes) ?? new List<NodeModel>())
            {
                if (item is IExexcuteExpressionsBase)
                {
                    CSharpScriptEngineExtension.SetExpire((item as TextNode).Text);
                }
            }
            await Task.Delay(60000, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task RestartRuleRuntimeAsync()
    {
        try
        {
            await RestartLock.WaitAsync().ConfigureAwait(false);
            Cancel();
            Clear();
            TokenSource ??= new CancellationTokenSource();

            Rules = await App.GetService<IRulesService>().GetFromDBAsync(a => a).ConfigureAwait(false);
            Diagrams = new();
            foreach (var rules in Rules.Where(a => a.Status))
            {
                var item = Init(rules);
                TaskStart(item.rulesLog, item.blazorDiagram, TokenSource.Token);
            }
            dispatchService.Dispatch(null);

            _logger.LogInformation(ThingsGateway.Gateway.Application.AppResource.RulesEngineTaskStart);
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RestartLock.WaitAsync(cancellationToken).ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码
            Cancel();
            Clear();
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




    #endregion worker服务
}
