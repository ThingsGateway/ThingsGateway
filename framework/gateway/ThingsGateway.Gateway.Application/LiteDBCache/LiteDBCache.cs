#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using LiteDB;

using System.Text.RegularExpressions;

namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 缓存帮助类
/// </summary>
public class LiteDBCache
{
    private readonly string _id;
    private readonly string _typeName;
    private LiteDBDatabaseProvider _dbProvider;
    public LiteDatabase Litedb { get; private set; }
    public ILiteCollection<CacheItem> Cache { get; private set; }
    private readonly string _name;
    public string FileName { get; private set; }

    /// <summary>
    /// 检测文件大小，如果大于设定值，则删除2/1，并适当压缩数据库
    /// </summary>
    public void DeleteOldData(double maxLength = 500)
    {
        var length1 = new FileInfo(FileName).Length;
        var mb1 = Math.Round((double)length1 / (double)1024 / (double)1024, 2);
        if (mb1 > maxLength)
        {
            InitDb();
            var length = new FileInfo(FileName).Length;
            var mb = Math.Round((double)length / (double)1024 / (double)1024, 2);
            if (mb > maxLength)
            {
                // 查询要删除的数据
                var itemsToDelete = Cache.Find(Query.All(nameof(CacheItem.Id)), 0, Cache.Count() / 2);

                // 构建要删除的数据的主键列表
                var idsToDelete = itemsToDelete.Select(item => item.Id);

                // 执行批量删除操作
                Cache.DeleteMany(a => idsToDelete.Contains(a.Id));

                InitDb();
            }
        }
    }

    /// <summary>
    /// init database
    /// </summary>
    public void InitDb()
    {
        lock (Litedb)
        {
            Litedb.Checkpoint();
            Litedb.Rebuild();
            lock (Cache)
            {
                Cache.EnsureIndex(c => c.Type);
            }
        }

    }
    /// <summary>
    /// 构造函数传入Id号作为数据库文件名称
    /// </summary>
    /// <param name="id">id号</param>
    /// <param name="typeName">种类，与ID结合作为文件名称</param>
    public LiteDBCache(string id, string typeName)
    {
        _id = id;
        _typeName = typeName;
        var dir = Path.Combine(AppContext.BaseDirectory, "ListDBCache");
        Directory.CreateDirectory(dir);
        var op = new LiteDBOptions() { FilePath = dir, FileName = $"{id}_{Regex.Replace($"{typeName}", "[^a-zA-Z]", "_")}.ldb", ConnectionType = ConnectionType.Shared };
        LiteDBDatabaseProvider dbProvider = new(op);
        FileName = op.DataSource;
        this._dbProvider = dbProvider;
        this.Litedb = _dbProvider.GetConnection();
        this.Cache = Litedb.GetCollection<CacheItem>(Regex.Replace($"tg_{typeName}", "[^a-zA-Z]", "_"));
        this._name = typeName;
        InitDb();
    }

    /// <summary>
    /// 删除不需要的缓存文件
    /// </summary>
    /// <param name="ids">当前需要的id集合，除此以外都会删除</param>
    public static void DeleteOldData(List<long> ids)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "ListDBCache");
        Directory.CreateDirectory(dir);
        string searchPattern = "*.ldb"; // 文件名匹配模式
        string[] files = Directory.GetFiles(dir, searchPattern);
        foreach (string file in files)
        {
            if (!ids.Any(a => Path.GetFileName(file).StartsWith(a.ToString())))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }


}
