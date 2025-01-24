﻿// ------------------------------------------------------------------------
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using System.Reflection;

using ThingsGateway;
using ThingsGateway.ConfigurableOptions;
using ThingsGateway.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 可变选项服务拓展类
/// </summary>
[SuppressSniffer]
public static class ConfigurableOptionsServiceCollectionExtensions
{
    /// <summary>
    /// 添加选项配置
    /// </summary>
    /// <typeparam name="TOptions">选项类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddConfigurableOptions<TOptions>(this IServiceCollection services)
        where TOptions : class, IConfigurableOptions
    {
        var optionsType = typeof(TOptions);

        // 获取选项配置
        var (optionsSettings, path) = Penetrates.GetOptionsConfiguration(optionsType);

        // 配置选项（含验证信息）
        var configurationRoot = App.Configuration;
        var optionsConfiguration = configurationRoot.GetSection(path);

        // 配置选项监听
        if (typeof(IConfigurableOptionsListener<TOptions>).IsAssignableFrom(optionsType))
        {
            var onListenerMethod = optionsType.GetMethod(nameof(IConfigurableOptionsListener<TOptions>.OnListener));
            if (onListenerMethod != null)
            {
                // 监听全局配置改变，目前该方式存在触发两次的 bug：https://github.com/dotnet/aspnetcore/issues/2542
                ChangeToken.OnChange(() => configurationRoot.GetReloadToken(), ((Action)(() =>
                {
                    var options = optionsConfiguration.Get<TOptions>();
                    if (options != null) onListenerMethod.Invoke(options, new object[] { options, optionsConfiguration });
                })).Debounce());
            }
        }

        var optionsConfigure = services.AddOptions<TOptions>()
              .Bind(optionsConfiguration, options =>
              {
                  options.BindNonPublicProperties = true; // 绑定私有变量
              })
              .ValidateDataAnnotations()
              .ValidateOnStart();

        // 实现 Key 映射
        services.PostConfigureAll<TOptions>(options =>
        {
            // 查找所有贴了 MapSettings 的键值对
            var remapKeys = optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                           .Where(u => u.IsDefined(typeof(MapSettingsAttribute), true));
            if (!remapKeys.Any()) return;

            foreach (var prop in remapKeys)
            {
                var propType = prop.PropertyType;
                var realKey = prop.GetCustomAttribute<MapSettingsAttribute>(true).Path;
                var realValue = configurationRoot.GetValue(propType, $"{path}:{realKey}");
                prop.SetValue(options, realValue);
            }
        });

        // 配置复杂验证后后期配置
        var validateInterface = optionsType.GetInterfaces()
            .FirstOrDefault(u => u.IsGenericType && typeof(IConfigurableOptions).IsAssignableFrom(u.GetGenericTypeDefinition()));
        if (validateInterface != null)
        {
            var genericArguments = validateInterface.GenericTypeArguments;

            // 配置复杂验证
            if (genericArguments.Length > 1)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IValidateOptions<TOptions>), genericArguments.Last()));
            }

            // 配置后期配置
            var postConfigureMethod = optionsType.GetMethod(nameof(IConfigurableOptions<TOptions>.PostConfigure));
            if (postConfigureMethod != null)
            {
                if (optionsSettings?.PostConfigureAll != true)
                    optionsConfigure.PostConfigure(options => postConfigureMethod.Invoke(options, new object[] { options, optionsConfiguration }));
                else
                    services.PostConfigureAll<TOptions>(options => postConfigureMethod.Invoke(options, new object[] { options, optionsConfiguration }));
            }
        }

        return services;
    }
}
