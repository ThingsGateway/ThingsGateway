//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// Rpc写入日志
///</summary>
[SugarTable("rpc_log", TableDescription = "RPC操作日志")]
[Tenant(SqlSugarConst.DB_Log)]
public class RpcLog : PrimaryIdEntity
{
    /// <summary>
    /// 日志时间
    /// </summary>
    [SugarColumn(ColumnDescription = "日志时间", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 操作源
    ///</summary>
    [SugarColumn(ColumnDescription = "操作源", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string OperateSource { get; set; }

    /// <summary>
    /// 操作对象
    ///</summary>
    [SugarColumn(ColumnDescription = "操作对象", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string OperateObject { get; set; }

    /// <summary>
    /// 操作方法
    ///</summary>
    [SugarColumn(ColumnDescription = "Rpc方法", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string OperateMethod { get; set; }

    /// <summary>
    /// 操作结果
    ///</summary>
    [SugarColumn(ColumnDescription = "操作结果", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 请求参数
    ///</summary>
    [SugarColumn(ColumnDescription = "请求参数", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, ShowTips = true)]
    public string? ParamJson { get; set; }

    /// <summary>
    /// 返回结果
    ///</summary>
    [SugarColumn(ColumnDescription = "返回结果", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, ShowTips = true)]
    public string? ResultJson { get; set; }

    /// <summary>
    /// 具体消息
    ///</summary>
    [SugarColumn(ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, ShowTips = true)]
    public string? OperateMessage { get; set; }
}
