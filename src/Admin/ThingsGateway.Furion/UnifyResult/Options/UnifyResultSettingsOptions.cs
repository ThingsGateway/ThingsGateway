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

namespace ThingsGateway.UnifyResult;

/// <summary>
/// 规范化配置选项
/// </summary>
public sealed class UnifyResultSettingsOptions : IConfigurableOptions<UnifyResultSettingsOptions>
{
    /// <summary>
    /// 设置返回 200 状态码列表
    /// <para>默认：401，403，如果设置为 null，则标识所有状态码都返回 200 </para>
    /// </summary>
    public int[] Return200StatusCodes { get; set; }

    /// <summary>
    /// 适配（篡改）Http 状态码（只支持短路状态码，比如 401，403，500 等）
    /// </summary>
    public int[][] AdaptStatusCodes { get; set; }

    /// <summary>
    /// 是否支持 MVC 控制台规范化处理
    /// </summary>
    public bool? SupportMvcController { get; set; }

    /// <summary>
    /// 默认只显示验证错误的首个消息
    /// </summary>
    public bool? SingleValidationErrorDisplay { get; set; }

    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(UnifyResultSettingsOptions options, IConfiguration configuration)
    {
        options.Return200StatusCodes ??= new[] { 401, 403 };
        options.SupportMvcController ??= false;
        options.SingleValidationErrorDisplay ??= false;
    }
}