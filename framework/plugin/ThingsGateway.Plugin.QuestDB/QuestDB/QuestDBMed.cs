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

using Furion;

using Mapster;

using Microsoft.Extensions.Hosting;

using SqlSugar;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.QuestDB;

/// <summary>
/// MqttClient
/// </summary>
public partial class QuestDB : UpLoadBaseWithCache<DeviceData, QuestDBHistoryValue>
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly QuestDBProperty _driverPropertys = new();
    private readonly QuestDBVariableProperty _variablePropertys = new();

    private volatile bool success = true;
    private static object ValueReturn(DeviceVariableRunTime src)
    {
        var data = src.Value?.ToString()?.ToBool();
        if (data != null)
        {
            return data;
        }
        else
        {
            return src.Value;
        }
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<QuestDBHistoryValue> dev)
    {
        var data = dev.ChunkBetter(_driverPropertys.CacheItemCount);
        foreach (var item in data)
        {
            var cacheItem = new CacheItem()
            {
                Id = YitIdHelper.NextId(),
                Type = varType,
                Value = item.ToJsonString(),
            };
            cacheItems.Add(cacheItem);
        }
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
    }

    /// <summary>
    /// Aop设置
    /// </summary>
    /// <param name="db"></param>
    private static void AopSetting(SqlSugarClient db)
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
        //db.Aop.OnError = (ex) =>
        //{
        //    //如果是开发环境就打印日志
        //    if (App.WebHostEnvironment.IsDevelopment())
        //    {
        //        if (ex.Parametres == null) return;
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
        //        WriteSqlLogError(UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres));
        //        Console.ForegroundColor = ConsoleColor.White;
        //    }
        //};

    }
    private static void WriteSqlLog(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
    private static void WriteSqlLogError(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }

    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    private SqlSugarClient GetDb()
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
            ConnectionString = _driverPropertys.BigTextConnectStr,//连接字符串
            DbType = _driverPropertys.DbType,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
    }

    private async Task<OperResult> InserableAsync(SqlSugarClient db, List<QuestDBHistoryValue> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            var result = await db.Insertable(dbInserts).UseParameter().ExecuteCommandAsync();//不要加分表
            if (result > 0)
            {
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{nameof(QuestDBHistoryValue)}");
            }
            return OperResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
            return new(ex);
        }

    }
}
