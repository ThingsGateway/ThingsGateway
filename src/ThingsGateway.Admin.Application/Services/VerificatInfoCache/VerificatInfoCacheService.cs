//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

using ThingsGateway.Admin.Application.ConcurrentList;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作内存，只在程序停止/启动时设置/获取持久化数据
/// </summary>
public class VerificatInfoCacheService : BaseService<VerificatInfoCacheItem>, IVerificatInfoCacheService
{
    /// <inheritdoc/>
    public List<VerificatInfo> HashGetOne(long id)
    {
        lock (this)
        {
            var all = GetAll();
            var data = all.FirstOrDefault(a => a.Key == id);
            return data.Value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<List<VerificatInfo>> HashGet(long[] ids)
    {
        lock (this)
        {
            var data = GetAll();
            var results = data.Where(a => ids.Contains(a.Key)).Select(a => a.Value);
            return results;
        }
    }

    /// <inheritdoc/>
    public void HashAdd(long id, List<VerificatInfo> verificatInfos)
    {
        lock (this)
        {
            var data = GetAll();
            if (data.ContainsKey(id))
            {
                data[id] = verificatInfos;
            }
            else
            {
                data.TryAdd(id, verificatInfos);
            }
#if DEBUG
                HashSetDB(data);
#endif
        }
    }

    /// <inheritdoc/>
    public void HashSet(Dictionary<long, List<VerificatInfo>> dict)
    {
        lock (this)
        {
            var key = CacheConst.Cache_Token;
            App.CacheService.Set(key, dict);
#if DEBUG
                HashSetDB(dict);
#endif
        }
    }

    /// <inheritdoc/>
    public void Remove()
    {
        lock (this)
        {
            var key = CacheConst.Cache_Token;
            App.CacheService.Set(key, new Dictionary<long, List<VerificatInfo>>());
#if DEBUG
                HashSetDB(new Dictionary<long, List<VerificatInfo>>());
#endif
        }
    }

    /// <inheritdoc/>
    public void HashDel(params long[] ids)
    {
        lock (this)
        {
            var data = GetAll();
            data.RemoveWhere(a => ids.Contains(a.Key));
#if DEBUG
            if(ids.Length > 0)
            {
                HashSetDB(data);
            }
#endif
        }
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public Dictionary<long, List<VerificatInfo>> GetAll()
    {
        lock (this)
        {
            var key = CacheConst.Cache_Token;
            var dict = App.CacheService.Get<Dictionary<long, List<VerificatInfo>>>(key);
            if (dict == null)
            {
                using var db = GetDB();
                dict = db.Queryable<VerificatInfoCacheItem>()?.ToList()?.ToDictionary(a => a.Id, a => a.VerificatInfos);
                App.CacheService.Set(key, dict);
            }
            return dict;
        }
    }

    /// <summary>
    /// 持久化数据
    /// </summary>
    /// <param name="dictionary"></param>
    public void HashSetDB(Dictionary<long, List<VerificatInfo>> dictionary)
    {
        lock (this)
        {
            using var db = GetDB();
            var count = db.Storageable(dictionary.Select(a => new VerificatInfoCacheItem() { Id = a.Key, VerificatInfos = a.Value }).ToList()).ExecuteCommand();
            if (count > 0)
                RemoveCache();
        }
    }

    private void RemoveCache()
    {
        App.CacheService.Remove(CacheConst.Cache_Token);
    }
}

[SugarTable("tg_verificat", TableDescription = "验证缓存表")]
[Tenant(SqlSugarConst.DB_TokenCache)]
public class VerificatInfoCacheItem : PrimaryIdEntity
{
    [SugarColumn(ColumnDescription = "验证列表", IsNullable = false, IsJson = true)]
    public List<VerificatInfo> VerificatInfos { get; set; } = new();
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
    public ConcurrentList<long> ClientIds { get; set; } = new();

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public override long Id { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SqlSugar.SugarColumn(IsIgnore = true)]
    public bool Online => ClientIds.Any();

    /// <summary>
    /// 过期时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public int Expire { get; set; }

    /// <summary>
    /// verificat剩余有效期
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SqlSugar.SugarColumn(IsIgnore = true)]
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
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
