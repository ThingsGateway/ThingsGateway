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

using SqlSugar;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Admin.Core;
using ThingsGateway.Plugin.SqlDB;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 上传数据库插件静态方法
/// </summary>
public static class SqlDBBusinessDatabaseUtil
{
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public static SqlSugarClient GetDb(SqlDBProducerProperty sqlDBProducerProperty)
    {
        var configureExternalServices = new ConfigureExternalServices
        {
            SplitTableService = new SqlDBDateSplitTableService(sqlDBProducerProperty),
            EntityService = (type, column) => // 修改列可空-1、带?问号 2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                    column.IsNullable = true;
            },
        };
        var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = sqlDBProducerProperty.BigTextConnectStr,//连接字符串
            DbType = sqlDBProducerProperty.DbType,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        }
        );
        DbContext.AopSetting(sqlSugarClient);//aop配置
        return sqlSugarClient;
    }
}