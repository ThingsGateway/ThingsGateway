//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

public class ClearTokenUtil
{
    private static IRelationService RelationService;
    private static ISysUserService SysUserService;
    private static IVerificatInfoService VerificatInfoService;

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

    /// <summary>
    /// 根据模块ID列表清除用户token
    /// </summary>
    public async Task DeleteUserTokenByModuleId(long moduleId)
    {
        // 解析关系服务
        RelationService ??= App.RootServices!.GetRequiredService<IRelationService>();
        var roleModuleRelations =
            await RelationService.GetRelationListByTargetIdAndCategoryAsync(moduleId.ToString(), RelationCategoryEnum.RoleHasModule).ConfigureAwait(false);//角色模块关系
        var userModuleRelations =
            await RelationService.GetRelationListByTargetIdAndCategoryAsync(moduleId.ToString(), RelationCategoryEnum.UserHasModule).ConfigureAwait(false);//用户模块关系
        var userIds = userModuleRelations.Select(it => it.ObjectId).ToList();//用户ID列表
        var roleIds = roleModuleRelations.Select(it => it.ObjectId).ToList();//角色ID列表
        var userRoleRelations = await RelationService.GetRelationListByTargetIdListAndCategoryAsync(roleIds.Select(it => it.ToString()).ToList(),
            RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//用户角色关系
        userIds.AddRange(userRoleRelations.Select(it => it.ObjectId));//添加用户ID列表

        // 解析用户服务
        SysUserService ??= App.RootServices!.GetRequiredService<ISysUserService>();
        //从cache中删除用户信息
        SysUserService.DeleteUserFromCache(userIds);

        VerificatInfoService ??= App.RootServices!.GetRequiredService<IVerificatInfoService>();//获取服务

        var verificatInfoIds = userIds.SelectMany(a => VerificatInfoService.GetListByUserId(a)).ToList();
        //从cache中删除用户token
        VerificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent() { ClientIds = verificatInfoIds.SelectMany(a => a.ClientIds).ToList(), Message = App.CreateLocalizerByType(typeof(SysUser))["ExitVerificat"] }).ConfigureAwait(false);
    }
}
