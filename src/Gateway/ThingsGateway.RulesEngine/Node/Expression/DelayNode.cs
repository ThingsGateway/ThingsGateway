using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Expression", ImgUrl = "_content/ThingsGateway.RulesEngine/img/Delay.svg", Desc = nameof(DelayNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(NumberWidget))]
public class DelayNode : NumberNode, IExpressionNode
{
    public DelayNode(string id, Point? position = null) : base(id, position) { Title = "DelayNode"; Placeholder = "DelayNode.Placeholder"; }

    async Task<NodeOutput> IExpressionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        LogMessage?.Trace($"Delay {Number} ms");
        await Task.Delay(Number ?? 0, cancellationToken).ConfigureAwait(false);
        return new NodeOutput();
    }

}


