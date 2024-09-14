//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using ThingsGateway.Core.Extension;

namespace ThingsGateway;

/// <summary>
/// 数据库上下文对象
/// </summary>
public static class DbContext
{
    /// <summary>
    /// SqlSugar 数据库实例
    /// </summary>
    public static readonly SqlSugarScope Db;

    /// <summary>
    /// 读取配置文件中的 ConnectionStrings:Sqlsugar 配置节点
    /// </summary>
    public static readonly SqlSugarOptions DbConfigs;
    public static readonly ISugarAopService SugarAopService;

    static DbContext()
    {
        // 配置映射
        DbConfigs = NetCoreApp.Configuration?.GetSection(nameof(SqlSugarOptions)).Get<SqlSugarOptions>()!;
        SugarAopService = NetCoreApp.RootServices.GetService<ISugarAopService>();
        Db = new(DbConfigs.Select(a => (ConnectionConfig)a).ToList(), db =>
        {
            DbConfigs.ForEach(it =>
            {
                var sqlsugarScope = db.GetConnectionScope(it.ConfigId);//获取当前库
                MoreSetting(sqlsugarScope);//更多设置
                SugarAopService.AopSetting(sqlsugarScope, it.IsShowSql);//aop配置
            }
            );
        });
    }


    /// <summary>
    /// 实体更多配置
    /// </summary>
    /// <param name="db"></param>
    private static void MoreSetting(SqlSugarScopeProvider db)
    {
        db.CurrentConnectionConfig.MoreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = true//设置默认nvarchar
        };
    }

    public static void WriteErrorLogWithSql(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }

    public static void WriteLog(string msg)
    {
        Console.WriteLine("【库操作】：" + msg + Environment.NewLine);
    }

    public static void WriteLogWithSql(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
}
