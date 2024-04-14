
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using NewLife;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 缓存
/// </summary>
public class CacheDB : DisposeBase
{
    internal CacheDB(Type type, CacheDBOption? cacheDBOption = null)
    {
        TableType = type;
        DBProvider = GetConnection(cacheDBOption);
    }

    private Type TableType { get; set; }
    public SqlSugarClient DBProvider { get; private set; }

    public void InitDb()
    {
        try
        {
            DBProvider.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
            DBProvider.CodeFirst.InitTables(TableType);
        }
        catch
        {
        }
    }

    protected override void Dispose(bool disposing)
    {
        lock (this)
        {
            try { DBProvider.Dispose(); } catch { }
        }
    }

    private SqlSugarClient GetConnection(CacheDBOption options)
    {
        SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = options.DataSource,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
        });
        return db;
    }
}