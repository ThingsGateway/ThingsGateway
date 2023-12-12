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

using Mapster;

using Microsoft.Extensions.Hosting;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 上传数据库插件静态方法
/// </summary>
public static class UploadDatabaseUtil
{
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public static SqlSugarClient GetDb(DbType dbType, string connectionString)
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
            ConnectionString = connectionString,//连接字符串
            DbType = dbType,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
    }

    /// <summary>
    /// Aop设置
    /// </summary>
    /// <param name="db"></param>
    public static void AopSetting(SqlSugarClient db)
    {
        var config = db.CurrentConnectionConfig;

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;

        // 打印SQL语句
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            //如果是开发环境就打印sql
            if (App.HostEnvironment.IsDevelopment())
            {
                //if (sql.StartsWith("SELECT"))
                //{
                //    Console.ForegroundColor = ConsoleColor.Green;
                //}
                //if (sql.StartsWith("UPDATE"))
                //{
                //    Console.ForegroundColor = ConsoleColor.Yellow;
                //}
                //if (sql.StartsWith("INSERT"))
                //{
                //    Console.ForegroundColor = ConsoleColor.Blue;
                //}
                //if (sql.StartsWith("DELETE"))
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //}
                //WriteSqlLog(UtilMethods.GetSqlString(config.DbType, sql, pars));
                //Console.ForegroundColor = ConsoleColor.White;
                //Console.WriteLine();
            }
        };
        //异常
        db.Aop.OnError = (ex) =>
        {
            //如果是开发环境就打印日志
            if (App.WebHostEnvironment.IsDevelopment())
            {
                if (ex.Parametres == null) return;
                Console.ForegroundColor = ConsoleColor.Red;
                var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                WriteSqlLogError(UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres));
                Console.ForegroundColor = ConsoleColor.White;
            }
        };

    }
    public static void WriteSqlLog(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
    public static void WriteSqlLogError(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }

}


