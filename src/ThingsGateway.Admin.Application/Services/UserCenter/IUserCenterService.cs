//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public interface IUserCenterService
{
    /// <summary>
    /// 获取个人工作台
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>个人工作台信息</returns>
    Task<WorkbenchInfo> GetLoginWorkbenchAsync(long userId);

    /// <summary>
    /// 获取菜单列表，不会转成树形数据
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <param name="moduleId">模块id</param>
    /// <returns>菜单列表</returns>
    Task<IEnumerable<SysResource>> GetOwnMenuAsync(long userId, long moduleId);

    /// <summary>
    /// 设置默认模块
    /// </summary>
    /// <param name="moduleId">模块id</param>
    /// <returns></returns>
    Task SetDefaultModule(long moduleId);

    /// <summary>
    /// 更新密码
    /// </summary>
    /// <param name="input">密码更新输入</param>
    /// <returns></returns>
    Task UpdatePasswordAsync(UpdatePasswordInput input);

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="input">用户信息</param>
    /// <returns></returns>
    Task UpdateUserInfoAsync(SysUser input);

    /// <summary>
    /// 更新个人工作台信息
    /// </summary>
    /// <param name="input">个人工作台信息</param>
    /// <returns></returns>
    Task UpdateWorkbenchInfoAsync(WorkbenchInfo input);
}
