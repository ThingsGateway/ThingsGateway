public class DefaultItem : IDefaultItem<DefaultItem>
{
    public List<DefaultItem> Children { get; set; }
    public bool Divider { get; set; }
    public bool HasChildren => Children.Any();
    public string Heading { get; set; }
    public string Href { get; set; }

    public string Icon { get; set; }

    public string SubTitle { get; set; }
    public string Target { get; set; }
    public string Title { get; set; }
    public StringNumber Value { get; set; }
}