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

using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ThingsGateway.DynamicApiController;

/// <summary>
/// 动态 WebAPI 构建器
/// </summary>
[SuppressSniffer]
public sealed class DynamicApiControllerBuilder
{
    /// <summary>
    /// 提供生成控制器过滤器
    /// </summary>
    /// <remarks>返回 <c>true</c> 将生成控制器，否则跳过。</remarks>
    public Func<ControllerModel, bool> ControllerFilter { get; set; }

    /// <summary>
    /// 添加 Action 自定义配置
    /// </summary>
    /// <remarks>返回 <c>true</c> 将生成 Action，否则跳过。</remarks>
    public Action<ActionModel> ActionConfigure { get; set; }
}