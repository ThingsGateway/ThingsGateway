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

using ThingsGateway.NewLife.X;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 缓存
/// </summary>
public class CacheDB : DisposeBase
{
    private SqlSugarClient _dBProvider;

    internal CacheDB(Type type, CacheDBOption? cacheDBOption = null)
    {
        TableType = type;
        CacheDBOption = cacheDBOption;
        _dBProvider = GetConnection(cacheDBOption);
    }

    public CacheDBOption CacheDBOption { get; }

    public SqlSugarClient DBProvider
    {
        get
        {
            return _dBProvider;
        }
    }

    private Type TableType { get; set; }

    public void InitDb()
    {
        lock (CacheDBOption.FileFullName)
        {
            if (!Disposed)
            {
                DBProvider.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
                DBProvider.CodeFirst.InitTables(TableType);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        {
            try { DBProvider.Dispose(); } catch { }
        }
    }

    private SqlSugarClient GetConnection(CacheDBOption options)
    {
        SqlSugarClient sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = options.DataSource,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
        });
        NetCoreApp.RootServices.GetService<ISugarAopService>().AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
    }
}
