//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 权限校验服务
/// </summary>
public interface IAuthService : ITransient
{
    /// <summary>
    /// 获取图形验证码
    /// </summary>
    /// <returns></returns>
    ValidCodeOutput GetCaptchaInfo();

    /// <summary>
    /// 获取登录用户信息
    /// </summary>
    /// <returns></returns>
    Task<LoginUserOutput> GetLoginUserAsync();

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="input">登录参数</param>
    /// <param name="loginClientType">登录类型</param>
    /// <returns>Token信息</returns>
    Task<LoginOutput> LoginAsync(LoginInput input);

    /// <summary>
    /// 登录,忽略验证码
    /// </summary>
    /// <param name="input">登录参数</param>
    /// <param name="loginClientType">登录类型</param>
    /// <returns>Token信息</returns>
    Task<LoginOutput> LoginCoreAsync(LoginInput input);

    /// <summary>
    /// 退出登录
    /// </summary>
    /// <param name="long">verificatId</param>
    /// <returns></returns>
    Task LoginOutAsync(long verificatId);
}