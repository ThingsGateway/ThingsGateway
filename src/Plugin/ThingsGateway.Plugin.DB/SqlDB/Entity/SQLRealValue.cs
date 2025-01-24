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

namespace ThingsGateway.Plugin.SqlDB;

[SugarTable(TableDescription = "设备采集实时表")]
[SugarIndex("{table}_index_Id", nameof(SQLRealValue.Id), OrderByType.Desc)]
[SugarIndex("{table}_index_Name", nameof(SQLRealValue.Name), OrderByType.Desc)]
[SugarIndex("{table}_index_DeviceName", nameof(SQLRealValue.DeviceName), OrderByType.Desc)]
[SugarIndex("{table}_index_CollectTime", nameof(SQLRealValue.CollectTime), OrderByType.Desc)]
public class SQLRealValue : IPrimaryIdEntity
{
    [SugarColumn(ColumnDescription = "变量Id", IsPrimaryKey = true)]
    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public long Id { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称")]
    [AutoGenerateColumn(Order = 14, Visible = true, Sortable = true, Filterable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    [AutoGenerateColumn(Order = 15, Visible = true, Sortable = true, Filterable = false)]
    public string DeviceName { get; set; }

    ///<summary>
    ///实时值
    ///</summary>
    [SugarColumn(ColumnDescription = "实时值")]
    [AutoGenerateColumn(Order = 21, Visible = true, Sortable = true, Filterable = false)]
    public string Value { get; set; }

    ///<summary>
    ///是否在线
    ///</summary>
    [SugarColumn(ColumnDescription = "是否在线")]
    [AutoGenerateColumn(Order = 23, Visible = true, Sortable = true, Filterable = false)]
    public bool IsOnline { get; set; }

    [AutoGenerateColumn(Order = 22, Visible = true, Sortable = true, Filterable = false)]
    [SugarColumn(ColumnDescription = "采集时间")]
    public DateTime CollectTime { get; set; }
}
