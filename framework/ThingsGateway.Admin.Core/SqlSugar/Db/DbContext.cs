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
using Furion.DataEncryption;

using Mapster;

using Microsoft.Extensions.Hosting;

using SqlSugar;

using System.Collections;
using System.Data;
using System.Reflection;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 数据库上下文对象
/// </summary>
public class DbContext
{
    static DbContext()
    {
        // 读取配置文件中的 ConnectionStrings:Sqlsugar 配置节点
        DbConfigs = App.GetConfig<List<SqlSugarConfig>>("ConnectionStrings:SqlSugar");
        Db = new(
        DbConfigs.Adapt<List<ConnectionConfig>>()
        , db =>
        {
            //遍历配置的数据库
            DbConfigs.ForEach(it =>
            {
                var sqlsugarScope = db.GetConnectionScope(it.ConfigId);//获取当前库
                MoreSetting(sqlsugarScope);//更多设置
                ExternalServicesSetting(sqlsugarScope, it);//实体拓展配置
                AopSetting(sqlsugarScope);//aop配置
                FilterSetting(sqlsugarScope);//过滤器配置
            });
        });
        //遍历配置
        DbConfigs.ForEach(it =>
        {
            //是否需要初始化数据库
            if (it.IsInitDb)
            {
                InitDb(it);//初始化数据库表结构
            }
            //是否需要更新种子数据
            if (it.IsSeedData)
            {
                InitSeedData(it);//初始化种子数据
            }
        });
    }


    /// <summary>
    /// SqlSugar 数据库实例
    /// </summary>
    public static SqlSugarScope Db { get; }

    /// <summary>
    /// DbConfigs
    /// </summary>
    public static List<SqlSugarConfig> DbConfigs { get; }



    /// <summary>
    /// Aop设置
    /// </summary>
    /// <param name="db"></param>
    public static void AopSetting(SqlSugarScopeProvider db)
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
                    entityInfo.SetValue(SysDateTimeExtensions.CurrentDateTime);
                //手机号和密码自动加密
                if ((entityInfo.PropertyName == nameof(SysUser.Password) || entityInfo.PropertyName == nameof(SysUser.Phone)))
                    if (oldValue is not null)
                        entityInfo.SetValue(DESCEncryption.Encrypt(oldValue?.ToString(), DESCKeyConst.DESCKey));
                if (App.User != null)
                {
                    //创建人和创建机构ID
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
                    entityInfo.SetValue(SysDateTimeExtensions.CurrentDateTime);
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
            if (entity.Entity.Type == typeof(SysUser) || entity.Entity.Type == typeof(OpenApiUser))
            {
                var phone = entity.GetValue(nameof(SysUser.Phone));
                //如果手机号不为空
                if (phone != null)
                {
                    //手机号数据转换
                    entity.SetValue(nameof(SysUser.Phone), DESCEncryption.Decrypt(phone.ToString(), DESCKeyConst.DESCKey));
                }
                var password = entity.GetValue(nameof(SysUser.Password));
                //如果密码不为空
                if (password != null)
                {
                    //密码数据转换
                    entity.SetValue(nameof(SysUser.Password), DESCEncryption.Decrypt(password.ToString(), DESCKeyConst.DESCKey));
                }
            }
        };
    }

    /// <summary>
    /// 过滤器设置
    /// </summary>
    /// <param name="db"></param>
    public static void FilterSetting(SqlSugarScopeProvider db)
    {

    }

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

            },
            //自定义类型多库兼容
            EntityService = (c, p) =>
            {
                //默认不写IsNullable为非必填
                //if (new NullabilityInfoContext().Create(c).WriteState is NullabilityState.Nullable)
                //    p.IsNullable = true;
            }
        };
    }

    /// <summary>
    /// 初始化数据库表结构
    /// </summary>
    /// <param name="config">数据库配置</param>
    private static void InitDb(SqlSugarConfig config)
    {
        // 获取所有实体表-初始化表结构
        var entityTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false));
        if (!entityTypes.Any()) return;//没有就退出
        var db = DbContext.Db.GetConnectionScope(config.ConfigId);//获取数据库对象
        db.DbMaintenance.CreateDatabase();//创建数据库
        foreach (var entityType in entityTypes)
        {
            var ignoreSqlTableAtt = entityType.GetCustomAttribute<IgnoreSqlTableAttribute>();//忽略创建表
            if (ignoreSqlTableAtt != null) continue;

            var tenantAtt = entityType.GetCustomAttribute<TenantAttribute>();//获取Sqlsugar多租户特性

            if (tenantAtt != null && tenantAtt.configId.ToString() != config.ConfigId) continue;//如果特性存在并且租户ID是当前数据库ID
            var splitTable = entityType.GetCustomAttribute<SplitTableAttribute>();//获取自动分表特性
            if (splitTable == null)//如果特性是空
                db.CodeFirst.InitTables(entityType);//普通创建
            else
                db.CodeFirst.SplitTables().InitTables(entityType);//自动分表创建
        }
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    /// <param name="config"></param>
    private static void InitSeedData(SqlSugarConfig config)
    {
        SqlSugarScopeProvider db = DbContext.Db.GetConnectionScope(config.ConfigId);//获取数据库对象

        var seedDataTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
            && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ISqlSugarEntitySeedData<>))));// 获取所有种子配置-初始化数据
        if (!seedDataTypes.Any()) return;
        foreach (var seedType in seedDataTypes)//遍历种子类
        {
            //使用与指定参数匹配程度最高的构造函数来创建指定类型的实例。
            var instance = Activator.CreateInstance(seedType);
            //获取SeedData方法
            var hasDataMethod = seedType.GetMethod("SeedData");
            //判断是否有种子数据
            var seedData = ((IEnumerable)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
            if (seedData == null) continue;//没有种子数据就下一个
            var entityType = seedType.GetInterfaces().First().GetGenericArguments().First();//获取实体类型
            var tenantAtt = entityType.GetCustomAttribute<TenantAttribute>();//获取sqlsugar租户特性
            if (tenantAtt != null && tenantAtt.configId.ToString() != config.ConfigId) continue;//如果不是当前租户的就下一个
            var seedDataTable = seedData.ToList().ToDataTable();//获取种子数据
            seedDataTable.TableName = db.EntityMaintenance.GetEntityInfo(entityType).DbTableName;//获取表名
            var ignoreAdd = hasDataMethod.GetCustomAttribute<IgnoreSeedDataAddAttribute>();//读取忽略插入特性
            var ignoreUpdate = hasDataMethod.GetCustomAttribute<IgnoreSeedDataUpdateAttribute>();//读取忽略更新特性
            if (seedDataTable.Columns.Contains(nameof(PrimaryIdEntity.Id)))//判断种子数据是否有主键
            {
                //根据判断主键插入或更新
                var storage = db.Storageable(seedDataTable).WhereColumns(nameof(PrimaryIdEntity.Id)).ToStorage();
                if (ignoreAdd == null) storage.AsInsertable.ExecuteCommand();//执行插入
                if (ignoreUpdate == null && config.IsUpdateSeedData) storage.AsUpdateable.ExecuteCommand();//只有没有忽略更新的特性才执行更新
            }
            else // 没有主键或者不是预定义的主键(有重复的可能)
            {
                //全量插入
                var storage = db.Storageable(seedDataTable).ToStorage();
                if (ignoreAdd == null) storage.AsInsertable.ExecuteCommand();
            }
        }
    }
    /// <summary>
    /// 实体更多配置
    /// </summary>
    /// <param name="db"></param>
    private static void MoreSetting(SqlSugarScopeProvider db)
    {
        db.CurrentConnectionConfig.MoreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = true,//设置默认nvarchar
        };
    }

    private static void WriteSqlLog(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
    private static void WriteSqlLogError(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
}