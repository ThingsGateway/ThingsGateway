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

using Mapster;

using Microsoft.AspNetCore.Mvc;

using System.Data;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc cref="IRpcLogService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class RpcLogService : DbRepository<RpcLog>, IRpcLogService
{
    private readonly IImportExportService _importExportService;

    public RpcLogService(IImportExportService importExportService)
    {
        _importExportService = importExportService;
    }

    /// <inheritdoc />
    [OperDesc("删除网关Rpc日志")]
    public async Task DeleteAsync()
    {
        await AsDeleteable().ExecuteCommandAsync();
    }

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<RpcLog>> PageAsync(RpcLogPageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <inheritdoc/>
    private ISugarQueryable<RpcLog> GetPage(RpcLogPageInput input)
    {
        var query = Context.Queryable<RpcLog>()
                           .WhereIF(input.StartTime != null, a => a.LogTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.LogTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.Source), it => it.OperateSource.Contains(input.Source))
                           .WhereIF(!string.IsNullOrEmpty(input.Object), it => it.OperateObject.Contains(input.Object))
                           .WhereIF(!string.IsNullOrEmpty(input.Method), it => it.OperateMethod.Contains(input.Method));
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    /// <inheritdoc/>
    [OperDesc("导出RPC日志", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(IDataReader? input = null)
    {
        if (input != null)
        {
            return await _importExportService.ExportAsync<RpcLog>(input, "RpcLog");
        }

        var query = Context.Queryable<RpcLog>().ExportIgnoreColumns();
        var sqlObj = query.ToSql();
        using IDataReader? dataReader = await Context.Ado.GetDataReaderAsync(sqlObj.Key, sqlObj.Value);
        return await _importExportService.ExportAsync<RpcLog>(dataReader, "RpcLog");
    }

    /// <inheritdoc/>
    [OperDesc("导出RPC日志", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(RpcLogInput input)
    {
        var query = GetPage(input.Adapt<RpcLogPageInput>()).ExportIgnoreColumns();
        var sqlObj = query.ToSql();
        using IDataReader? dataReader = await Context.Ado.GetDataReaderAsync(sqlObj.Key, sqlObj.Value);
        return await ExportFileAsync(dataReader);
    }

    public async Task<List<RpcLogDayStatisticsOutput>> StatisticsByDayAsync(int day)
    {
        //取最近七天
        var dayArray = Enumerable.Range(0, day).Select(it => DateTime.Now.Date.AddDays(it * -1)).ToList();
        //生成时间表
        var queryableLeft = Context.Reportable(dayArray).ToQueryable<DateTime>();
        //ReportableDateType.MonthsInLast1yea 表式近一年月份 并且queryable之后还能在where过滤
        var queryableRight = Context.Queryable<RpcLog>(); //声名表
        //报表查询
        var list = await Context.Queryable(queryableLeft, queryableRight, JoinType.Left, (x1, x2)
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