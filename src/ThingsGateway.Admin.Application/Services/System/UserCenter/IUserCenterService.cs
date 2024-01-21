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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 个人信息中心服务
/// </summary>
public interface IUserCenterService : ISugarService, ITransient
{
    #region 查询

    /// <summary>
    /// 获取个人菜单
    /// </summary>
    /// <returns></returns>
    Task<(List<SysResource> menuTree, List<SysResource> menu)> GetOwnMenuAsync(long userId);

    /// <summary>
    /// 获取个人工作台
    /// </summary>
    /// <returns></returns>
    Task<RelationUserWorkBench> GetLoginWorkbenchAsync(long userId);

    #endregion 查询

    #region 编辑

    /// <summary>
    /// 更新个人信息
    /// </summary>
    /// <param name="input">信息参数</param>
    /// <returns></returns>
    Task UpdateUserInfoAsync(UpdateInfoInput input);

    /// <summary>
    /// 编辑个人工作台
    /// </summary>
    /// <param name="input">工作台</param>
    /// <returns></returns>
    Task UpdateWorkbenchAsync(UpdateWorkbenchInput input);

    /// <summary>
    /// 编辑个人主页
    /// </summary>
    /// <param name="input">主页</param>
    /// <returns></returns>
    Task UpdateDefaultRazorAsync(UpdateDefaultRazorInput input);

    /// <summary>
    /// 修改个人密码
    /// </summary>
    /// <param name="input">密码信息</param>
    /// <returns></returns>
    Task UpdatePasswordAsync(UpdatePasswordInput input);

    #endregion 编辑
}