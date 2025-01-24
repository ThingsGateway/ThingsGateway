//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Plugin.SqlHistoryAlarm;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SqlHistoryAlarmProperty : BusinessPropertyWithCache
{
    [DynamicProperty]
    public DbType DbType { get; set; } = DbType.SqlServer;

    [DynamicProperty]
    [Required]
    [AutoGenerateColumn(ComponentType = typeof(Textarea), Rows = 1)]
    public string BigTextConnectStr { get; set; } = "server=.;uid=sa;pwd=111111;database=test;";

    [DynamicProperty]
    [Required]
    public string TableName { get; set; } = "historyAlarm";

}
