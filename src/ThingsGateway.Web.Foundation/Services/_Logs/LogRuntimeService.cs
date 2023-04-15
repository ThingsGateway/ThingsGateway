using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <inheritdoc cref="IRuntimeLogService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class RuntimeLogService : DbRepository<RuntimeLog>, IRuntimeLogService
    {
        /// <inheritdoc />
        [OperDesc("删除网关运行日志")]
        public async Task DeleteAsync()
        {
            await AsDeleteable().ExecuteCommandAsync();
        }

        /// <inheritdoc />
        public async Task<SqlSugarPagedList<RuntimeLog>> PageAsync(RuntimeLogPageInput input)
        {
            var query = Context.Queryable<RuntimeLog>()
                               .WhereIF(!string.IsNullOrEmpty(input.Source), it => it.LogSource.Contains(input.Source))
                               .WhereIF(!string.IsNullOrEmpty(input.Level), it => it.LogLevel.ToString().Contains(input.Level))
                               .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                               .OrderBy(it => it.Id, OrderByType.Desc);

            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }
    }
}