//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.Hosting;

using ThingsGateway.Core.Extension;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 数据库上下文对象
/// </summary>
public static class DbContext
{
    /// <summary>
    /// 读取配置文件中的 ConnectionStrings:Sqlsugar 配置节点
    /// </summary>
    public static readonly List<SqlSugarConfig> DbConfigs = App.GetConfig<List<SqlSugarConfig>>("SqlSugarSettings:ConnectionStrings");

    /// <summary>
    /// SqlSugar 数据库实例
    /// </summary>
    public static readonly SqlSugarScope Db = new SqlSugarScope(DbConfigs.Adapt<List<ConnectionConfig>>(), db =>
    {
        //遍历配置的数据库
        DbConfigs.ForEach(it =>
        {
            var sqlsugarScope = db.GetConnectionScope(it.ConfigId);//获取当前库
            MoreSetting(sqlsugarScope);//更多设置
            ExternalServicesSetting(sqlsugarScope, it);//实体拓展配置
            AopSetting(sqlsugarScope, it);//aop配置
            FilterSetting(sqlsugarScope);//过滤器配置
        });
    });

    /// <summary>
    /// 实体拓展配置,自定义类型多库兼容
    /// </summary>
    /// <param name="db"></param>
    /// <param name="config"></param>
    private static void ExternalServicesSetting(SqlSugarScopeProvider db, SqlSugarConfig config)
    {
        db.CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices
        {
            // 处理表
            EntityNameService = (type, entity) =>
            {
                if (config.IsUnderLine && !entity.DbTableName.Contains('_'))
                    entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName);// 驼峰转下划线
            },
            //自定义类型多库兼容
            EntityService = (c, p) =>
            {
                //如果是mysql并且是varchar(max) 已弃用
                //if (config.DbType == SqlSugar.DbType.MySql && (p.DataType == SqlsugarConst.NVarCharMax))
                //{
                //    p.DataType = SqlsugarConst.LongText;//转成mysql的longtext
                //}
                //else if (config.DbType == SqlSugar.DbType.Sqlite && (p.DataType == SqlsugarConst.NVarCharMax))
                //{
                //    p.DataType = SqlsugarConst.Text;//转成sqlite的text
                //}
                //默认不写IsNullable为非必填
                //if (new NullabilityInfoContext().Create(c).WriteState is NullabilityState.Nullable)
                //    p.IsNullable = true;
                if (config.IsUnderLine && !p.IsIgnore && !p.DbColumnName.Contains('_'))
                    p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);// 驼峰转下划线
            }
        };
    }

    /// <summary>
    /// Aop设置
    /// </summary>
    /// <param name="db"></param>
    public static void AopSetting(ISqlSugarClient db, SqlSugarConfig sqlSugarConfig = null)
    {
        var config = db.CurrentConnectionConfig;

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;

        // 打印SQL语句
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            //如果不是开发环境就打印sql
            if (App.HostEnvironment.IsDevelopment() && sqlSugarConfig?.IsShowSql == true)
            {
                if (sql.StartsWith("SELECT"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteLog($"查询{config.ConfigId}库操作");
                }
                if (sql.StartsWith("UPDATE"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    WriteLog($"修改{config.ConfigId}库操作");
                }
                if (sql.StartsWith("INSERT"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    WriteLog($"添加{config.ConfigId}库操作");
                }
                if (sql.StartsWith("DELETE"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteLog($"删除{config.ConfigId}库操作");
                }
                WriteLogWithSql(UtilMethods.GetSqlString(config.DbType, sql, pars));
                WriteLog($"{config.ConfigId}库操作结束");
                Console.ForegroundColor = ConsoleColor.White;
            }
        };
        //异常
        db.Aop.OnError = (ex) =>
        {
            if (ex.Parametres == null) return;
            Console.ForegroundColor = ConsoleColor.Red;
            var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
            WriteLog($"{config.ConfigId}库操作异常");
            WriteErrorLogWithSql(UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n");
            Console.ForegroundColor = ConsoleColor.White;
        };
        //插入和更新过滤器
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            // 新增操作
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                // 主键(long类型)且没有值的---赋值雪花Id
                if (entityInfo.EntityColumnInfo.IsPrimarykey && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                    if (id == null || (long)id == 0)
                        entityInfo.SetValue(YitIdHelper.NextId());
                }
                if (entityInfo.PropertyName == nameof(BaseEntity.CreateTime))
                    entityInfo.SetValue(DateTime.Now);

                if (App.User != null)
                {
                    //创建人
                    if (entityInfo.PropertyName == nameof(BaseEntity.CreateUserId))
                        entityInfo.SetValue(App.User.FindFirst(ClaimConst.UserId)?.Value);
                    if (entityInfo.PropertyName == nameof(BaseEntity.CreateUser))
                        entityInfo.SetValue(App.User?.FindFirst(ClaimConst.Account)?.Value);
                }
            }
            // 更新操作
            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                //更新时间
                if (entityInfo.PropertyName == nameof(BaseEntity.UpdateTime))
                    entityInfo.SetValue(DateTime.Now);
                //更新人
                if (App.User != null)
                {
                    if (entityInfo.PropertyName == nameof(BaseEntity.UpdateUserId))
                        entityInfo.SetValue(App.User?.FindFirst(ClaimConst.UserId)?.Value);
                    if (entityInfo.PropertyName == nameof(BaseEntity.UpdateUser))
                        entityInfo.SetValue(App.User?.FindFirst(ClaimConst.Account)?.Value);
                }
            }
        };

        //查询数据转换
        db.Aop.DataExecuted = (value, entity) =>
        {
        };
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

    /// <summary>
    /// 过滤器设置
    /// </summary>
    /// <param name="db"></param>
    public static void FilterSetting(SqlSugarScopeProvider db)
    {
        // 假删除过滤器
        //LogicDeletedEntityFilter(db);
    }

    /// <summary>
    /// 假删除过滤器
    /// </summary>
    /// <param name="db"></param>
    private static void LogicDeletedEntityFilter(SqlSugarScopeProvider db)
    {
    }

    private static void WriteLog(string msg)
    {
        Console.WriteLine("【库操作】：" + msg + Environment.NewLine);
    }

    private static void WriteLogWithSql(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }

    private static void WriteErrorLogWithSql(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
}