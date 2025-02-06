//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using ThingsGateway.List;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作内存，只在程序停止/启动时设置/获取持久化数据
/// </summary>
internal sealed class VerificatInfoService : BaseService<VerificatInfo>, IVerificatInfoService
{
    #region 查询

    public VerificatInfo GetOne(long id, bool delete = true)
    {
        //先从Cache拿
        var verificatInfo = App.CacheService.HashGetOne<VerificatInfo>(CacheConst.Cache_Token, id.ToString());
        verificatInfo ??= GetFromDb(id);
        if (verificatInfo != null && delete)
        {
            if (verificatInfo.VerificatTimeout.AddSeconds(30) < DateTime.Now)
            {
                Delete(verificatInfo.Id);
                return null;
            }
        }
        return verificatInfo;
    }

    private VerificatInfo? GetFromDb(long id)
    {
        using var db = GetDB();
        var verificatInfo = db.Queryable<VerificatInfo>().First(u => u.Id == id);
        if (verificatInfo != null)
            VerificatInfoService.SetCahce(verificatInfo);
        return verificatInfo;
    }

    private static void SetCahce(VerificatInfo verificatInfo)
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
        VerificatInfoService.RemoveCache(verificatInfo.Id);
        if (verificatInfo != null)
            VerificatInfoService.SetCahce(verificatInfo);
    }

    #endregion 添加

    #region 更新

    public void Update(VerificatInfo verificatInfo)
    {
        using var db = GetDB();
        db.Updateable<VerificatInfo>(verificatInfo).ExecuteCommand();
        VerificatInfoService.RemoveCache(verificatInfo.Id);
        if (verificatInfo != null)
            VerificatInfoService.SetCahce(verificatInfo);
    }

    #endregion 更新

    #region 删除

    public void Delete(long id)
    {
        using var db = GetDB();
        db.Deleteable<VerificatInfo>(id).ExecuteCommand();
        VerificatInfoService.RemoveCache(id);
    }

    public void Delete(List<long> ids)
    {
        using var db = GetDB();
        db.Deleteable<VerificatInfo>().Where(it => ids.Contains(it.Id)).ExecuteCommand();
        foreach (var id in ids)
        {
            VerificatInfoService.RemoveCache(id);
        }
    }

    #endregion 删除

    #region 去除全部在线Id

    public void RemoveAllClientId()
    {
        using var db = GetDB();
        db.Updateable<VerificatInfo>().SetColumns("ClientIds", new ConcurrentList<long>().ToJsonNetString()).Where(a => a.Id >= 0).ExecuteCommand();
        VerificatInfoService.RemoveCache();
    }

    #endregion 去除全部在线Id

    private static void RemoveCache()
    {
        App.CacheService.Remove(CacheConst.Cache_Token);
    }

    private static void RemoveCache(long id)
    {
        App.CacheService.HashDel<VerificatInfo>(CacheConst.Cache_Token, id.ToString());
    }
}
