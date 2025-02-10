using Newtonsoft.Json.Linq;

using SqlSugar;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.RulesEngine;

[SugarTable("rules", TableDescription = "规则引擎")]
[SugarIndex("unique_rules_name", nameof(Rules.Name), OrderByType.Asc, true)]
[Tenant(SqlSugarConst.DB_Custom)]
public class Rules : BaseDataEntity
{
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 状态
    ///</summary>
    [SugarColumn(ColumnDescription = "状态", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public bool Status { get; set; } = true;

    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "RulesJson", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public RulesJson RulesJson { get; set; } = new();

}
public class RulesJson
{
    public List<NodeJson> NodeJsons { get; set; } = new();
    public List<LinkJson> LinkJsons { get; set; } = new();
}
public class LinkJson
{
    public Anchor SourcePortAnchor { get; set; } = new();
    public Anchor TargetPortAnchor { get; set; } = new();
}

public class Anchor
{
    public string NodelId { get; set; }
    public PortAlignment PortAlignment { get; set; }
}

public class NodeJson
{
    public string Id { get; set; }
    public string DraggedType { get; set; }
    public JObject CValues { get; set; } = new();
    public Point Point { get; set; }
}