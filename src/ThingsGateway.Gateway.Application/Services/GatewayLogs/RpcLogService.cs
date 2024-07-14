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

using System.Data;

namespace ThingsGateway.Gateway.Application;

public class RpcLogService : BaseService<RpcLog>, IRpcLogService
{
    #region 查询

    /// <summary>
    /// 最新十条
    /// </summary>
    /// <param name="account">操作人</param>
    public async Task<List<RpcLog>> GetNewLog()
    {
        using var db = GetDB();
        var data = await db.Queryable<RpcLog>().OrderByDescending(a => a.LogTime).Take(10).ToListAsync();
        return data;
    }

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public Task<QueryData<RpcLog>> PageAsync(QueryPageOptions option)
    {
        return QueryAsync(option);
    }

    #endregion 查询

    #region 删除

    /// <inheritdoc />
    [OperDesc("DeleteRpcLog", localizerType: typeof(RpcLog))]
    public async Task DeleteRpcLogAsync()
    {
        using var db = GetDB();
        await db.Deleteable<RpcLog>().ExecuteCommandAsync();
    }

    #endregion 删除

    public async Task<List<RpcLogDayStatisticsOutput>> StatisticsByDayAsync(int day)
    {
        using var db = GetDB();
        //取最近七天
        var dayArray = Enumerable.Range(0, day).Select(it => DateTime.Now.Date.AddDays(it * -1)).ToList();
        //生成时间表
        var queryableLeft = db.Reportable(dayArray).ToQueryable<DateTime>();
        //ReportableDateType.MonthsInLast1yea 表式近一年月份 并且queryable之后还能在where过滤
        var queryableRight = db.Queryable<RpcLog>(); //声名表
        //报表查询
        var list = await db.Queryable(queryableLeft, queryableRight, JoinType.Left, (x1, x2)
            => x2.LogTime.ToString("yyyy-MM-dd") == x1.ColumnName.ToString("yyyy-MM-dd"))
        .GroupBy((x1, x2) => x1.ColumnName)//根据时间分组
        .OrderBy((x1, x2) => x1.ColumnName)//根据时间升序排序
        .Select((x1, x2) => new RpcLogDayStatisticsOutput
        {
            SuccessCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.IsSuccess == true, 1, 0)), //null的数据要为0所以不能用count
            FailCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.IsSuccess == false, 1, 0)), //null的数据要为0所以不能用count
            Date = x1.ColumnName.ToString("yyyy-MM-dd")
        }
        )

        .ToListAsync();
        return list;
    }
}
