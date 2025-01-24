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

using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Plugin.SqlHistoryAlarm;

public class HistoryAlarmPageInput : ITableSearchModel
{
    /// <summary>
    /// 报警类型
    /// </summary>
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public EventTypeEnum? EventType { get; set; }

    /// <summary>
    /// 时间区间
    /// </summary>
    public DateTimeRangeValue? SearchDate { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    public virtual string? VariableName { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(VariableName), () => new SearchFilterAction(nameof(HistoryAlarm.Name), VariableName));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(HistoryAlarm.EventTime), SearchDate!.Start, FilterAction.GreaterThanOrEqual));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(HistoryAlarm.EventTime), SearchDate!.End, FilterAction.LessThanOrEqual));
        ret.AddIF(AlarmType != null, () => new SearchFilterAction(nameof(HistoryAlarm.AlarmType), AlarmType, FilterAction.Equal));
        ret.AddIF(EventType != null, () => new SearchFilterAction(nameof(HistoryAlarm.EventType), EventType, FilterAction.Equal));

        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        SearchDate = null;
        VariableName = null;
        EventType = null;
        AlarmType = null;
    }
}
