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

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.SqlHisAlarm;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SqlHisAlarmProperty : BusinessPropertyWithCache
{
    [DynamicProperty("链接字符串", "")]
    public string BigTextConnectStr { get; set; } = "server=localhost;Database=test;Uid=root;Pwd=111111;AllowLoadLocalInfile=true;";

    [DynamicProperty("数据库类型", "MySql/SqlServer")]
    public DbType DbType { get; set; } = DbType.MySql;
}