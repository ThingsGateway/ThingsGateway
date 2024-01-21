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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IOperateLogService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class OperateLogService : DbRepository<SysOperateLog>, IOperateLogService
{
    private readonly IImportExportService _importExportService;

    public OperateLogService(IImportExportService importExportService)
    {
        _importExportService = importExportService;
    }

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<SysOperateLog>> PageAsync(OperateLogPageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    private ISugarQueryable<SysOperateLog> GetPage(OperateLogPageInput input)
    {
        var query = Context.Queryable<SysOperateLog>()
                             .WhereIF(input.StartTime != null, a => a.OpTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.OpTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.OpAccount == input.Account)//根据账号查询
                           .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//根据分类查询
                           .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey) || it.OpIp.Contains(input.SearchKey))//根据关键字查询
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序
        return query;
    }

    /// <inheritdoc/>
    [OperDesc("导出操作日志", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(IDataReader? input = null)
    {
        if (input != null)
        {
            return await _importExportService.ExportAsync<SysOperateLog>(input, "OperateLog");
        }

        var query = Context.Queryable<SysOperateLog>().ExportIgnoreColumns();
        var sqlObj = query.ToSql();
        using IDataReader? dataReader = await Context.Ado.GetDataReaderAsync(sqlObj.Key, sqlObj.Value);
        return await _importExportService.ExportAsync<SysOperateLog>(dataReader, "OperateLog");
    }

    /// <inheritdoc/>
    [OperDesc("导出操作日志", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(OperateLogInput input)
    {
        var query = GetPage(input.Adapt<OperateLogPageInput>()).ExportIgnoreColumns();
        var sqlObj = query.ToSql();
        using IDataReader? dataReader = await Context.Ado.GetDataReaderAsync(sqlObj.Key, sqlObj.Value);
        return await ExportFileAsync(dataReader);
    }

    /// <inheritdoc />
    public async Task<List<OperateLogDayStatisticsOutput>> StatisticsByDayAsync(int day)
    {
        //取最近七天
        var dayArray = Enumerable.Range(0, day).Select(it => DateTime.Now.Date.AddDays(it * -1)).ToList();
        //生成时间表
        var queryableLeft = Context.Reportable(dayArray).ToQueryable<DateTime>();
        //ReportableDateType.MonthsInLast1yea 表式近一年月份 并且queryable之后还能在where过滤
        var queryableRight = Context.Queryable<SysOperateLog>(); //声名表
        //报表查询
        var list = await Context.Queryable(queryableLeft, queryableRight, JoinType.Left, (x1, x2)
            => x2.OpTime.ToString("yyyy-MM-dd") == x1.ColumnName.ToString("yyyy-MM-dd"))
        .GroupBy((x1, x2) => x1.ColumnName)//根据时间分组
        .OrderBy((x1, x2) => x1.ColumnName)//根据时间升序排序
        .Select((x1, x2) => new OperateLogDayStatisticsOutput
        {
            OperateCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Category == CateGoryConst.Log_OPERATE, 1, 0)), //null的数据要为0所以不能用count
            ExceptionCount = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Category == CateGoryConst.Log_EXCEPTION, 1, 0)), //null的数据要为0所以不能用count
            Date = x1.ColumnName.ToString("yyyy-MM-dd")
        }
        ).ToListAsync();

        return list;
    }

    /// <inheritdoc />
    [OperDesc("删除操作日志", IsRecordPar = false)]
    public async Task DeleteAsync(string category)
    {
        await DeleteAsync(it => it.Category == category);//删除对应分类日志
    }

    ///// <inheritdoc />
    //public async Task<SysOperateLog> DetailAsync(BaseIdInput input)
    //{
    //    return await GetFirstAsync(it => it.Id == input.Id);//删除对应分类日志
    //}
}