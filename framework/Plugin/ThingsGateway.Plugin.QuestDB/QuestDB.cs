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

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Plugin.QuestDB;
public class QuestDB : UpLoadBase
{
    private readonly ConcurrentQueue<QuestDBHistoryValue> DeviceVariableRunTimes = new();
    private readonly QuestDBProperty driverPropertys = new();
    private readonly QuestDBVariableProperty variablePropertys = new();
    private TypeAdapterConfig _config;
    private GlobalDeviceData _globalDeviceData;

    private List<DeviceVariableRunTime> _uploadVariables = new();
    private TimerTick exTimerTick;
    public QuestDB()
    {
        _config = new TypeAdapterConfig();
        _config.ForType<DeviceVariableRunTime, HistoryValue>()
            .Map(dest => dest.Value, (src) => ValueReturn(src))
            .Map(dest => dest.CollectTime, (src) => src.CollectTime.ToUniversalTime())//注意sqlsugar插入时无时区，直接utc时间
            .Map(dest => dest.CreateTime, (src) => DateTime.UtcNow);//注意sqlsugar插入时无时区，直接utc时间
    }
    private static object ValueReturn(DeviceVariableRunTime src)
    {
        if (src.Value?.ToString()?.IsBoolValue() == true)
        {
            if (src.Value.ToBoolean())
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return src.Value;
        }
    }

    public override Type DriverDebugUIType => null;
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;

    public override VariablePropertyBase VariablePropertys => variablePropertys;

    public override Task AfterStopAsync()
    {
        return Task.CompletedTask;
    }

    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        SqlSugarClient db = GetHisDbAsync();
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables(typeof(QuestDBHistoryValue));
        await Task.CompletedTask;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var db = GetHisDbAsync();
        {
            if (!driverPropertys.IsInterval)
            {
                try
                {
                    ////变化推送
                    var varList = DeviceVariableRunTimes.ToListWithDequeue();
                    if (varList?.Count != 0)
                    {
                        await InserableAsync(db, varList, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex);
                }

            }
            else
            {
                if (exTimerTick.IsTickHappen())
                {
                    try
                    {
                        var varList = _uploadVariables.ToList().Adapt<List<QuestDBHistoryValue>>(_config);
                        if (varList?.Count != 0)
                        {
                            await InserableAsync(db, varList, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }
            }
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
        }

    }

    public override bool IsConnected() => _uploadVariables?.Count > 0;

    protected override void Dispose(bool disposing)
    {
        try
        {
            _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            _uploadVariables = null;
        }
        catch (Exception ex)
        {
            LogMessage?.LogError(ex);
        }

        base.Dispose(disposing);
    }

    protected override void Init(UploadDeviceRunTime device)
    {

        _globalDeviceData = App.GetService<GlobalDeviceData>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).ToBoolean())
           .ToList();

        _uploadVariables = tags;

        if (!driverPropertys.IsInterval)
        {
            _uploadVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });
        }

        if (_uploadVariables.Count == 0)
        {
            LogMessage.LogWarning("插件变量数量为0");
        }
        if (driverPropertys.IntervalTime < 1)
            driverPropertys.IntervalTime = 10;
        exTimerTick = new(driverPropertys.IntervalTime * 1000);
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
            //如果不是开发环境就打印sql
            if (App.HostEnvironment.IsDevelopment())
            {
                if (sql.StartsWith("SELECT"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (sql.StartsWith("UPDATE"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                if (sql.StartsWith("INSERT"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                if (sql.StartsWith("DELETE"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                WriteSqlLog(UtilMethods.GetSqlString(config.DbType, sql, pars));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
        };
        //异常
        db.Aop.OnError = (ex) =>
        {
            //如果不是开发环境就打印日志
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
    private SqlSugarClient GetHisDbAsync()
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
            ConnectionString = driverPropertys.ConnectStr,//连接字符串
            DbType = DbType.QuestDB,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
    }

    private async Task InserableAsync(SqlSugarClient db, List<QuestDBHistoryValue> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            var result = await db.Insertable(dbInserts).ExecuteCommandAsync(cancellationToken);
            if (result > 0)
                LogMessage.Trace(FoundationConst.LogMessageHeader + dbInserts.ToJsonString());
            //连接成功时补发缓存数据
            var cacheData = await CacheDb.GetCacheData();
            foreach (var item in cacheData)
            {
                try
                {
                    var data = item.CacheStr.FromJsonString<List<QuestDBHistoryValue>>();
                    var cacheresult = await db.Insertable(data).ExecuteCommandAsync(cancellationToken);
                    if (cacheresult > 0)
                    {
                        await CacheDb.DeleteCacheData(item.Id);
                        LogMessage.Trace(FoundationConst.LogMessageHeader + $"主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");
                    }
                }
                catch (Exception ex)
                {
                                                LogMessage.LogWarning(ex);
                }


            }

        }
        catch (Exception ex)
        {
                                        LogMessage.LogWarning(ex);
            await CacheDb.AddCacheData("", dbInserts.ToJsonString(), driverPropertys.CacheMaxCount);
        }

    }

    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        if (!driverPropertys.IsInterval)
            DeviceVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<QuestDBHistoryValue>(_config));
    }
}



