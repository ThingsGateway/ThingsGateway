
namespace ThingsGateway.RulesEngine;

public abstract class NumberNode : PlaceholderModel
{

    public NumberNode(string id, Point? position = null) : base(id, position)
    {
    }
    [ModelValue]
    public int? Number { get; set; }


}
