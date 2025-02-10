namespace ThingsGateway.RulesEngine;


[CategoryNode(Category = "Start/End", ImgUrl = "_content/ThingsGateway.RulesEngine/img/End.svg", Desc = nameof(EndNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(DefaultWidget))]
public class EndNode : BaseNode
{

    public EndNode(string id, Point? position = null) : base(id, position)
    { Title = "End"; }


}
