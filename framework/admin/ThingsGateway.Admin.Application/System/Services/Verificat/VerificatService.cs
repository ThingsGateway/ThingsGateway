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

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IVerificatService"/>
public class VerificatService : DbRepository<SysVerificat>, IVerificatService
{
    private readonly IServiceScope _serviceScope;

    public VerificatService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    public async Task<List<VerificatInfo>> GetOpenApiVerificatIdAsync(long userId)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<List<VerificatInfo>>(CacheConst.CACHE_OPENAPIUSERVERIFICAT + userId, true);
        if (data != null)
        {
            var infos = data.Where(it => it.VerificatTimeout > DateTimeExtensions.CurrentDateTime).ToList();//去掉登录超时的
            if (infos.Count != data.Count)
                await SetOpenApiVerificatIdAsync(userId, infos);
            return infos;
        }
        else
        {
            var sys = await Context.Queryable<SysVerificat>().Where(it => it.Id == userId).FirstAsync();
            if (sys != null)
            {
                var infos = sys.VerificatInfos.Where(it => it.VerificatTimeout > DateTimeExtensions.CurrentDateTime).ToList();//去掉登录超时的
                await SetOpenApiVerificatIdAsync(userId, infos);
                return infos;
            }
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<List<VerificatInfo>> GetVerificatIdAsync(long userId)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<List<VerificatInfo>>(CacheConst.CACHE_USERVERIFICAT + userId, true);
        if (data != null)
        {
            var infos = data.Where(it => it.VerificatTimeout > DateTimeExtensions.CurrentDateTime).ToList();//去掉登录超时的
            if (infos.Count != data.Count)
                await SetVerificatIdAsync(userId, infos);
            return infos;
        }
        else
        {
            var sys = await Context.Queryable<SysVerificat>().Where(it => it.Id == userId).FirstAsync();
            if (sys != null)
            {
                var infos = sys.VerificatInfos.Where(it => it.VerificatTimeout > DateTimeExtensions.CurrentDateTime).ToList();//去掉登录超时的
                await SetVerificatIdAsync(userId, infos);
                return infos;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task SetOpenApiVerificatIdAsync(long userId, List<VerificatInfo> values)
    {
        SysVerificat sysverificat = new()
        {
            Id = userId,
            VerificatInfos = values
        };
        //数据库变化后，更新缓存
        var x = Context.Storageable(sysverificat).ToStorage();
        var i1 = await x.AsInsertable.ExecuteCommandAsync();//不存在插入
        var i2 = await x.AsUpdateable.ExecuteCommandAsync();//存在更新
        if (i1 + i2 > 0)
        {
            _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.CACHE_OPENAPIUSERVERIFICAT + userId, values, false);
        }
    }

    /// <inheritdoc/>
    public async Task SetVerificatIdAsync(long userId, List<VerificatInfo> values)
    {
        SysVerificat sysverificat = new()
        {
            Id = userId,
            VerificatInfos = values
        };
        //数据库变化后，更新缓存
        var x = Context.Storageable(sysverificat).ToStorage();
        var i1 = await x.AsInsertable.ExecuteCommandAsync();//不存在插入
        var i2 = await x.AsUpdateable.ExecuteCommandAsync();//存在更新
        if (i1 + i2 > 0)
        {
            _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.CACHE_USERVERIFICAT + userId, values, false);
        }
    }
}