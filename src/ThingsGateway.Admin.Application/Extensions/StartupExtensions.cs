//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

public static class StartupExtensions
{
    private static ConcurrentBag<AppStartup> AppStartups = new();

    /// <summary>
    /// 排除的配置文件前缀
    /// </summary>
    private static readonly string[] excludeJsonPrefixs = ["appsettings", "bundleconfig", "compilerconfig"];

    /// <summary>
    /// 排除运行时 Json 后缀
    /// </summary>
    private static readonly string[] runtimeJsonSuffixs =
    [
            "deps.json",
            "runtimeconfig.dev.json",
            "runtimeconfig.prod.json",
            "runtimeconfig.json",
            "staticwebassets.runtime.json"
    ];

    /// <summary>
    /// 对配置文件名进行分组
    /// </summary>
    /// <param name="configFiles"></param>
    /// <returns></returns>
    private static IEnumerable<IGrouping<string, string>> SplitConfigFileNameToGroups(IEnumerable<string> configFiles)
    {
        // 分组
        return configFiles.GroupBy(Function);

        // 本地函数
        static string Function(string file)
        {
            // 根据 . 分隔
            var fileNameParts = Path.GetFileName(file).Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (fileNameParts.Length == 2) return fileNameParts[0];

            return string.Join('.', fileNameParts.Take(fileNameParts.Length - 2));
        }
    }

    /// <summary>
    /// 加载自定义 .json 配置文件
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="hostEnvironment"></param>
    internal static void AddJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment hostEnvironment)
    {
        // 获取根配置
#if !NET5_0
        var configuration = configurationBuilder is ConfigurationManager
            ? (configurationBuilder as ConfigurationManager)
            : configurationBuilder.Build();
#else
        var configuration = configurationBuilder.Build();
#endif

        // 获取程序执行目录
        var executeDirectory = AppContext.BaseDirectory;

        // 获取自定义配置扫描目录
        var configurationScanDirectories = (configuration.GetSection("ConfigurationScanDirectories")
                .Get<string[]>()
            ?? Array.Empty<string>()).Select(u => Path.Combine(executeDirectory, u));

        // 扫描执行目录及自定义配置目录下的 *.json 文件
        var jsonFiles = new[] { executeDirectory }
                            .Concat(configurationScanDirectories)
                            .SelectMany(u =>
                                Directory.GetFiles(u, "*.json", SearchOption.TopDirectoryOnly));

        // 如果没有配置文件，中止执行
        if (!jsonFiles.Any()) return;

        // 获取环境变量名，如果没找到，则读取 NETCORE_ENVIRONMENT 环境变量信息识别（用于非 Web 环境）
        var envName = hostEnvironment?.EnvironmentName ?? Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Unknown";

        // 读取忽略的配置文件
        var ignoreConfigurationFiles = (configuration.GetSection("IgnoreConfigurationFiles")
                .Get<string[]>()
            ?? Array.Empty<string>());

        // 处理控制台应用程序
        var _excludeJsonPrefixs = hostEnvironment == default ? excludeJsonPrefixs.Where(u => !u.Equals("appsettings")) : excludeJsonPrefixs;

        // 将所有文件进行分组
        var jsonFilesGroups = SplitConfigFileNameToGroups(jsonFiles)
                                                                .Where(u => !_excludeJsonPrefixs.Contains(u.Key, StringComparer.OrdinalIgnoreCase) && !u.Any(c => runtimeJsonSuffixs.Any(z => c.EndsWith(z, StringComparison.OrdinalIgnoreCase)) || ignoreConfigurationFiles.Contains(Path.GetFileName(c), StringComparer.OrdinalIgnoreCase) || ignoreConfigurationFiles.Any(i => new Matcher().AddInclude(i).Match(Path.GetFileName(c)).HasMatches)));

        // 遍历所有配置分组
        foreach (var group in jsonFilesGroups)
        {
            // 限制查找的 json 文件组
            var limitFileNames = new[] { $"{group.Key}.json", $"{group.Key}.{envName}.json" };

            // 查找默认配置和环境配置
            var files = group.Where(u => limitFileNames.Contains(Path.GetFileName(u), StringComparer.OrdinalIgnoreCase))
                                             .OrderBy(u => Path.GetFileName(u).Length);

            // 循环加载
            foreach (var jsonFile in files)
            {
                configurationBuilder.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
            }
        }
    }

    /// <summary>
    /// 反射获取所有AppStartup的继承类，执行名称为第一个参数是<see cref="IServiceCollection"/>的方法
    /// </summary>
    /// <param name="service"></param>
    public static void ConfigureServices(this WebApplicationBuilder service)
    {
        AddStartups(service);
    }

    /// <summary>
    /// ConfigureServices获取的全部实例中，执行名称为第一个参数是<see cref="IApplicationBuilder"/>的方法
    /// </summary>
    public static void UseServices(this IApplicationBuilder builder)
    {
        UseStartups(AppStartups, builder);
    }

    /// <summary>
    /// 添加 Startup 自动扫描
    /// </summary>
    internal static void AddStartups(this WebApplicationBuilder builder)
    {
        App.Configuration = builder.Configuration;
        AddJsonFiles(builder.Configuration, builder.Environment);

        if (builder.Environment is IWebHostEnvironment webHostEnvironment)
            App.WebRootPath = webHostEnvironment.WebRootPath;
        App.ContentRootPath = builder.Environment.ContentRootPath;
        App.IsDevelopment = builder.Environment.IsDevelopment();

        // 扫描所有继承 AppStartup 的类
        var startups = App.EffectiveTypes
            .Where(u => typeof(AppStartup).IsAssignableFrom(u) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
            .OrderByDescending(u => GetStartupOrder(u));

        // 注册自定义 startup
        foreach (var type in startups)
        {
            var startup = Activator.CreateInstance(type) as AppStartup;
            AppStartups.Add(startup!);

            // 获取所有符合依赖注入格式的方法，如返回值void，且第一个参数是 IServiceCollection 类型
            var serviceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters().First().ParameterType == typeof(IServiceCollection));

            if (!serviceMethods.Any()) continue;

            // 自动安装属性调用
            foreach (var method in serviceMethods)
            {
                method.Invoke(startup, new[] { builder.Services });
            }
        }
    }

    /// <summary>
    /// 批量将自定义 AppStartup 添加到 Startup.cs 的 Configure 中
    /// </summary>
    /// <param name="startups"></param>
    /// <param name="app"></param>
    private static void UseStartups(IEnumerable<AppStartup> startups, IApplicationBuilder app)
    {
        App.RootServices = app.ApplicationServices;
        App.CacheService = app.ApplicationServices.GetRequiredService<ICacheService>();

        // 遍历所有
        foreach (var startup in startups)
        {
            var type = startup.GetType();

            // 获取所有符合依赖注入格式的方法，如返回值 void，且第一个参数是 IApplicationBuilder 类型
            var configureMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters().First().ParameterType == typeof(IApplicationBuilder));

            if (!configureMethods.Any()) continue;

            // 自动安装属性调用
            foreach (var method in configureMethods)
            {
                method.Invoke(startup, ResolveMethodParameterInstances(app, method));
            }
        }
        AppStartups.Clear();
    }

    /// <summary>
    /// 获取 Startup 排序
    /// </summary>
    /// <param name="type">排序类型</param>
    /// <returns>int</returns>
    private static int GetStartupOrder(Type type)
    {
        return !type.IsDefined(typeof(AppStartupAttribute), true) ? 0 : type.GetCustomAttribute<AppStartupAttribute>(true)!.Order;
    }

    /// <summary>
    /// 解析方法参数实例
    /// </summary>
    /// <param name="app"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static object[] ResolveMethodParameterInstances(IApplicationBuilder app, MethodInfo method)
    {
        // 获取方法所有参数
        var parameters = method.GetParameters();
        var parameterInstances = new object[parameters.Length];
        parameterInstances[0] = app;

        // 解析服务
        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            parameterInstances[i] = app.ApplicationServices.GetRequiredService(parameter.ParameterType);
        }

        return parameterInstances;
    }
}

public abstract class AppStartup
{
}

/// <summary>
/// 注册服务启动配置
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AppStartupAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="order"></param>
    public AppStartupAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }
}
