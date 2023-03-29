using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 历史数据表
/// </summary>
[IgnoreSqlTableAttribute]
[SugarTable("value_his", TableDescription = "历史数据表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class ValueHis : PrimaryIdEntity
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
    public DateTime CollectTime { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDataType = "symbol")]
    [Description("变量名称")]
    public string Name { get; set; }
    /// <summary>
    /// 质量戳
    /// </summary>
    [Description("质量戳")]
    public int Quality { get; set; }

    /// <summary>
    /// 变量值
    /// </summary>
    [Description("变量值")]
    public double Value { get; set; }
}
/// <summary>
/// 数据库类型
/// </summary>
public enum HisDbType
{
    /// <summary>
    /// 时序库QuestDB
    /// </summary>
    QuestDB,
}