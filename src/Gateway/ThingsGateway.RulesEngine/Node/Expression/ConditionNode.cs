
using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;



namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Expression", ImgUrl = "_content/ThingsGateway.RulesEngine/img/CSharpScript.svg", Desc = nameof(ConditionNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(CSharpScriptWidget))]
public class ConditionNode : TextNode, IConditionNode
{
    public ConditionNode(string id, Point? position = null) : base(id, position) { Title = "ConditionNode"; Placeholder = "ConditionNode.Placeholder"; }

    Task<bool> IConditionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        var value = Text.GetExpressionsResult(input.Value, LogMessage);
        var next = value.ToBoolean(false);
        LogMessage?.Trace($"Condition result: {next}");
        return Task.FromResult(next);
    }

}
