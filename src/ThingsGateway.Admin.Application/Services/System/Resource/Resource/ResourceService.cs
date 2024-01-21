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

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NewLife;

using System.ComponentModel;
using System.Reflection;

using ThingsGateway.Core.Extension;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IResourceService"/>
public class ResourceService : DbRepository<SysResource>, IResourceService
{
    private readonly ISimpleCacheService _simpleCacheService;

    public ResourceService(ISimpleCacheService simpleCacheService)
    {
        _simpleCacheService = simpleCacheService;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetCodeByIdsAsync(List<long> ids, string category)
    {
        //根据分类获取所有
        var sysResources = await GetListByCategoryAsync(category);
        //条件查询
        var result = sysResources.Where(it => ids.Contains(it.Id)).Select(it => it.Code).ToList();
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<SysResource>> GetListAsync(List<string>? categoryList = null)
    {
        //定义结果
        var sysResources = new List<SysResource>();

        //定义资源分类列表,如果是空的则获取全部资源
        categoryList = categoryList != null
            ? categoryList
            : new List<string>
                { CateGoryConst.Resource_MENU, CateGoryConst.Resource_BUTTON, CateGoryConst.Resource_SPA };
        //遍历列表
        foreach (var category in categoryList)
        {
            //根据分类获取到资源列表
            var data = await GetListByCategoryAsync(category);
            //添加到结果集
            sysResources.AddRange(data);
        }
        return sysResources;
    }

    /// <inheritdoc/>
    public async Task<List<SysResource>> GetMenuAndSpaListAsync()
    {
        //获取所有的菜单和模块以及单页面列表，
        var sysResources = await GetListAsync(new List<string>
            {CateGoryConst.Resource_MENU, CateGoryConst.Resource_SPA });
        if (sysResources != null)
        {
            //并按分类和排序码排序
            sysResources = sysResources.OrderBy(it => it.Category).ThenBy(it => it.SortCode).ToList();
        }
        return sysResources;
    }

    /// <inheritdoc/>
    public async Task RefreshCacheAsync(string category = null)
    {
        //如果分类是空的
        if (category == null)
        {
            //删除全部key
            _simpleCacheService.DelByPattern(SystemConst.Cache_SysResource);
            await GetListAsync();
        }
        else
        {
            //否则只删除一个Key
            _simpleCacheService.Remove(SystemConst.Cache_SysResource + category);
            await GetListByCategoryAsync(category);
        }
    }

    /// <inheritdoc />
    public async Task<List<SysResource>> GetChildListByIdAsync(long resId, bool isContainOneself = true)
    {
        //获取所有机构
        var sysResources = await GetListAsync();
        //查找下级
        var childLsit = GetResourceChilden(sysResources, resId);
        if (isContainOneself)//如果包含自己
        {
            //获取自己的机构信息
            var self = sysResources.Where(it => it.Id == resId).FirstOrDefault();
            if (self != null) childLsit.Insert(0, self);//如果机构不为空就插到第一个
        }
        return childLsit;
    }

    /// <inheritdoc />
    public List<SysResource> GetChildListById(List<SysResource> sysResources, long resId, bool isContainOneself = true)
    {
        //查找下级
        var childLsit = GetResourceChilden(sysResources, resId);
        if (isContainOneself)//如果包含自己
        {
            //获取自己的机构信息
            var self = sysResources.Where(it => it.Id == resId).FirstOrDefault();
            if (self != null) childLsit.Insert(0, self);//如果机构不为空就插到第一个
        }
        return childLsit;
    }

    /// <inheritdoc />
    public async Task<List<SysResource>> GetListByCategoryAsync(string category)
    {
        //先从Redis拿
        var sysResources = _simpleCacheService.Get<List<SysResource>>(SystemConst.Cache_SysResource + category);
        if (sysResources == null)
        {
            //redis没有就去数据库拿
            sysResources = await base.GetListAsync(it => it.Category == category);
            if (sysResources.Count > 0)
            {
                //插入Redis
                _simpleCacheService.Set(SystemConst.Cache_SysResource + category, sysResources);
            }
        }
        return sysResources;
    }

    /// <inheritdoc />
    public async Task<ResTreeSelector> ResourceTreeSelectorAsync()
    {
        ResTreeSelector resourceTreeSelector = new();//定义结果
        resourceTreeSelector.Menu = await GetRoleGrantResourceMenusAsync();
        return resourceTreeSelector;
    }

    /// <inheritdoc />
    public List<PermissionTreeSelector> PermissionTreeSelector(List<string> routes)
    {
        List<PermissionTreeSelector> permissions = new List<PermissionTreeSelector>();//权限列表

        // 获取所有需要数据权限的控制器
        var controllerTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
    && u.IsDefined(typeof(AuthorizeAttribute), false)
    && u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false));

        foreach (var controller in controllerTypes)
        {
            //获取数据权限特性
            var routeAttributes = controller.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>().ToList();
            if (routeAttributes == null)
                continue;
            var route = routeAttributes[0];//取第一个值
            var routeName = GetRouteName(controller.Name, route.Template);//赋值路由名称
            //如果路由包含在路由列表中
            if (routes.Contains(routeName))
            {
                {
                    //获取忽略数据权限特性
                    {
                        //获取接口描述
                        var displayName = controller.FindDisplayAttribute();
                        {
                            var permissionName = displayName ?? routeName;
                            //添加到权限列表
                            permissions.Add(new PermissionTreeSelector
                            {
                                ApiName = routeName,
                                ApiRoute = routeName,
                                PermissionName = permissionName
                            });
                        }
                    }
                }
            }
        }
        return permissions;
    }

    /// <inheritdoc />
    public List<OpenApiPermissionTreeSelector> ApiPermissionTreeSelector()
    {
        List<OpenApiPermissionTreeSelector> permissions = new List<OpenApiPermissionTreeSelector>();//权限列表

        // 获取所有需要数据权限的控制器
        var controllerTypes =
            App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(RolePermissionAttribute), false));
        foreach (var controller in controllerTypes)
        {
            //获取数据权限特性
            var routeAttributes = controller.GetCustomAttributes<RouteAttribute>().ToList();
            if (routeAttributes == null || routeAttributes.Count == 0)
                continue;

            var description = controller.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var parid = YitIdHelper.NextId();
            var route = routeAttributes[0];//取第一个值
            var routeName = GetRouteName(controller.Name, route.Template);//赋值路由名称
            OpenApiPermissionTreeSelector openApiGroup = new() { ApiName = description ?? routeName, Id = parid, PermissionName = description ?? routeName };
            //获取所有方法
            var menthods = controller.GetMethods();
            //遍历方法
            foreach (var menthod in menthods)
            {
                //获取忽略数据权限特性
                var ignoreRolePermission = menthod.GetCustomAttribute<IgnoreRolePermissionAttribute>();
                if (ignoreRolePermission == null)//如果是空的代表需要数据权限
                {
                    //获取接口描述
                    var methodDesc = menthod.GetCustomAttribute<DescriptionAttribute>();
                    //if (methodDesc != null)
                    {
                        //默认路由名称
                        var apiRoute = menthod.Name.FirstCharToLower();
                        //获取get特性
                        var requestGet = menthod.GetCustomAttribute<HttpGetAttribute>();
                        if (requestGet != null)//如果是get方法
                            apiRoute = requestGet.Template;
                        else
                        {
                            //获取post特性
                            var requestPost = menthod.GetCustomAttribute<HttpPostAttribute>();
                            if (requestPost != null)//如果是post方法
                                apiRoute = requestPost.Template;
                            else
                                continue;
                        }

                        //apiRoute = route.Template + $"/{apiRoute}";
                        apiRoute = routeName + $"/{apiRoute}";
                        var apiName = methodDesc?.Description;

                        //合并
                        var permissionName = apiRoute + $"[{apiName}]";
                        //添加到权限列表
                        openApiGroup.Children ??= new();
                        openApiGroup.Children.Add(new OpenApiPermissionTreeSelector
                        {
                            Id = YitIdHelper.NextId(),
                            ParentId = parid,
                            ApiName = apiName,
                            ApiRoute = apiRoute,
                            PermissionName = permissionName
                        });
                    }
                }
            }

            permissions.Add(openApiGroup);
        }
        return permissions;
    }

    public async Task<List<SysResource>> GetMenuByMenuIdsAsync(List<long> menuIds)
    {
        //获取所有菜单
        var menuList = await GetListByCategoryAsync(CateGoryConst.Resource_MENU);
        //获取菜单信息
        var menus = menuList.Where(it => menuIds.Contains(it.Id)).ToList();
        return menus;
    }

    /// <inheritdoc />
    public List<SysResource> GetResourceParent(List<SysResource> resourceList, long parentId)
    {
        //找上级资源ID列表
        var resources = resourceList.Where(it => it.Id == parentId).FirstOrDefault();
        if (resources != null)//如果数量大于0
        {
            var data = new List<SysResource>();
            var parents = GetResourceParent(resourceList, resources.ParentId!.Value);
            data.AddRange(parents);//添加子节点;
            data.Add(resources);//添加到列表
            return data;//返回结果
        }
        return new List<SysResource>();
    }

    #region 方法

    /// <summary>
    /// 获取路由地址名称
    /// </summary>
    /// <param name="controllerName">控制器地址</param>
    /// <param name="template">路由名称</param>
    /// <returns></returns>
    private string GetRouteName(string controllerName, string template)
    {
        if (!template.StartsWith("/"))
            template = "/" + template;//如果路由名称不是/开头则加上/防止控制器没写
        if (template.Contains("[controller]"))
        {
            controllerName = controllerName.Replace("Controller", "");//去掉Controller
            controllerName = controllerName.FirstCharToLower();//转首字母小写写
            template = template.Replace("[controller]", controllerName);//替换[controller]
        }

        return template;
    }

    /// <summary>
    /// 获取资源所有下级
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public List<SysResource> GetResourceChilden(List<SysResource> resourceList, long parentId)
    {
        //找下级资源ID列表
        var resources = resourceList.Where(it => it.ParentId == parentId).ToList();
        if (resources.Count > 0)//如果数量大于0
        {
            var data = new List<SysResource>();
            foreach (var item in resources)//遍历资源
            {
                var res = GetResourceChilden(resourceList, item.Id);
                data.AddRange(res);//添加子节点;
                data.Add(item);//添加到列表
            }
            return data;//返回结果
        }
        return new List<SysResource>();
    }

    /// <summary>
    /// 获取授权菜单
    /// </summary>
    /// <param name="moduleId">模块ID</param>
    /// <returns></returns>
    public async Task<List<ResTreeSelector.RoleGrantResourceMenu>> GetRoleGrantResourceMenusAsync()
    {
        var roleGrantResourceMenus = new List<ResTreeSelector.RoleGrantResourceMenu>();//定义结果
        List<SysResource> allMenuList = (await GetListByCategoryAsync(CateGoryConst.Resource_MENU)).ToList();//获取所有菜单列表
        List<SysResource> allButtonList = await GetListByCategoryAsync(CateGoryConst.Resource_BUTTON);//获取所有按钮列表
        var parentMenuList = allMenuList.Where(it => it.ParentId == 0).ToList();//获取一级目录

        //遍历一级目录
        foreach (var parent in parentMenuList)
        {
            //如果是目录则去遍历下级
            if (parent.MenuType == MenuTypeEnum.CATALOG)
            {
                //获取所有下级菜单
                var menuList = GetChildListById(allMenuList, parent.Id, false);
                if (menuList.Count > 0)//如果有菜单
                {
                    //遍历下级菜单
                    foreach (var menu in menuList)
                    {
                        //如果菜单类型是菜单
                        if (menu.MenuType == MenuTypeEnum.MENU)
                        {
                            //获取菜单下按钮集合并转换成对应实体
                            var buttonList = allButtonList.Where(it => it.ParentId == menu.Id).ToList();
                            var buttons = buttonList.Adapt<List<ResTreeSelector.RoleGrantResourceButton>>();
                            roleGrantResourceMenus.Add(new ResTreeSelector.RoleGrantResourceMenu
                            {
                                Id = menu.Id,
                                ParentId = parent.Id,
                                ParentName = parent.Title,
                                Title = GetRoleGrantResourceMenuTitle(menuList, menu),//菜单名称需要特殊处理因为有二级菜单
                                Button = buttons
                            });
                        }
                        else if (menu.MenuType == MenuTypeEnum.LINK || menu.MenuType == MenuTypeEnum.IFRAME)//如果是内链或者外链
                        {
                            //直接加到资源列表
                            roleGrantResourceMenus.Add(new ResTreeSelector.RoleGrantResourceMenu
                            {
                                Id = menu.Id,
                                ParentId = parent.Id,
                                ParentName = parent.Title,
                                Title = menu.Title
                            });
                        }
                    }
                }
                else
                {
                    //否则就将自己加到一级目录里面
                    roleGrantResourceMenus.Add(new ResTreeSelector.RoleGrantResourceMenu
                    {
                        Id = parent.Id,
                        ParentId = parent.Id,
                        ParentName = parent.Title,
                    });
                }
            }
            else
            {
                //就将自己加到一级目录里面
                var roleGrantResourcesButtons = new ResTreeSelector.RoleGrantResourceMenu
                {
                    Id = parent.Id,
                    ParentId = parent.Id,
                    ParentName = parent.Title,
                    Title = parent.Title
                };
                //如果菜单类型是菜单
                if (parent.MenuType == MenuTypeEnum.MENU)
                {
                    //获取菜单下按钮集合并转换成对应实体
                    var buttonList = allButtonList.Where(it => it.ParentId == parent.Id).ToList();
                    roleGrantResourcesButtons.Button = buttonList.Adapt<List<ResTreeSelector.RoleGrantResourceButton>>();
                }
                roleGrantResourceMenus.Add(roleGrantResourcesButtons);
            }
        }
        return roleGrantResourceMenus;
    }

    /// <summary>
    /// 获取授权菜单类菜单名称
    /// </summary>
    /// <param name="menuList">菜单列表</param>
    /// <param name="menu">当前菜单</param>
    /// <returns></returns>
    private string GetRoleGrantResourceMenuTitle(List<SysResource> menuList, SysResource menu)
    {
        //查找菜单上级
        var parentList = GetResourceParent(menuList, menu.ParentId!.Value);
        //如果有父级菜单
        if (parentList.Count > 0)
        {
            var titles = parentList.Select(it => it.Title).ToList();//提取出父级的name
            var title = string.Join("- ", titles) + $"-{menu.Title}";//根据-分割,转换成字符串并在最后加上菜单的title
            return title;
        }
        else
        {
            return menu.Title;//原路返回
        }
    }

    #endregion 方法
}