namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 报警扩展
/// </summary>
public static class AlarmHostServiceHelpers
{
    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    public static AlarmEnum GetBoolAlarmCode(CollectVariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;
        if (tag.BoolCloseAlarmEnable && tag.Value.ToBoolean() == false)
        {
            limit = false.ToString();
            expressions = tag.BoolCloseRestrainExpressions;
            text = tag.BoolCloseAlarmText;
            return AlarmEnum.Close;
        }
        if (tag.BoolOpenAlarmEnable && tag.Value.ToBoolean() == true)
        {
            limit = true.ToString();
            expressions = tag.BoolOpenRestrainExpressions;
            text = tag.BoolOpenAlarmText;
            return AlarmEnum.Open;
        }
        return AlarmEnum.None;
    }

    /// <summary>
    /// 获取value报警类型
    /// </summary>
    public static AlarmEnum GetDecimalAlarmDegree(CollectVariableRunTime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty;
        expressions = string.Empty;
        text = string.Empty;

        if (tag.HHAlarmEnable && tag.Value.ToDecimal() > tag.HHAlarmCode.ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString();
            expressions = tag.HHRestrainExpressions;
            text = tag.HHAlarmText;
            return AlarmEnum.HH;
        }

        if (tag.HAlarmEnable && tag.Value.ToDecimal() > tag.HAlarmCode.ToDecimal())
        {
            limit = tag.HAlarmCode.ToString();
            expressions = tag.HRestrainExpressions;
            text = tag.HAlarmText;
            return AlarmEnum.H;
        }

        if (tag.LAlarmEnable && tag.Value.ToDecimal() < tag.LAlarmCode.ToDecimal())
        {
            limit = tag.LAlarmCode.ToString();
            expressions = tag.LRestrainExpressions;
            text = tag.LAlarmText;
            return AlarmEnum.L;
        }
        if (tag.LLAlarmEnable && tag.Value.ToDecimal() < tag.LLAlarmCode.ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString();
            expressions = tag.LLRestrainExpressions;
            text = tag.LLAlarmText;
            return AlarmEnum.LL;
        }
        return AlarmEnum.None;
    }
}