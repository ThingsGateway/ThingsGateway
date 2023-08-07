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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel;

namespace ThingsGateway.Application;
/// <summary>
/// 历史数据表
/// </summary>
[IgnoreSqlTable]
[SugarTable("historyValue", TableDescription = "历史数据表")]
public class HistoryValue : PrimaryIdEntity
{
    /// <summary>
    /// 忽略Id，无实际上传字段
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public override long Id { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    [TimeDbSplitField(DateType.Month)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    [Description("上传时间")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DateTime CollectTime { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDataType = "symbol")]
    [Description("变量名称")]
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Name { get; set; }
    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 变量值
    /// </summary>
    [Description("变量值")]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public double Value { get; set; }
}
