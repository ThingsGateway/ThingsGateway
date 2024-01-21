#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using BlazorComponent;

namespace ThingsGateway.Components;

/// <inheritdoc/>
public class NavItem
{
    /// <inheritdoc/>
    public bool HasChildren()
    {
        return Children?.Count > 0;
    }

    /// <inheritdoc/>
    public List<NavItem> Children { get; set; }

    /// <inheritdoc/>
    public bool Divider { get; set; }

    /// <inheritdoc/>
    public string Group { get; set; }

    /// <inheritdoc/>
    public string Heading { get; set; }

    /// <inheritdoc/>
    public string Href { get; set; }

    /// <inheritdoc/>
    public string Icon { get; set; }

    /// <inheritdoc/>
    public string SubTitle { get; set; }

    /// <inheritdoc/>
    public string Target { get; set; }

    /// <inheritdoc/>
    public string Title { get; set; }

    /// <inheritdoc/>
    public bool Hidden { get; set; }

    /// <inheritdoc/>
    public StringNumber Value { get; set; }
}