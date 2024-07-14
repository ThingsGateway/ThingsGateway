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

using ThingsGateway.Core.List;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作内存，只在程序停止/启动时设置/获取持久化数据
/// </summary>
public class VerificatInfoService : BaseService<VerificatInfo>, IVerificatInfoService
{
    #region 查询

    public VerificatInfo GetOne(long id)
    {
        //先从Cache拿
        var verificatInfo = App.CacheService.HashGetOne<VerificatInfo>(CacheConst.Cache_Token, id.ToString());
        verificatInfo ??= GetFromDb(id);
        if (verificatInfo != null)
            if (verificatInfo.VerificatTimeout.AddSeconds(30) < DateTime.Now)
            {
                Delete(verificatInfo.Id);
                return null;
            }
        return verificatInfo;
    }

    private VerificatInfo? GetFromDb(long id)
    {
        using var db = GetDB();
        var verificatInfo = db.Queryable<VerificatInfo>().First(u => u.Id == id);
        if (verificatInfo != null)
            SetCahce(verificatInfo);
        return verificatInfo;
    }

    private void SetCahce(VerificatInfo verificatInfo)
    {
        App.CacheService.HashAdd<VerificatInfo>(CacheConst.Cache_Token, verificatInfo.Id.ToString(), verificatInfo);
    }

    public List<VerificatInfo>? GetListByUserId(long userId)
    {
        using var db = GetDB();
        var verificatInfo = db.Queryable<VerificatInfo>().Where(u => u.UserId == userId).ToList();
        return verificatInfo;
    }

    public List<VerificatInfo>? GetListByIds(List<long> ids)
    {
        using var db = GetDB();
        var verificatInfos = db.Queryable<VerificatInfo>().Where(u => ids.Contains(u.Id)).ToList();
        var ids1 = new List<long>();
        foreach (var verificatInfo in verificatInfos)
        {
            if (verificatInfo.VerificatTimeout.AddSeconds(30) < DateTime.Now)
            {
                ids1.Add(verificatInfo.Id);
            }
        }

        if (ids1.Count > 0)
        {
            Delete(ids1);
        }
        return verificatInfos;
    }

    public List<VerificatInfo>? GetListByUserIds(List<long> userIds)
    {
        using var db = GetDB();
        var verificatInfos = db.Queryable<VerificatInfo>().Where(u => userIds.Contains(u.UserId)).ToList();

        List<long> ids = new List<long>();
        foreach (var verificatInfo in verificatInfos)
        {
            if (verificatInfo.VerificatTimeout.AddSeconds(30) < DateTime.Now)
            {
                ids.Add(verificatInfo.Id);
            }
        }
        if (ids.Count > 0)
        {
            Delete(ids);
        }
        return verificatInfos;
    }

    public List<long>? GetIdListByUserId(long userId)
    {
        using var db = GetDB();
        var verificatInfo = db.Queryable<VerificatInfo>().Where(u => u.UserId == userId).Select(a => a.Id).ToList();

        return verificatInfo;
    }

    public List<long>? GetClientIdListByUserId(long userId)
    {
        using var db = GetDB();
        var verificatInfo = db.Queryable<VerificatInfo>().Where(u => u.UserId == userId).Select(a => a.ClientIds).ToList().SelectMany(a => a).ToList();

        return verificatInfo;
    }

    #endregion 查询

    #region 添加

    public void Add(VerificatInfo verificatInfo)
    {
        using var db = GetDB();
        db.Insertable<VerificatInfo>(verificatInfo).ExecuteCommand();
        RemoveCache(verificatInfo.Id);
        if (verificatInfo != null)
            SetCahce(verificatInfo);
    }

    #endregion 添加

    #region 更新

    public void Update(VerificatInfo verificatInfo)
    {
        using var db = GetDB();
        db.Updateable<VerificatInfo>(verificatInfo).ExecuteCommand();
        RemoveCache(verificatInfo.Id);
        if (verificatInfo != null)
            SetCahce(verificatInfo);
    }

    #endregion 更新

    #region 删除

    public void Delete(long id)
    {
        using var db = GetDB();
        db.Deleteable<VerificatInfo>(id).ExecuteCommand();
        RemoveCache(id);
    }

    public void Delete(List<long> ids)
    {
        using var db = GetDB();
        db.Deleteable<VerificatInfo>().Where(it => ids.Contains(it.Id)).ExecuteCommand();
        foreach (var id in ids)
        {
            RemoveCache(id);
        }
    }

    #endregion 删除

    #region 去除全部在线Id

    public void RemoveAllClientId()
    {
        using var db = GetDB();
        db.Updateable<VerificatInfo>().SetColumns(it => it.ClientIds == default).Where(a => a.Id >= 0).ExecuteCommand();
        RemoveCache();
    }

    #endregion 去除全部在线Id

    private void RemoveCache()
    {
        App.CacheService.Remove(CacheConst.Cache_Token);
    }

    private void RemoveCache(long id)
    {
        App.CacheService.HashDel<VerificatInfo>(CacheConst.Cache_Token, id.ToString());
    }
}

/// <summary>
/// 会话信息
/// </summary>

[SugarTable("verificatinfo", TableDescription = "验证缓存表")]
[Tenant(SqlSugarConst.DB_TokenCache)]
public class VerificatInfo : PrimaryIdEntity
{
    /// <summary>
    /// 客户端ID列表
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    [SugarColumn(ColumnDescription = "客户端ID列表", IsNullable = true, IsJson = true)]
    public ConcurrentList<long> ClientIds { get; set; } = new();

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public long UserId { get; set; }

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [IgnoreExcel]
    public override long Id { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SugarColumn(IsIgnore = true)]
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
    [SugarColumn(IsIgnore = true)]
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
