//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;

public class VerificatInfoCacheService : IVerificatInfoCacheService
{
    public LiteDBCache<VerificatInfoCacheItem> UserTokenCache = LiteDBCacheUtil.GetCache<VerificatInfoCacheItem>(nameof(UserTokenCache), $"{typeof(VerificatInfoCacheItem).FullName}");

    /// <inheritdoc/>
    public List<VerificatInfo> HashGetOne(long id)
    {
        var results = UserTokenCache.Collection.FindById(id);
        return results?.VerificatInfos ?? new();
    }

    /// <inheritdoc/>
    public IEnumerable<List<VerificatInfo>> HashGet(long[] ids)
    {
        var results = UserTokenCache.Collection.Find(a => ids.Contains(a.Id)).ToList().Select(a => a.VerificatInfos);
        return results;
    }

    /// <inheritdoc/>
    public void HashAdd(long id, List<VerificatInfo> verificatInfos)
    {
        VerificatInfoCacheItem userTokenCache = new() { Id = id, VerificatInfos = verificatInfos };
        if (UserTokenCache.GetOne(userTokenCache.Id) == null)
        {
            UserTokenCache.Add(userTokenCache);
        }
        else
        {
            UserTokenCache.Collection.Update(userTokenCache);
        }
    }

    /// <inheritdoc/>
    public Dictionary<long, List<VerificatInfo>> HashGetAll()
    {
        return UserTokenCache.GetAll().ToDictionary(a => a.Id, a => a.VerificatInfos);
    }

    /// <inheritdoc/>
    public void HashSet(Dictionary<long, List<VerificatInfo>> dictionary)
    {
        var data = dictionary.Select(kv => new VerificatInfoCacheItem
        {
            Id = kv.Key,
            VerificatInfos = kv.Value
        });
        var find = UserTokenCache.Collection.Find(a => data.Select(a => a.Id).Contains(a.Id)).Select(item => item.Id);
        UserTokenCache.Collection.DeleteMany(a => find.Contains(a.Id));
        UserTokenCache.Collection.InsertBulk(data);
    }

    /// <inheritdoc/>
    public void Remove()
    {
        UserTokenCache.Collection.DeleteAll();
    }

    /// <inheritdoc/>
    public void HashDel(params long[] ids)
    {
        UserTokenCache.Collection.DeleteMany(a => ids.Contains(a.Id));
    }
}

public class VerificatInfoCacheItem : PrimaryIdEntity
{
    public List<VerificatInfo> VerificatInfos { get; set; }
}

/// <summary>
/// 会话信息
/// </summary>
public class VerificatInfo : PrimaryIdEntity
{
    /// <summary>
    /// 客户端ID列表
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public List<long> ClientIds { get; set; } = new();

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public override long Id { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [LiteDB.BsonIgnore]
    public bool Online => ClientIds.Any();

    /// <summary>
    /// 连接数量
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [LiteDB.BsonIgnore]
    public int OnlineNum => ClientIds.Count();

    /// <summary>
    /// 过期时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public int Expire { get; set; }

    /// <summary>
    /// verificat剩余有效期
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [LiteDB.BsonIgnore]
    public string VerificatRemain { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public DateTime VerificatTimeout { get; set; }

    /// <summary>
    /// 登录设备
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true, Width = 100)]
    public AuthDeviceTypeEnum Device { get; set; }
}