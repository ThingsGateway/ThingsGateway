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

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SQLDB;
public class SQLDB : UpLoadBase
{
    private readonly ConcurrentQueue<SQLHistoryValue> DeviceVariableRunTimes = new();
    private readonly SQLDBProperty driverPropertys = new();
    private readonly SQLDBVariableProperty variablePropertys = new();
    private TypeAdapterConfig _config;
    private GlobalDeviceData _globalDeviceData;

    private List<DeviceVariableRunTime> _uploadVariables = new();
    private TimerTick exTimerTick;
    private TimerTick exRealTimerTick;
    public SQLDB()
    {
        _config = new TypeAdapterConfig();
        _config.ForType<DeviceVariableRunTime, SQLHistoryValue>()
.Map(dest => dest.Id, (src) => YitIdHelper.NextId())
.Map(dest => dest.CreateTime, (src) => DateTime.Now);
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
        var db = GetHisDbAsync();
        db.CodeFirst.InitTables(typeof(SQLHistoryValue));
        db.MappingTables.Add("SQLRealValue", driverPropertys.ReadDBTableName); // typeof(类).Name 可以拿到类名
        db.CodeFirst.InitTables(typeof(SQLRealValue)); //生成的表名是 newTableName
        await Task.CompletedTask;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var db = GetHisDbAsync();

        if (driverPropertys.IsReadDB)
        {
            if (exRealTimerTick.IsTickHappen())
            {
                try
                {
                    var varList = _uploadVariables.ToList().Adapt<List<SQLRealValue>>();
                    if (varList?.Count != 0)
                    {
                        //var result = await db.Storageable(varList).As(driverPropertys.ReadDBTableName).ExecuteCommandAsync(cancellationToken);
                        await db.Fastest<SQLRealValue>().AS(driverPropertys.ReadDBTableName).PageSize(100000).BulkUpdateAsync(varList);

                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex);
                }
            }


        }
        else
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
                        var varList = _uploadVariables.ToList().Adapt<List<SQLHistoryValue>>(_config);
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

    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public SqlSugarClient GetHisDbAsync()
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
            DbType = driverPropertys.DbType,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
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

        if (!driverPropertys.IsReadDB)
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
        exRealTimerTick = new(driverPropertys.IntervalTime * 1000);
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
    private async Task InserableAsync(SqlSugarClient db, List<SQLHistoryValue> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync();
            if (result > 0)
                LogMessage.Trace(FoundationConst.LogMessageHeader + dbInserts.ToJsonString());
            //连接成功时补发缓存数据
            var cacheData = await CacheDb.GetCacheData();
            foreach (var item in cacheData)
            {
                try
                {
                    var data = item.CacheStr.FromJsonString<List<SQLHistoryValue>>();
                    var cacheresult = await db.Insertable(data).SplitTable().ExecuteCommandAsync();
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
        if (!driverPropertys.IsReadDB)
            if (!driverPropertys.IsInterval)
                DeviceVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<SQLHistoryValue>(_config));
    }
}



