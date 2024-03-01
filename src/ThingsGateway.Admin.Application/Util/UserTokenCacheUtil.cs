//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// UserTokenCacheUtil
/// </summary>
public class UserTokenCacheUtil
{
    public static LiteDBCache<UserTokenCache> UserTokenCache = LiteDBCacheUtil.GetDB<UserTokenCache>(nameof(UserTokenCache), $"{typeof(UserTokenCache).FullName}", true, false);

    public static List<VerificatInfo>? HashGetOne(long id)
    {
        var results = UserTokenCache.Collection.FindById(id);
        return results?.VerificatInfos;
    }

    public static List<List<VerificatInfo>> HashGet(long[] ids)
    {
        var results = UserTokenCache.Collection.Find(a => ids.Contains(a.Id)).Select(a => a.VerificatInfos).ToList();
        return results;
    }

    public static void HashAdd(long id, List<VerificatInfo> verificatInfos)
    {
        UserTokenCache userTokenCache = new() { Id = id, VerificatInfos = verificatInfos };
        if (UserTokenCache.GetOne(userTokenCache.Id) == null)
        {
            UserTokenCache.Add(userTokenCache);
        }
        else
        {
            UserTokenCache.Collection.Update(userTokenCache);
        }
    }

    public static Dictionary<long, List<VerificatInfo>> HashGetAll()
    {
        return UserTokenCache.GetAll().ToDictionary(a => a.Id, a => a.VerificatInfos);
    }

    public static void HashSet(Dictionary<long, List<VerificatInfo>> dictionary)
    {
        var data = dictionary.Select(kv => new UserTokenCache
        {
            Id = kv.Key,
            VerificatInfos = kv.Value
        });
        var find = UserTokenCache.Collection.Find(a => data.Select(a => a.Id).Contains(a.Id)).Select(item => item.Id);
        UserTokenCache.Collection.DeleteMany(a => find.Contains(a.Id));
        UserTokenCache.Collection.InsertBulk(data);
    }

    public static void Remove()
    {
        UserTokenCache.Collection.DeleteAll();
    }

    public static void HashDel(params long[] ids)
    {
        UserTokenCache.Collection.DeleteMany(a => ids.Contains(a.Id));
    }
}

public class UserTokenCache : PrimaryIdEntity
{
    public List<VerificatInfo> VerificatInfos { get; set; }
}