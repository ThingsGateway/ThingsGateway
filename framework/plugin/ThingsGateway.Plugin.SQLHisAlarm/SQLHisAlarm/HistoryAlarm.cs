using SqlSugar;

namespace ThingsGateway.Plugin.SQLHisAlarm;


[SugarTable("historyAlarm", TableDescription = "历史报警表")]
internal class HistoryAlarm : AlarmVariable
{
}
