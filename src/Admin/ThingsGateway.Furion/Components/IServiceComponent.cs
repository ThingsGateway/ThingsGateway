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

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Components;

namespace System;

/// <summary>
/// 服务组件依赖接口
/// </summary>
public interface IServiceComponent : IComponent
{
    /// <summary>
    /// 装载服务
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="componentContext">组件上下文</param>
    void Load(IServiceCollection services, ComponentContext componentContext);
}