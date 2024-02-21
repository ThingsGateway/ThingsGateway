//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.FriendlyException;

using Mapster;

using ThingsGateway.Admin.Core.Utils;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="ISysUserService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class SysUserService : DbRepository<SysUser>, ISysUserService
{
    private readonly ISimpleCacheService _simpleCacheService;
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly IRoleService _roleService;
    private readonly IConfigService _configService;
    private readonly IEventPublisher _eventPublisher;

    public SysUserService(ISimpleCacheService simpleCacheService,
        IRelationService relationService,
        IResourceService resourceService, IEventPublisher eventPublisher,
        IRoleService roleService,
        IConfigService configService)
    {
        _simpleCacheService = simpleCacheService;
        _relationService = relationService;
        _resourceService = resourceService;
        _roleService = roleService;
        _configService = configService;
        _eventPublisher = eventPublisher;
    }

    #region 查询

    /// <inheritdoc/>
    public async Task<SysUser> GetUserByAccountAsync(string account)
    {
        var userId = await GetIdByAccountAsync(account);//获取用户ID
        if (userId > 0)
        {
            var sysUser = await GetUserByIdAsync(userId);//获取用户信息
            if (sysUser.Account == account)//这里做了比较用来限制大小写
                return sysUser;
            else
                return null;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SysUser> GetUserByPhoneAsync(string phone)
    {
        var userId = await GetIdByPhoneAsync(phone);//获取用户ID
        if (userId > 0)
        {
            return await GetUserByIdAsync(userId);//获取用户信息
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<long> GetIdByPhoneAsync(string phone)
    {
        //先从Redis拿
        var userId = _simpleCacheService.HashGetOne<long>(SystemConst.Cache_SysUserPhone, phone);
        if (userId == 0)
        {
            phone = CryptogramUtil.Sm2Encrypt(phone);//SM4加密一下
            //单查获取用户手机号对应的账号
            userId = await GetFirstAsync(it => it.Phone == phone, it => it.Id);
            if (userId > 0)
            {
                //插入Redis
                _simpleCacheService.HashAdd(SystemConst.Cache_SysUserPhone, phone, userId);
            }
        }
        return userId;
    }

    /// <inheritdoc/>
    public async Task<SysUser> GetUserByIdAsync(long userId)
    {
        //先从Redis拿
        var sysUser = _simpleCacheService.HashGetOne<SysUser>(SystemConst.Cache_SysUser, userId.ToString());
        sysUser ??= await GetUserFromDb(userId);//从数据库拿用户信息
        return sysUser;
    }

    /// <inheritdoc/>
    public async Task<T> GetUserByIdAsync<T>(long userId)
    {
        //先从Redis拿
        var sysUser = _simpleCacheService.HashGetOne<T>(SystemConst.Cache_SysUser, userId.ToString());
        if (sysUser == null)
        {
            var user = await GetUserFromDb(userId);//从数据库拿用户信息
            if (user != null)
            {
                sysUser = user.Adapt<T>();
            }
        }
        return sysUser;
    }

    /// <inheritdoc/>
    public async Task<long> GetIdByAccountAsync(string account)
    {
        //先从Redis拿
        var userId = _simpleCacheService.HashGetOne<long>(SystemConst.Cache_SysUserAccount, account);
        if (userId == 0)
        {
            //单查获取用户账号对应ID
            userId = await GetFirstAsync(it => it.Account == account, it => it.Id);
            if (userId != 0)
            {
                //插入Redis
                _simpleCacheService.HashAdd(SystemConst.Cache_SysUserAccount, account, userId);
            }
        }
        return userId;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetButtonCodeListAsync(long userId)
    {
        var buttonCodeList = new List<string>();//按钮ID集合
        //获取用户资源集合
        var resourceList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);
        var buttonIdList = new List<long>();//按钮ID集合
        if (resourceList.Count == 0)//如果有表示用户单独授权了不走用户角色
        {
            //获取用户角色关系集合
            var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
            if (roleIdList.Count > 0)//如果该用户有角色
            {
                resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList,
                    CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//获取资源集合
            }
        }
        resourceList.ForEach(it =>
        {
            if (!string.IsNullOrEmpty(it.ExtJson))
                buttonIdList.AddRange(it.ExtJson.FromJsonString<RelationRoleResource>().ButtonInfo);//如果有按钮权限，将按钮ID放到buttonIdList
        });
        if (buttonIdList.Count > 0)
        {
            buttonCodeList = await _resourceService.GetCodeByIdsAsync(buttonIdList, CateGoryConst.Resource_BUTTON);
        }
        return buttonCodeList;
    }

    /// <inheritdoc/>
    public async Task<List<DataScope>> GetPermissionListByUserIdAsync(long userId)
    {
        var permissions = new List<DataScope>();//权限集合

        #region Razor页面权限

        {
            var sysRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_PERMISSION);//根据用户ID获取用户权限
            if (sysRelations.Count == 0)//如果有表示用户单独授权了不走用户角色
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
                if (roleIdList.Count > 0)//如果角色ID不为空
                {
                    //获取角色权限信息
                    sysRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()).ToList(),
                        CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);
                }
            }
            var relationGroup = sysRelations.GroupBy(it => it.TargetId).ToList();//根据目标ID,也就是接口名分组，因为存在一个用户多个角色
                                                                                 //遍历分组
            foreach (var it in relationGroup)
            {
                var scopeSet = new HashSet<long>();//定义不可重复列表
                var relationList = it.ToList();//关系列表
                                               //获取角色权限信息列表
                var rolePermissions = relationList.Select(it => it.ExtJson.FromJsonString<RelationRolePermission>()).ToList();
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });//将改URL的权限集合加入权限集合列表
            }
        }

        #endregion Razor页面权限

        #region API权限

        {
            var apiRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_OPENAPIPERMISSION);//根据用户ID获取用户权限
            if (apiRelations.Count == 0)//如果有表示用户单独授权了不走用户角色
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
                if (roleIdList.Count > 0)//如果角色ID不为空
                {
                    //获取角色权限信息
                    apiRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()).ToList(),
                        CateGoryConst.Relation_SYS_ROLE_HAS_OPENAPIPERMISSION);
                }
            }
            var relationGroup = apiRelations.GroupBy(it => it.TargetId).ToList();//根据目标ID,也就是接口名分组，因为存在一个用户多个角色

            //遍历分组
            foreach (var it in relationGroup)
            {
                var scopeSet = new HashSet<long>();//定义不可重复列表
                var relationList = it.ToList();//关系列表
                                               //获取角色权限信息列表
                var rolePermissions = relationList.Select(it => it.ExtJson.FromJsonString<RelationRolePermission>()).ToList();
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });//将改URL的权限集合加入权限集合列表
            }
        }

        #endregion API权限

        return permissions;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<UserSelectorOutput>> UserSelectorAsync(UserSelectorInput input)
    {
        var result = await Context.Queryable<SysUser>()
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey))//根据关键字查询
            .Select(u => new UserSelectorOutput
            {
                Id = u.Id,
                Account = u.Account,
            })
            .ToPagedListAsync(input.Current, input.Size);
        return result;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SysUser>> PageAsync(UserPageInput input)
    {
        var query = GetQuery(input);//获取查询条件
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <inheritdoc/>
    public async Task<List<SysUser>> ListAsync(UserPageInput input)
    {
        var query = GetQuery(input);//获取查询条件
        var list = await query.ToListAsync();
        return list;
    }

    /// <inheritdoc/>
    public async Task<List<long>> OwnRoleAsync(BaseIdInput input)
    {
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
        return relations.Select(it => it.TargetId.ToLong()).ToList();
    }

    /// <inheritdoc />
    public async Task<RoleOwnResourceOutput> OwnResourceAsync(BaseIdInput input)
    {
        return await _roleService.OwnResourceAsync(input, CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);
    }

    /// <inheritdoc />
    public async Task<RoleOwnPermissionOutput> OwnPermissionAsync(BaseIdInput input)
    {
        var roleOwnPermission = new RoleOwnPermissionOutput
        {
            Id = input.Id
        };//定义结果集
        var grantInfoList = new List<RelationRolePermission>();//已授权信息集合
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_USER_HAS_PERMISSION);
        //遍历关系表
        relations.ForEach(it =>
        {
            //将扩展信息转为实体
            var relationPermission = it.ExtJson.FromJsonString<RelationRolePermission>();
            grantInfoList.Add(relationPermission);//添加到已授权信息
        });
        roleOwnPermission.GrantInfoList = grantInfoList;//赋值已授权信息
        return roleOwnPermission;
    }

    /// <inheritdoc />
    public async Task<List<string>> UserPermissionTreeSelectorAsync(BaseIdInput input)
    {
        var permissionTreeSelectors = new List<string>();//授权树结果集
        //获取用户资源关系
        var relationsRes = await _relationService.GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);
        var menuIds = relationsRes.Where(it => it.ObjectId == input.Id).Select(it => it.TargetId.ToLong()).ToList();
        if (menuIds.Any())
        {
            //获取菜单信息
            var menus = await _resourceService.GetMenuByMenuIdsAsync(menuIds);
            //获取权限授权树
            var permissions = _resourceService.PermissionTreeSelector(menus.Select(it => it.Href).ToList());
            if (permissions.Count > 0)
            {
                permissionTreeSelectors = permissions.Select(it => it.PermissionName).ToList();//返回授权树权限名称列表
            }
        }
        return permissionTreeSelectors;
    }

    /// <inheritdoc />
    public async Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IdListInput input)
    {
        var userList = await Context.Queryable<SysUser>().Where(it => input.IdList.Contains(it.Id)).Select<UserSelectorOutput>().ToListAsync();
        return userList;
    }

    #endregion 查询

    #region OPENAPI

    /// <inheritdoc />
    public async Task<RoleOwnPermissionOutput> ApiOwnPermissionAsync(BaseIdInput input)
    {
        var roleOwnPermission = new RoleOwnPermissionOutput
        {
            Id = input.Id
        };//定义结果集
        var grantInfoList = new List<RelationRolePermission>();//已授权信息集合
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_USER_HAS_OPENAPIPERMISSION);
        //遍历关系表
        relations.ForEach(it =>
        {
            //将扩展信息转为实体
            var relationPermission = it.ExtJson.FromJsonString<RelationRolePermission>();
            grantInfoList.Add(relationPermission);//添加到已授权信息
        });
        roleOwnPermission.GrantInfoList = grantInfoList;//赋值已授权信息
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("用户授权API权限")]
    public async Task ApiGrantPermissionAsync(GrantPermissionInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户
        if (sysUser != null)
        {
            var apiUrls = input.GrantInfoList.Select(it => it.ApiUrl).ToList();//apiurl列表
            var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_USER_HAS_OPENAPIPERMISSION, input.Id, apiUrls, extJsons,
                true);//添加到数据库
            DeleteUserFromRedis(input.Id);
        }
    }

    #endregion OPENAPI

    #region 新增

    /// <inheritdoc/>
    [OperDesc("添加用户")]
    public async Task AddAsync(UserAddInput input)
    {
        await CheckInput(input);//检查参数
        var sysUser = input.Adapt<SysUser>();//实体转换
        //获取默认密码
        sysUser.Password = await GetDefaultPassWord(true);//设置密码
        sysUser.UserStatus = true;//默认状态
        await InsertAsync(sysUser);//添加数据
    }

    #endregion 新增

    #region 编辑

    /// <inheritdoc/>
    [OperDesc("编辑用户")]
    public async Task EditAsync(UserEditInput input)
    {
        await CheckInput(input);//检查参数
        var exist = await GetUserByIdAsync(input.Id);//获取用户信息
        if (exist != null)
        {
            var isSuperAdmin = exist.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin && !UserManager.SuperAdmin)
                throw Oops.Bah($"不可修改系统内置超管用户账号");
            var sysUser = input.Adapt<SysUser>();//实体转换
            if (await Context.Updateable(sysUser).IgnoreColumns(it =>
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
                    }).ExecuteCommandAsync() > 0)//修改数据
            {
                DeleteUserFromRedis(sysUser.Id);//删除用户缓存
                //删除用户verificat缓存
                UserTokenCacheUtil.HashDel(sysUser.Id);
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("禁用用户")]
    public async Task DisableUserAsync(BaseIdInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户信息
        if (sysUser != null)
        {
            var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin)
                throw Oops.Bah($"不可禁用系统内置超管用户账号");
            CheckSelf(input.Id, SimpleAdminConst.Disable);//判断是不是自己
            //设置状态为禁用
            if (await UpdateSetColumnsTrueAsync(it => new SysUser
            {
                UserStatus = false
            }, it => it.Id == input.Id))
                DeleteUserFromRedis(input.Id);//从redis删除用户信息
        }
    }

    /// <inheritdoc/>
    [OperDesc("启用用户")]
    public async Task EnableUserAsync(BaseIdInput input)
    {
        CheckSelf(input.Id, SimpleAdminConst.Enable);//判断是不是自己
        //设置状态为启用
        if (await UpdateSetColumnsTrueAsync(it => new SysUser
        {
            UserStatus = true
        }, it => it.Id == input.Id))
            DeleteUserFromRedis(input.Id);//从redis删除用户信息
    }

    /// <inheritdoc/>
    [OperDesc("重置密码")]
    public async Task ResetPasswordAsync(BaseIdInput input)
    {
        var password = await GetDefaultPassWord(true);//获取默认密码,这里不走Aop所以需要加密一下
        //重置密码
        if (await UpdateSetColumnsTrueAsync(it => new SysUser
        {
            Password = password
        }, it => it.Id == input.Id))
            DeleteUserFromRedis(input.Id);//从redis删除用户信息
    }

    /// <inheritdoc />
    [OperDesc("用户授权角色")]
    public async Task GrantRoleAsync(UserGrantRoleInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户信息
        if (sysUser != null)
        {
            var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin)
                throw Oops.Bah($"不能给超管分配角色");
            CheckSelf(input.Id, SimpleAdminConst.GrantRole);//判断是不是自己
            //给用户赋角色
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_USER_HAS_ROLE, input.Id,
                input.RoleIdList.Select(it => it.ToString()).ToList(), null, true);
            DeleteUserFromRedis(input.Id);//从redis删除用户信息
        }
    }

    /// <inheritdoc />
    [OperDesc("用户授权资源")]
    public async Task GrantResourceAsync(UserGrantResourceInput input)
    {
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//菜单ID
        var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
        var relationRoles = new List<SysRelation>();//要添加的用户资源和授权关系表
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户
        if (sysUser != null)
        {
            #region 用户资源处理

            //遍历角色列表
            for (var i = 0; i < menuIds.Count; i++)
            {
                //将用户资源添加到列表
                relationRoles.Add(new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = menuIds[i].ToString(),
                    Category = CateGoryConst.Relation_SYS_USER_HAS_RESOURCE,
                    ExtJson = extJsons?[i]
                });
            }

            #endregion 用户资源处理

            #region 用户权限处理.

            var relationRolePer = new List<SysRelation>();//要添加的角色有哪些权限列表

            //获取菜单信息
            var menus = await _resourceService.GetMenuByMenuIdsAsync(menuIds);
            if (menus.Count > 0)
            {
                //获取权限授权树
                var permissions = _resourceService.PermissionTreeSelector(menus.Select(it => it.Href).ToList());
                permissions.ForEach(it =>
                {
                    //新建角色权限关系
                    relationRolePer.Add(new SysRelation
                    {
                        ObjectId = sysUser.Id,
                        TargetId = it.ApiRoute,
                        Category = CateGoryConst.Relation_SYS_USER_HAS_PERMISSION,
                        ExtJson = new RelationRolePermission
                        {
                            ApiUrl = it.ApiRoute,
                        }.ToJsonString()
                    });
                });
            }
            relationRoles.AddRange(relationRolePer);//合并列表

            #endregion 用户权限处理.

            #region 保存数据库

            //事务
            var result = await NewContent.UseTranAsync(async () =>
            {
                var relationRep = ChangeRepository<DbRepository<SysRelation>>();//切换仓储
                relationRep.NewContent = NewContent;
                await relationRep.DeleteAsync(it =>
                    it.ObjectId == sysUser.Id && (it.Category == CateGoryConst.Relation_SYS_USER_HAS_PERMISSION
                    || it.Category == CateGoryConst.Relation_SYS_USER_HAS_RESOURCE));
                await relationRep.InsertRangeAsync(relationRoles);//添加新的
            });
            if (result.IsSuccess)//如果成功了
            {
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_PERMISSION);//刷新关系缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);//刷新关系缓存
                DeleteUserFromRedis(input.Id);//删除该用户缓存
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion 保存数据库
        }
    }

    /// <inheritdoc />
    [OperDesc("用户授权权限")]
    public async Task GrantPermissionAsync(GrantPermissionInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户
        if (sysUser != null)
        {
            var apiUrls = input.GrantInfoList.Select(it => it.ApiUrl).ToList();//apiurl列表
            var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_USER_HAS_PERMISSION, input.Id, apiUrls, extJsons,
                true);//添加到数据库
            DeleteUserFromRedis(input.Id);
        }
    }

    #endregion 编辑

    #region 删除

    /// <inheritdoc/>
    [OperDesc("删除用户")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        //获取所有ID
        var ids = input.Select(it => it.Id).ToList();
        if (ids.Count > 0)
        {
            var containsSuperAdmin = await IsAnyAsync(it => it.Account == RoleConst.SuperAdmin && ids.Contains(it.Id));//判断是否有超管
            if (containsSuperAdmin)
                throw Oops.Bah($"不可删除系统内置超管用户");
            if (ids.Contains(UserManager.UserId))
                throw Oops.Bah($"不可删除自己");

            //定义删除的关系
            var delRelations = new List<string>
            {
                CateGoryConst.Relation_SYS_USER_HAS_RESOURCE, CateGoryConst.Relation_SYS_USER_HAS_PERMISSION, CateGoryConst.Relation_SYS_USER_HAS_ROLE, CateGoryConst.Relation_SYS_USER_HAS_OPENAPIPERMISSION
            };
            //事务
            var result = await NewContent.UseTranAsync(async () =>
            {
                //删除用户
                await DeleteByIdsAsync(ids.Cast<object>().ToArray());

                var relationRep = ChangeRepository<DbRepository<SysRelation>>();//切换仓储
                relationRep.NewContent = NewContent;
                //删除关系表用户与资源关系，用户与权限关系,用户与角色关系
                await relationRep.DeleteAsync(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category));
            });
            if (result.IsSuccess)//如果成功了
            {
                DeleteUserFromRedis(ids);//redis删除用户
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_ROLE);//关系表刷新SYS_USER_HAS_ROLE缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);//关系表刷新SYS_USER_HAS_ROLE缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_PERMISSION);//关系表刷新SYS_USER_HAS_ROLE缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_OPENAPIPERMISSION);//关系表刷新Relation_SYS_USER_HAS_OPENAPIPERMISSION缓存
                var idArray = ids.ToArray();
                //将这些用户踢下线，并永久注销这些用户
                foreach (var item in idArray)
                {
                    var verificatInfos = UserTokenCacheUtil.HashGetOne(item);
                    await UserLoginOut(item, verificatInfos);
                }
                //从列表中删除
                UserTokenCacheUtil.HashDel(idArray);
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
    }

    /// <inheritdoc />
    public void DeleteUserFromRedis(long userId)
    {
        DeleteUserFromRedis(new List<long>
        {
            userId
        });
    }

    /// <inheritdoc />
    public void DeleteUserFromRedis(List<long> ids)
    {
        var userIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
        var sysUsers = _simpleCacheService.HashGet<SysUser>(SystemConst.Cache_SysUser, userIds);//获取用户列表
        sysUsers = sysUsers.Where(it => it != null).ToList();//过滤掉不存在的
        if (sysUsers.Count > 0)
        {
            var accounts = sysUsers.Select(it => it.Account).ToArray();//账号集合
            var phones = sysUsers.Select(it => it.Phone).ToArray();//手机号集合
            //删除用户信息
            _simpleCacheService.HashDel<SysUser>(SystemConst.Cache_SysUser, userIds);
            //删除账号
            _simpleCacheService.HashDel<long>(SystemConst.Cache_SysUserAccount, accounts);
            //删除手机
            if (phones != null)
                _simpleCacheService.HashDel<long>(SystemConst.Cache_SysUserPhone, phones);
        }
    }

    #endregion 删除

    /// <inheritdoc/>
    public async Task SetUserDefaultAsync(List<SysUser> sysUsers)
    {
        var defaultPassword = await GetDefaultPassWord(true);//默认密码

        //默认值赋值
        sysUsers.ForEach(user =>
        {
            user.UserStatus = true;//状态
            user.Phone = CryptogramUtil.Sm2Encrypt(user.Phone);//手机号
            user.Password = defaultPassword;//默认密码
        });
    }

    #region 方法

    /// <summary>
    /// 通知用户下线
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="verificatInfos">Token列表</param>
    private async Task UserLoginOut(long userId, List<VerificatInfo> verificatInfos)
    {
        await _eventPublisher.PublishAsync(EventSubscriberConst.UserLoginOut, new UserLoginOutEvent
        {
            Message = "您的账号已在别处登录!",
            VerificatInfos = verificatInfos,
            UserId = userId
        });//通知用户下线
    }

    /// <summary>
    /// 获取默认密码
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetDefaultPassWord(bool isSm4 = false)
    {
        //获取默认密码
        var defaultPassword = (await _configService.GetByConfigKeyAsync(CateGoryConst.Config_PWD_POLICY, ConfigConst.PWD_DEFAULT_PASSWORD)).ConfigValue;
        return isSm4 ? CryptogramUtil.Sm2Encrypt(defaultPassword) : defaultPassword;//判断是否需要加密
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysUser"></param>
    private async Task CheckInput(SysUser sysUser)
    {
        //判断账号重复,直接从redis拿
        var accountId = await GetIdByAccountAsync(sysUser.Account);
        if (accountId > 0 && accountId != sysUser.Id)
            throw Oops.Bah($"存在重复的账号:{sysUser.Account}");
        //如果手机号不是空
        if (!string.IsNullOrEmpty(sysUser.Phone))
        {
            if (!sysUser.Phone.MatchPhoneNumber())//验证手机格式
                throw Oops.Bah($"手机号码：{sysUser.Phone} 格式错误");
            var phoneId = await GetIdByPhoneAsync(sysUser.Phone);
            if (phoneId > 0 && sysUser.Id != phoneId)//判断重复
                throw Oops.Bah($"存在重复的手机号:{sysUser.Phone}");
            sysUser.Phone = CryptogramUtil.Sm2Encrypt(sysUser.Phone);
        }
        //如果邮箱不是空
        if (!string.IsNullOrEmpty(sysUser.Email))
        {
            var (ismatch, match) = sysUser.Email.MatchEmail();//验证邮箱格式
            if (!ismatch)
                throw Oops.Bah($"邮箱：{sysUser.Email} 格式错误");
            if (await IsAnyAsync(it => it.Email == sysUser.Email && it.Id != sysUser.Id))
                throw Oops.Bah($"存在重复的邮箱:{sysUser.Email}");
        }
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
            throw Oops.Bah($"禁止{operate}自己");
        }
    }

    /// <summary>
    /// 根据日期计算年龄
    /// </summary>
    /// <param name="birthdate"></param>
    /// <returns></returns>
    public int GetAgeByBirthdate(DateTime birthdate)
    {
        var now = DateTime.Now;
        var age = now.Year - birthdate.Year;
        if (now.Month < birthdate.Month || now.Month == birthdate.Month && now.Day < birthdate.Day)
        {
            age--;
        }
        return age < 0 ? 0 : age;
    }

    /// <summary>
    /// 获取Sqlsugar的ISugarQueryable
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private ISugarQueryable<SysUser> GetQuery(UserPageInput input)
    {
        var query = Context.Queryable<SysUser>()
            .WhereIF(input.Expression != null, input.Expression?.ToExpression())//动态查询
            .WhereIF(input.UserStatus != null, u => u.UserStatus == input.UserStatus)//根据状态查询
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey));//根据关键字查询
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序
        query = query.OrderBy(u => u.Id);//排序

        query.Mapper(u =>
            {
                u.Password = null;//密码清空
                u.Phone = CryptogramUtil.Sm2Decrypt(u.Phone);//手机号解密
            });
        return query;
    }

    /// <summary>
    /// 数据库获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    private async Task<SysUser> GetUserFromDb(long userId)
    {
        var sysUser = await Context.Queryable<SysUser>()
            .Where(u => u.Id == userId).FirstAsync();
        if (sysUser != null)
        {
            sysUser.Password = CryptogramUtil.Sm2Decrypt(sysUser.Password);//解密密码
            sysUser.Phone = CryptogramUtil.Sm2Decrypt(sysUser.Phone);//解密手机号
            //获取按钮码
            var buttonCodeList = await GetButtonCodeListAsync(sysUser.Id);
            //获取数据权限
            var dataScopeList = await GetPermissionListByUserIdAsync(sysUser.Id);
            //获取权限码
            var permissionCodeList = dataScopeList.Select(it => it.ApiUrl).ToList();
            //获取角色码
            var roleCodeList = await _roleService.GetRoleListByUserIdAsync(sysUser.Id);
            //权限码赋值
            sysUser.ButtonCodeList = buttonCodeList;
            sysUser.RoleCodeList = roleCodeList.Select(it => it.Code).ToList();
            sysUser.RoleIdList = roleCodeList.Select(it => it.Id).ToList();
            sysUser.PermissionCodeList = permissionCodeList;
            sysUser.DataScopeList = dataScopeList;
            //插入Redis
            _simpleCacheService.HashAdd(SystemConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);
            return sysUser;
        }
        return null;
    }

    #endregion 方法
}