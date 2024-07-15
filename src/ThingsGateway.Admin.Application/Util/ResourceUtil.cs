//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using Microsoft.AspNetCore.Authorization;

using NewLife;

using System.Globalization;
using System.Reflection;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

public class ResourceUtil
{
    #region 菜单

    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildMenuSelectList(IEnumerable<SysResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Menu)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
            {
                GroupName = items.FirstOrDefault(a => a.Id == item.ParentId)?.Title!
            }
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造树形菜单
    /// </summary>
    /// <param name="items">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public static IEnumerable<MenuItem> BuildMenuTrees(IEnumerable<SysResource> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new MenuItem()
            {
                Match = item.NavLinkMatch ?? Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All,
                Text = item.Title,
                Icon = item.Icon,
                Url = item.Href,
                Target = item.Target.ToString(),
                Items = BuildMenuTrees(items, item.Id).ToList()
            }
        );
    }

    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildModuleSelectList(IEnumerable<SysResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Module)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
            {
                GroupName = items.FirstOrDefault(a => a.Id == item.ParentId)?.Title!
            }
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造树形数据
    /// </summary>
    /// <param name="items">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public static IEnumerable<TableTreeNode<SysResource>> BuildTableTrees(IEnumerable<SysResource> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new TableTreeNode<SysResource>(item)
            {
                HasChildren = items.Any(i => i.ParentId == item.Id),
                IsExpand = items.Any(i => i.ParentId == item.Id),
                Items = BuildTableTrees(items, item.Id).ToList()
            }
        );
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<SysResource>> BuildTreeItemList(IEnumerable<SysResource> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<SysResource> render, long parentId = 0, TreeViewItem<SysResource>? parent = null)
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<SysResource>>();
        var roots = sysresources.Where(i => i.ParentId == parentId).OrderBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<SysResource>(node)
            {
                Text = node.Title,
                Icon = node.Icon,
                IsActive = selectedItems.Any(v => node.Id == v),
                IsExpand = selectedItems.Any(v => node.Id == v),
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(sysresources, selectedItems, render, node.Id, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }

    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<OpenApiPermissionTreeSelector>> BuildTreeItemList(IEnumerable<OpenApiPermissionTreeSelector> openApiPermissionTreeSelectors, List<string> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<OpenApiPermissionTreeSelector> render, TreeViewItem<OpenApiPermissionTreeSelector>? parent = null)
    {
        if (openApiPermissionTreeSelectors == null) return null;
        var trees = new List<TreeViewItem<OpenApiPermissionTreeSelector>>();
        foreach (var node in openApiPermissionTreeSelectors)
        {
            var item = new TreeViewItem<OpenApiPermissionTreeSelector>(node)
            {
                Text = node.ApiRoute,
                IsActive = selectedItems.Any(v => node.ApiRoute == v),
                Parent = parent,
                IsExpand = selectedItems.Any(v => node.ApiRoute == v),
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.ApiRoute) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(node.Children, selectedItems, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }

    /// <summary>
    /// 构造树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public static IEnumerable<SysResource> ConstructMenuTrees(IEnumerable<SysResource> resourceList, long parentId = 0)
    {
        //找下级资源ID列表
        var resources = resourceList.Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode);
        if (resources.Any())//如果数量大于0
        {
            foreach (var item in resources)//遍历资源
            {
                var children = ConstructMenuTrees(resourceList, item.Id).ToList();//添加子节点
                item.Children = children.Count > 0 ? children : null;
            }
        }
        return resources;
    }

    /// <summary>
    /// 获取父菜单集合
    /// </summary>
    /// <param name="allMenuList">所有菜单列表</param>
    /// <param name="myMenus">我的菜单列表</param>
    /// <returns></returns>
    public static IEnumerable<SysResource> GetMyParentResources(IEnumerable<SysResource> allMenuList, IEnumerable<SysResource> myMenus)
    {
        var parentList = myMenus
            .SelectMany(it => ResourceUtil.GetResourceParent(allMenuList, it.ParentId))
                                .Where(parent => parent != null
                                && !myMenus.Contains(parent)
                                && !myMenus.Any(m => m.Id == parent.Id))
                                .Distinct();
        return parentList;
    }

    /// <summary>
    /// 获取资源所有下级，结果不会转为树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父Id</param>
    /// <returns></returns>
    public static IEnumerable<SysResource> GetResourceChilden(IEnumerable<SysResource> resourceList, long parentId)
    {
        //找下级资源ID列表
        return resourceList.Where(it => it.ParentId == parentId)
                           .SelectMany(item => new List<SysResource> { item }.Concat(GetResourceChilden(resourceList, item.Id)));
    }

    /// <summary>
    /// 获取资源所有父级，结果不会转为树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="resourceId">Id</param>
    /// <returns></returns>
    public static IEnumerable<SysResource> GetResourceParent(IEnumerable<SysResource> resourceList, long resourceId)
    {
        //找上级资源ID列表
        return resourceList.Where(it => it.Id == resourceId)
                           .SelectMany(item => new List<SysResource> { item }.Concat(GetResourceParent(resourceList, item.ParentId)));
    }

    #endregion 菜单

    #region 权限相关

    /// <inheritdoc />
    public static List<OpenApiPermissionTreeSelector> ApiPermissionTreeSelector()
    {
        var cacheKey = $"{nameof(ApiPermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
        var permissions = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            List<OpenApiPermissionTreeSelector> permissions = new();//权限列表

            // 获取所有需要数据权限的控制器
            var controllerTypes =
                App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(RolePermissionAttribute), false));
            foreach (var controller in controllerTypes)
            {
                //获取数据权限特性
                var route = controller.GetCustomAttributes<Microsoft.AspNetCore.Mvc.RouteAttribute>().FirstOrDefault();
                if (route == null) continue;

                var description = controller.GetTypeDisplayName();
                var routeName = GetRouteName(controller.Name, route.Template);//赋值路由名称
                OpenApiPermissionTreeSelector openApiGroup = new() { ApiName = description ?? routeName, ApiRoute = routeName };
                //获取所有方法
                var menthods = controller.GetRuntimeMethods();
                //遍历方法
                foreach (var menthod in menthods)
                {
                    //获取忽略数据权限特性
                    var ignoreRolePermission = menthod.GetCustomAttribute<IgnoreRolePermissionAttribute>();
                    if (ignoreRolePermission == null)//如果是空的代表需要数据权限
                    {
                        //获取接口描述
                        var methodDesc = controller.GetMethodDisplayName(menthod.Name);
                        //if (methodDesc != null)
                        {
                            //默认路由名称
                            var apiRoute = menthod.Name.ToLowerCamelCase();
                            //获取get特性
                            var requestGet = menthod.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>();
                            if (requestGet != null)//如果是get方法
                                apiRoute = requestGet.Template;
                            else
                            {
                                //获取post特性
                                var requestPost = menthod.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>();
                                if (requestPost != null)//如果是post方法
                                    apiRoute = requestPost.Template;
                                else
                                    continue;
                            }

                            //apiRoute = route.Template + $"/{apiRoute}";
                            apiRoute = routeName + $"/{apiRoute}";

                            //添加到权限列表
                            openApiGroup.Children ??= new();
                            openApiGroup.Children.Add(new OpenApiPermissionTreeSelector
                            {
                                ApiName = methodDesc,
                                ApiRoute = apiRoute,
                            });
                        }
                    }
                }

                permissions.Add(openApiGroup);
            }
            return permissions;
        });
        return permissions;
    }

    /// <summary>
    /// 获取路由地址名称
    /// </summary>
    /// <param name="controllerName">控制器地址</param>
    /// <param name="template">路由名称</param>
    /// <returns></returns>
    public static string GetRouteName(string controllerName, string template)
    {
        if (!template.StartsWith("/"))
            template = "/" + template;//如果路由名称不是/开头则加上/防止控制器没写
        if (template.Contains("[controller]"))
        {
            controllerName = controllerName.Replace("Controller", "");//去掉Controller
            controllerName = controllerName.ToLowerCamelCase();//转首字母小写写
            template = template.Replace("[controller]", controllerName);//替换[controller]
        }

        return template;
    }

    /// <inheritdoc />
    public static IEnumerable<PermissionTreeSelector> PermissionTreeSelector(IEnumerable<string> routes)
    {
        List<PermissionTreeSelector> permissions = PermissionTreeSelector();
        return permissions.Where(a => routes.Contains(a.ApiRoute));
    }

    /// <inheritdoc />
    public static List<PermissionTreeSelector> PermissionTreeSelector()
    {
        var cacheKey = $"{nameof(PermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
        var permissions = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            List<PermissionTreeSelector> permissions = new();//权限列表

            // 获取所有需要数据权限的控制器
            var controllerTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
        && u.IsDefined(typeof(AuthorizeAttribute), false)
        && u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false));

            foreach (var controller in controllerTypes)
            {
                //获取数据权限特性
                var route = controller.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>().FirstOrDefault();
                if (route == null) continue;
                var apiRoute = GetRouteName(controller.Name, route.Template);//赋值路由名称

                var desc = controller.GetTypeDisplayName();
                //添加到权限列表
                permissions.Add(new PermissionTreeSelector
                {
                    ApiName = desc,
                    ApiRoute = apiRoute,
                });
            }
            return permissions;
        });

        return permissions;
    }

    #endregion 权限相关
}
