// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using StackExchange.Profiling;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;

using ThingsGateway.ConfigurableOptions;
using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Collections;
using ThingsGateway.NewLife.Log;
using ThingsGateway.Reflection;
using ThingsGateway.Templates;

namespace ThingsGateway;

/// <summary>
/// 全局应用类
/// </summary>
[SuppressSniffer]
public static class App
{
    /// <summary>
    /// 私有设置，避免重复解析
    /// </summary>
    internal static AppSettingsOptions _settings;

    /// <summary>
    /// 应用全局配置
    /// </summary>
    public static AppSettingsOptions Settings => _settings ??= GetConfig<AppSettingsOptions>("AppSettings", true);

    /// <summary>
    /// 全局配置选项
    /// </summary>
    public static IConfiguration Configuration => CatchOrDefault(() => InternalApp.Configuration.Reload(), new ConfigurationBuilder().Build());

    /// <summary>
    /// 获取Web主机环境，如，是否是开发环境，生产环境等
    /// </summary>
    public static IWebHostEnvironment WebHostEnvironment => InternalApp.WebHostEnvironment;

    /// <summary>
    /// 获取泛型主机环境，如，是否是开发环境，生产环境等
    /// </summary>
    public static IHostEnvironment HostEnvironment => InternalApp.HostEnvironment;

    /// <summary>
    /// 存储根服务，可能为空
    /// </summary>
    public static IServiceProvider RootServices => InternalApp.RootServices;

    private static IStringLocalizerFactory? stringLocalizerFactory;

    /// <summary>
    /// 本地化服务工厂
    /// </summary>
    public static IStringLocalizerFactory? StringLocalizerFactory

    {
        get
        {
            if ((stringLocalizerFactory == null))
            {
                stringLocalizerFactory = RootServices?.GetService<IStringLocalizerFactory>();
            }
            return stringLocalizerFactory;
        }
    }

    [NotNull]
    private static ICache? cacheService;

    /// <summary>
    /// 当前缓存服务
    /// </summary>
    public static ICache? CacheService

    {
        get
        {
            if ((cacheService == null))
            {
                cacheService = App.GetService<ICache>();
            }
            return cacheService;
        }
    }

    /// <summary>
    /// 根据类型创建本地化服务
    /// </summary>
    public static IStringLocalizer? CreateLocalizerByType(Type resourceSource)
    {
        return resourceSource.Assembly.IsDynamic ? null : StringLocalizerFactory?.Create(resourceSource);
    }

    /// <summary>
    /// 根据名称创建本地化服务
    /// </summary>
    public static IStringLocalizer? CreateLocalizerByName(string baseName, string location)
    {
        return StringLocalizerFactory?.Create(baseName, location);
    }


    /// <summary>
    /// 判断是否是单文件环境
    /// </summary>
    public static bool SingleFileEnvironment => string.IsNullOrWhiteSpace(Assembly.GetEntryAssembly().Location);

    /// <summary>
    /// 应用有效程序集
    /// </summary>
    public static readonly List<Assembly> Assemblies;

    /// <summary>
    /// 有效程序集类型
    /// </summary>
    public static readonly List<Type> EffectiveTypes;

    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext HttpContext => CatchOrDefault(() =>
{
    var httpContextAccessor = RootServices?.GetService<IHttpContextAccessor>();
    try
    {
        return httpContextAccessor.HttpContext;
    }
    catch
    {
        return null;
    }
});

    /// <summary>
    /// 获取请求上下文用户
    /// </summary>
    /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
    public static ClaimsPrincipal User => HttpContext?.User;


    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="path">配置中对应的Key</param>
    /// <param name="loadPostConfigure"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetConfig<TOptions>(string path, bool loadPostConfigure = false)
    {
        var options = Configuration.GetSection(path).Get<TOptions>();

        // 加载默认选项配置
        if (loadPostConfigure && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
        {
            var postConfigure = typeof(TOptions).GetMethod("PostConfigure");
            if (postConfigure != null)
            {
                options ??= Activator.CreateInstance<TOptions>();
                postConfigure.Invoke(options, new object[] { options, Configuration });
            }
        }

        return options;
    }

    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptions<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptions<TOptions>>(serviceProvider ?? RootServices)?.Value;
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetService(Type type, IServiceProvider serviceProvider = default)
    {
        return serviceProvider == null ? RootServices.GetService(type) : serviceProvider.GetService(type);
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static TService GetService<TService>(IServiceProvider serviceProvider = default)
        where TService : class
    {
        return GetService(typeof(TService), serviceProvider) as TService;
    }
    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptionsMonitor<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptionsMonitor<TOptions>>(serviceProvider ?? RootServices)?.CurrentValue;
    }

    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptionsSnapshot<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        // 这里不能从根服务解析，因为是 Scoped 作用域
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptionsSnapshot<TOptions>>(serviceProvider)?.Value;
    }

    /// <summary>
    /// 获取命令行配置
    /// </summary>
    /// <param name="args"></param>
    /// <param name="switchMappings"></param>
    /// <returns></returns>
    public static CommandLineConfigurationProvider GetCommandLineConfiguration(string[] args, IDictionary<string, string> switchMappings = null)
    {
        var commandLineConfiguration = new CommandLineConfigurationProvider(args, switchMappings);
        commandLineConfiguration.Load();

        return commandLineConfiguration;
    }

    /// <summary>
    /// 获取当前线程 Id
    /// </summary>
    /// <returns></returns>
    public static int GetThreadId()
    {
        return Environment.CurrentManagedThreadId;
    }

    /// <summary>
    /// 获取当前请求 TraceId
    /// </summary>
    /// <returns></returns>
    public static string GetTraceId()
    {
        return Activity.Current?.Id ?? (InternalApp.RootServices == null ? default : HttpContext?.TraceIdentifier);
    }

    /// <summary>
    /// 获取一段代码执行耗时
    /// </summary>
    /// <param name="action">委托</param>
    /// <returns><see cref="long"/></returns>
    public static long GetExecutionTime(Action action)
    {
        // 空检查
        if (action == null) throw new ArgumentNullException(nameof(action));

        // 计算接口执行时间
        var timeOperation = Stopwatch.StartNew();
        action();
        timeOperation.Stop();
        return timeOperation.ElapsedMilliseconds;
    }

    /// <summary>
    /// 获取服务注册的生命周期类型
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static ServiceLifetime? GetServiceLifetime(Type serviceType)
    {
        var serviceDescriptor = InternalApp.InternalServices
            .FirstOrDefault(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType));

        return serviceDescriptor?.Lifetime;
    }


    /// <summary>
    /// 打印验证信息到 MiniProfiler
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="state">状态</param>
    /// <param name="message">消息</param>
    /// <param name="isError">是否为警告消息</param>
    public static void PrintToMiniProfiler(string category, string state, string message = null, bool isError = false)
    {
        if (!CanBeMiniProfiler()) return;

        // 打印消息
        var titleCaseCategory = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(category);
        var customTiming = MiniProfiler.Current?.CustomTiming(category, string.IsNullOrWhiteSpace(message) ? $"{titleCaseCategory} {state}" : message, state);
        if (customTiming == null) return;

        // 判断是否是警告消息
        if (isError) customTiming.Errored = true;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    static App()
    {

        // 加载程序集
        var assObject = GetAssemblies();
        Assemblies = assObject.Assemblies.ToList();
        ExternalAssemblies = assObject.ExternalAssemblies;
        PathOfExternalAssemblies = assObject.PathOfExternalAssemblies;

        // 获取有效的类型集合
        EffectiveTypes = Assemblies.SelectMany(GetTypes).ToList();
        RazorAssemblies = EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
&& u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), true)).Select(a => a.Assembly).Distinct().ToList();
        AppStartups = new ConcurrentBag<AppStartup>();
    }

    /// <summary>
    /// 应用所有启动配置对象
    /// </summary>
    internal static ConcurrentBag<AppStartup> AppStartups;

    /// <summary>
    /// 外部程序集
    /// </summary>
    internal static IEnumerable<Assembly> ExternalAssemblies;

    /// <summary>
    /// 外部程序集文件路径
    /// </summary>
    internal static IEnumerable<string> PathOfExternalAssemblies;

    /// <summary>
    /// 直接引用程序集中的Route Razor类，不支持单文件
    /// </summary>
    public static IEnumerable<Assembly> RazorAssemblies { get; private set; }

    public static readonly ConcurrentHashSet<String> BakImagePaths = new();
    public static readonly ConcurrentHashSet<String> BakImageNames = new();

    /// <summary>
    /// 获取应用有效程序集
    /// </summary>
    /// <returns>IEnumerable</returns>
    private static (IEnumerable<Assembly> Assemblies, IEnumerable<Assembly> ExternalAssemblies, IEnumerable<string> PathOfExternalAssemblies) GetAssemblies()
    {
        // 需排除的程序集后缀
        var excludeAssemblyNames = new string[] {
                "Database.Migrations",
                "ThingsGateway.NewLife.X"
            };

        // 读取应用配置
        var supportPackageNamePrefixs = Settings.SupportPackageNamePrefixs ?? Array.Empty<string>();

        IEnumerable<Assembly> scanAssemblies;

        // 获取入口程序集
        var entryAssembly = Assembly.GetEntryAssembly();

        // 非独立发布/非单文件发布
        if (!string.IsNullOrWhiteSpace(entryAssembly.Location))
        {
            var dependencyContext = DependencyContext.Default;

            // 读取项目程序集或 内部发布的包，或手动添加引用的dll，或配置特定的包前缀
            scanAssemblies = dependencyContext.RuntimeLibraries
               .Where(u =>
                      (u.Type == "project" && !excludeAssemblyNames.Any(j => u.Name.EndsWith(j))) ||
                      (u.Type == "package" && !excludeAssemblyNames.Any(j => u.Name.EndsWith(j)) && (
                      //(u.Name.StartsWith(nameof(ThingsGateway)) && !u.Name.Contains("Plugin")) ||
                      supportPackageNamePrefixs.Any(p => u.Name.StartsWith(p) && u.RuntimeAssemblyGroups.Count > 0))) ||
                      (Settings.EnabledReferenceAssemblyScan == true && u.Type == "reference"))    // 判断是否启用引用程序集扫描
               .Select(u => Reflect.GetAssembly(u.Name)).Where(a => a != null);
        }
        // 独立发布/单文件发布
        else
        {
            IEnumerable<Assembly> fixedSingleFileAssemblies = new[] { entryAssembly };

            // 扫描实现 ISingleFilePublish 接口的类型
            var singleFilePublishType = entryAssembly.GetTypes()
                                                .FirstOrDefault(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(ISingleFilePublish).IsAssignableFrom(u));
            if (singleFilePublishType != null)
            {
                var singleFilePublish = Activator.CreateInstance(singleFilePublishType) as ISingleFilePublish;

                // 加载用户自定义配置单文件所需程序集
                var nativeAssemblies = singleFilePublish.IncludeAssemblies();
                var loadAssemblies = singleFilePublish.IncludeAssemblyNames()
                                                .Select(u => Reflect.GetAssembly(u)).Where(a => a != null);

                fixedSingleFileAssemblies = fixedSingleFileAssemblies.Concat(nativeAssemblies)
                                                            .Concat(loadAssemblies);


            }
            else
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                // 提示没有正确配置单文件配置
                Console.WriteLine(TP.Wrapper("Deploy Console"
                    , "Single file deploy error."
                    , "##Exception## Single file deployment configuration error."
                    , "##Documentation## https://furion.net/docs/singlefile"));
                Console.ResetColor();
            }

            // 通过 AppDomain.CurrentDomain 扫描，默认为延迟加载，正常只能扫描到 此程序集 和 入口程序集（启动层）
            scanAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(ass =>
                                            // 排除 System，Microsoft，netstandard 开头的程序集
                                            !ass.FullName.StartsWith(nameof(System))
                                            && !ass.FullName.StartsWith(nameof(Microsoft))
                                            && !ass.FullName.StartsWith("netstandard"))
                                    .Concat(fixedSingleFileAssemblies)
                                    .Distinct();
        }

        IEnumerable<Assembly> externalAssemblies = Array.Empty<Assembly>();
        IEnumerable<string> pathOfExternalAssemblies = Array.Empty<string>();

        // 加载 appsettings.json 配置的外部程序集
        if (Settings.ExternalAssemblies != null && Settings.ExternalAssemblies.Length > 0)
        {
            var externalDlls = new List<string>();
            foreach (var item in Settings.ExternalAssemblies)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;

                var path = Path.Combine(AppContext.BaseDirectory, item);

                // 若以 .dll 结尾则认为是一个文件
                if (item.EndsWith(".dll"))
                {
                    if (File.Exists(path)) externalDlls.Add(path);
                }
                // 否则作为目录查找或拼接 .dll 后缀作为文件名查找
                else
                {
                    // 作为目录查找所有 .dll 文件
                    if (Directory.Exists(path))
                    {
                        externalDlls.AddRange(Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories));
                    }
                    // 拼接 .dll 后缀查找
                    else
                    {
                        var pathDll = path + ".dll";
                        if (File.Exists(pathDll)) externalDlls.Add(pathDll);
                    }
                }
            }

            // 加载外部程序集
            foreach (var assemblyFileFullPath in externalDlls)
            {
                try
                {
                    if (BakImagePaths.Contains(assemblyFileFullPath)) continue;
                    // 根据路径加载程序集
                    //var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFileFullPath);
                    var runtimeAssembliesPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
                    // 将目标程序集和运行时核心程序集一起提供给 PathAssemblyResolver
                    var assemblies = Directory.GetFiles(runtimeAssembliesPath, "*.dll");
                    var resolver = new PathAssemblyResolver(new[] { assemblyFileFullPath }.Concat(assemblies));
                    // 使用 MetadataLoadContext
                    using var metadataContext = new MetadataLoadContext(resolver);

                    var referencedAssemblies = metadataContext.LoadFromAssemblyPath(assemblyFileFullPath)?.GetReferencedAssemblies();
                    if ((referencedAssemblies?.Any(a => a.Name.StartsWith("ThingsGateway"))) != true)
                    {
                        continue;
                    }
                    var loadedAssembly = Reflect.LoadAssembly(assemblyFileFullPath);
                    if (loadedAssembly == default) continue;

                    var loadTypes = GetTypes(loadedAssembly);
                    if (!loadTypes.Any())
                    {

                        BakImagePaths.TryAdd(assemblyFileFullPath);
                        BakImageNames.TryAdd(loadedAssembly.GetName().Name);
                        continue;
                    }
                    var assembly = new[] { loadedAssembly };

                    if (scanAssemblies.Any(u => u == loadedAssembly)) continue;

                    // 合并程序集
                    scanAssemblies = scanAssemblies.Concat(assembly);
                    externalAssemblies = externalAssemblies.Concat(assembly);
                    pathOfExternalAssemblies = pathOfExternalAssemblies.Concat(new[] { assemblyFileFullPath });
                }
                catch (Exception ex)
                {
                    BakImagePaths.TryAdd(assemblyFileFullPath);
                    XTrace.Log.Warn("Load external assembly error: {0} {1} {2}", assemblyFileFullPath, Environment.NewLine, ex);
                    Console.WriteLine("Load external assembly error: {0} {1} {2}", assemblyFileFullPath, Environment.NewLine, ex);
                }
            }
        }

        // 处理排除的程序集
        if (Settings.ExcludeAssemblies != null && Settings.ExcludeAssemblies.Length > 0)
        {
            scanAssemblies = scanAssemblies.Where(ass => !Settings.ExcludeAssemblies.Contains(ass.GetName().Name, StringComparer.OrdinalIgnoreCase));
        }

        return (scanAssemblies.Distinct(), externalAssemblies.Distinct(), pathOfExternalAssemblies.Distinct());
    }

    /// <summary>
    /// 加载程序集中的所有类型
    /// </summary>
    /// <param name="ass"></param>
    /// <returns></returns>
    private static IEnumerable<Type> GetTypes(Assembly ass)
    {
        var types = Array.Empty<Type>();

        try
        {
            types = ass.GetTypes();
        }
        catch
        {
            XTrace.Log.Warn($"Error load `{ass.FullName}` assembly.");
            Console.WriteLine($"Error load `{ass.FullName}` assembly.");
        }

        return types.Where(u => u.IsPublic && !u.IsDefined(typeof(SuppressSnifferAttribute), false));
    }

    /// <summary>
    /// 判断是否启用 MiniProfiler
    /// </summary>
    /// <returns></returns>
    internal static bool CanBeMiniProfiler()
    {
        // 减少不必要的监听
        if (Settings.InjectMiniProfiler != true || HttpContext == null
            || !(HttpContext.Request.Headers.TryGetValue("request-from", out var value) && value == "swagger")) return false;

        return true;
    }

    /// <summary>
    /// 处理获取对象异常问题
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">获取对象委托</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>T</returns>
    private static T CatchOrDefault<T>(Func<T> action, T defaultValue = null)
        where T : class
    {
        try
        {
            return action();
        }
        catch
        {
            return defaultValue ?? null;
        }
    }
}
