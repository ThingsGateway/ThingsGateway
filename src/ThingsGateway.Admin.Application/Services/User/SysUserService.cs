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

using ThingsGateway.Extension;
using ThingsGateway.Json.Extension;
using ThingsGateway.NewLife.X;
using ThingsGateway.NewLife.X.Extension;

namespace ThingsGateway.Admin.Application;

public class SysUserService : BaseService<SysUser>, ISysUserService
{
    private readonly IRelationService _relationService;
    private readonly ISysResourceService _sysResourceService;
    private readonly ISysRoleService _roleService;
    private readonly ISysDictService _configService;
    private readonly IVerificatInfoService _verificatInfoService;

    public SysUserService(
        IVerificatInfoService verificatInfoService,
        IRelationService relationService,
        ISysResourceService sysResourceService,
        ISysRoleService roleService,
        ISysDictService configService)
    {
        _relationService = relationService;
        _sysResourceService = sysResourceService;
        _roleService = roleService;
        _configService = configService;
        _verificatInfoService = verificatInfoService;
    }

    #region 查询

    /// <summary>
    /// 根据账号获取用户，不存在返回null
    /// </summary>
    /// <param name="account">账号</param>
    /// <returns>用户</returns>
    public async Task<SysUser?> GetUserByAccountAsync(string account)
    {
        var userId = await GetIdByAccountAsync(account).ConfigureAwait(false);//获取用户ID
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
        var sysUser = NetCoreApp.CacheService.HashGetOne<SysUser>(CacheConst.Cache_SysUser, userId.ToString());
        sysUser ??= await GetUserFromDb(userId).ConfigureAwait(false);//从数据库拿用户信息
        return sysUser;
    }

    /// <summary>
    /// 根据账号获取用户id
    /// </summary>
    /// <param name="account">账号</param>
    /// <returns>用户id</returns>
    public async Task<long> GetIdByAccountAsync(string account)
    {
        //先从Cache拿
        var userId = NetCoreApp.CacheService.HashGetOne<long>(CacheConst.Cache_SysUserAccount, account);
        if (userId == 0)
        {
            //单查获取用户账号对应ID
            using var db = GetDB();
            userId = await db.Queryable<SysUser>().Where(it => it.Account == account).Select(it => it.Id).FirstAsync().ConfigureAwait(false);
            if (userId != 0)
            {
                //插入Cache
                NetCoreApp.CacheService.HashAdd(CacheConst.Cache_SysUserAccount, account, userId);
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
        var buttonIdList = resourceList.Select(it => it.ExtJson?.FromSystemTextJsonString<long>());
        var allResources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);
        var button = allResources.Where(it => it.Category == ResourceCategoryEnum.Button && buttonIdList.Contains(it.Id));
        Dictionary<string, List<string>> buttonCodeList = new();
        foreach (var item in button)
        {
            var href = allResources.FirstOrDefault(b => b.Id == item.ParentId)?.Href ?? string.Empty;
            if (buttonCodeList.TryGetValue(href, out var buttonCode))
            {
                buttonCode.Add(item.Title);
            }
            else
            {
                buttonCodeList.Add(href, new List<string>() { item.Title });
            }
        }
        return buttonCodeList!;
    }

    /// <summary>
    /// 获取用户拥有的权限
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>权限</returns>
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

            permissions.AddRange(relationGroup.Select(a => new DataScope
            {
                ApiUrl = a.Key!,
            }));
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

            permissions.AddRange(relationGroup.Select(a => new DataScope
            {
                ApiUrl = a.Key!,
            }));
        }

        #endregion API权限

        return permissions;
    }

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public Task<QueryData<SysUser>> PageAsync(QueryPageOptions option)
    {
        return QueryAsync(option, query => query.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText)).Mapper(u =>
        {
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            u.Password = null;//密码清空
            u.Phone = DESCEncryption.Decrypt(u.Phone);//解密手机号
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        }));
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
        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromSystemTextJsonString<RelationRolePermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("UserGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户
        if (sysUser != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.UserHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToSystemTextJsonString())),
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
            var exist = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户信息
            if (exist != null)
            {
                var isSuperAdmin = exist.Account == RoleConst.SuperAdmin;//判断是否有超管
                if (isSuperAdmin && !UserManager.SuperAdmin)
                    throw Oops.Bah(Localizer["CanotEditAdminUser"]);

                if (isSuperAdmin && input.Status != exist.Status)
                    throw Oops.Bah(Localizer["CanotEditAdminUser"]);

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
        if (sysUser != null)
        {
            var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
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
        var menuIdsExtJsons1 = input.GrantInfoList;//菜单ID拓展信息
        var relationUsers = new List<SysRelation>();//要添加的用户资源和授权关系表
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//获取用户
        if (sysUser != null)
        {
            var resources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);

            var menus1 = resources.Where(a => menuIdsExtJsons1.Contains(a.Id));
            var data = ResourceUtil.GetMyParentResources(resources, menus1);
            var menuIdsExtJsons = menuIdsExtJsons1.Concat(data.Select(a => a.Id)).Distinct();
            var menus = menus1.Concat(data).Distinct();

            #region 用户资源处理

            //遍历菜单列表
            foreach (var item in menuIdsExtJsons)
            {
                //将角色资源添加到列表
                relationUsers.Add(new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.UserHasResource,
                    ExtJson = item!.ToSystemTextJsonString()
                });
            }

            #endregion 用户资源处理

            #region 用户权限处理.

            //获取菜单信息
            if (menus.Any())
            {
                #region 用户模块关系

                //获取我的模块信息Id列表
                var moduleIds = menus.Select(it => it.Module).Distinct();
                var relationUserPer = moduleIds.Select(it => new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = it.ToString(),
                    Category = RelationCategoryEnum.UserHasModule,
                });
                relationUsers.AddRange(relationUserPer);//合并列表

                #endregion 用户模块关系

                //获取权限授权树
                var permissions = ResourceUtil.PermissionTreeSelector(menus.Select(it => it.Href));
                //要添加的角色有哪些权限列表
                var relationRolePer = permissions.Select(it => new SysRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.UserHasPermission,
                    ExtJson = new RelationRolePermission { ApiUrl = it.ApiRoute }
                            .ToSystemTextJsonString()
                });
                relationUsers.AddRange(relationRolePer);//合并列表
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

        //定义删除的关系
        var delRelations = new List<RelationCategoryEnum>
            {
                RelationCategoryEnum.UserHasResource, RelationCategoryEnum.UserHasPermission, RelationCategoryEnum.UserHasRole, RelationCategoryEnum.UserHasOpenApiPermission
                , RelationCategoryEnum.UserHasModule
            };
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            //删除用户
            await db.Deleteable<SysUser>().In(ids.ToList()).ExecuteCommandHasChangeAsync().ConfigureAwait(false);//删除

            //删除关系表用户与资源关系，用户与权限关系,用户与角色关系
            await db.Deleteable<SysRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync().ConfigureAwait(false);
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
        var sysUsers = NetCoreApp.CacheService.HashGet<SysUser>(CacheConst.Cache_SysUser, userIds);//获取用户列表
        if (sysUsers.Any())
        {
            var accounts = sysUsers.Where(it => it != null).Select(it => it.Account);//账号集合
            var phones = sysUsers.Select(it => it.Phone);//手机号集合
            //删除用户信息
            NetCoreApp.CacheService.HashDel<SysUser>(CacheConst.Cache_SysUser, userIds);
            //删除账号
            NetCoreApp.CacheService.HashDel<long>(CacheConst.Cache_SysUserAccount, accounts.ToArray());
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
            Message = Localizer["SingleLoginWarn"],
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
        return isEncrypt ? DESCEncryption.Encrypt(appConfig.PasswordPolicy.DefaultPassword) : appConfig.PasswordPolicy.DefaultPassword;//判断是否需要加密
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysUser"></param>
    private async Task CheckInput(SysUser sysUser)
    {
        //判断账号重复,直接从cache拿
        var accountId = await GetIdByAccountAsync(sysUser.Account).ConfigureAwait(false);
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
            sysUser.Phone = DESCEncryption.Encrypt(sysUser.Phone);
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
            throw Oops.Bah(Localizer["CheckSelf", operate]);
        }
    }

    /// <summary>
    /// 数据库获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    private async Task<SysUser?> GetUserFromDb(long userId)
    {
        using var db = GetDB();
        var sysUser = await db.Queryable<SysUser>().FirstAsync(u => u.Id == userId).ConfigureAwait(false);
        if (sysUser != null)
        {
            sysUser.Password = DESCEncryption.Decrypt(sysUser.Password);//解密密码
            sysUser.Phone = DESCEncryption.Decrypt(sysUser.Phone);//解密手机号
            //获取按钮码
            var buttonCodeList = await GetButtonCodeListAsync(sysUser.Id).ConfigureAwait(false);
            //获取数据权限
            var dataScopeList = await GetPermissionListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //获取权限码
            var permissionCodeList = dataScopeList.Select(it => it.ApiUrl);
            //获取角色码
            var roleCodeList = await _roleService.GetRoleListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //权限码赋值
            sysUser.ButtonCodeList = buttonCodeList;
            sysUser.RoleCodeList = roleCodeList.Select(it => it.Code);
            sysUser.RoleIdList = roleCodeList.Select(it => it.Id);
            sysUser.PermissionCodeList = permissionCodeList;
            sysUser.DataScopeList = dataScopeList;

            if (sysUser.Account == RoleConst.SuperAdmin)
            {
                var modules = (await _sysResourceService.GetAllAsync().ConfigureAwait(false)).Where(a => a.Category == ResourceCategoryEnum.Module);
                sysUser.ModuleList = modules;//模块列表赋值给用户
            }
            else
            {
                var moduleIds = await _relationService.GetUserModuleId(sysUser.RoleIdList, sysUser.Id).ConfigureAwait(false);//获取模块ID列表
                var modules = await _sysResourceService.GetMuduleByMuduleIdsAsync(moduleIds).ConfigureAwait(false);//获取模块列表
                sysUser.ModuleList = modules;//模块列表赋值给用户
            }

            //插入Cache
            NetCoreApp.CacheService.HashAdd(CacheConst.Cache_SysUserAccount, sysUser.Account, sysUser.Id);
            NetCoreApp.CacheService.HashAdd(CacheConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);

            return sysUser;
        }
        return null;
    }

    #endregion 方法
}
