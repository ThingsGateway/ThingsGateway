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

using BlazorComponent;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// Breadcrumb
/// </summary>
public partial class Breadcrumb
{
    private List<BreadcrumbItem> BreadcrumbItems = new();

    [Inject]
    UserResoures UserResoures { get; set; }
    /// <inheritdoc/>
    public override void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        BreadcrumbItems = GetBreadcrumbItems();
        base.OnInitialized();
    }
    private List<BreadcrumbItem> GetBreadcrumbItems()
    {
        var items = new List<BreadcrumbItem>();
        var currentNav = UserResoures.AllSameLevelMenuSpas.FirstOrDefault(n => n.Component is not null && NavigationManager.Uri.Replace(NavigationManager.BaseUri, "/") == (n.Component));
        if (currentNav is not null)
        {
            if (currentNav.ParentId != 0)
            {
                var parentNav = UserResoures.AllSameLevelMenuSpas.FirstOrDefault(n => n.Id == currentNav.ParentId);
                if (parentNav != null)
                    items.Add(new BreadcrumbItem { Text = parentNav.Title, Href = null });
            }

            items.Add(new BreadcrumbItem() { Text = currentNav.Title, Href = currentNav.Component });
            items.Last().Href = currentNav.Component;
        }

        return items;
    }

    private void OnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        BreadcrumbItems = GetBreadcrumbItems();
        InvokeAsync(StateHasChanged);
    }
}