
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Actuator", ImgUrl = $"{INode.ModuleBasePath}img/Rpc.svg", Desc = nameof(VariableRpcNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.VariableWidget,{INode.FileModulePath}")]
public class VariableRpcNode : VariableNode, IActuatorNode
{
    public VariableRpcNode(string id, Point? position = null) : base(id, position)
    { Title = "VariableRpcNode"; }


    async Task<OperResult<NodeOutput>> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        try
        {
            if ((!DeviceText.IsNullOrWhiteSpace()) && GlobalData.TryGetDeviceRuntime(DeviceText, out var device))
            {
                if (device.ReadOnlyVariableRuntimes.TryGetValue(Text, out var value))
                {
                    var data = await value.RpcAsync(input.JToken.ToString(), $"RulesEngine: {RulesEngineName}", cancellationToken).ConfigureAwait(false);
                    if (data.IsSuccess)
                        Logger?.LogDebug($" VariableRpcNode - VariableName {Text} : execute success");
                    else
                        Logger?.LogWarning($" VariableRpcNode - VariableName {Text} : {data.ErrorMessage}");
                    return new OperResult<NodeOutput>() { Content = new NodeOutput() { Value = data } };
                }
            }
            Logger?.LogWarning($" VariableRpcNode - VariableName {Text} : not found");

            return new OperResult<NodeOutput>() { Content = new NodeOutput() };
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex);
            return new OperResult<NodeOutput>(ex);
        }
    }
}
