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

using Furion.DataEncryption;
using Furion.DependencyInjection;
using Furion.FriendlyException;

using Mapster;

using SqlSugar;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="ISysUserService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class SysUserService : DbRepository<SysUser>, ISysUserService
{
    private readonly IConfigService _configService;
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly IRoleService _roleService;
    private readonly IVerificatService _verificatService;
    /// <inheritdoc cref="ISysUserService"/>
    public SysUserService(
                       IRelationService relationService,
                       IResourceService resourceService,
                       IVerificatService verificatService,
                       IRoleService roleService,
                       IConfigService configService)
    {
        _relationService = relationService;
        _resourceService = resourceService;
        _roleService = roleService;
        _configService = configService;
        _verificatService = verificatService;
    }

    /// <inheritdoc/>
    [OperDesc("添加用户")]
    public async Task AddAsync(UserAddInput input)
    {
        await CheckInputAsync(input);//检查参数
        var account_Id = await GetIdByAccountAsync(input.Account);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的账号:{input.Account}");

        var sysUser = input.Adapt<SysUser>();//实体转换

        //获取默认密码
        sysUser.Password = await GetDefaultPassWord();//设置密码
        sysUser.UserEnable = true;//默认状态
        await InsertAsync(sysUser);//添加数据
    }

    /// <inheritdoc/>
    [OperDesc("删除用户")]
    public async Task DeleteAsync(params long[] ids)
    {
        //获取所有ID
        if (ids.Length > 0)
        {
            var containsSuperAdmin = await IsAnyAsync(it => it.Account == RoleConst.SuperAdmin && ids.Contains(it.Id));//判断是否有超管
            if (containsSuperAdmin)
                throw Oops.Bah($"不可删除系统内置超管用户");
            if (ids.Contains(UserManager.UserId))
                throw Oops.Bah($"不可删除自己");

            var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
            if (result)
            {
                //从列表中删除
                foreach (var id in ids)
                {
                    await _verificatService.SetVerificatIdAsync(id, new());
                }
                DeleteUserFromCache(ids);
            }
        }
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(params long[] ids)
    {
        List<SysUser> sysUsers = new();
        foreach (var item in ids)
        {
            var user = CacheStatic.Cache.Get<SysUser>(CacheConst.CACHE_SYSUSER + item, false);//获取用户列表
            sysUsers.Add(user);
            //删除账号
            CacheStatic.Cache.Remove(CacheConst.CACHE_SYSUSER + item);
        }
        sysUsers = sysUsers.Where(it => it != null).ToList();//过滤掉不存在的
        if (sysUsers.Count > 0)
        {
            var accounts = sysUsers.Select(it => it.Account).ToArray();//账号集合
            foreach (var item in accounts)
            {
                //删除账号
                CacheStatic.Cache.Remove(CacheConst.CAHCE_SYSUSERACCOUNT + item);
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("禁用用户")]
    public async Task DisableUserAsync(long input)
    {
        var sysUser = await GetUserByIdAsync(input);//获取用户信息
        if (sysUser != null)
        {
            var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin)
                throw Oops.Bah($"不可禁用系统内置超管用户账号");
            CheckSelf(input, AdminConst.Disable);//判断是不是自己
                                                 //设置状态为禁用
            if (await UpdateAsync(it => new SysUser { UserEnable = false }, it => it.Id == input))
            {
                //从列表中删除
                await _verificatService.SetVerificatIdAsync(input, new());
                DeleteUserFromCache(input);//从cache删除用户信息
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑用户")]
    public async Task EditAsync(UserEditInput input)
    {
        await CheckInputAsync(input);//检查参数
        var exist = await GetUserByIdAsync(input.Id);//获取用户信息
        if (exist != null)
        {
            var isSuperAdmin = exist.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin && !UserManager.IsSuperAdmin)
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
                DeleteUserFromCache(sysUser.Id);//用户缓存到cache
        }
    }

    /// <inheritdoc/>
    [OperDesc("启用用户")]
    public async Task EnableUserAsync(long input)
    {
        CheckSelf(input, AdminConst.Enable);//判断是不是自己

        //设置状态为启用
        if (await UpdateAsync(it => new SysUser { UserEnable = true }, it => it.Id == input))
            DeleteUserFromCache(input);//从cache删除用户信息
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetButtonCodeListAsync(long userId)
    {
        List<string> buttonCodeList = new();//按钮ID集合

        //获取关系集合
        var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
        var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
        if (roleIdList.Count > 0)//如果该用户有角色
        {
            List<long> buttonIdList = new();//按钮ID集合
            var resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList, CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//获取资源集合
            resourceList.ForEach(it =>
            {
                if (!string.IsNullOrEmpty(it.ExtJson)) buttonIdList.AddRange(it.ExtJson.ToJsonWithT<RelationRoleResuorce>().ButtonInfo);//如果有按钮权限，将按钮ID放到buttonIdList
            });
            if (buttonIdList.Count > 0)
            {
                buttonCodeList = await _resourceService.GetCodeByIdsAsync(buttonIdList, ResourceCategoryEnum.BUTTON);
            }
        }
        return buttonCodeList;
    }

    /// <inheritdoc/>
    public async Task<long> GetIdByAccountAsync(string account)
    {
        //先从Cache拿
        var userId = CacheStatic.Cache.Get<long>(CacheConst.CAHCE_SYSUSERACCOUNT + account, false);
        if (userId == 0)
        {
            //单查获取用户账号对应ID
            userId = await GetFirstAsync(it => it.Account == account, it => it.Id);
            if (userId != 0)
            {
                //插入Cache
                CacheStatic.Cache.Set(CacheConst.CAHCE_SYSUSERACCOUNT + account, userId, false);
            }
        }
        return userId;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetPermissionListByUserIdAsync(long userId)
    {
        var permissions = new List<string>();//权限集合
        var roleIdList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
        if (roleIdList.Count > 0)//如果角色ID不为空
        {
            //获取角色权限信息
            var sysRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()).ToList(), CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);
            var relationGroup = sysRelations.GroupBy(it => it.TargetId).ToList();//根据目标ID,也就是接口名分组，因为存在一个用户多个角色

            //遍历分组
            relationGroup.ForEach(it =>
            {
                HashSet<string> scopeSet = new();//定义不可重复列表
                var relationList = it.ToList();//关系列表
                relationList.ForEach(it =>
                {
                    var rolePermission = it.ExtJson.ToJsonWithT<RelationRolePermission>();
                    scopeSet.Add(rolePermission.ApiUrl);
                });
                permissions.AddRange(scopeSet);//将改URL的权限集合加入权限集合列表
            });
        }
        return permissions;
    }

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
    public async Task<SysUser> GetUserByIdAsync(long Id)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var sysUser = CacheStatic.Cache.Get<SysUser>(CacheConst.CACHE_SYSUSER + Id.ToString(), true);
        if (sysUser == null)
        {
            sysUser = await Context.Queryable<SysUser>()
            .Where(u => u.Id == Id)
            .FirstAsync();
            if (sysUser != null)
            {
                //获取按钮码
                var buttonCodeList = await GetButtonCodeListAsync(sysUser.Id);
                //获取角色码
                var roleCodeList = await _roleService.GetRoleListByUserIdAsync(sysUser.Id);
                //获取权限码
                var permissionCodeList = await GetPermissionListByUserIdAsync(sysUser.Id);

                //权限码赋值
                sysUser.ButtonCodeList = buttonCodeList;
                sysUser.RoleCodeList = roleCodeList.Select(it => it.Code).ToList();
                sysUser.RoleIdList = roleCodeList.Select(it => it.Id).ToList();
                sysUser.PermissionCodeList = permissionCodeList;
                //插入Cache
                CacheStatic.Cache.Set(CacheConst.CACHE_SYSUSER + sysUser.Id.ToString(), sysUser, true);
            }
        }
        return sysUser;
    }

    /// <inheritdoc />
    [OperDesc("用户授权")]
    public async Task GrantRoleAsync(UserGrantRoleInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id);//获取用户信息
        if (sysUser != null)
        {
            var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
            if (isSuperAdmin)
                throw Oops.Bah($"不能给超管分配角色");
            CheckSelf(input.Id, AdminConst.GrantRole);//判断是不是自己

            //给用户赋角色
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_USER_HAS_ROLE, input.Id, input.RoleIdList.Select(it => it.ToString()).ToList(), null, true);
            DeleteUserFromCache(input.Id);//从cache删除用户信息
        }
    }

    /// <inheritdoc/>
    public async Task<List<long>> OwnRoleAsync(BaseIdInput input)
    {
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
        return relations.Select(it => it.TargetId.ToLong()).ToList();
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SysUser>> PageAsync(UserPageInput input)
    {
        var query = Context.Queryable<SysUser>()
         .WhereIF(input.Expression != null, input.Expression?.ToExpression())//动态查询
         .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey))//根据关键字查询
         .Mapper(u =>
         {
             u.Password = null;//密码清空
         });
        for (int i = 0; i < input.SortField.Count; i++)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序
        query = query.OrderBy(u => u.Id);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <inheritdoc/>
    [OperDesc("重置密码")]
    public async Task ResetPasswordAsync(long input)
    {
        var password = await GetDefaultPassWord(true);//获取默认密码,这里不走Aop所以需要加密一下
        //重置密码
        if (await UpdateAsync(it => new SysUser { Password = password }, it => it.Id == input))
        {
            //从列表中删除
            await _verificatService.SetVerificatIdAsync(input, new());
            DeleteUserFromCache(input);//从cache删除用户信息
        }
    }

    /// <inheritdoc/>
    public async Task<List<UserSelectorOutput>> UserSelectorAsync(string searchKey)
    {
        var result = await Context.Queryable<SysUser>()
                         .WhereIF(!string.IsNullOrEmpty(searchKey), it => it.Account.Contains(searchKey))//根据关键字查询
                         .Select<UserSelectorOutput>()//映射成SysUserSelectorOutput
                         .ToListAsync();
        return result;
    }

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysUser"></param>
    private async Task CheckInputAsync(SysUser sysUser)
    {
        //判断账号重复,直接从cache拿
        var account_Id = await GetIdByAccountAsync(sysUser.Account);
        if (account_Id > 0 && account_Id != sysUser.Id)
            throw Oops.Bah($"存在重复的账号:{sysUser.Account}");
        //如果手机号不是空
        if (!string.IsNullOrEmpty(sysUser.Phone))
        {
            if (!sysUser.Phone.MatchPhoneNumber())//验证手机格式
                throw Oops.Bah($"手机号码：{sysUser.Phone} 格式错误");
            sysUser.Phone = DESCEncryption.Encrypt(sysUser.Phone, DESCKeyConst.DESCKey);

        }
        //如果邮箱不是空
        if (!string.IsNullOrEmpty(sysUser.Email))
        {
            var ismatch = sysUser.Email.MatchEmail();//验证邮箱格式
            if (!ismatch)
                throw Oops.Bah($"邮箱：{sysUser.Email} 格式错误");
            if (await IsAnyAsync(it => it.Email == sysUser.Email && it.Id != sysUser.Id))
                throw Oops.Bah($"存在重复的邮箱:{sysUser.Email}");
        }
    }

    /// <summary>
    /// 检查是否为自己
    /// </summary>
    private void CheckSelf(long id, string operate)
    {
        if (id == UserManager.UserId)//如果是自己
        {
            throw Oops.Bah($"禁止{operate}自己");
        }
    }

    /// <summary>
    /// 获取默认密码
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetDefaultPassWord(bool isSm4 = false)
    {
        //获取默认密码
        var defaultPassword = (await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_PASSWORD)).ConfigValue;
        return isSm4 ? DESCEncryption.Encrypt(defaultPassword, DESCKeyConst.DESCKey) : defaultPassword;//判断是否需要加密
    }

    #endregion 方法
}