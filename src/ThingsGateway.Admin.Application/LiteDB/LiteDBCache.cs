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

using NewLife;

using System.Linq.Expressions;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 缓存帮助类
/// </summary>
public class LiteDBCache<T> : DisposeBase where T : IPrimaryIdEntity
{
    internal LiteDBCache(string name, LiteDBOptions? liteDBOptions = null)
    {
        liteDBOptions ??= new LiteDBOptions()
        {
            DataSource = name,
            ConnectionType = ConnectionType.Shared
        };
        var dbProvider = GetConnection(liteDBOptions);
        this.DBProvider = dbProvider;
        this.Collection = DBProvider.GetCollection<T>();
        InitDb();
    }

    public ILiteCollection<T> Collection { get; private set; }
    public LiteDatabase DBProvider { get; private set; }

    public void Add(T data)
    {
        lock (this)
        {
            Collection.Insert(data);
        }
    }

    public int AddRange(IEnumerable<T> data, int batchSize = 5000)
    {
        lock (this)
        {
            var results = Collection.InsertBulk(data, batchSize);
            return results;
        }
    }

    public int DeleteMany(IEnumerable<T> data)
    {
        lock (this)
        {
            var results = Collection.DeleteMany(a => data.Select(item => item.Id).Contains(a.Id));
            return results;
        }
    }

    public int DeleteMany(Expression<Func<T, bool>> predicate)
    {
        lock (this)
        {
            var results = Collection.DeleteMany(predicate);
            return results;
        }
    }

    public IEnumerable<T>? Get(long[] ids)
    {
        lock (this)
        {
            var results = Collection.Find(a => ids.Contains(a.Id));
            return results;
        }
    }

    public IEnumerable<T> GetAll()
    {
        lock (this)
        {
            var results = Collection.FindAll();
            return results;
        }
    }

    public T? GetOne(long id)
    {
        lock (this)
        {
            var results = Collection.FindById(id);
            return results;
        }
    }

    public IEnumerable<T> GetPage(int skipCount, int pageSize)
    {
        lock (this)
        {
            var results = Collection.Find(Query.All(), skipCount, pageSize);
            return results;
        }
    }

    public void InitDb(bool isRebuild = false)
    {
        lock (this)
        {
            try
            {
                DBProvider.Checkpoint();
                if (isRebuild)
                    DBProvider.Rebuild();
            }
            catch
            {
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        lock (this)
        {
            try { DBProvider.Dispose(); } catch { }
        }
    }

    private LiteDatabase GetConnection(LiteDBOptions options)
    {
        lock (this)
        {
            ConnectionString builder = new ConnectionString()
            {
                Filename = options.DataSource,
                Connection = options.ConnectionType,
                Password = options.Password
            };
            var _conn = new LiteDatabase(builder);

            return _conn;
        }
    }
}