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

namespace ThingsGateway.Plugin.QuestDB;

[SugarTable("historyValue")]
public class QuestDBHistoryValue : IPrimaryIdEntity, IDBHistoryValue
{
    [SugarColumn(ColumnDescription = "变量Id")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public long Id { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [TimeDbSplitField(DateType.Month)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public DateTime CollectTime { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string DeviceName { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 变量值
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string Value { get; set; }
}
