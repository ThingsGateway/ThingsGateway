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

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IOperateLogService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class OperateLogService : DbRepository<SysOperateLog>, IOperateLogService
{
    /// <inheritdoc />
    [OperDesc("删除操作日志")]
    public async Task DeleteAsync(params string[] category)
    {
        await AsDeleteable().Where(it => category.Contains(it.Category)).ExecuteCommandAsync();
    }
    /// <inheritdoc/>
    [OperDesc("导出操作日志", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<SysOperateLog> input = null)
    {
        input ??= await GetListAsync();

        //总数据
        Dictionary<string, object> sheets = new();
        List<Dictionary<string, object>> devExports = new();
        foreach (var devData in input)
        {
            #region sheet
            //变量页
            var data = devData.GetType().GetProperties();
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

        sheets.Add("操作日志", devExports);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <inheritdoc/>
    [OperDesc("导出操作日志", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(OperateLogInput input)
    {
        var query = GetPage(input.Adapt<OperateLogPageInput>());
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }
    /// <inheritdoc />
    public async Task<ISqlSugarPagedList<SysOperateLog>> PageAsync(OperateLogPageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    private ISugarQueryable<SysOperateLog> GetPage(OperateLogPageInput input)
    {
        var query = Context.Queryable<SysOperateLog>()
                             .WhereIF(input.StartTime != null, a => a.OpTime >= input.StartTime.Value.ToLocalTime())
                           .WhereIF(input.EndTime != null, a => a.OpTime <= input.EndTime.Value.ToLocalTime())
                           .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.OpAccount == input.Account)//根据账号查询
                           .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//根据分类查询
                           .WhereIF(!string.IsNullOrEmpty(input.ExeStatus), it => it.ExeStatus == input.ExeStatus)//根据结果查询
                           .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey) || it.OpIp.Contains(input.SearchKey));//根据关键字查询


        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序
        return query;
    }
}