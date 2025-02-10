using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Actuator", ImgUrl = "_content/ThingsGateway.RulesEngine/img/Rpc.svg", Desc = nameof(VariableRpcNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
public class VariableRpcNode : TextNode, IActuatorNode
{

    public VariableRpcNode(string id, Point? position = null) : base(id, position)
    { Title = "VariableRpcNode"; Placeholder = "VariableRpcNode.Placeholder"; }

    async Task<NodeOutput> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        if (GlobalData.ReadOnlyVariables.TryGetValue(Text, out var value))
        {
            var data = await value.RpcAsync(input.JToken.ToString(), $"RulesEngine: {RulesEngineName}", cancellationToken).ConfigureAwait(false);
            if (data.IsSuccess)
                LogMessage?.Trace($" VariableRpcNode - VariableName {Text} : execute success");
            else
                LogMessage?.Warning($" VariableRpcNode - VariableName {Text} : {data.ErrorMessage}");
            return new NodeOutput() { Value = data };
        }
        else
        {
            LogMessage?.Warning($" VariableRpcNode - VariableName {Text} : not found");
            return new NodeOutput() { };
        }
    }


}
