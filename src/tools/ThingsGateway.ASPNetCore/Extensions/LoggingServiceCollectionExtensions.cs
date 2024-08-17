// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ThingsGateway;
using ThingsGateway.Logging;

public static class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// 添加日志监视器服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure">添加更多配置</param>
    /// <param name="jsonKey">配置文件对于的 Key，默认为 Logging:Monitor</param>
    /// <returns></returns>
    public static IServiceCollection AddMonitorLogging(this IServiceCollection services, Action<LoggingMonitorSettings> configure = default, string jsonKey = "Logging:Monitor")
    {
        // 读取配置
        var settings = NetCoreApp.Configuration?.GetSection(jsonKey).Get<LoggingMonitorSettings>()
            ?? new LoggingMonitorSettings();
        settings.IsMvcFilterRegister = false;   // 解决过去 Mvc Filter 全局注册的问题
        settings.FromGlobalFilter = true;   // 解决局部和全局触发器同时配置触发两次问题
        settings.IncludeOfMethods ??= Array.Empty<string>();
        settings.ExcludeOfMethods ??= Array.Empty<string>();
        settings.MethodsSettings ??= Array.Empty<LoggingMonitorMethod>();

        // 添加外部配置
        configure?.Invoke(settings);

        // 如果配置 GlobalEnabled = false 且 IncludeOfMethods 和 ExcludeOfMethods 都为空，则不注册服务
        if (settings.GlobalEnabled == false
            && settings.IncludeOfMethods.Length == 0
            && settings.ExcludeOfMethods.Length == 0) return services;

        // 注册日志监视器过滤器
        services.AddMvcFilter(new LoggingMonitorAttribute(settings));

        return services;
    }
}
