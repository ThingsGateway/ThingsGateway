
using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

public abstract class BaseNode : NodeModel, INode
{
    public BaseNode(string id, Point? position = null) : base(id, position)
    {

    }

    public string RulesEngineName { get; set; }
    public ILog LogMessage { get; set; }
}
