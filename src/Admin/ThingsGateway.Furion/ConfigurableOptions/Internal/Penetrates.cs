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

using System.Reflection;

namespace ThingsGateway.ConfigurableOptions;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 获取选项配置
    /// </summary>
    /// <param name="optionsType">选项类型</param>
    /// <returns></returns>
    internal static (OptionsSettingsAttribute, string) GetOptionsConfiguration(Type optionsType)
    {
        var optionsSettings = optionsType.GetCustomAttribute<OptionsSettingsAttribute>(false);

        // 默认后缀
        var defaultStuffx = nameof(Options);

        return (optionsSettings, optionsSettings switch
        {
            // // 没有贴 [OptionsSettings]，如果选项类以 `Options` 结尾，则移除，否则返回类名称
            null => optionsType.Name.EndsWith(defaultStuffx) ? optionsType.Name[0..^defaultStuffx.Length] : optionsType.Name,
            // 如果贴有 [OptionsSettings] 特性，但未指定 Path 参数，则直接返回类名，否则返回 Path
            _ => optionsSettings != null && string.IsNullOrWhiteSpace(optionsSettings.Path) ? optionsType.Name : optionsSettings.Path,
        });
    }

    /// <summary>
    /// 在主机启动时获取选项
    /// </summary>
    /// <remarks>解决 v4.5.2+ 历史版本升级问题</remarks>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    internal static TOptions GetOptionsOnStarting<TOptions>()
        where TOptions : class, new()
    {
        if (App.RootServices == null && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
        {
            var (_, path) = GetOptionsConfiguration(typeof(TOptions));
            return App.GetConfig<TOptions>(path, true);
        }

        return null;
    }
}