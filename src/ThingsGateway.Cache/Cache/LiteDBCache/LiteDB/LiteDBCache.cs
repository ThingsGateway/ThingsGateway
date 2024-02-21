//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using LiteDB;

using System.Linq.Expressions;
using System.Text.RegularExpressions;

using ThingsGateway.Core;

namespace ThingsGateway.Cache;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

/// <summary>
/// 缓存帮助类
/// </summary>
public class LiteDBCache<T> : IDisposable where T : IPrimaryIdEntity
{
    public readonly string Id;
    public readonly string Type;

    /// <summary>
    /// 构造函数传入Id号作为数据库文件名称
    /// </summary>
    /// <param name="id">id号,作为文件夹</param>
    /// <param name="typeName">种类，与ID，时间结合作为文件名称</param>
    /// <param name="fullFileName">全路径</param>
    internal LiteDBCache(string id, string typeName, string fullFileName)
    {
        Id = id;
        Type = typeName;
        var op = new LiteDBOptions()
        {
            DataSource = fullFileName,
            ConnectionType = ConnectionType.Shared
        };

        var dbProvider = GetConnection(op);
        FileName = op.DataSource;
        this._dbProvider = dbProvider;
        this.Collection = _dbProvider.GetCollection<T>(Regex.Replace($"tg_{typeName}", "[^a-zA-Z]", "_"));
        InitDb();
    }

    ~LiteDBCache()
    {
        Dispose();
    }

    public LiteDatabase _dbProvider { get; private set; }
    public ILiteCollection<T> Collection { get; private set; }

    public string FileName { get; private set; }

    public void Dispose()
    {
        try { _dbProvider.Dispose(); } catch { }
    }

    public IEnumerable<T>? GetPage(int skipCount, int pageSize)
    {
        var results = Collection.Find(Query.All(), skipCount, pageSize);
        return results;
    }

    public T? GetOne(long id)
    {
        var results = Collection.FindById(id);
        return results;
    }

    public IEnumerable<T>? Get(long[] ids)
    {
        var results = Collection.Find(a => ids.Contains(a.Id));
        return results;
    }

    public IEnumerable<T>? GetAll()
    {
        var results = Collection.FindAll();
        return results;
    }

    public int AddRange(IEnumerable<T> data, int batchSize = 5000)
    {
        var results = Collection.InsertBulk(data, batchSize);
        return results;
    }

    public void Add(T data)
    {
        Collection.Insert(data);
    }

    public int DeleteMany(IEnumerable<T> data)
    {
        var results = Collection.DeleteMany(a => data.Select(item => item.Id).Contains(a.Id));
        return results;
    }

    public int DeleteMany(Expression<Func<T, bool>> predicate)
    {
        var results = Collection.DeleteMany(predicate);
        return results;
    }

    public int DeleteMany(BsonExpression bsonExpression)
    {
        var results = Collection.DeleteMany(bsonExpression);
        return results;
    }

    /// <summary>
    /// init database
    /// </summary>
    public void InitDb()
    {
        lock (_dbProvider)
        {
            try
            {
                _dbProvider.Checkpoint();
                //_dbProvider.Rebuild();
                lock (Collection)
                {
                    //建立索引
                }
            }
            catch
            {
            }
        }
    }

    private LiteDatabase GetConnection(LiteDBOptions options)
    {
        ConnectionString builder = new ConnectionString()
        {
            Filename = options.DataSource,
            //InitialSize = options.InitialSize,
            Connection = options.ConnectionType,
            Password = options.Password
        };
        var _conn = new LiteDatabase(builder);

        return _conn;
    }
}