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

using ThingsGateway.Core.Json.Extension;
using ThingsGateway.NewLife.X;
using ThingsGateway.NewLife.X.Extension;

namespace ThingsGateway.Admin.Application;

public class SysRoleService : BaseService<SysRole>, ISysRoleService
{
    private readonly IRelationService _relationService;
    private readonly ISysResourceService _sysResourceService;

    public SysRoleService(IRelationService relationService, ISysResourceService sysResourceService)
    {
        _relationService = relationService;
        _sysResourceService = sysResourceService;
    }

    #region 查询

    /// <summary>
    /// 从缓存/数据库获取全部角色信息
    /// </summary>
    /// <returns>角色列表</returns>
    public async Task<List<SysRole>> GetAllAsync()
    {
        var key = CacheConst.Cache_SysRole;
        var sysRoles = NetCoreApp.CacheService.Get<List<SysRole>>(key);
        if (sysRoles == null)
        {
            using var db = GetDB();
            sysRoles = await db.Queryable<SysRole>().ToListAsync();
            NetCoreApp.CacheService.Set(key, sysRoles);
        }
        return sysRoles;
    }

    /// <summary>
    /// 根据用户id获取角色列表
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>角色列表</returns>
    public async Task<IEnumerable<SysRole>> GetRoleListByUserIdAsync(long userId)
    {
        var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole);//根据用户ID获取角色ID
        var roleIdList = roleList.Select(x => x.TargetId.ToLong());//角色ID列表
        return (await GetAllAsync()).Where(it => roleIdList.Contains(it.Id));
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public Task<QueryData<SysRole>> PageAsync(QueryPageOptions option)
    {
        return QueryAsync(option, a => a.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText!)));
    }

    #endregion 查询

    #region 修改

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="ids">id列表</param>
    [OperDesc("DeleteRole")]
    public async Task<bool> DeleteRoleAsync(IEnumerable<long> ids)
    {
        var sysRoles = await GetAllAsync();//获取所有角色
        var hasSuperAdmin = sysRoles.Any(it => it.Code == RoleConst.SuperAdmin && ids.Contains(it.Id));//判断是否有超级管理员
        if (hasSuperAdmin)
            throw Oops.Bah(Localizer["CanotDeleteAdmin"]);

        //数据库是string所以这里转下
        var targetIds = ids.Select(it => it.ToString());
        //定义删除的关系
        var delRelations = new List<RelationCategoryEnum> {
            RelationCategoryEnum.RoleHasResource,
            RelationCategoryEnum.RoleHasPermission,
            RelationCategoryEnum.RoleHasModule,
            RelationCategoryEnum.RoleHasOpenApiPermission };
        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<SysRole>().In(ids.ToList()).ExecuteCommandHasChangeAsync();//删除
            //删除关系表角色与资源关系，角色与权限关系
            await db.Deleteable<SysRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync();
            //删除关系表角色与用户关系
            await db.Deleteable<SysRelation>(it => targetIds.Contains(it.TargetId) && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync();
        });
        if (result.IsSuccess)//如果成功了
        {
            RefreshCache();//刷新缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//关系表刷新UserHasRole缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//关系表刷新RoleHasResource缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//关系表刷新RoleHasPermission缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//关系表刷新RoleHasModule缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasOpenApiPermission);//关系表刷新RoleHasOpenApiPermission缓存
            await ClearTokenUtil.DeleteUserCacheByRoleIds(ids);//清除角色下用户缓存
            return true;
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <summary>
    /// 保存角色
    /// </summary>
    /// <param name="input">角色</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveRole")]
    public async Task<bool> SaveRoleAsync(SysRole input, ItemChangedType type)
    {
        if (input.Code == RoleConst.SuperAdmin)
            throw Oops.Bah(Localizer["CanotEditAdmin"]);
        await CheckInput(input);//检查参数
        if (await base.SaveAsync(input, type))
        {
            RefreshCache();
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id });//清除角色下用户缓存
            return true;
        }
        return false;
    }

    #endregion 修改

    #region 授权

    /// <summary>
    /// 根据角色id获取角色列表
    /// </summary>
    /// <param name="input">角色id列表</param>
    /// <returns>角色列表</returns>
    public async Task<IEnumerable<SysRole>> GetRoleListByIdListAsync(IEnumerable<long> input)
    {
        var roles = await GetAllAsync();
        var roleList = roles.Where(it => input.Contains(it.Id));
        return roleList;
    }

    /// <summary>
    /// 授权资源
    /// </summary>
    /// <param name="input">授权信息</param>
    [OperDesc("RoleGrantResource")]
    public async Task GrantResourceAsync(GrantResourceData input)
    {
        var menuIdsExtJsons1 = input.GrantInfoList;//菜单ID拓展信息
        var relationRoles = new List<SysRelation>();//要添加的角色资源和授权关系表
        var sysRole = (await GetAllAsync()).FirstOrDefault(it => it.Id == input.Id);//获取角色
        if (sysRole != null)
        {
            if (sysRole.Category == RoleCategoryEnum.Api)
                throw Oops.Bah(Localizer["ApiRoleCanotGrantResource"]);
            var resources = await _sysResourceService.GetAllAsync();

            var menus1 = resources.Where(a => menuIdsExtJsons1.Contains(a.Id));
            var data = ResourceUtil.GetMyParentResources(resources, menus1);
            var menuIdsExtJsons = menuIdsExtJsons1.Concat(data.Select(a => a.Id)).Distinct();
            var menus = menus1.Concat(data).Distinct();

            #region 角色模块处理

            //获取我的模块信息Id列表
            var moduleIds = menus.Select(it => it.Module).Distinct().ToList();
            moduleIds.ForEach(it =>
            {
                //将角色资源添加到列表
                relationRoles.Add(new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = it.ToString(),
                    Category = RelationCategoryEnum.RoleHasModule
                });
            });

            #endregion 角色模块处理

            #region 角色资源处理

            //遍历菜单列表
            foreach (var item in menuIdsExtJsons)
            {
                //将角色资源添加到列表
                relationRoles.Add(new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.RoleHasResource,
                    ExtJson = item!.ToSystemTextJsonString()
                });
            }

            #endregion 角色资源处理

            #region 角色权限处理.

            if (menus.Any())
            {
                //获取权限授权树
                var permissions = ResourceUtil.PermissionTreeSelector(menus.Select(it => it.Href));
                //要添加的角色有哪些权限列表
                var relationRolePer = permissions.Select(it => new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.RoleHasPermission,
                    ExtJson = new RelationRolePermission { ApiUrl = it.ApiRoute }
                            .ToSystemTextJsonString()
                });
                relationRoles.AddRange(relationRolePer);//合并列表
            }

            #endregion 角色权限处理.

            #region 保存数据库

            using var db = GetDB();

            //事务
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<SysRelation>(it =>
                    it.ObjectId == sysRole.Id && (it.Category == RelationCategoryEnum.RoleHasPermission || it.Category == RelationCategoryEnum.RoleHasResource
                    || it.Category == RelationCategoryEnum.RoleHasModule

                    )).ExecuteCommandAsync();
                await db.Insertable(relationRoles).ExecuteCommandAsync();//添加新的
            });
            if (result.IsSuccess)//如果成功了
            {
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//关系表刷新
                await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id });//清除角色下用户缓存
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion 保存数据库
        }
    }

    /// <summary>
    /// 授权用户
    /// </summary>
    /// <param name="input">授权参数</param>
    [OperDesc("RoleGrantUser")]
    public async Task GrantUserAsync(GrantUserOrRoleInput input)
    {
        var sysRelations = input.GrantInfoList.Select(it =>
       new SysRelation()
       {
           ObjectId = it,
           TargetId = input.Id.ToString(),
           Category = RelationCategoryEnum.UserHasRole
       }
       );
        using var db = GetDB();

        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var targetId = input.Id.ToString();
            await db.Deleteable<SysRelation>(it => it.TargetId == targetId && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync();//删除老的
            await db.Insertable(sysRelations.ToList()).ExecuteCommandAsync();//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//刷新关系表UserHasRole缓存
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id });//清除角色下用户缓存
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <summary>
    /// 获取拥有的资源
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="category">类型</param>
    public async Task<GrantResourceData> OwnResourceAsync(long id, RelationCategoryEnum category = RelationCategoryEnum.RoleHasResource)
    {
        var roleOwnResource = new GrantResourceData() { Id = id };//定义结果集

        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, category);
        roleOwnResource.GrantInfoList = relations.Select(it => (it.ExtJson?.FromSystemTextJsonString<long>() ?? 0)).Where(a => a != 0);
        return roleOwnResource;
    }

    #region OPENAPI

    /// <summary>
    /// 获取角色拥有的OpenApi权限
    /// </summary>
    /// <param name="id">角色id</param>
    public async Task<GrantPermissionData> ApiOwnPermissionAsync(long id)
    {
        var roleOwnPermission = new GrantPermissionData { Id = id };//定义结果集
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.RoleHasOpenApiPermission);

        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromSystemTextJsonString<RelationRolePermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <summary>
    /// 授权OpenApi权限
    /// </summary>
    /// <param name="input">授权信息</param>
    [OperDesc("RoleGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var sysRole = (await GetAllAsync()).FirstOrDefault(it => it.Id == input.Id);//获取角色
        if (sysRole != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.RoleHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToSystemTextJsonString()))
                , true);//添加到数据库
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id });//清除角色下用户缓存
        }
    }

    #endregion OPENAPI

    /// <summary>
    /// 获取角色的用户id列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> OwnUserAsync(long id)
    {
        //获取关系列表
        var relations = await _relationService.GetRelationListByTargetIdAndCategoryAsync(id.ToString(), RelationCategoryEnum.UserHasRole);
        return relations.Select(it => it.ObjectId);
    }

    /// <summary>
    /// 刷新权限
    /// </summary>
    public void RefreshCache()
    {
        NetCoreApp.CacheService.Remove(CacheConst.Cache_SysRole);//删除KEY
    }

    #endregion 授权

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysRole"></param>
    private async Task CheckInput(SysRole sysRole)
    {
        //判断分类
        if (sysRole.Category != RoleCategoryEnum.Global && sysRole.Category != RoleCategoryEnum.Api)
            throw Oops.Bah(Localizer["CategoryError", sysRole.Category]);

        var sysRoles = await GetAllAsync();//获取所有
        var repeatName = sysRoles.Any(it => it.Name == sysRole.Name && it.Id != sysRole.Id);//是否有重复角色名称
        if (repeatName)//如果有
        {
            throw Oops.Bah(Localizer["NameDup", sysRole.Name]);
        }
    }

    #endregion 方法
}
