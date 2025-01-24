//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using System.Globalization;

using ThingsGateway.FriendlyException;

namespace ThingsGateway.Admin.Application;

internal sealed class SysResourceService : BaseService<SysResource>, ISysResourceService
{
    private readonly IRelationService _relationService;

    private string CacheKey = $"{CacheConst.Cache_SysResource}-{CultureInfo.CurrentUICulture.Name}";

    public SysResourceService(IRelationService relationService)
    {
        _relationService = relationService;
    }

    #region 增删改查


    [OperDesc("CopyResource")]
    public async Task CopyAsync(IEnumerable<long> ids, long moduleId)
    {
        var resourceList = await GetAllAsync().ConfigureAwait(false);
        var myResourceList = resourceList.Where(a => ids.Contains(a.Id)).ToList();

        var parent = GetMyParentResources(resourceList, myResourceList);
        myResourceList = myResourceList.Concat(parent).Where(a => a.Category != ResourceCategoryEnum.Module).DistinctBy(a => a.Id).ToList();
        var tree = ConstructMenuTrees(myResourceList).ToList();
        SysResourceService.SetTreeValue(tree, moduleId, 0);
        var data = MenuTreesToSaveLevel(tree);
        using var db = GetDB();
        var result = await db.Insertable(data).ExecuteCommandAsync().ConfigureAwait(false);
        RefreshCache();//刷新缓存
    }

    private static void SetTreeValue(List<SysResource> tree, long moduleId, long parentId)
    {
        if (tree == null) return;
        foreach (var item in tree)
        {
            item.Id = CommonUtils.GetSingleId();
            item.ParentId = parentId;
            item.Code = RandomHelper.CreateRandomString(10);
            item.Module = moduleId;
            SysResourceService.SetTreeValue(item.Children, moduleId, item.Id);
        }
    }

    [OperDesc("ChangeParentResource")]
    public async Task ChangeParentAsync(long id, long parentMenuId)
    {
        var resourceList = await GetAllAsync().ConfigureAwait(false);
        var resource = resourceList.First(a => a.Id == id);
        resource.ParentId = parentMenuId;
        using var db = GetDB();
        var result = await db.Updateable(resource).ExecuteCommandAsync().ConfigureAwait(false);
        RefreshCache();//刷新缓存
        _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//关系表刷新缓存
        _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//关系表刷新缓存
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns></returns>
    [OperDesc("DeleteResource")]
    public async Task<bool> DeleteResourceAsync(IEnumerable<long> ids)
    {
        //删除
        if (ids.Any())
        {
            //获取所有菜单和按钮
            var resourceList = await GetAllAsync().ConfigureAwait(false);
            //找到要删除的菜单
            var delSysResources = resourceList.Where(it => ids.Contains(it.Id));
            //找到要删除的模块
            var delModules = resourceList.Where(a => a.Category == ResourceCategoryEnum.Module).Where(it => ids.Contains(it.Id));
            if (delModules.Any())
            {
                //获取模块下的所有列表
                var delModuleResources = resourceList.Where(it => delModules.Select(a => a.Id).Contains(it.Module));
                delSysResources = delSysResources.Concat(delModuleResources).ToHashSet();
            }
            //查找内置菜单
            var system = delSysResources.FirstOrDefault(it => it.Code == ResourceConst.System);
            if (system != null)
                throw Oops.Bah(Localizer["CanotDeleteSystemResource", system.Title]);

            //需要删除的资源ID列表
            var resourceIds = delSysResources.SelectMany(it =>
            {
                var child = GetResourceChilden(resourceList, it.Id);
                return child.Select(c => c.Id).Concat(new List<long>() { it.Id });
            });
            var deleteIds = ids.Concat(resourceIds).ToHashSet();//添加到删除ID列表

            using var db = GetDB();
            //事务
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<SysResource>().In(deleteIds.ToList()).ExecuteCommandAsync().ConfigureAwait(false);//删除菜单和按钮
                await db.Deleteable<SysRelation>()//关系表删除对应RoleHasResource
                 .Where(it => it.Category == RelationCategoryEnum.RoleHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Deleteable<SysRelation>()//关系表删除对应UserHasResource
               .Where(it => it.Category == RelationCategoryEnum.UserHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (result.IsSuccess)//如果成功了
            {
                RefreshCache();//资源表菜单刷新缓存
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//关系表刷新缓存
                _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//关系表刷新缓存
                return true;
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
        return false;
    }

    /// <summary>
    /// 从缓存/数据库读取全部资源列表
    /// </summary>
    /// <returns>全部资源列表</returns>
    public async Task<List<SysResource>> GetAllAsync()
    {
        var sysResources = App.CacheService.Get<List<SysResource>>(CacheKey);
        if (sysResources == null)
        {
            using var db = GetDB();
            sysResources = await db.Queryable<SysResource>().ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(CacheKey, sysResources);
        }
        return sysResources;
    }

    /// <summary>
    /// 根据菜单Id获取菜单列表
    /// </summary>
    /// <param name="menuIds">菜单id列表</param>
    /// <returns>菜单列表</returns>
    public async Task<IEnumerable<SysResource>> GetMenuByMenuIdsAsync(IEnumerable<long> menuIds)
    {
        var menuList = await GetAllAsync().ConfigureAwait(false);
        var menus = menuList.Where(it => it.Category == ResourceCategoryEnum.Menu && menuIds.Contains(it.Id));
        return menus;
    }

    /// <summary>
    /// 根据模块Id获取模块列表
    /// </summary>
    /// <param name="moduleIds">模块id列表</param>
    /// <returns>菜单列表</returns>
    public async Task<IEnumerable<SysResource>> GetMuduleByMuduleIdsAsync(IEnumerable<long> moduleIds)
    {
        var moduleList = await GetAllAsync().ConfigureAwait(false);
        var modules = moduleList.Where(it => it.Category == ResourceCategoryEnum.Module && moduleIds.Contains(it.Id));
        return modules;
    }

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="options">查询条件</param>
    /// <param name="searchModel">查询条件</param>
    /// <returns></returns>
    public Task<QueryData<SysResource>> PageAsync(QueryPageOptions options, ResourceTableSearchModel searchModel)
    {
        return QueryAsync(options, b => b.Where(a => (a.Category == ResourceCategoryEnum.Module && a.Id == searchModel.Module) || (a.Category != ResourceCategoryEnum.Module && a.Module == searchModel.Module)));
    }

    /// <summary>
    /// 保存资源
    /// </summary>
    /// <param name="input">资源</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveResource")]
    public async Task<bool> SaveResourceAsync(SysResource input, ItemChangedType type)
    {
        var resource = await CheckInput(input).ConfigureAwait(false);//检查参数
        using var db = GetDB();

        if (type == ItemChangedType.Add)
        {
            var result = await db.Insertable(input).ExecuteCommandAsync().ConfigureAwait(false);
            RefreshCache();//刷新缓存
            return result > 0;
        }
        else
        {
            var permissions = new List<SysRelation>();
            if (resource.Href != input.Href)
            {
                //获取所有角色和用户的权限关系
                var rolePermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.RoleHasPermission).ConfigureAwait(false);
                var userPermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.UserHasPermission).ConfigureAwait(false);
                //找到所有匹配的权限
                rolePermissions = rolePermissions.Where(it => it.TargetId!.Contains(resource.Href)).ToList();
                userPermissions = userPermissions.Where(it => it.TargetId!.Contains(resource.Href)).ToList();
                //更新路径
                rolePermissions.ForEach(it => it.TargetId = it.TargetId!.Replace(resource.Href, input.Href));
                userPermissions.ForEach(it => it.TargetId = it.TargetId!.Replace(resource.Href, input.Href));
                //添加到权限列表
                permissions.AddRange(rolePermissions);
                permissions.AddRange(userPermissions);
            }
            //事务
            var result = await db.UseTranAsync(async () =>
            {
                await db.Updateable(input).ExecuteCommandAsync().ConfigureAwait(false);//更新数据
                if (permissions.Count > 0)//如果权限列表大于0就更新
                {
                    await db.Updateable(permissions).ExecuteCommandAsync().ConfigureAwait(false);//更新关系表
                }
            }).ConfigureAwait(false);
            if (result.IsSuccess)//如果成功了
            {
                RefreshCache();//刷新菜单缓存
                if (resource.Href != input.Href)
                {
                    _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);
                    _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);
                }
                return true;
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
    }

    #endregion 增删改查

    #region 缓存

    /// <summary>
    /// 刷新缓存
    /// </summary>
    public void RefreshCache()
    {
        App.CacheService.Remove(CacheKey);
        //删除超级管理员的缓存
        App.RootServices.GetRequiredService<ISysUserService>().DeleteUserFromCache(RoleConst.SuperAdminId);
    }

    #endregion 缓存

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysResource">资源</param>
    private async Task<SysResource> CheckInput(SysResource sysResource)
    {
        if (sysResource.Code.IsNullOrWhiteSpace()) //默认编码
        {
            sysResource.Code = RandomHelper.CreateRandomString(10);
        }

        //如果菜单类型是菜单
        //if (sysResource.Category == ResourceCategoryEnum.Menu)
        //{
        //    if (string.IsNullOrEmpty(sysResource.Href))
        //        throw Oops.Bah("ResourceMenuHrefNotNull");
        //}

        //获取所有列表
        var menList = await GetAllAsync().ConfigureAwait(false);
        //判断是否有同级且同名
        if (menList.Any(it => it.ParentId == sysResource.ParentId && it.Title == sysResource.Title && it.Id != sysResource.Id && it.Module == sysResource.Module))
            throw Oops.Bah(Localizer["ResourceDup", sysResource.Title]);
        if (sysResource.ParentId != 0)
        {
            //获取父级,判断父级ID正不正确
            var parent = menList.Where(it => it.Id == sysResource.ParentId).FirstOrDefault();
            if (parent != null)
            {
                if (parent.Module != sysResource.Module)//如果父级的模块和当前模块不一样
                    throw Oops.Bah(Localizer["ModuleIdDiff"]);
                if (parent.Id == sysResource.Id)
                    throw Oops.Bah(Localizer["ResourceChoiceSelf"]);
            }
            else
            {
                throw Oops.Bah(Localizer["ResourceParentNull", sysResource.ParentId]);
            }
        }

        //如果ID大于0表示编辑
        if (sysResource.Id > 0)
        {
            var resource = menList.FirstOrDefault(it => it.Id == sysResource.Id);
            if (resource == null)
                throw Oops.Bah(Localizer["NotFoundResource"]);
            return resource;
        }

        return null;
    }

    #endregion 方法


    /// <inheritdoc/>
    private static List<SysResource> MenuTreesToSaveLevel(IEnumerable<SysResource> resourceList)
    {
        var flatList = new List<SysResource>();

        void TraverseTree(SysResource node)
        {
            // 添加当前节点到平级列表
            flatList.Add(node);

            // 如果当前节点有子节点，则递归处理每个子节点
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    TraverseTree(child);
                }
            }
        }

        // 遍历资源列表中的每个顶级节点
        foreach (var resource in resourceList)
        {
            TraverseTree(resource);
        }

        return flatList;
    }


    /// <inheritdoc/>
    public IEnumerable<SysResource> ConstructMenuTrees(IEnumerable<SysResource> resourceList, long parentId = 0)
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


    /// <inheritdoc/>
    public IEnumerable<SysResource> GetMyParentResources(IEnumerable<SysResource> allMenuList, IEnumerable<SysResource> myMenus)
    {
        var parentList = myMenus
            .SelectMany(it => GetResourceParent(allMenuList, it.ParentId))
                                .Where(parent => parent != null
                                && !myMenus.Contains(parent)
                                && !myMenus.Any(m => m.Id == parent.Id))
                                .Distinct();
        return parentList;
    }


    /// <inheritdoc/>
    public IEnumerable<SysResource> GetResourceChilden(IEnumerable<SysResource> resourceList, long parentId)
    {
        //找下级资源ID列表
        return resourceList.Where(it => it.ParentId == parentId)
                           .SelectMany(item => new List<SysResource> { item }.Concat(GetResourceChilden(resourceList, item.Id)));
    }

    /// <inheritdoc/>
    public IEnumerable<SysResource> GetResourceParent(IEnumerable<SysResource> resourceList, long resourceId)
    {
        //找上级资源ID列表
        return resourceList.Where(it => it.Id == resourceId)
                           .SelectMany(item => new List<SysResource> { item }.Concat(GetResourceParent(resourceList, item.ParentId)));
    }



}
