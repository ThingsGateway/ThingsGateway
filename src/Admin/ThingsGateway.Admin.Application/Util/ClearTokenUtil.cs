//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public class ClearTokenUtil
{
    private static IRelationService RelationService;
    private static ISysUserService SysUserService;

    /// <summary>
    /// 根据角色ID列表清除用户缓存
    /// </summary>
    public static async Task DeleteUserCacheByRoleIds(IEnumerable<long> roleIds)
    {
        // 解析角色服务
        RelationService ??= App.RootServices!.GetRequiredService<IRelationService>();
        //获取用户和角色关系
        var relations = await RelationService.GetRelationListByTargetIdListAndCategoryAsync(roleIds.Select(it => it.ToString()), RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
        if (relations.Any())
        {
            var userIds = relations.Select(it => it.ObjectId);//用户ID列表

            // 解析用户服务
            SysUserService ??= App.RootServices!.GetRequiredService<ISysUserService>();
            SysUserService.DeleteUserFromCache(userIds);
        }
    }

    public static async Task DeleteUserTokenByOrgIds(HashSet<long> orgIds)
    {
        // 获取用户ID列表
        var userIds = await DbContext.Db.CopyNew().QueryableWithAttr<SysUser>().Where(it => orgIds.Contains(it.OrgId)).Select(it => it.Id).ToListAsync().ConfigureAwait(false);
        //从redis中删除所属机构的用户token
        App.CacheService.HashDel<VerificatInfo>(CacheConst.Cache_Token, userIds.Select(it => it.ToString()).ToArray());
    }
}
