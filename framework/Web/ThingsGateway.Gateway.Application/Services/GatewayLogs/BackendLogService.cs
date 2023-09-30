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

using Furion.DependencyInjection;

using Mapster;

using MiniExcelLibs;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc cref="IBackendLogService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class BackendLogService : DbRepository<BackendLog>, IBackendLogService
{
    /// <inheritdoc />
    [OperDesc("删除网关运行日志")]
    public async Task DeleteAsync()
    {
        await AsDeleteable().ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    [OperDesc("导出后台日志", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<BackendLog> input = null)
    {
        input ??= await GetListAsync();

        //总数据
        Dictionary<string, object> sheets = new();
        List<Dictionary<string, object>> devExports = new();
        foreach (var devData in input)
        {
            #region sheet
            //变量页
            var data = devData.GetType().GetPropertiesWithCache();
            Dictionary<string, object> devExport = new();
            foreach (var item in data)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }

            devExports.Add(devExport);

            #endregion
        }

        sheets.Add("后台日志", devExports);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(BackendLogInput input)
    {
        var query = GetPage(input.Adapt<BackendLogPageInput>());
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<BackendLog>> PageAsync(BackendLogPageInput input)
    {
        var query = GetPage(input);

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    private ISugarQueryable<BackendLog> GetPage(BackendLogPageInput input)
    {
        var query = Context.Queryable<BackendLog>()
                           .WhereIF(input.StartTime != null, a => a.LogTime >= input.StartTime.Value.ToLocalTime())
                           .WhereIF(input.EndTime != null, a => a.LogTime <= input.EndTime.Value.ToLocalTime())
                           .WhereIF(!string.IsNullOrEmpty(input.Source), it => it.LogSource.Contains(input.Source))
                           .WhereIF(!string.IsNullOrEmpty(input.Level), it => it.LogLevel.ToString().Contains(input.Level));
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }
}