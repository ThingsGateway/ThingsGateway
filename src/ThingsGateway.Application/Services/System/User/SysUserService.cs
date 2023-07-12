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

namespace ThingsGateway.Application
{
    /// <inheritdoc cref="ISysUserService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class SysUserService : DbRepository<SysUser>, ISysUserService
    {
        private readonly IConfigService _configService;
        private readonly IRelationService _relationService;
        private readonly IResourceService _resourceService;
        private readonly IRoleService _roleService;
        private readonly SysCacheService _sysCacheService;

        /// <inheritdoc cref="ISysUserService"/>
        public SysUserService(SysCacheService sysCacheService,
                           IRelationService relationService,
                           IResourceService resourceService,
                           IRoleService roleService,
                           IConfigService configService)
        {
            _sysCacheService = sysCacheService;
            _relationService = relationService;
            _resourceService = resourceService;
            _roleService = roleService;
            this._configService = configService;
        }

        /// <inheritdoc/>
        [OperDesc("添加用户")]
        public async Task Add(UserAddInput input)
        {
            var account_Id = await GetIdByAccount(input.Account);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的账号:{input.Account}");

            var sysUser = input.Adapt<SysUser>();//实体转换

            //获取默认密码
            sysUser.Password = await GetDefaultPassWord();//设置密码
            sysUser.UserStatus = true;//默认状态
            var result = await InsertReturnEntityAsync(sysUser);//添加数据
            _sysCacheService.Set(CacheConst.Cache_UserId, result.Id.ToString(), result.Id);
        }

        /// <inheritdoc/>
        [OperDesc("删除用户")]
        public async Task Delete(List<BaseIdInput> input)
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

                var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
                if (result)
                {
                    //从列表中删除
                    foreach (var id in ids)
                    {
                        _sysCacheService.SetVerificatId(id, new());
                    }
                    DeleteUserFromCache(ids);
                }
            }
        }

        /// <inheritdoc />
        public void DeleteUserFromCache(long userId)
        {
            DeleteUserFromCache(new List<long> { userId });
        }

        /// <inheritdoc />
        public void DeleteUserFromCache(List<long> ids)
        {
            var userIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
            List<SysUser> sysUsers = new List<SysUser>();
            foreach (var item in userIds)
            {
                var user = _sysCacheService.Get<SysUser>(CacheConst.Cache_SysUser, item);//获取用户列表
                sysUsers.Add(user);
                _sysCacheService.Remove(CacheConst.Cache_UserId, item);
            }
            sysUsers = sysUsers.Where(it => it != null).ToList();//过滤掉不存在的
            if (sysUsers.Count > 0)
            {
                var accounts = sysUsers.Select(it => it.Account).ToArray();//账号集合
                foreach (var item in userIds)
                {
                    //删除用户信息
                    _sysCacheService.Remove(CacheConst.Cache_SysUser, item);
                }
                foreach (var item in accounts)
                {
                    //删除账号
                    _sysCacheService.Remove(CacheConst.Cache_SysUserAccount, item);
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("禁用用户")]
        public async Task DisableUser(BaseIdInput input)
        {
            var sysUser = await GetUsertById(input.Id);//获取用户信息
            if (sysUser != null)
            {
                var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
                if (isSuperAdmin)
                    throw Oops.Bah($"不可禁用系统内置超管用户账号");
                CheckSelf(input.Id, AdminConst.Disable);//判断是不是自己
                                                        //设置状态为禁用
                if (await UpdateAsync(it => new SysUser { UserStatus = false }, it => it.Id == input.Id))
                {
                    //从列表中删除
                    _sysCacheService.SetVerificatId(input.Id, new());
                    DeleteUserFromCache(input.Id);//从cache删除用户信息
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑用户")]
        public async Task Edit(UserEditInput input)
        {
            await CheckInput(input);//检查参数
            var exist = await GetUsertById(input.Id);//获取用户信息
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
                    DeleteUserFromCache(sysUser.Id);//用户缓存到cache
            }
        }

        /// <inheritdoc/>
        [OperDesc("启用用户")]
        public async Task EnableUser(BaseIdInput input)
        {
            CheckSelf(input.Id, AdminConst.Enable);//判断是不是自己
                                                   //设置状态为启用
            if (await UpdateAsync(it => new SysUser { UserStatus = true }, it => it.Id == input.Id))
                DeleteUserFromCache(input.Id);//从cache删除用户信息
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetButtonCodeList(long userId)
        {
            List<string> buttonCodeList = new();//按钮ID集合
                                                //获取关系集合
            var roleList = await _relationService.GetRelationListByObjectIdAndCategory(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
            if (roleIdList.Count > 0)//如果该用户有角色
            {
                List<long> buttonIdList = new List<long>();//按钮ID集合
                var resourceList = await _relationService.GetRelationListByObjectIdListAndCategory(roleIdList, CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//获取资源集合
                resourceList.ForEach(it =>
                {
                    if (!string.IsNullOrEmpty(it.ExtJson)) buttonIdList.AddRange(it.ExtJson.ToJsonEntity<RelationRoleResuorce>().ButtonInfo);//如果有按钮权限，将按钮ID放到buttonIdList
                });
                if (buttonIdList.Count > 0)
                {
                    buttonCodeList = await _resourceService.GetCodeByIds(buttonIdList, MenuCategoryEnum.BUTTON);
                }
            }
            return buttonCodeList;
        }

        /// <inheritdoc/>
        public async Task<long> GetIdByAccount(string account)
        {
            //先从Cache拿
            var userId = _sysCacheService.Get<long>(CacheConst.Cache_SysUserAccount, account);
            if (userId == 0)
            {
                //单查获取用户账号对应ID
                userId = await GetFirstAsync(it => it.Account == account, it => it.Id);
                if (userId != 0)
                {
                    //插入Cache
                    _sysCacheService.Set(CacheConst.Cache_SysUserAccount, account, userId);
                }
            }
            return userId;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetPermissionListByUserId(long userId)
        {
            var permissions = new List<string>();//权限集合
            var roleIdList = await _relationService.GetRelationListByObjectIdAndCategory(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
            if (roleIdList.Count > 0)//如果角色ID不为空
            {
                //获取角色权限信息
                var sysRelations = await _relationService.GetRelationListByObjectIdListAndCategory(roleIdList.Select(it => it.TargetId.ToLong()).ToList(), CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);
                var relationGroup = sysRelations.GroupBy(it => it.TargetId).ToList();//根据目标ID,也就是接口名分组，因为存在一个用户多个角色
                                                                                     //遍历分组
                relationGroup.ForEach(it =>
                {
                    HashSet<string> scopeSet = new();//定义不可重复列表
                    var relationList = it.ToList();//关系列表
                    relationList.ForEach(it =>
                    {
                        var rolePermission = it.ExtJson.ToJsonEntity<RelationRolePermission>();
                        scopeSet.Add(rolePermission.ApiUrl);
                    });
                    permissions.AddRange(scopeSet);//将改URL的权限集合加入权限集合列表
                });
            }
            return permissions;
        }

        /// <inheritdoc/>
        public async Task<SysUser> GetUserByAccount(string account)
        {
            var userId = await GetIdByAccount(account);//获取用户ID
            if (userId > 0)
            {
                var sysUser = await GetUsertById(userId);//获取用户信息
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
        public async Task<SysUser> GetUsertById(long Id)
        {
            //先从Cache拿
            var sysUser = _sysCacheService.Get<SysUser>(CacheConst.Cache_SysUser, Id.ToString());
            if (sysUser == null)
            {
                sysUser = await Context.Queryable<SysUser>()
                .Where(u => u.Id == Id)
                .Select((u) => new SysUser { Id = u.Id.SelectAll() })
                .FirstAsync();
                if (sysUser != null)
                {
                    //获取按钮码
                    var buttonCodeList = await GetButtonCodeList(sysUser.Id);
                    //获取角色码
                    var roleCodeList = await _roleService.GetRoleListByUserId(sysUser.Id);
                    //获取权限码
                    var permissionCodeList = await GetPermissionListByUserId(sysUser.Id);

                    //权限码赋值
                    sysUser.ButtonCodeList = buttonCodeList;
                    sysUser.RoleCodeList = roleCodeList.Select(it => it.Code).ToList();
                    sysUser.RoleIdList = roleCodeList.Select(it => it.Id).ToList();
                    sysUser.PermissionCodeList = permissionCodeList;
                    //插入Cache
                    _sysCacheService.Set(CacheConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);
                }
            }
            return sysUser;
        }

        /// <inheritdoc />
        [OperDesc("用户授权")]
        public async Task GrantRole(UserGrantRoleInput input)
        {
            var sysUser = await GetUsertById(input.Id.Value);//获取用户信息
            if (sysUser != null)
            {
                var isSuperAdmin = sysUser.Account == RoleConst.SuperAdmin;//判断是否有超管
                if (isSuperAdmin)
                    throw Oops.Bah($"不能给超管分配角色");
                CheckSelf(input.Id.Value, AdminConst.GrantRole);//判断是不是自己
                                                                //给用户赋角色
                await _relationService.SaveRelationBatch(CateGoryConst.Relation_SYS_USER_HAS_ROLE, input.Id.Value, input.RoleIdList.Select(it => it.ToString()).ToList(), null, true);
                DeleteUserFromCache(input.Id.Value);//从cache删除用户信息
            }
        }

        /// <inheritdoc/>
        public async Task<List<long>> OwnRole(BaseIdInput input)
        {
            var relations = await _relationService.GetRelationListByObjectIdAndCategory(input.Id, CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            return relations.Select(it => it.TargetId.ToLong()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<SysUser>> Page(UserPageInput input)
        {
            var query = Context.Queryable<SysUser>()
             .WhereIF(input.Expression != null, input.Expression?.ToExpression())//动态查询
             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey))//根据关键字查询
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new SysUser { Id = u.Id.SelectAll() })
             .Mapper(u =>
             {
                 u.Password = null;//密码清空
             });
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        /// <inheritdoc/>
        [OperDesc("重置密码")]
        public async Task ResetPassword(BaseIdInput input)
        {
            var password = await GetDefaultPassWord(true);//获取默认密码,这里不走Aop所以需要加密一下
            //重置密码
            if (await UpdateAsync(it => new SysUser { Password = password }, it => it.Id == input.Id))
            {
                //从列表中删除
                _sysCacheService.SetVerificatId(input.Id, new());
                DeleteUserFromCache(input.Id);//从cache删除用户信息
            }
        }

        /// <inheritdoc/>
        public async Task<List<UserSelectorOutPut>> UserSelector(string searchKey)
        {
            var result = await Context.Queryable<SysUser>()
                             .WhereIF(!string.IsNullOrEmpty(searchKey), it => it.Account.Contains(searchKey))//根据关键字查询
                             .Select<UserSelectorOutPut>()//映射成SysUserSelectorOutPut
                             .ToListAsync();
            return result;
        }

        #region 方法

        /// <inheritdoc />
        public void DeleteTokenFromCache(List<long> ids)
        {
        }

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="sysUser"></param>
        private async Task CheckInput(SysUser sysUser)
        {
            //判断账号重复,直接从cache拿
            var account_Id = await GetIdByAccount(sysUser.Account);
            if (account_Id > 0 && account_Id != sysUser.Id)
                throw Oops.Bah($"存在重复的账号:{sysUser.Account}");
            //如果手机号不是空
            if (!string.IsNullOrEmpty(sysUser.Phone))
            {
                if (!sysUser.Phone.MatchPhoneNumber())//验证手机格式
                    throw Oops.Bah($"手机号码：{sysUser.Phone} 格式错误");
                sysUser.Phone = CryptogramUtil.Sm4Encrypt(sysUser.Phone);
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
            var defaultPassword = (await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_PASSWORD)).ConfigValue;
            return isSm4 ? CryptogramUtil.Sm4Encrypt(defaultPassword) : defaultPassword;//判断是否需要加密
        }

        #endregion 方法
    }
}