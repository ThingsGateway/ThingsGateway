public interface IDefaultItem<TItem>
{
    /// <summary>
    /// 子菜单
    /// </summary>
    List<TItem> Children { get; }

    /// <summary>
    /// 是否启用下划线
    /// </summary>
    bool Divider { get; set; }

    /// <summary>
    /// 菜单头部标题
    /// </summary>
    string Heading { get; }

    /// <summary>
    /// 链接
    /// </summary>
    string Href { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    string Icon { get; set; }

    /// <summary>
    /// 菜单副标题
    /// </summary>
    string SubTitle { get; set; }

    string Target { get; set; }

    /// <summary>
    /// 菜单标题
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// 菜单值
    /// </summary>
    StringNumber Value { get; set; }

    /// <summary>
    /// 是否有子菜单
    /// </summary>
    /// <returns></returns>
    bool HasChildren()
    {
        return Children is not null && Children.Any();
    }
}