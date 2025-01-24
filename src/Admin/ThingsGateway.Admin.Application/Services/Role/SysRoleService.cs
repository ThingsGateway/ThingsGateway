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

using SqlSugar;

using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Admin.Application;

internal sealed class SysRoleService : BaseService<SysRole>, ISysRoleService
{
    private readonly IRelationService _relationService;
    private readonly ISysResourceService _sysResourceService;
    private readonly ISysOrgService _sysOrgService;
    private ISysUserService _sysUserService;
    private ISysUserService SysUserService
    {
        get
        {
            if (_sysUserService == null)
            {
                _sysUserService = App.GetService<ISysUserService>();
            }
            return _sysUserService;
        }
    }
    private IDispatchService<SysRole> _dispatchService;

    public SysRoleService(IRelationService relationService, ISysResourceService sysResourceService, ISysOrgService sysOrgService, IDispatchService<SysRole> dispatchService)
    {
        _relationService = relationService;
        _sysResourceService = sysResourceService;
        _sysOrgService = sysOrgService;
        _dispatchService = dispatchService;
    }

    #region 查询

    /// <inheritdoc/>
    public async Task<List<RoleTreeOutput>> TreeAsync()
    {
        var result = new List<RoleTreeOutput>();//返回结果
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//获取所有机构
        var sysRoles = await GetAllAsync().ConfigureAwait(false);//获取所有角色
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//在指定组织列表查询
        sysRoles = sysRoles
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)

            .ToList();//在指定职位列表查询

        var topOrgList = sysOrgList.Where(it => it.ParentId == 0);//获取顶级机构
        var globalRole = sysRoles.Where(it => it.Category == RoleCategoryEnum.Global);//获取全局角色
        if (globalRole.Any())
        {
            result.Add(new RoleTreeOutput()
            {
                Id = CommonUtils.GetSingleId(),
                Name = Localizer["Global"],
                Children = globalRole.Select(it => new RoleTreeOutput
                {
                    Id = it.Id,
                    Name = it.Name,
                    IsRole = true
                }).ToList()
            });//添加全局角色
        }
        //遍历顶级机构
        foreach (var org in topOrgList)
        {
            var childIds = await _sysOrgService.GetOrgChildIdsAsync(org.Id, true, sysOrgList).ConfigureAwait(false);//获取机构下的所有子级ID
            var childRoles = sysRoles.Where(it => it.OrgId != 0 && childIds.Contains(it.OrgId));//获取机构下的所有角色
            if (childRoles.Any())
            {
                var roleTreeOutput = new RoleTreeOutput
                {
                    Id = org.Id,
                    Name = org.Name,
                    IsRole = false
                };//实例化角色树
                foreach (var it in childRoles)
                {
                    roleTreeOutput.Children.Add(new RoleTreeOutput()
                    {
                        Id = it.Id,
                        Name = it.Name,
                        IsRole = true
                    });
                }
                result.Add(roleTreeOutput);
            }
        }
        return result;
    }

    /// <summary>
    /// 从缓存/数据库获取全部角色信息
    /// </summary>
    /// <returns>角色列表</returns>
    public async Task<List<SysRole>> GetAllAsync()
    {
        var key = CacheConst.Cache_SysRole;
        var sysRoles = App.CacheService.Get<List<SysRole>>(key);
        if (sysRoles == null)
        {
            using var db = GetDB();
            sysRoles = await db.Queryable<SysRole>().ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(key, sysRoles);
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
        var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//根据用户ID获取角色ID
        var roleIdList = roleList.Select(x => x.TargetId.ToLong());//角色ID列表
        return (await GetAllAsync().ConfigureAwait(false)).Where(it => roleIdList.Contains(it.Id));
    }

    /// <inheritdoc/>
    public async Task<QueryData<SysRole>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<SysRole>, ISugarQueryable<SysRole>>? queryFunc = null)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false); //获取机构ID范围
        queryFunc += a => a
             .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
             .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);

        return await QueryAsync(option, queryFunc).ConfigureAwait(false);
    }
    /// <summary>
    /// 根据角色id获取角色列表
    /// </summary>
    /// <param name="input">角色id列表</param>
    /// <returns>角色列表</returns>
    public async Task<IEnumerable<SysRole>> GetRoleListByIdListAsync(IEnumerable<long> input)
    {
        var roles = await GetAllAsync().ConfigureAwait(false);
        var roleList = roles.Where(it => input.Contains(it.Id));
        return roleList;
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
        var sysRoles = await GetAllAsync().ConfigureAwait(false);//获取所有角色
        var hasSuperAdmin = sysRoles.Any(it => it.Id == RoleConst.SuperAdminRoleId && ids.Contains(it.Id));//判断是否有超级管理员
        if (hasSuperAdmin)
            throw Oops.Bah(Localizer["CanotDeleteAdmin"]);


        var dels = (await GetAllAsync().ConfigureAwait(false)).Where(a => ids.Contains(a.Id));
        await SysUserService.CheckApiDataScopeAsync(dels.Select(a => a.OrgId).ToList(), dels.Select(a => a.CreateUserId).ToList()).ConfigureAwait(false);

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
            await db.Deleteable<SysRole>().In(ids.ToList()).ExecuteCommandHasChangeAsync().ConfigureAwait(false);//删除
            //删除关系表角色与资源关系，角色与权限关系
            await db.Deleteable<SysRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync().ConfigureAwait(false);
            //删除关系表角色与用户关系
            await db.Deleteable<SysRelation>(it => targetIds.Contains(it.TargetId) && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            RefreshCache();//刷新缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//关系表刷新UserHasRole缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//关系表刷新RoleHasResource缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//关系表刷新RoleHasPermission缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//关系表刷新RoleHasModule缓存
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasOpenApiPermission);//关系表刷新RoleHasOpenApiPermission缓存
            await ClearTokenUtil.DeleteUserCacheByRoleIds(ids).ConfigureAwait(false);//清除角色下用户缓存
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

        await CheckInput(input).ConfigureAwait(false);//检查参数

        if (type == ItemChangedType.Add)
        {
            if (!((await SysUserService.GetUserByIdAsync(UserManager.UserId).ConfigureAwait(false)).IsGlobal))
            {
                input.Category = RoleCategoryEnum.Org;
            }
        }
        else
        {
            await SysUserService.CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);
        }

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {
            RefreshCache();
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//清除角色下用户缓存
            return true;
        }
        return false;
    }

    #endregion 修改

    #region 授权



    #region 资源

    /// <summary>
    /// 获取拥有的资源
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="category">类型</param>
    public async Task<GrantResourceData> OwnResourceAsync(long id, RelationCategoryEnum category = RelationCategoryEnum.RoleHasResource)
    {
        var roleOwnResource = new GrantResourceData() { Id = id };//定义结果集

        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, category).ConfigureAwait(false);
        roleOwnResource.GrantInfoList = relations.Select(it => (it.ExtJson?.FromJsonNetString<RelationResourcePermission?>())).Where(a => a != null);
        return roleOwnResource;
    }
    /// <summary>
    /// 授权资源
    /// </summary>
    /// <param name="input">授权信息</param>
    [OperDesc("RoleGrantResource")]
    public async Task GrantResourceAsync(GrantResourceData input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//判断是否有超管
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//菜单ID
        var extJsons = input.GrantInfoList.Select(it => it.ToJsonNetString()).ToList();//拓展信息
        var relationRoles = new List<SysRelation>();//要添加的角色资源和授权关系表
        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(it => it.Id == input.Id);//获取角色

        await SysUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

        if (sysRole != null)
        {
            var resources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);
            var menusList = resources.Where(a => a.Category == ResourceCategoryEnum.Menu).Where(a => menuIds.Contains(a.Id));

            #region 角色模块处理

            //获取我的模块信息Id列表
            var moduleIds = menusList.Select(it => it.Module).Distinct();
            foreach (var item in moduleIds)
            {
                //将角色资源添加到列表
                relationRoles.Add(new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.RoleHasModule
                });
            }

            #endregion 角色模块处理

            #region 角色资源处理

            //遍历菜单列表
            for (var i = 0; i < menuIds.Count; i++)
            {
                //将角色资源添加到列表
                relationRoles.Add(new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = menuIds[i].ToString(),
                    Category = RelationCategoryEnum.RoleHasResource,
                    ExtJson = extJsons?[i]
                });
            }

            #endregion 角色资源处理

            #region 角色权限处理.
            var defaultDataScope = sysRole.DefaultDataScope;//获取默认数据范围

            if (menusList.Any())
            {
                //获取权限授权树
                var permissions = App.GetService<IApiPermissionService>().PermissionTreeSelector(menusList.Select(it => it.Href));
                //要添加的角色有哪些权限列表
                var relationRolePer = permissions.Select(it => new SysRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.RoleHasPermission,
                    ExtJson = new RelationPermission
                    {
                        ApiUrl = it.ApiRoute,
                    }.ToJsonNetString()
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

                    )).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Insertable(relationRoles).ExecuteCommandAsync().ConfigureAwait(false);//添加新的
            }).ConfigureAwait(false);
            if (result.IsSuccess)//如果成功了
            {
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//关系表刷新
                await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//清除角色下用户缓存
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion 保存数据库
        }
    }
    #endregion

    #region OPENAPI

    /// <summary>
    /// 获取角色拥有的OpenApi权限
    /// </summary>
    /// <param name="id">角色id</param>
    public async Task<GrantPermissionData> ApiOwnPermissionAsync(long id)
    {
        var roleOwnPermission = new GrantPermissionData { Id = id };//定义结果集
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.RoleHasOpenApiPermission).ConfigureAwait(false);

        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromJsonNetString<RelationPermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <summary>
    /// 授权OpenApi权限
    /// </summary>
    /// <param name="input">授权信息</param>
    [OperDesc("RoleGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//判断是否有超管
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);

        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(it => it.Id == input.Id);//获取角色

        await SysUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

        if (sysRole != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.RoleHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToJsonNetString()))
                , true).ConfigureAwait(false);//添加到数据库
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//清除角色下用户缓存
        }
    }

    #endregion OPENAPI

    #region 用户

    /// <inheritdoc/>
    public async Task<IEnumerable<long>> OwnUserAsync(long id)
    {
        //获取关系列表
        var relations = await _relationService.GetRelationListByTargetIdAndCategoryAsync(id.ToString(), RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
        return relations.Select(it => it.ObjectId);
    }


    /// <summary>
    /// 授权用户
    /// </summary>
    /// <param name="input">授权参数</param>
    [OperDesc("RoleGrantUser")]
    public async Task GrantUserAsync(GrantUserOrRoleInput input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//判断是否有超管
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);

        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(a => a.Id == input.Id);
        await SysUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

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
            await db.Deleteable<SysRelation>(it => it.TargetId == targetId && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync().ConfigureAwait(false);//删除老的
            await db.Insertable(sysRelations.ToList()).ExecuteCommandAsync().ConfigureAwait(false);//添加新的
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//刷新关系表UserHasRole缓存
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//清除角色下用户缓存
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }


    #endregion

    /// <inheritdoc/>
    public void RefreshCache()
    {
        App.CacheService.Remove(CacheConst.Cache_SysRole);//删除KEY

        _dispatchService.Dispatch(null);

    }

    #endregion 授权

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysRole"></param>
    private async Task CheckInput(SysRole sysRole)
    {
        if (sysRole.Id == RoleConst.SuperAdminRoleId)
            throw Oops.Bah(Localizer["CanotEditAdmin"]);
        if (sysRole.Category == RoleCategoryEnum.Org && sysRole.OrgId == 0)
            throw Oops.Bah(Localizer["OrgNotNull"]);

        if (sysRole.Category == RoleCategoryEnum.Global)//如果是全局
            sysRole.OrgId = 0;//机构id设0

        var sysRoles = await GetAllAsync().ConfigureAwait(false);//获取所有
        var repeatName = sysRoles.Any(it => it.OrgId == sysRole.OrgId && it.Name == sysRole.Name && it.Id != sysRole.Id);//是否有重复角色名称
        if (repeatName)//如果有
        {
            if (sysRole.OrgId == 0)
                throw Oops.Bah(Localizer["SameOrgNameDup", sysRole.Name]);
            throw Oops.Bah(Localizer["NameDup", sysRole.Name]);
        }

        if (!((await GetRoleListByUserIdAsync(UserManager.UserId).ConfigureAwait(false)).Any(a => a.Category == RoleCategoryEnum.Global)) && sysRole.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ALL)
            throw Oops.Bah(Localizer["CannotRoleScopeAll"]);

        //如果code没填
        if (string.IsNullOrEmpty(sysRole.Code))
        {
            sysRole.Code = RandomHelper.CreateRandomString(10);//赋值Code
        }
        //判断是否有相同的Code
        if (sysRoles.Any(it => it.Code == sysRole.Code && it.Id != sysRole.Id))
            throw Oops.Bah(Localizer["CodeDup", sysRole.Code]);


    }


    #endregion 方法
}
