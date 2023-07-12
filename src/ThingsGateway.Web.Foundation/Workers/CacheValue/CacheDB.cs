#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion


using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 缓存帮助类
/// </summary>
public class CacheDb
{
    string Id;
    /// <summary>
    /// 构造函数传入Id号作为Sqlite文件名称
    /// </summary>
    /// <param name="id"></param>
    public CacheDb(string id)
    {
        Id = id;
        Directory.CreateDirectory("CacheDb");
        GetCacheDb().DbMaintenance.CreateDatabase();//创建数据库
        GetCacheDb().CodeFirst.InitTables(typeof(CacheTable));
    }
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    private SqlSugarClient GetCacheDb()
    {
        var configureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 修改列可空-1、带?问号 2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                    column.IsNullable = true;
            },
        };

        var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = $"Data Source=CacheDb/{Id}.db;",//连接字符串
            DbType = DbType.Sqlite,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        return sqlSugarClient;
    }

    /// <summary>
    /// 获取缓存表前n条
    /// </summary>
    /// <returns></returns>
    public async Task<List<CacheTable>> GetCacheData(int take)
    {
        var db = GetCacheDb();
        var data = await db.Queryable<CacheTable>().Take(take).ToListAsync();
        return data;
    }
    /// <summary>
    /// 获取缓存表全部
    /// </summary>
    /// <returns></returns>
    public async Task<List<CacheTable>> GetCacheData()
    {
        var db = GetCacheDb();
        var data = await db.Queryable<CacheTable>().ToListAsync();
        return data;
    }

    /// <summary>
    /// 增加离线缓存，限制表最大默认2000行
    /// </summary>
    /// <returns></returns>
    public async Task<bool> AddCacheData(string topic, string data, int max = 2000)
    {
        var db = GetCacheDb();
        var count = await db.Queryable<CacheTable>().CountAsync();
        if (count > max)
        {
            var data1 = await db.Queryable<CacheTable>().OrderBy(a => a.Id).Take(count - max).ToListAsync();
            await db.Deleteable(data1).ExecuteCommandAsync();
        }
        var result = await db.Insertable(new CacheTable() { Id = YitIdHelper.NextId(), Topic = topic, CacheStr = data }).ExecuteCommandAsync();
        return result > 0;
    }
    /// <summary>
    /// 清除离线缓存
    /// </summary>
    /// <returns></returns>
    public async Task<bool> DeleteCacheData(params long[] data)
    {
        var db = GetCacheDb();
        var result = await db.Deleteable<CacheTable>().In(data).ExecuteCommandAsync();
        return result > 0;
    }
}
/// <summary>
/// 缓存表
/// </summary>
public class CacheTable
{
    /// <summary>
    /// Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }
    /// <summary>
    /// Topic
    /// </summary>
    public string Topic { get; set; }
    /// <summary>
    /// 缓存值
    /// </summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string CacheStr { get; set; }
}