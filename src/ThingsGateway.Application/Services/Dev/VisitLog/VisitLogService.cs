namespace ThingsGateway.Application
{
    /// <summary>
    /// <inheritdoc cref="IVisitLogService"/>
    /// </summary>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class VisitLogService : DbRepository<DevLogVisit>, IVisitLogService
    {
        /// <inheritdoc />
        [OperDesc("删除登录日志")]
        public async Task Delete(params string[] category)
        {
            await AsDeleteable().Where(it => category.Contains(it.Category)).ExecuteCommandAsync();
            //删除对应分类日志
        }

        /// <inheritdoc />
        public async Task<SqlSugarPagedList<DevLogVisit>> Page(VisitLogPageInput input)
        {
            var query = Context.Queryable<DevLogVisit>()
                               .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.OpAccount.Contains(input.Account))//根据账号查询
                               .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//根据分类查询
                               .WhereIF(!string.IsNullOrEmpty(input.ExeStatus), it => it.ExeStatus == input.ExeStatus)//根据结果查询
                               .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey) || it.OpIp.Contains(input.SearchKey))//根据关键字查询
                               .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                               .OrderBy(it => it.Id, OrderByType.Desc);
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }
    }
}