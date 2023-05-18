using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
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
            var query = Context.Queryable<RpcLog>()
                               .WhereIF(!string.IsNullOrEmpty(input.Source), it => it.OperateSource.Contains(input.Source))
                               .WhereIF(!string.IsNullOrEmpty(input.Object), it => it.OperateObject.Contains(input.Object))
                               .WhereIF(!string.IsNullOrEmpty(input.Method), it => it.OperateMethod.Contains(input.Method))
                               .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                               .OrderBy(it => it.Id, OrderByType.Desc);

            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }
    }
}