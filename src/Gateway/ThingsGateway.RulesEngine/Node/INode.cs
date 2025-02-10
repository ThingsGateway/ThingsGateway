namespace ThingsGateway.RulesEngine;

public interface INode
{
    public TouchSocket.Core.ILog LogMessage { get; set; }
}
public interface IConditionNode : INode
{
    public Task<bool> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}
public interface IExpressionNode : INode
{
    public Task<NodeOutput> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}
public interface IActuatorNode : INode
{
    public Task<NodeOutput> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}

public interface ITriggerNode : INode
{
    public Task StartAsync(Func<NodeOutput, Task> func);
}
public interface IExexcuteExpressionsBase
{
}
public interface IExexcuteExpressions : IExexcuteExpressionsBase
{
    public TouchSocket.Core.ILog Logger { get; set; }
    Task<NodeOutput> ExecuteAsync(NodeInput input, CancellationToken cancellationToken);
}

