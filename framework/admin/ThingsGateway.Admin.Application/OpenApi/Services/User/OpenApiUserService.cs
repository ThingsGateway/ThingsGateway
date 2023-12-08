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

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IOpenApiUserService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class OpenApiUserService : DbRepository<OpenApiUser>, IOpenApiUserService
{
    private readonly IVerificatService _verificatService;
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc cref="IOpenApiUserService"/>
    public OpenApiUserService(
        IVerificatService verificatService, IServiceScopeFactory serviceScopeFactory
                      )
    {
        _verificatService = verificatService;
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    [OperDesc("添加用户")]
    public async Task AddAsync(OpenApiUserAddInput input)
    {
        var account_Id = await GetIdByAccountAsync(input.Account);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的账号:{input.Account}");

        var openApiUser = input.Adapt<OpenApiUser>();//实体转换
        await InsertAsync(openApiUser);//添加数据
    }

    /// <inheritdoc/>
    [OperDesc("删除用户")]
    public async Task DeleteAsync(params long[] ids)
    {
        //获取所有ID
        if (ids.Length > 0)
        {
            var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
            if (result)
            {
                //从列表中删除
                foreach (var id in ids)
                {
                    await _verificatService.SetOpenApiVerificatIdAsync(id, new());
                }
                DeleteUserFromCache(ids);
            }
        }
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(params long[] ids)
    {
        var userIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
        List<OpenApiUser> openApiUsers = new();
        foreach (var item in userIds)
        {
            var user = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<OpenApiUser>(CacheConst.CACHE_OPENAPIUSER + item, false);//获取用户列表
            openApiUsers.Add(user);
            _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(CacheConst.CACHE_OPENAPIUSER + item);
        }
        openApiUsers = openApiUsers.Where(it => it != null).ToList();//过滤掉不存在的
        if (openApiUsers.Count > 0)
        {
            var accounts = openApiUsers.Select(it => it.Account).ToArray();//账号集合
            foreach (var item in accounts)
            {
                //删除账号
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(CacheConst.CACHE_OPENAPIUSERACCOUNT + item);
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("禁用用户")]
    public async Task DisableUserAsync(long input)
    {
        var openApiUser = await GetUsertByIdAsync(input);//获取用户信息
        if (openApiUser != null)
        {
            if (await UpdateAsync(it => new OpenApiUser { UserEnable = false }, it => it.Id == input))
            {
                await _verificatService.SetOpenApiVerificatIdAsync(input, new());
                DeleteUserFromCache(input);//从cache删除用户信息
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑用户")]
    public async Task EditAsync(OpenApiUserEditInput input)
    {
        await CheckInputAsync(input);//检查参数
        var exist = await GetUsertByIdAsync(input.Id);//获取用户信息
        if (exist != null)
        {
            var openApiUser = input.Adapt<OpenApiUser>();//实体转换
            openApiUser.Password = DESCEncryption.Encrypt(openApiUser.Password, DESCKeyConst.DESCKey);
            if (await Context.Updateable(openApiUser).IgnoreColumns(it =>
            new
            {
                //忽略更新字段
                it.LastLoginDevice,
                it.LastLoginIp,
                it.LastLoginTime,
                it.LatestLoginDevice,
                it.LatestLoginIp,
                it.LatestLoginTime
            }).ExecuteCommandAsync() > 0)//修改数据
                DeleteUserFromCache(openApiUser.Id);//用户缓存到cache
        }
        //编辑操作可能会修改用户密码等信息，认证时需要实时获取用户并验证
    }

    /// <inheritdoc/>
    [OperDesc("启用用户")]
    public async Task EnableUserAsync(long input)
    {
        //设置状态为启用
        if (await UpdateAsync(it => new OpenApiUser { UserEnable = true }, it => it.Id == input))
            DeleteUserFromCache(input);//从cache删除用户信息
    }

    /// <inheritdoc/>
    public async Task<long> GetIdByAccountAsync(string account)
    {
        //先从Cache拿
        var userId = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<long>(CacheConst.CACHE_OPENAPIUSERACCOUNT + account, false);
        if (userId == 0)
        {
            //单查获取用户账号对应ID
            userId = await GetFirstAsync(it => it.Account == account, it => it.Id);
            if (userId != 0)
            {
                //插入Cache
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.CACHE_OPENAPIUSERACCOUNT + account, userId, false);
            }
        }
        return userId;
    }

    /// <inheritdoc/>
    public async Task<OpenApiUser> GetUserByAccountAsync(string account)
    {
        var userId = await GetIdByAccountAsync(account);//获取用户ID
        if (userId > 0)
        {
            var openApiUser = await GetUsertByIdAsync(userId);//获取用户信息
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
    public async Task<OpenApiUser> GetUsertByIdAsync(long Id)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var openApiUser = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<OpenApiUser>(CacheConst.CACHE_OPENAPIUSER + Id.ToString(), true);
        if (openApiUser == null)
        {
            openApiUser = await Context.Queryable<OpenApiUser>()
            .Where(u => u.Id == Id)
            .FirstAsync();
            if (openApiUser != null)
            {
                //插入Cache
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.CACHE_OPENAPIUSER + openApiUser.Id.ToString(), openApiUser, true);
            }
        }
        return openApiUser;
    }

    /// <inheritdoc />
    [OperDesc("用户授权")]
    public async Task GrantRoleAsync(OpenApiUserGrantPermissionInput input)
    {
        var openApiUser = await GetUsertByIdAsync(input.Id.Value);//获取用户信息
        if (openApiUser != null)
        {
            openApiUser.PermissionCodeList = input.PermissionList;
            await CheckInputAsync(openApiUser);
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
    public async Task<List<string>> OwnPermissionsAsync(BaseIdInput input)
    {
        var openApiUser = await GetUsertByIdAsync(input.Id);//获取用户信息
        return openApiUser.PermissionCodeList;
    }

    /// <inheritdoc/>
    public async Task<ISqlSugarPagedList<OpenApiUser>> PageAsync(OpenApiUserPageInput input)
    {
        var query = Context.Queryable<OpenApiUser>()
         .WhereIF(!string.IsNullOrEmpty(input.SearchKey), u => u.Account.Contains(input.SearchKey));//根据关键字查询
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序
        query = query.OrderBy(u => u.Id);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="openApiUser"></param>
    private async Task CheckInputAsync(OpenApiUser openApiUser)
    {
        //判断账号重复,直接从cache拿
        var account_Id = await GetIdByAccountAsync(openApiUser.Account);
        if (account_Id > 0 && account_Id != openApiUser.Id)
            throw Oops.Bah($"存在重复的账号:{openApiUser.Account}");
        //如果手机号不是空
        if (!string.IsNullOrEmpty(openApiUser.Phone))
        {
            if (!openApiUser.Phone.MatchPhoneNumber())//验证手机格式
                throw Oops.Bah($"手机号码：{openApiUser.Phone} 格式错误");
            openApiUser.Phone = DESCEncryption.Encrypt(openApiUser.Phone, DESCKeyConst.DESCKey);
        }
        //如果邮箱不是空
        if (!string.IsNullOrEmpty(openApiUser.Email))
        {
            var ismatch = openApiUser.Email.MatchEmail();//验证邮箱格式
            if (!ismatch)
                throw Oops.Bah($"邮箱：{openApiUser.Email} 格式错误");
            if (await IsAnyAsync(it => it.Email == openApiUser.Email && it.Id != openApiUser.Id))
                throw Oops.Bah($"存在重复的邮箱:{openApiUser.Email}");
        }
    }
}