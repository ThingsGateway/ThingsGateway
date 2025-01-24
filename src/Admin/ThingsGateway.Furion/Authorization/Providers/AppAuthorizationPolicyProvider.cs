// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ThingsGateway.Authorization;

/// <summary>
/// 授权策略提供器
/// </summary>
internal sealed class AppAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>
    /// 默认回退策略
    /// </summary>
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public AppAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// 获取默认策略
    /// </summary>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return FallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    /// <summary>
    /// 获取回退策略
    /// </summary>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
    {
        return FallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    /// <summary>
    /// 获取策略
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // 判断是否是包含授权策略前缀
        if (policyName.StartsWith(Penetrates.AppAuthorizePrefix))
        {
            // 解析策略名并获取策略参数
            var policies = policyName[Penetrates.AppAuthorizePrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);

            // 添加策略需求
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new AppAuthorizeRequirement(policies));

            return Task.FromResult(policy.Build());
        }

        // 如果策略不匹配，则返回回退策略
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}