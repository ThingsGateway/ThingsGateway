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
using Microsoft.Extensions.Hosting;

using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway;

/// <summary>
/// 应用全局配置
/// </summary>
public sealed class AppSettingsOptions : IConfigurableOptions<AppSettingsOptions>
{
    /// <summary>
    /// 集成 MiniProfiler 组件
    /// </summary>
    public bool? InjectMiniProfiler { get; set; }

    /// <summary>
    /// 是否启用规范化文档
    /// </summary>
    public bool? InjectSpecificationDocument { get; set; }

    /// <summary>
    /// 是否启用引用程序集扫描
    /// </summary>
    public bool? EnabledReferenceAssemblyScan { get; set; }

    /// <summary>
    /// 外部程序集
    /// </summary>
    /// <remarks>扫描 dll 文件，如果是单文件发布，需拷贝放在根目录下</remarks>
    public string[] ExternalAssemblies { get; set; }

    /// <summary>
    /// 排除扫描的程序集
    /// </summary>
    public string[] ExcludeAssemblies { get; set; }

    /// <summary>
    /// 配置支持的包前缀名
    /// </summary>
    public string[] SupportPackageNamePrefixs { get; set; }

    /// <summary>
    /// 【部署】二级虚拟目录
    /// </summary>
    public string VirtualPath { get; set; }

    /// <summary>
    /// 后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(AppSettingsOptions options, IConfiguration configuration)
    {
        // 非 Web 环境总是 false，如果是生产环境且不配置 InjectMiniProfiler，默认总是false，MiniProfiler 生产环境耗内存
        if (App.WebHostEnvironment == default
            || (App.HostEnvironment.IsProduction() && options.InjectMiniProfiler == null)) options.InjectMiniProfiler = false;
        else options.InjectMiniProfiler ??= true;

        options.InjectSpecificationDocument ??= true;
        options.EnabledReferenceAssemblyScan ??= false;
        options.ExternalAssemblies ??= Array.Empty<string>();
        options.ExcludeAssemblies ??= Array.Empty<string>();
        options.SupportPackageNamePrefixs ??= Array.Empty<string>();
        options.VirtualPath ??= string.Empty;
    }
}
