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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 个人信息中心服务
/// </summary>
public interface IUserCenterService : ITransient
{
    /// <summary>
    /// 更改密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task EditPasswordAsync(PasswordInfoInput input);
    /// <summary>
    /// 获取个人主页
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<long> GetLoginDefaultRazorAsync(long userId);

    /// <summary>
    /// 获取个人首页快捷方式
    /// </summary>
    /// <returns></returns>
    Task<List<long>> GetLoginWorkbenchAsync();

    /// <summary>
    /// 获取个人菜单
    /// </summary>
    /// <returns></returns>
    Task<List<SysResource>> GetOwnMenuAsync(string UserAccount = null);

    /// <summary>
    /// 设置个人主页
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="defaultRazor"></param>
    /// <returns></returns>
    Task UpdateUserDefaultRazorAsync(long userId, long defaultRazor);

    /// <summary>
    /// 更新个人信息
    /// </summary>
    /// <param name="input">信息参数</param>
    /// <returns></returns>
    Task UpdateUserInfoAsync(UpdateInfoInput input);
    /// <summary>
    /// 编辑个人工作台
    /// </summary>
    /// <param name="input">工作台字符串</param>
    /// <returns></returns>
    Task UpdateWorkbenchAsync(List<long> input);
}