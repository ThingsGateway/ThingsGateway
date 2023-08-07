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

using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// PageTabs
/// </summary>
public partial class PageTabs
{
    private TabOptions TabOptions(PageTabPathValue value)
    {
        var item = UserResoures.PageTabItems.FirstOrDefault(u => value.IsMatch(u.Href));
        var title = item?.Title;
        var icon = item?.Icon;
        var titleClass = $"mx-2 text-capitalize {(value.Selected ? "primary--text" : "")}";
        var op = new TabOptions(title, icon, titleClass)
        {
            TitleStyle = "min-width:46px;",
            Class = "systemTab",
        };
        return op;
    }
    /// <summary>
    /// Tabsʵ��
    /// </summary>
    public PPageTabs PPageTabs { get; private set; }

    [Inject]
    UserResoures UserResoures { get; set; }
    /// <summary>
    /// SelfPatterns
    /// </summary>
    [Parameter]
    public IEnumerable<string> SelfPatterns { get; set; }
    /// <summary>
    /// �����
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}