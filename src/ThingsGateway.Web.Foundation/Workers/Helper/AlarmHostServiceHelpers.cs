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

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 报警扩展
/// </summary>
public static class AlarmHostServiceHelpers
{
    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    public static AlarmEnum GetBoolAlarmCode(DeviceVariableRunTime tag, out string limit, out string expressions, out string text)
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
    public static AlarmEnum GetDecimalAlarmDegree(DeviceVariableRunTime tag, out string limit, out string expressions, out string text)
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