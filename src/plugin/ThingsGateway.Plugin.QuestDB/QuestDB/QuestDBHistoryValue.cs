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

using SqlSugar;

using System.ComponentModel;

using ThingsGateway.Admin.Core;
using ThingsGateway.Core;

namespace ThingsGateway.Plugin.QuestDB;

/// <summary>
/// 历史数据表
/// </summary>
[SugarTable("historyValue", TableDescription = "历史数据表")]
[SugarIndex(null, nameof(QuestDBHistoryValue.Name), OrderByType.Asc)]
public class QuestDBHistoryValue : PrimaryIdEntity
{
    [SugarColumn(IsIgnore = true)]
    public override long Id { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [TimeDbSplitField(DateType.Month)]
    [Description("采集时间")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public DateTime CollectTime { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    [Description("上传时间")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string DeviceName { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDataType = "symbol")]
    [Description("变量名称")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Name { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 变量值
    /// </summary>
    [Description("变量值")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Value { get; set; }
}