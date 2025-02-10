namespace ThingsGateway.RulesEngine;

public sealed class CategoryNode : Attribute
{
    public Type WidgetType { get; set; } = typeof(DefaultWidget);
    public string ImgUrl { get; set; } = "ImgUrl";
    public string Desc { get; set; } = "Desc";
    public string Category { get; set; } = "Other";
    public Type LocalizerType { get; set; } = typeof(ThingsGateway.RulesEngine._Imports);
    public IStringLocalizer StringLocalizer;
}
