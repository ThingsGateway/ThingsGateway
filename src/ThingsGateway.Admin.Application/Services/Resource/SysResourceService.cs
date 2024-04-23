
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using Microsoft.Extensions.DependencyInjection;

using NewLife.Extension;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

public class SysResourceService : BaseService<SysResource>, ISysResourceService
{
    private readonly IRelationService _relationService;

    public SysResourceService(IRelationService relationService)
    {
        this._relationService = relationService;
    }

    #region 增删改查

    /// <summary>
    /// 从缓存/数据库读取全部资源列表
    /// </summary>
    /// <returns>全部资源列表</returns>
    public async Task<List<SysResource>> GetAllAsync()
    {
        var key = $"{CacheConst.Cache_SysResource}";
        var sysResources = App.CacheService.Get<List<SysResource>>(key);
        if (sysResources == null)
        {
            using var db = GetDB();
            sysResources = await db.Queryable<SysResource>().ToListAsync();
            App.CacheService.Set(key, sysResources);
        }
        return sysResources;
    }

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="options">查询条件</param>
    /// <param name="searchModel">查询条件</param>
    /// <returns></returns>
    public Task<QueryData<SysResource>> PageAsync(QueryPageOptions options, ResourceSearchInput searchModel)
    {
        return QueryAsync(options, b => b.Where(a => (a.Category == ResourceCategoryEnum.Module && a.Id == searchModel.Module) || (a.Category != ResourceCategoryEnum.Module && a.Module == searchModel.Module)));
    }

    /// <summary>
    /// 根据菜单Id获取菜单列表
    /// </summary>
    /// <param name="menuIds">菜单id列表</param>
    /// <returns>菜单列表</returns>
    public async Task<IEnumerable<SysResource>> GetMenuByMenuIdsAsync(IEnumerable<long> menuIds)
    {
        var menuList = await GetAllAsync();
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
        var moduleList = await GetAllAsync();
        var modules = moduleList.Where(it => it.Category == ResourceCategoryEnum.Module && moduleIds.Contains(it.Id));
        return modules;
    }

    /// <summary>
    /// 保存资源
    /// </summary>
    /// <param name="input">资源</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveResource")]
    public async Task<bool> SaveResourceAsync(SysResource input, ItemChangedType type)
    {
        var resource = await CheckInput(input);//检查参数
        using var db = GetDB();

        if (type == ItemChangedType.Add)
        {
            var result = await db.Insertable(input).ExecuteCommandAsync();
            RefreshCache();//刷新缓存
            return result > 0;
        }
        else
        {
            var permissions = new List<SysRelation>();
            if (resource.Href != input.Href)
            {
                //获取所有角色和用户的权限关系
                var rolePermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.RoleHasPermission);
                var userPermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.UserHasPermission);
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
                await db.Updateable(input).ExecuteCommandAsync();//更新数据
                if (permissions.Count > 0)//如果权限列表大于0就更新
                {
                    await db.Updateable(permissions).ExecuteCommandAsync();//更新关系表
                }
            });
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
            var resourceList = await GetAllAsync();
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
                throw Oops.Bah($"CanotDeleteSystemResource", system.Title);

            //需要删除的资源ID列表
            var resourceIds = delSysResources.SelectMany(it =>
            {
                var child = ResourceUtil.GetResourceChilden(resourceList, it.Id);
                return child.Select(c => c.Id).Concat(new List<long>() { it.Id });
            });
            var deleteIds = ids.Concat(resourceIds).ToHashSet();//添加到删除ID列表

            using var db = GetDB();
            //事务
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<SysResource>().In(deleteIds.ToList()).ExecuteCommandAsync();//删除菜单和按钮
                await db.Deleteable<SysRelation>()//关系表删除对应RoleHasResource
                 .Where(it => it.Category == RelationCategoryEnum.RoleHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync();
                await db.Deleteable<SysRelation>()//关系表删除对应UserHasResource
               .Where(it => it.Category == RelationCategoryEnum.UserHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync();
            });
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

    #endregion 增删改查

    #region 缓存

    /// <summary>
    /// 刷新缓存
    /// </summary>
    public void RefreshCache()
    {
        App.CacheService.Remove($"{CacheConst.Cache_SysResource}");
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
            sysResource.Code = Random.Shared.Next().ToString();
        }

        //如果菜单类型是菜单
        //if (sysResource.Category == ResourceCategoryEnum.Menu)
        //{
        //    if (string.IsNullOrEmpty(sysResource.Href))
        //        throw Oops.Bah("ResourceMenuHrefNotNull");
        //}

        //获取所有列表
        var menList = await GetAllAsync();
        //判断是否有同级且同名
        if (menList.Any(it => it.ParentId == sysResource.ParentId && it.Title == sysResource.Title && it.Id != sysResource.Id))
            throw Oops.Bah("ResourceDup", sysResource.Title);
        if (sysResource.ParentId != 0)
        {
            //获取父级,判断父级ID正不正确
            var parent = menList.Where(it => it.Id == sysResource.ParentId).FirstOrDefault();
            if (parent != null)
            {
                if (parent.Module != sysResource.Module)//如果父级的模块和当前模块不一样
                    throw Oops.Bah("ModuleIdDiff");
                if (parent.Id == sysResource.Id)
                    throw Oops.Bah($"ResourceChoiceSelf");
            }
            else
            {
                throw Oops.Bah("ResourceParentNull", sysResource.ParentId);
            }
        }

        //如果ID大于0表示编辑
        if (sysResource.Id > 0)
        {
            var resource = menList.FirstOrDefault(it => it.Id == sysResource.Id);
            if (resource == null)
                throw Oops.Bah($"NotFoundResource");
            return resource;
        }

        return null;
    }

    #endregion 方法
}
