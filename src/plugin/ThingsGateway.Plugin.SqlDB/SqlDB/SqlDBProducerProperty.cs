//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducerProperty
/// </summary>
public class SqlDBProducerProperty : BusinessPropertyWithCacheInterval
{
    [DynamicProperty("是否实时表", "true=>实时表更新")] public bool IsReadDB { get; set; } = false;
    [DynamicProperty("是否历史表", "true=>历史存储(按月分表)")] public bool IsHisDB { get; set; } = true;
    [DynamicProperty("实时表名称", "")] public string ReadDBTableName { get; set; } = "ReadDBTableName";

    [DynamicProperty("数据库类型", "MySql/SqlServer")] public DbType DbType { get; set; } = DbType.SqlServer;
    [DynamicProperty("链接字符串", "")] public string BigTextConnectStr { get; set; } = "server=.;uid=sa;pwd=111111;database=test;";
}