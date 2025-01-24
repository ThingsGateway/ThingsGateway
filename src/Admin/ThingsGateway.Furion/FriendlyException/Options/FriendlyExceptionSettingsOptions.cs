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

using Microsoft.Extensions.Configuration;

using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway.FriendlyException;

/// <summary>
/// 友好异常配置选项
/// </summary>
public sealed class FriendlyExceptionSettingsOptions : IConfigurableOptions<FriendlyExceptionSettingsOptions>
{
    /// <summary>
    /// 隐藏错误码
    /// </summary>
    public bool? HideErrorCode { get; set; }

    /// <summary>
    /// 默认错误码
    /// </summary>
    public string DefaultErrorCode { get; set; }

    /// <summary>
    /// 默认错误消息
    /// </summary>
    public string DefaultErrorMessage { get; set; }

    /// <summary>
    /// 标记 Oops.Oh 为业务异常
    /// </summary>
    /// <remarks>也就是不会进入异常处理</remarks>
    public bool? ThrowBah { get; set; }

    /// <summary>
    /// 是否输出异常日志
    /// </summary>
    public bool? LogError { get; set; }

    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(FriendlyExceptionSettingsOptions options, IConfiguration configuration)
    {
        options.HideErrorCode ??= false;
        options.DefaultErrorCode ??= string.Empty;
        //options.DefaultErrorMessage ??= "Internal Server Error";
        options.ThrowBah ??= false;
        options.LogError ??= true;
    }
}