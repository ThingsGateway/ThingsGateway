//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;

using SqlSugar;

using ThingsGateway.Admin.Core;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.SqlHisAlarm;

/// <summary>
/// HisAlarmPage
/// </summary>
public partial class HisAlarmPage : IDriverUIBase
{
    private readonly HistoryAlarmPageInput _search = new();
    private IAppDataTable _datatable;

    [Parameter, EditorRequired]
    public object Driver { get; set; }

    public SqlHisAlarm SqlHisAlarm => (SqlHisAlarm)Driver;

    private async Task<SqlSugarPagedList<HistoryAlarm>> QueryCallAsync(HistoryAlarmPageInput input)
    {
        await Task.Run(GetOption);
        await InvokeStateHasChangedAsync();
        using var db = BusinessDatabaseUtil.GetDb(SqlHisAlarm._driverPropertys.DbType, SqlHisAlarm._driverPropertys.BigTextConnectStr);
        var query = db.Queryable<HistoryAlarm>()
                             .WhereIF(input.StartTime != null, a => a.EventTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.EventTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           .WhereIF(input.AlarmType != null, it => it.AlarmType == input.AlarmType)
                           .WhereIF(input.EventType != null, it => it.EventType == input.EventType)
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private object eChartsOption = new();

    private async Task GetOption()
    {
        var dayStatisticsOutputs = await StatisticsByDayAsync(7);

        eChartsOption = new
        {
            backgroundColor = "",
            tooltip = new { trigger = "axis" },
            legend = new { data = new[] { AppService.I18n.T("报警次数"), AppService.I18n.T("恢复次数") }, left = "5%" },
            grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
            toolbox = new { feature = new { saveAsImage = new { } } },
            xAxis = new
            {
                type = "category",
                boundaryGap = false,
                data = dayStatisticsOutputs.Select(a => a.Date).ToArray()
            },
            yAxis = new { type = "value" },
            series = new[]
    {
        new { name =AppService.I18n.T( "Debug"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.AlarmCount).ToArray()
        },
        new { name = AppService.I18n.T("Info"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.FinishCount).ToArray()
        },
    }
        };
    }

    /// <inheritdoc />
    public async Task<List<HisAlarmDayStatisticsOutput>> StatisticsByDayAsync(int day)
    {
        using var db = BusinessDatabaseUtil.GetDb(SqlHisAlarm._driverPropertys.DbType, SqlHisAlarm._driverPropertys.BigTextConnectStr);
        //取最近七天
        var dayArray = Enumerable.Range(0, day).Select(it => DateTime.Now.Date.AddDays(it * -1)).ToList();
        //生成时间表
        var queryableLeft = db.Reportable(dayArray).ToQueryable<DateTime>();
        //ReportableDateType.MonthsInLast1yea 表式近一年月份 并且queryable之后还能在where过滤
        var queryableRight = db.Queryable<HistoryAlarm>(); //声名表
        //报表查询
        var list = await db.Queryable(queryableLeft, queryableRight, JoinType.Left, (x1, x2)
            => x2.EventTime.ToString("yyyy-MM-dd") == x1.ColumnName.ToString("yyyy-MM-dd"))
        .GroupBy((x1, x2) => x1.ColumnName)//根据时间分组
        .OrderBy((x1, x2) => x1.ColumnName)//根据时间升序排序
        .Select((x1, x2) => new HisAlarmDayStatisticsOutput
        {
            AlarmCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.EventType == EventTypeEnum.Alarm, 1, 0)), //null的数据要为0所以不能用count
            FinishCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.EventType == EventTypeEnum.Finish, 1, 0)), //null的数据要为0所以不能用count
            Date = x1.ColumnName.ToString("yyyy-MM-dd")
        }
        ).ToListAsync();

        return list;
    }
}