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

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 菜单服务
/// </summary>
public interface IMenuService : ITransient
{
    /// <summary>
    /// 添加菜单
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(MenuAddInput input);

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="input">删除菜单参数</param>
    /// <returns></returns>
    Task DeleteAsync(params long[] input);

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="input">id</param>
    /// <returns>详细信息</returns>
    Task<SysResource> DetailAsync(BaseIdInput input);

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="input">菜单编辑参数</param>
    /// <returns></returns>
    Task EditAsync(MenuEditInput input);

    /// <summary>
    /// 根据模块获取菜单树
    /// </summary>
    /// <param name="input">菜单树查询参数</param>
    /// <returns>菜单树列表</returns>
    Task<List<SysResource>> TreeAsync(MenuPageInput input);
}