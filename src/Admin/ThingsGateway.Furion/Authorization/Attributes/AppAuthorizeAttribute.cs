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

using ThingsGateway.Authorization;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// 策略授权特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AppAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="policies">多个策略</param>
    public AppAuthorizeAttribute(params string[] policies)
    {
        if (policies != null && policies.Length > 0) Policies = policies;
    }

    /// <summary>
    /// 策略
    /// </summary>
    public string[] Policies
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Policy)) return Array.Empty<string>();

            return Policy[Penetrates.AppAuthorizePrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        internal set => Policy = $"{Penetrates.AppAuthorizePrefix}{string.Join(',', value)}";
    }
}