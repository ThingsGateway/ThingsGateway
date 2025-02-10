namespace ThingsGateway.RulesEngine;

public abstract class PlaceholderModel : BaseNode
{

    protected PlaceholderModel(string id, Point? position = null) : base(id, position)
    {
    }

    public string Placeholder { get; set; }
}