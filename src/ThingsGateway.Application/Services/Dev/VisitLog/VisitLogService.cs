#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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