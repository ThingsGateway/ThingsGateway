#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;

/// <summary>
/// AppListGroup
/// </summary>
/// <typeparam name="TItem"></typeparam>
public partial class AppListGroup<TItem>
{
    /// <summary>
    /// icon
    /// </summary>
    [Parameter]
    public string Icon { get; set; }

    /// <summary>
    /// item
    /// </summary>
    [Parameter, EditorRequired]
    public TItem Item { get; set; }

    /// <summary>
    /// sub
    /// </summary>
    [Parameter]
    public bool SubGroup { get; set; }

    private List<string> _group = new();
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _group = GenGroup(Item.Children);
    }

    List<string> GenGroup(List<TItem> items)
    {
        if (items == null || !items.Any())
            return new List<string>();
        var groups = new List<string>();
        foreach (var item in items)
        {
            groups.AddRange(GenGroup(item.Children));
            if (item.HasChildren())
                continue;
            groups.Add(item.Href);
        }

        return groups;
    }
}