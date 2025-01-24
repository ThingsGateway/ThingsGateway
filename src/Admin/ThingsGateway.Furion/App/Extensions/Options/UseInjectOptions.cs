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

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ThingsGateway;

/// <summary>
/// UseInject 配置选项
/// </summary>
public sealed class UseInjectOptions
{
    /// <summary>
    /// 配置 Swagger
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureSwagger(Action<SwaggerOptions> configure)
    {
        SwaggerConfigure = configure;
    }

    /// <summary>
    /// 配置 Swagger UI
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureSwaggerUI(Action<SwaggerUIOptions> configure)
    {
        SwaggerUIConfigure = configure;
    }

    /// <summary>
    /// Swagger 配置
    /// </summary>
    internal static Action<SwaggerOptions> SwaggerConfigure { get; private set; }

    /// <summary>
    /// Swagger UI 配置
    /// </summary>
    internal static Action<SwaggerUIOptions> SwaggerUIConfigure { get; private set; }
}