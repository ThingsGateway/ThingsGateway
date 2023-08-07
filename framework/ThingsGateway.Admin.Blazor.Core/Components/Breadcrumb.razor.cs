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