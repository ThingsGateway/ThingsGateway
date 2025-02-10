namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Start/End", ImgUrl = "_content/ThingsGateway.RulesEngine/img/Start.svg", Desc = nameof(StartNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(DefaultWidget))]
public class StartNode : BaseNode
{

    public StartNode(string id, Point? position = null) : base(id, position)
    { Title = "Start"; }


}
