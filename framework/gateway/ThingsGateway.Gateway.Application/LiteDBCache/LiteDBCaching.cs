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

/// 代码来自EasyCaching.LiteDB
using LiteDB;

namespace ThingsGateway.Gateway.Application;

public class CacheItem
{
    public long Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
    public string Tag { get; set; }
}

public class LiteDBDatabaseProvider
{
    /// <summary>
    /// The conn.
    /// </summary>
    private LiteDatabase _conn;

    /// <summary>
    /// The options.
    /// </summary>
    private readonly LiteDBOptions _options;

    public LiteDBDatabaseProvider(LiteDBOptions options)
    {
        this._options = options;
    }

    public LiteDatabase GetConnection()
    {
        if (_conn == null)
        {
            ConnectionString builder = new ConnectionString()
            {
                Filename = _options.DataSource,
                //   InitialSize = _options.InitialSize,
                Connection = _options.ConnectionType,
                Password = _options.Password
            };

            _conn = new LiteDatabase(builder);
        }
        return _conn;
    }
}