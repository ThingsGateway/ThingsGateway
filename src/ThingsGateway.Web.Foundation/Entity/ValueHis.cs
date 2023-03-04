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
    [SugarColumn(IsIgnore = true)]
    public override long Id { get; set; }

    [TimeDbSplitField(DateType.Month)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    [Description("上传时间")]
    public DateTime CollectTime { get; set; }

    [SugarColumn(ColumnDataType = "symbol")]
    [Description("变量名称")]
    public string Name { get; set; }

    [Description("质量戳")]
    public int Quality { get; set; }

    [Description("变量值")]
    public double Value { get; set; }
}
public enum HisDbType
{
    QuestDB,
}