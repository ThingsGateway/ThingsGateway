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
    /// <summary>
    /// <inheritdoc cref="IOpenApiUserService"/>
    /// </summary>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class OpenApiUserService : DbRepository<OpenApiUser>, IOpenApiUserService
    {
        private readonly IConfigService _configService;
        private readonly SysCacheService _sysCacheService;

        /// <inheritdoc cref="IOpenApiUserService"/>
        public OpenApiUserService(SysCacheService sysCacheService,
                           IConfigService configService)
        {
            _sysCacheService = sysCacheService;
            _configService = configService;
        }

        /// <inheritdoc/>
        [OperDesc("添加用户")]
        public async Task Add(OpenApiUserAddInput input)
        {
            var account_Id = await GetIdByAccount(input.Account);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的账号:{input.Account}");

            var openApiUser = input.Adapt<OpenApiUser>();//实体转换
            openApiUser.UserStatus = true;//默认状态
            var result = await InsertReturnEntityAsync(openApiUser);//添加数据
            _sysCacheService.Set(CacheConst.Cache_OpenApiUserId, result.Id.ToString(), result.Id);
        }

        /// <inheritdoc/>
        [OperDesc("删除用户")]
        public async Task Delete(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            if (ids.Count > 0)
            {
                var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
                if (result)
                {
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
            List<OpenApiUser> openApiUsers = new List<OpenApiUser>();
            foreach (var item in userIds)
            {
                var user = _sysCacheService.Get<OpenApiUser>(CacheConst.Cache_OpenApiUser, item);//获取用户列表
                openApiUsers.Add(user);
                _sysCacheService.Remove(CacheConst.Cache_OpenApiUserId, item);
            }
            openApiUsers = openApiUsers.Where(it => it != null).ToList();//过滤掉不存在的
            if (openApiUsers.Count > 0)
            {
                var accounts = openApiUsers.Select(it => it.Account).ToArray();//账号集合
                foreach (var item in userIds)
                {
                    //删除用户信息
                    _sysCacheService.Remove(CacheConst.Cache_OpenApiUser, item);
                }
                foreach (var item in accounts)
                {
                    //删除账号
                    _sysCacheService.Remove(CacheConst.Cache_OpenApiUserAccount, item);
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("禁用用户")]
        public async Task DisableUser(BaseIdInput input)
        {
            var openApiUser = await GetUsertById(input.Id);//获取用户信息
            if (openApiUser != null)
            {
                if (await UpdateAsync(it => new OpenApiUser { UserStatus = false }, it => it.Id == input.Id))
                    DeleteUserFromCache(input.Id);//从cache删除用户信息
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑用户")]
        public async Task Edit(OpenApiUserEditInput input)
        {
            await CheckInput(input);//检查参数
            var exist = await GetUsertById(input.Id);//获取用户信息
            if (exist != null)
            {
                var openApiUser = input.Adapt<OpenApiUser>();//实体转换
                if (await Context.Updateable(openApiUser).IgnoreColumns(it =>
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
                    DeleteUserFromCache(openApiUser.Id);//用户缓存到cache
            }
        }


        /// <inheritdoc/>
        [OperDesc("启用用户")]
        public async Task EnableUser(BaseIdInput input)
        {
            //设置状态为启用
            if (await UpdateAsync(it => new OpenApiUser { UserStatus = true }, it => it.Id == input.Id))
                DeleteUserFromCache(input.Id);//从cache删除用户信息
        }

        /// <inheritdoc/>
        public async Task<long> GetIdByAccount(string account)
        {
            //先从Cache拿
            var userId = _sysCacheService.Get<long>(CacheConst.Cache_OpenApiUserAccount, account);
            if (userId == 0)
            {
                //单查获取用户账号对应ID
                userId = await GetFirstAsync(it => it.Account == account, it => it.Id);
                if (userId != 0)
                {
                    //插入Cache
                    _sysCacheService.Set(CacheConst.Cache_OpenApiUserAccount, account, userId);
                }
            }
            return userId;
        }

        /// <inheritdoc/>
        public async Task<OpenApiUser> GetUserByAccount(string account)
        {
            var userId = await GetIdByAccount(account);//获取用户ID
            if (userId > 0)
            {
                var openApiUser = await GetUsertById(userId);//获取用户信息
                if (openApiUser.Account == account)//这里做了比较用来限制大小写
                    return openApiUser;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OpenApiUser> GetUsertById(long Id)
        {
            //先从Cache拿
            var openApiUser = _sysCacheService.Get<OpenApiUser>(CacheConst.Cache_OpenApiUser, Id.ToString());
            if (openApiUser == null)
            {
                openApiUser = await Context.Queryable<OpenApiUser>()
                .Where(u => u.Id == Id)
                .Select((u) => new OpenApiUser { Id = u.Id.SelectAll() })
                .FirstAsync();
                if (openApiUser != null)//做个大小写限制
                {
                    //插入Cache
                    _sysCacheService.Set(CacheConst.Cache_OpenApiUser, openApiUser.Id.ToString(), openApiUser);
                }
            }
            return openApiUser;
        }

        /// <inheritdoc />
        [OperDesc("用户授权")]
        public async Task GrantRole(OpenApiUserGrantPermissionInput input)
        {
            var openApiUser = await GetUsertById(input.Id.Value);//获取用户信息
            if (openApiUser != null)
            {
                openApiUser.PermissionCodeList = input.PermissionList;
                await CheckInput(openApiUser);
                if (await Context.Updateable(openApiUser).IgnoreColumns(it =>
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
                    DeleteUserFromCache(input.Id.Value);//从cache删除用户信息
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> OwnPermissions(BaseIdInput input)
        {
            var openApiUser = await GetUsertById(input.Id);//获取用户信息
            return openApiUser.PermissionCodeList;
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<OpenApiUser>> Page(OpenApiUserPageInput input)
        {
            var query = Context.Queryable<OpenApiUser>()
             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey))//根据关键字查询
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new OpenApiUser { Id = u.Id.SelectAll() })
    ;
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="openApiUser"></param>
        private async Task CheckInput(OpenApiUser openApiUser)
        {
            //判断账号重复,直接从cache拿
            var account_Id = await GetIdByAccount(openApiUser.Account);
            if (account_Id > 0 && account_Id != openApiUser.Id)
                throw Oops.Bah($"存在重复的账号:{openApiUser.Account}");
            //如果手机号不是空
            if (!string.IsNullOrEmpty(openApiUser.Phone))
            {
                if (!openApiUser.Phone.MatchPhoneNumber())//验证手机格式
                    throw Oops.Bah($"手机号码：{openApiUser.Phone} 格式错误");
                openApiUser.Phone = CryptogramUtil.Sm4Encrypt(openApiUser.Phone);
            }
            //如果邮箱不是空
            if (!string.IsNullOrEmpty(openApiUser.Email))
            {
                var (ismatch, match) = openApiUser.Email.MatchEmail();//验证邮箱格式
                if (!ismatch)
                    throw Oops.Bah($"邮箱：{openApiUser.Email} 格式错误");
                if (await IsAnyAsync(it => it.Email == openApiUser.Email && it.Id != openApiUser.Id))
                    throw Oops.Bah($"存在重复的邮箱:{openApiUser.Email}");
            }
        }
    }
}