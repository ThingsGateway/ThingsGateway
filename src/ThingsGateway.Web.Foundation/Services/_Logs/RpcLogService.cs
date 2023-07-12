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

using MiniExcelLibs;

using System.IO;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <inheritdoc cref="IRpcLogService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class RpcLogService : DbRepository<RpcLog>, IRpcLogService
{

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
                           .WhereIF(!string.IsNullOrEmpty(input.Source), it => it.OperateSource.Contains(input.Source))
                           .WhereIF(!string.IsNullOrEmpty(input.Object), it => it.OperateObject.Contains(input.Object))
                           .WhereIF(!string.IsNullOrEmpty(input.Method), it => it.OperateMethod.Contains(input.Method))
                           .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                           .OrderBy(it => it.Id, OrderByType.Desc);
        return query;
    }

    /// <inheritdoc/>
    [OperDesc("导出操作日志", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<RpcLog> input = null)
    {
        input ??= await GetListAsync();

        //总数据
        Dictionary<string, object> sheets = new Dictionary<string, object>();
        List<Dictionary<string, object>> devExports = new();
        foreach (var devData in input)
        {
            #region 变量sheet
            //变量页
            var data = devData.GetType().GetAllProps();
            Dictionary<string, object> devExport = new();
            foreach (var item in data)
            {
                //描述
                var desc = ObjectExtensions.FindDisplayAttribute(item);
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }

            devExports.Add(devExport);

            #endregion
        }

        sheets.Add("操作日志", devExports);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(RpcLogPageInput input)
    {
        var query = GetPage(input);
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }
}