
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Actuator", ImgUrl = "_content/ThingsGateway.RulesEngine/img/CSharpScript.svg", Desc = nameof(ExecuteScriptNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(CSharpScriptWidget))]
public class ExecuteScriptNode : TextNode, IActuatorNode, IExexcuteExpressionsBase
{
    public ExecuteScriptNode(string id, Point? position = null) : base(id, position) { Title = "ExecuteScriptNode"; Placeholder = "ExecuteScriptNode.Placeholder"; }

    Task<NodeOutput> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        LogMessage?.Trace($"Execute script");
        var exexcuteExpressions = CSharpScriptEngineExtension.Do<IExexcuteExpressions>(Text);
        exexcuteExpressions.Logger = LogMessage;
        return exexcuteExpressions.ExecuteAsync(input, cancellationToken);

    }
}
