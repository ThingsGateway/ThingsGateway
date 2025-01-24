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

using Swashbuckle.AspNetCore.SwaggerGen;

using ThingsGateway.DataValidation;
using ThingsGateway.FriendlyException;

namespace ThingsGateway;

/// <summary>
/// AddInject 配置选项
/// </summary>
public sealed class AddInjectOptions
{
    /// <summary>
    /// 配置 Swagger Gen
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureSwaggerGen(Action<SwaggerGenOptions> configure)
    {
        SwaggerGenConfigure = configure;
    }

    /// <summary>
    /// 配置 DataValidation
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureDataValidation(Action<DataValidationOptions> configure)
    {
        DataValidationConfigure = configure;
    }

    /// <summary>
    /// 配置 FriendlyException
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureFriendlyException(Action<FriendlyExceptionOptions> configure)
    {
        FriendlyExceptionConfigure = configure;
    }

    /// <summary>
    /// Swagger Gen 配置
    /// </summary>
    internal static Action<SwaggerGenOptions> SwaggerGenConfigure { get; private set; }

    /// <summary>
    /// DataValidation 配置
    /// </summary>
    internal static Action<DataValidationOptions> DataValidationConfigure { get; private set; }

    /// <summary>
    /// FriendlyException 配置
    /// </summary>
    internal static Action<FriendlyExceptionOptions> FriendlyExceptionConfigure { get; private set; }
}