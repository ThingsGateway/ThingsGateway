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

using Mapster;

using SqlSugar;

using ThingsGateway.DataEncryption;
using ThingsGateway.Extension;
using ThingsGateway.Extension.Generic;
using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Admin.Application;

internal sealed class SysUserService : BaseService<SysUser>, ISysUserService
{
    private readonly IRelationService _relationService;
    private readonly ISysResourceService _sysResourceService;
    private readonly ISysRoleService _roleService;
    private readonly ISysDictService _configService;
    private readonly ISysPositionService _sysPositionService;
    private readonly ISysOrgService _sysOrgService;
    private readonly IVerificatInfoService _verificatInfoService;

    public SysUserService(
        IVerificatInfoService verificatInfoService,
        IRelationService relationService,
        ISysPositionService sysPositionService,
        ISysOrgService sysOrgService,
        ISysResourceService sysResourceService,
        ISysRoleService roleService,
        ISysDictService configService)
    {
        _sysOrgService = sysOrgService;
        _sysPositionService = sysPositionService;
        _relationService = relationService;
        _sysResourceService = sysResourceService;
        _roleService = roleService;
        _configService = configService;
        _verificatInfoService = verificatInfoService;
    }


    #region 数据范围相关

    /// <inheritdoc/>
    public async Task<HashSet<long>?> GetCurrentUserDataScopeAsync()
    {
        if (UserManager.SuperAdmin || UserManager.UserId == 0)
            return null;
        var userInfo = await GetUserByIdAsync(UserManager.UserId).ConfigureAwait(false);//获取用户信息
        var roles = await _roleService.GetRoleListByUserIdAsync(UserManager.UserId).ConfigureAwait(false);
        if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ALL))
        {
            return null;
        }
        else
        {
            var scopeDefineOrgIdList = roles.Where(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_DEFINE).SelectMany(a => a.DefaultDataScope.ScopeDefineOrgIdList);

            HashSet<long> orgChilds = new();
            HashSet<long> orgs = new();
            if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_CHILD))
            {
                orgChilds = userInfo.ScopeOrgChildList;
            }
            if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_CHILD))
            {
                orgs = new HashSet<long>() { userInfo.OrgId };
            }
            return scopeDefineOrgIdList.Concat(orgChilds).Concat(orgs).ToHashSet();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CheckApiDataScopeAsync(long? orgId, long createUerId, bool throwEnable = true)
    {
        var hasPermission = true;
        //判断数据范围
        var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        if (dataScope is { Count: > 0 })//如果有机构
        {
            if (orgId == null || !dataScope.Contains(orgId.Value))//判断机构id是否在数据范围
                hasPermission = false;
        }
        else if (dataScope is { Count: 0 })// 表示仅自己
        {
            if (createUerId != 0 && createUerId != UserManager.UserId)
                hasPermission = false;//机构的创建人不是自己则报错
        }
        if (!hasPermission && throwEnable)
        {
            throw Oops.Bah(App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Application.OperDescAttribute))["NoPermission"]);
        }
        return hasPermission;
    }


    public async Task<bool> CheckApiDataScopeAsync(IEnumerable<long> orgIds, IEnumerable<long> createUerIds, bool throwEnable = true)
    {
        var hasPermission = true;
        //判断数据范围
        var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        if (dataScope is { Count: > 0 })//如果有机构
        {
            if (orgIds == null || !dataScope.ContainsAll(orgIds))//判断机构id列表是否全在数据范围
                hasPermission = false;
        }
        else if (dataScope is { Count: 0 })// 表示仅自己
        {
            if (createUerIds.Any(it => it != 0 && it != UserManager.UserId))//如果创建者id里有任何不是自己创建的机构
                hasPermission = false;
        }
        if (!hasPermission && throwEnable)
        {
            throw Oops.Bah(App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Application.OperDescAttribute))["NoPermission"]);
        }
        return hasPermission;
    }

    #endregion

    #region 查询


    /// <inheritdoc/>
    public async Task<SysUser?> GetUserByAccountAsync(string account, long? tenantId)
    {
        var userId = await GetIdByAccountAsync(account, tenantId).ConfigureAwait(false);//获取用户ID
        if (userId > 0)
        {
            var sysUser = await GetUserByIdAsync(userId).ConfigureAwait(false);//获取用户信息
            if (sysUser?.Account == account)
                return sysUser;
            else
                return null;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 根据用户id获取用户，不存在返回null
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>用户</returns>
    public async Task<SysUser?> GetUserByIdAsync(long userId)
    {
        //先从Cache拿
        var sysUser = App.CacheService.HashGetOne<SysUser>(CacheConst.Cache_SysUser, userId.ToString());
        sysUser ??= await GetUserFromDbAsync(userId).ConfigureAwait(false);//从数据库拿用户信息
        return sysUser;
    }

    /// <summary>
    /// 根据账号获取用户id
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="tenantId">租户id</param>
    /// <returns>用户id</returns>
    public async Task<long> GetIdByAccountAsync(string account, long? tenantId)
    {
        var key = CacheConst.Cache_SysUserAccount;
        var orgIds = new HashSet<long>();
        if (tenantId > 0)
        {
            key += $":{tenantId}";
            orgIds = await _sysOrgService.GetOrgChildIdsAsync(tenantId.Value).ConfigureAwait(false);//获取下级机构
        }
        //先从Cache拿
        var userId = App.CacheService.HashGetOne<long>(key, account);
        if (userId == 0)
        {

            //单查获取用户账号对应ID
            using var db = GetDB();
            userId = await db.Queryable<SysUser>()
                .Where(it => it.Account == account)
                .WhereIF(orgIds.Count > 0, it => orgIds.Contains(it.OrgId))
                .Select(it => it.Id).FirstAsync().ConfigureAwait(false);
            if (userId != 0)
            {
                //插入Cache
                App.CacheService.HashAdd(key, account, userId);
            }
        }
        return userId;
    }

    /// <summary>
    /// 获取用户拥有的按钮编码
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>按钮编码</returns>
    public async Task<Dictionary<string, List<string>>> GetButtonCodeListAsync(long userId)
    {
        //获取用户资源集合
        var resourceList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasResource).ConfigureAwait(false);
        if (!resourceList.Any())//如果有表示用户单独授权了不走用户角色
        {
            //获取用户角色关系集合
            var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
            var roleIdList = roleList.Select(x => x.TargetId.ToLong());//角色ID列表
            if (roleIdList.Any())//如果该用户有角色
            {
                resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList,
                    RelationCategoryEnum.RoleHasResource).ConfigureAwait(false);//获取资源集合
            }
        }
        var relationResourcePermissions = resourceList.Select(it => it.ExtJson?.FromJsonNetString<RelationResourcePermission>());
        var allResources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);


        var menus = allResources.Where(it => it.Category == ResourceCategoryEnum.Menu && relationResourcePermissions.Select(a => a.MenuId).Contains(it.Id)).ToDictionary(a => a, a => relationResourcePermissions.FirstOrDefault(b => b.MenuId == a.Id));

        Dictionary<string, List<string>> buttonCodeList = new();
        foreach (var item in menus)
        {
            if (buttonCodeList.TryGetValue(item.Key.Href, out var buttonCode))
            {
                var buttonS = allResources.Where(a => item.Value.ButtonIds.Contains(a.Id));
                buttonCode.AddRange(buttonS.Select(a => a.Title));
            }
            else
            {
                var buttonS = allResources.Where(a => item.Value.ButtonIds.Contains(a.Id));
                buttonCodeList.Add(item.Key.Href, buttonS.Select(a => a.Title).ToList());
            }
        }

        var firstbuttons = allResources.Where(it => it.Category == ResourceCategoryEnum.Button && relationResourcePermissions.FirstOrDefault(a => a.MenuId == 0)?.ButtonIds?.Contains(it.Id) == true);
        buttonCodeList.Add(string.Empty, firstbuttons?.Select(a => a.Title).ToList());

        return buttonCodeList!;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataScope>> GetPermissionListByUserIdAsync(long userId)
    {
        List<DataScope>? permissions = new();

        #region Razor页面权限

        {
            var sysRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasPermission).ConfigureAwait(false);//根据用户ID获取用户权限
            if (!sysRelations.Any())//如果有表示用户单独授权了不走用户角色
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//根据用户ID获取角色ID
                if (roleIdList.Any())//如果角色ID不为空
                {
                    //获取角色权限信息
                    sysRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()),
                        RelationCategoryEnum.RoleHasPermission).ConfigureAwait(false);
                }
            }
            var relationGroup = sysRelations.GroupBy(it => it.TargetId);//根据目标ID,也就是接口名分组，因为存在一个用户多个角色

            //遍历分组
            foreach (var it in relationGroup)
            {
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });
            }

        }

        #endregion Razor页面权限

        #region API权限

        {
            var apiRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasOpenApiPermission).ConfigureAwait(false);//根据用户ID获取用户权限
            if (!apiRelations.Any())//如果有表示用户单独授权了不走用户角色
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//根据用户ID获取角色ID
                if (roleIdList.Any())//如果角色ID不为空
                {
                    //获取角色权限信息
                    apiRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()),
                        RelationCategoryEnum.RoleHasOpenApiPermission).ConfigureAwait(false);
                }
            }
            var relationGroup = apiRelations.GroupBy(it => it.TargetId);//根据目标ID,也就是接口名分组，因为存在一个用户多个角色

            //遍历分组
            foreach (var it in relationGroup)
            {
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });
            }

        }

        #endregion API权限

        return permissions;
    }

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="input">查询条件</param>
    public async Task<QueryData<SysUser>> PageAsync(QueryPageOptions option, UserSelectorInput input)
    {
        if (input != null)
        {
            option.SortName = "u." + option.SortName;
            var orgIds = await _sysOrgService.GetOrgChildIdsAsync(input.OrgId).ConfigureAwait(false);//获取下级机构
            var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);

            return await QueryAsync(option, query =>
            query.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText))
             .WhereIF(input.OrgId > 0, u => orgIds.Contains(u.OrgId))//指定机构
                .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.OrgId))//在指定机构列表查询
                .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
                .WhereIF(input.PositionId > 0, u => u.PositionId == input.PositionId)//指定职位

                .WhereIF(input.RoleId > 0,
                    u => SqlFunc.Subqueryable<SysRelation>()
                        .Where(r => r.TargetId == input.RoleId.ToString() && r.ObjectId == u.Id && r.Category == RelationCategoryEnum.UserHasRole)
                        .Any())//指定角色

       .LeftJoin<SysOrg>((u, o) => u.OrgId == o.Id).LeftJoin<SysPosition>((u, o, p) => u.PositionId == p.Id)
          .Select((u, o, p) => new SysUser
          {
              Id = u.Id.SelectAll(),
              OrgName = o.Name,
              PositionName = p.Name,
              OrgNames = o.Names
          }
                  )
            .Mapper(u =>
            {
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
                u.Password = null;//密码清空
                u.Phone = DESEncryption.Decrypt(u.Phone);//解密手机号
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            })).ConfigureAwait(false);

        }
        else
        {
            return await QueryAsync(option, query =>
        query.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText)).Mapper(u =>
                {
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
                    u.Password = null;//密码清空
                    u.Phone = DESEncryption.Decrypt(u.Phone);//解密手机号
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
                })).ConfigureAwait(false);
        }




    }

    /// <summary>
    /// 获取用户拥有的角色
    /// </summary>
    /// <param name="id">用户id</param>
    /// <returns>角色id列表</returns>
    public async Task<IEnumerable<long>> OwnRoleAsync(long id)
    {
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
        return relations.Select(it => it.TargetId.ToLong());
    }

    /// <summary>
    /// 获取用户拥有的资源
    /// </summary>
    /// <param name="id">用户id</param>
    public async Task<GrantResourceData> OwnResourceAsync(long id)
    {
        return await _roleService.OwnResourceAsync(id, RelationCategoryEnum.UserHasResource).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据用户id获取用户列表
    /// </summary>
    /// <param name="input">用户id列表</param>
    /// <returns>用户列表</returns>
    public async Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IEnumerable<long> input)
    {
        using var db = GetDB();
        var userList = await db.Queryable<SysUser>().Where(it => input.Contains(it.Id)).Select<UserSelectorOutput>().ToListAsync().ConfigureAwait(false);
        return userList;
    }

    #endregion 查询

    #region OPENAPI

    /// <summary>
    /// 获取用户拥有的OpenApi权限
    /// </summary>
    /// <param name="id">用户id</param>
    public async Task<GrantPermissionData> ApiOwnPermissionAsync(long id)
    {
        var roleOwnPermission = new GrantPermissionData { Id = id };//定义结果集
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.UserHasOpenApiPermission).ConfigureAwait(false);
        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromJsonNetString<RelationPermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("UserGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户

        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.UserHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToJsonNetString())),
                true).ConfigureAwait(false);//添加到数据库
            DeleteUserFromCache(input.Id);
        }
    }

    #endregion OPENAPI

    #region 新增

    /// <inheritdoc/>
    [OperDesc("SaveUser", isRecordPar: false)]
    public async Task<bool> SaveUserAsync(SysUser input, ItemChangedType changedType)
    {
        await CheckInput(input).ConfigureAwait(false);//检查参数

        if (changedType == ItemChangedType.Add)
        {
            var sysUser = input.Adapt<SysUser>();
            //获取默认密码
            sysUser.Avatar = input.Avatar;
            sysUser.Password = await GetDefaultPassWord(true).ConfigureAwait(false);//设置密码
            sysUser.Status = true;//默认状态
            return await SaveAsync(sysUser, changedType).ConfigureAwait(false);//添加数据
        }
        else
        {
            await CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);
            var exist = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户信息
            if (exist != null)
            {
                var isSuperAdmin = exist.Account == RoleConst.SuperAdmin;//判断是否有超管
                if (isSuperAdmin && !UserManager.SuperAdmin)
                    throw Oops.Bah(Localizer["CanotEditAdminUser"]);

                if (input.Status != exist.Status)
                    CheckSelf(input.Id, input.Status ? Localizer["Enable"] : Localizer["Disable"]);//判断是不是自己

                var sysUser = input;//实体转换
                using var db = GetDB();
                var result = await db.Updateable(sysUser).IgnoreColumns(it =>
                        new
                        {
                            //忽略更新字段
                            it.Password,
                            it.LastLoginDevice,
                            it.LastLoginIp,
                            it.LastLoginTime,
                            it.LatestLoginDevice,
                            it.LatestLoginIp,
                            it.LatestLoginTime
                        }).ExecuteCommandAsync().ConfigureAwait(false) > 0;
                if (result)//修改数据
                {
                    DeleteUserFromCache(sysUser.Id);//删除用户缓存

                    var verificatInfoIds = _verificatInfoService.GetListByUserId(sysUser.Id);

                    //从列表中删除
                    //删除用户verificat缓存
                    _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
                    await NoticeUtil.UserLoginOut(new UserLoginOutEvent() { ClientIds = verificatInfoIds.SelectMany(a => a.ClientIds).ToList(), Message = Localizer["ExitVerificat"] }).ConfigureAwait(false);
                }
                return result;
            }
        }
        return false;
    }

    #endregion 新增

    #region 编辑

    /// <inheritdoc/>
    [OperDesc("ResetPassword")]
    public async Task ResetPasswordAsync(long id)
    {
        var sysUser = await GetUserByIdAsync(id).ConfigureAwait(false);

        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);

        var password = await GetDefaultPassWord(true).ConfigureAwait(false);//获取默认密码,这里不走Aop所以需要加密一下
        using var db = GetDB();
        //重置密码
        if (await db.UpdateSetColumnsTrueAsync<SysUser>(it => new SysUser
        {
            Password = password
        }, it => it.Id == id).ConfigureAwait(false))
        {
            DeleteUserFromCache(id);//从cache删除用户信息
            var verificatInfoIds = _verificatInfoService.GetListByUserId(id);
            //删除用户verificat缓存
            _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
            await NoticeUtil.UserLoginOut(new UserLoginOutEvent() { ClientIds = verificatInfoIds.SelectMany(a => a.ClientIds).ToList(), Message = Localizer["ExitVerificat"] }).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [OperDesc("UserGrantRole")]
    public async Task GrantRoleAsync(GrantUserOrRoleInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户信息
        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            var isSuperAdmin = (sysUser.Account == RoleConst.SuperAdmin || input.GrantInfoList.Any(a => a == RoleConst.SuperAdminRoleId)) && !UserManager.SuperAdmin;//判断是否有超管
            if (isSuperAdmin)
                throw Oops.Bah(Localizer["CanotGrantAdmin"]);

            CheckSelf(input.Id, Localizer["GrantRole"]);//判断是不是自己

            //给用户赋角色
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.UserHasRole, input.Id, input.GrantInfoList.Select(it => (it.ToString(), string.Empty)), true).ConfigureAwait(false);
            DeleteUserFromCache(input.Id);//从cache删除用户信息
        }
    }

    /// <inheritdoc />
    [OperDesc("UserGrantResource")]
    public async Task GrantResourceAsync(GrantResourceData input)
    {
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//菜单ID
        var extJsons = input.GrantInfoList.Select(it => it.ToJsonNetString()).ToList();//拓展信息
        var relationUsers = new List<SysRelation>();//要添加的用户资源和授权关系表
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户
        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            var resources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);
            var menusList = resources.Where(a => a.Category == ResourceCategoryEnum.Menu).Where(a => menuIds.Contains(a.Id));


            #region 用户模块处理

            //获取我的模块信息Id列表
            var moduleIds = menusList.Select(it => it.Module).Distinct();
            foreach (var item in moduleIds)
            {
                //将角色资源添加到列表
                relationUsers.Add(new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.UserHasModule
                });
            }

            #endregion 用户模块处理

            #region 用户资源处理

            for (var i = 0; i < menuIds.Count; i++)
            {
                //将角色资源添加到列表
                relationUsers.Add(new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = menuIds[i].ToString(),
                    Category = RelationCategoryEnum.UserHasResource,
                    ExtJson = extJsons?[i]
                });
            }
            #endregion 用户资源处理

            #region 用户权限处理.

            //获取菜单信息
            if (menusList.Any())
            {
                //获取权限授权树
                var permissions = App.GetService<IApiPermissionService>().PermissionTreeSelector(menusList.Select(it => it.Href));
                //要添加的角色有哪些权限列表
                var relationUserPer = permissions.Select(it => new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.UserHasPermission,
                    ExtJson = new RelationPermission { ApiUrl = it.ApiRoute }
                            .ToJsonNetString()
                });
                relationUsers.AddRange(relationUserPer);//合并列表
            }

            #endregion 用户权限处理.

            #region 保存数据库

            using var db = GetDB();

            //事务
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<SysRelation>(it =>
                    it.ObjectId == sysUser.Id && (it.Category == RelationCategoryEnum.UserHasPermission
                    || it.Category == RelationCategoryEnum.UserHasResource
                    || it.Category == RelationCategoryEnum.UserHasModule

                    )).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Insertable(relationUsers).ExecuteCommandAsync().ConfigureAwait(false);//添加新的
            }).ConfigureAwait(false);
            if (result.IsSuccess)//如果成功了
            {
                _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//刷新关系缓存
                _relationService.RefreshCache(RelationCategoryEnum.UserHasModule);//刷新关系缓存
                DeleteUserFromCache(input.Id);//删除该用户缓存
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion 保存数据库
        }
    }

    #endregion 编辑

    #region 删除

    /// <inheritdoc/>
    [OperDesc("DeleteUser")]
    public async Task<bool> DeleteUserAsync(IEnumerable<long> ids)
    {
        using var db = GetDB();
        var containsSuperAdmin = await db.Queryable<SysUser>().Where(it => it.Account == RoleConst.SuperAdmin && ids.Contains(it.Id)).AnyAsync().ConfigureAwait(false);//判断是否有超管
        if (containsSuperAdmin)
            throw Oops.Bah(Localizer["CanotDeleteAdminUser"]);
        if (ids.Contains(UserManager.UserId))
            throw Oops.Bah(Localizer["CanotDeleteSelf"]);

        var sysUsers = await GetUserListByIdListAsync(ids).ConfigureAwait(false);//获取用户信息
        await CheckApiDataScopeAsync(sysUsers.Select(a => a.OrgId).ToList(), sysUsers.Select(a => a.CreateUserId).ToList()).ConfigureAwait(false);

        //定义删除的关系
        var delRelations = new List<RelationCategoryEnum>
            {
                RelationCategoryEnum.UserHasResource, RelationCategoryEnum.UserHasPermission, RelationCategoryEnum.UserHasRole, RelationCategoryEnum.UserHasOpenApiPermission
                , RelationCategoryEnum.UserHasModule
            };
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            //清除该用户作为主管信息
            await db.Updateable<SysUser>().SetColumns(it => new SysUser
            {
                DirectorId = null
            })
            .Where(it => ids.Contains(it.DirectorId.Value))
            .ExecuteCommandAsync().ConfigureAwait(false);

            //删除用户
            await db.Deleteable<SysUser>().In(ids.ToList()).ExecuteCommandHasChangeAsync().ConfigureAwait(false);//删除

            //删除关系表用户与资源关系，用户与权限关系,用户与角色关系
            await db.Deleteable<SysRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync().ConfigureAwait(false);

            //删除组织表主管信息
            await db.Deleteable<SysOrg>(it => ids.Contains(it.DirectorId.Value)).ExecuteCommandAsync().ConfigureAwait(false);

        }).ConfigureAwait(false);

        if (result.IsSuccess)//如果成功了
        {
            DeleteUserFromCache(ids);//cache删除用户
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//关系表刷新UserHasRole缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//关系表刷新UserHasRole缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasModule);//关系表刷新UserHasModule缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);//关系表刷新UserHasRole缓存
            _relationService.RefreshCache(RelationCategoryEnum.UserHasOpenApiPermission);//关系表刷新Relation_SYS_USER_HAS_OPENAPIPERMISSION缓存
            //将这些用户踢下线，并永久注销这些用户
            foreach (var id in ids)
            {
                var verificatInfoIds = _verificatInfoService.GetListByUserId(id);
                _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
                await UserLoginOut(id, verificatInfoIds.SelectMany(a => a.ClientIds).ToList()).ConfigureAwait(false);
            }

            return true;
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(long userId)
    {
        DeleteUserFromCache(new List<long>
        {
            userId
        });
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(IEnumerable<long> ids)
    {
        var userIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
        var sysUsers = App.CacheService.HashGet<SysUser>(CacheConst.Cache_SysUser, userIds).Where(it => it != null);//获取用户列表
        if (sysUsers.Any() == true)
        {
            var accounts = sysUsers.Where(it => it != null).Select(it => it.Account).ToArray();//账号集合
            var phones = sysUsers.Select(it => it.Phone);//手机号集合

            if (sysUsers.Any(it => it.TenantId != null))//如果有租户id不是空的表示是多租户模式
            {
                var userAccountKey = CacheConst.Cache_SysUserAccount;
                var tenantIds = sysUsers.Where(it => it.TenantId != null).Select(it => it.TenantId.Value).Distinct().ToArray();//租户id列表
                foreach (var tenantId in tenantIds)
                {
                    userAccountKey = $"{userAccountKey}:{tenantId}";
                    //删除账号
                    App.CacheService.HashDel<long>(userAccountKey, accounts);
                }
            }
            //删除用户信息
            App.CacheService.HashDel<SysUser>(CacheConst.Cache_SysUser, userIds);
            //删除账号
            App.CacheService.HashDel<long>(CacheConst.Cache_SysUserAccount, accounts);

            App.CacheService.HashDel<VerificatInfo>(CacheConst.Cache_Token, userIds.Select(it => it.ToString()).ToArray());


        }
    }

    #endregion 删除

    #region 方法

    /// <summary>
    /// 通知用户下线
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="verificatInfoIds">Token列表</param>
    private async Task UserLoginOut(long userId, List<long>? verificatInfoIds)
    {
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent
        {
            Message = Localizer["ExitVerificat"],
            ClientIds = verificatInfoIds,
        }).ConfigureAwait(false);//通知用户下线
    }

    /// <summary>
    /// 获取默认密码
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetDefaultPassWord(bool isEncrypt = false)
    {
        //获取默认密码
        var appConfig = await _configService.GetAppConfigAsync().ConfigureAwait(false);
        return isEncrypt ? DESEncryption.Encrypt(appConfig.PasswordPolicy.DefaultPassword) : appConfig.PasswordPolicy.DefaultPassword;//判断是否需要加密
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysUser"></param>
    private async Task CheckInput(SysUser sysUser)
    {

        var sysOrgList = await _sysOrgService.GetAllAsync().ConfigureAwait(false);//获取组织列表
        var userOrg = sysOrgList.FirstOrDefault(it => it.Id == sysUser.OrgId);
        if (userOrg == null)
            throw Oops.Bah(Localizer[$"NoOrg"]);
        var tenantId = await _sysOrgService.GetTenantIdByOrgIdAsync(sysUser.OrgId, sysOrgList).ConfigureAwait(false);

        //判断账号重复,直接从cache拿
        var accountId = await GetIdByAccountAsync(sysUser.Account, tenantId).ConfigureAwait(false);
        if (accountId > 0 && accountId != sysUser.Id)
            throw Oops.Bah(Localizer["AccountDup", sysUser.Account]);
        //如果邮箱不是空
        if (!string.IsNullOrEmpty(sysUser.Email))
        {
            var isMatch = sysUser.Email.MatchEmail();//验证邮箱格式
            if (!isMatch)
                throw Oops.Bah(Localizer["EmailError", sysUser.Email]);

            using var db = GetDB();
            if (await db.Queryable<SysUser>().Where(it => it.Email == sysUser.Email && it.Id != sysUser.Id).AnyAsync().ConfigureAwait(false))
                throw Oops.Bah(Localizer["EmailDup", sysUser.Email]);
        }
        //如果手机号不是空
        if (!string.IsNullOrEmpty(sysUser.Phone))
        {
            if (!sysUser.Phone.MatchPhoneNumber())//验证手机格式
                throw Oops.Bah(Localizer["PhoneError", sysUser.Phone]);
            sysUser.Phone = DESEncryption.Encrypt(sysUser.Phone);
        }

        if (sysUser.DirectorId == UserManager.UserId)
            throw Oops.Bah(Localizer["DirectorSelf"]);
    }

    /// <summary>
    /// 检查是否为自己
    /// </summary>
    /// <param name="id"></param>
    /// <param name="operate">操作名称</param>
    private void CheckSelf(long id, string operate)
    {
        if (id == UserManager.UserId)//如果是自己
        {
            throw Oops.Bah(Localizer["CheckSelf", operate]);
        }
    }

    /// <summary>
    /// 数据库获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    private async Task<SysUser?> GetUserFromDbAsync(long userId)
    {
        using var db = GetDB();
        var sysUser = await db.Queryable<SysUser>()
             .LeftJoin<SysOrg>((u, o) => u.OrgId == o.Id)//连表
            .LeftJoin<SysPosition>((u, o, p) => u.PositionId == p.Id)//连表
            .Where(u => u.Id == userId)
            .Select((u, o, p) => new SysUser
            {
                Id = u.Id.SelectAll(),
                OrgName = o.Name,
                OrgNames = o.Names,
                PositionName = p.Name,
                OrgAndPosIdList = o.ParentIdList
            }).FirstAsync()
            .ConfigureAwait(false);
        if (sysUser != null)
        {
            sysUser.Password = DESEncryption.Decrypt(sysUser.Password);//解密密码
            sysUser.Phone = DESEncryption.Decrypt(sysUser.Phone);//解密手机号

            sysUser.OrgAndPosIdList.AddRange(sysUser.OrgId, sysUser.PositionId ?? 0);//添加组织和职位Id
            if (sysUser.DirectorId != null)
            {
                sysUser.DirectorInfo = (await GetUserByIdAsync(sysUser.DirectorId.Value).ConfigureAwait(false)).Adapt<UserSelectorOutput>();//获取主管信息
            }

            //获取按钮码
            var buttonCodeList = await GetButtonCodeListAsync(sysUser.Id).ConfigureAwait(false);
            //获取数据权限
            var dataScopeList = await GetPermissionListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //获取权限码
            var permissionCodeList = dataScopeList.Select(it => it.ApiUrl).ToHashSet();
            //获取角色码
            var roleCodeList = await _roleService.GetRoleListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //权限码赋值
            sysUser.ButtonCodeList = buttonCodeList;
            sysUser.RoleIdList = roleCodeList.Select(it => it.Id).ToHashSet();
            sysUser.PermissionCodeList = permissionCodeList;
            sysUser.IsGlobal = roleCodeList.Any(a => a.Category == RoleCategoryEnum.Global);



            var sysOrgList = await _sysOrgService.GetAllAsync().ConfigureAwait(false);
            var scopeOrgChildList =
                (await _sysOrgService.GetChildListByIdAsync(sysUser.OrgId, true, sysOrgList).ConfigureAwait(false)).Select(it => it.Id).ToHashSet();//获取所属机构的下级机构Id列表
            sysUser.ScopeOrgChildList = scopeOrgChildList;
            var tenantId = await _sysOrgService.GetTenantIdByOrgIdAsync(sysUser.OrgId, sysOrgList).ConfigureAwait(false);
            sysUser.TenantId = tenantId;

            if (sysUser.Account == RoleConst.SuperAdmin)
            {
                var modules = (await _sysResourceService.GetAllAsync().ConfigureAwait(false)).Where(a => a.Category == ResourceCategoryEnum.Module);
                sysUser.ModuleList = modules.ToList();//模块列表赋值给用户
            }
            else
            {
                var moduleIds = await _relationService.GetUserModuleId(sysUser.RoleIdList, sysUser.Id).ConfigureAwait(false);//获取模块ID列表
                var modules = await _sysResourceService.GetMuduleByMuduleIdsAsync(moduleIds).ConfigureAwait(false);//获取模块列表
                sysUser.ModuleList = modules.ToList();//模块列表赋值给用户
            }

            //插入Cache
            App.CacheService.HashAdd(CacheConst.Cache_SysUserAccount, sysUser.Account, sysUser.Id);
            App.CacheService.HashAdd(CacheConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);

            return sysUser;
        }
        return null;
    }

    #endregion 方法
}
