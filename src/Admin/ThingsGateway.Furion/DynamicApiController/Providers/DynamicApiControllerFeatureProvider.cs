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

using Microsoft.AspNetCore.Mvc.Controllers;

using System.Reflection;

namespace ThingsGateway.DynamicApiController;

/// <summary>
/// 动态接口控制器特性提供器
/// </summary>
[SuppressSniffer]
public sealed class DynamicApiControllerFeatureProvider : ControllerFeatureProvider
{
    /// <summary>
    /// 扫描控制器
    /// </summary>
    /// <param name="typeInfo">类型</param>
    /// <returns>bool</returns>
    protected override bool IsController(TypeInfo typeInfo)
    {
        return Penetrates.IsApiController(typeInfo);
    }
}