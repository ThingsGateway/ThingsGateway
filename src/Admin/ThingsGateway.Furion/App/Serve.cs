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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Runtime.Loader;

using ThingsGateway;
using ThingsGateway.Components;
using ThingsGateway.Extension.Generic;

namespace System;

/// <summary>
/// 主机静态类
/// </summary>
[SuppressSniffer]
public static class Serve
{
    /// <summary>
    /// 获取一个空闲 Web 主机地址（端口）
    /// </summary>
    public static (string Urls, int Port, string SSL_Urls) IdleHost => GetIdleHost();

    /// <summary>
    /// 获取一个空闲 Web 主机地址（端口）
    /// </summary>
    /// <param name="host">主机地址</param>
    public static (string Urls, int Port, string SSL_Urls) GetIdleHost(string host = default)
    {
        host = string.IsNullOrWhiteSpace(host) ? "localhost" : host.Trim();

        var port = Native.GetIdlePort();
        var urls = $"http://{host}:{port}";
        var ssl_urls = $"https://{host}:{port}";
        return (urls, port, ssl_urls);
    }

    /// <summary>
    /// 静默启动排除日志分类名
    /// </summary>
    private static readonly string[] SilenceExcludesOfLogCategoryName = new string[]
    {
        "Microsoft.Hosting"
        , "Microsoft.AspNetCore"
        , "Microsoft.Extensions.Hosting"
    };

    /// <summary>
    /// 启动原生应用（WinForm/WPF）主机
    /// </summary>
    /// <param name="additional"></param>
    /// <param name="includeWeb"></param>
    /// <param name="urls"></param>
    /// <param name="args"></param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost RunNative(Action<IServiceCollection> additional = default
        , bool includeWeb = true
        , string urls = default
        , string[] args = default)
    {
        IRunOptions runOptions = includeWeb
            // 迷你 Web 主机
            ? RunOptions.Default.WithArgs(args)
                     .ConfigureServices(additional)
                     .AddComponent<ServeServiceComponent>()
                     .UseComponent<ServeApplicationComponent>()
            // 泛型主机
            : GenericRunOptions.Default.WithArgs(args)
                     .ConfigureServices(additional);

        return RunNative(runOptions, urls);
    }

    /// <summary>
    /// 启动原生应用（WinForm/WPF）主机
    /// </summary>
    /// <param name="additional"></param>
    /// <param name="includeWeb"></param>
    /// <param name="urls"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunNativeAsync(Action<IServiceCollection> additional = default
        , bool includeWeb = true
        , string urls = default
        , string[] args = default
        , CancellationToken cancellationToken = default)
    {
        IRunOptions runOptions = includeWeb
            // 迷你 Web 主机
            ? RunOptions.Default.WithArgs(args)
                     .ConfigureServices(additional)
                     .AddComponent<ServeServiceComponent>()
                     .UseComponent<ServeApplicationComponent>()
            // 泛型主机
            : GenericRunOptions.Default.WithArgs(args)
                     .ConfigureServices(additional);

        return await RunNativeAsync(runOptions, urls, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动原生应用（WinForm/WPF）主机
    /// </summary>
    /// <param name="options"></param>
    /// <param name="urls"></param>
    /// <returns></returns>
    public static IHost RunNative(IRunOptions options, string urls = default)
    {
        dynamic dynamicOptions = options;

        // 动态配置静默参数
        bool isSilence = dynamicOptions.IsSilence;
        IRunOptions runOptions = isSilence
            ? options
            : dynamicOptions.Silence(true, false);

        // 创建主机
        var host = Run(runOptions, urls);

        // 监听主机关闭
        AssemblyLoadContext.Default.Unloading += (ctx) =>
        {
            host.StopAsync();
            host.Dispose();
        };

        // 监听未知异常
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            host.StopAsync();
            host.Dispose();
        };

        return host;
    }

    /// <summary>
    /// 启动原生应用（WinForm/WPF）主机
    /// </summary>
    /// <param name="options"></param>
    /// <param name="urls"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IHost> RunNativeAsync(IRunOptions options, string urls = default, CancellationToken cancellationToken = default)
    {
        dynamic dynamicOptions = options;

        // 动态配置静默参数
        bool isSilence = dynamicOptions.IsSilence;
        IRunOptions runOptions = isSilence
            ? options
            : dynamicOptions.Silence(true, false);

        // 创建主机
        var host = await RunAsync(runOptions, urls, cancellationToken).ConfigureAwait(false);

        // 监听主机关闭
        AssemblyLoadContext.Default.Unloading += async (ctx) =>
        {
            await host.StopAsync(cancellationToken).ConfigureAwait(false);
            host.Dispose();
        };

        // 监听未知异常
        AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
        {
            await host.StopAsync(cancellationToken).ConfigureAwait(false);
            host.Dispose();
        };

        return host;
    }

    /// <summary>
    /// 启动默认 Web 主机，含最基础的 Web 注册
    /// </summary>
    /// <param name="additional">配置额外服务</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(Action<IServiceCollection> additional
        , string urls = default
        , bool silence = false
        , bool logging = false
        , string[] args = default)
    {
        return Run(urls, silence, logging, args, additional);
    }

    /// <summary>
    /// 启动默认 Web 主机，含最基础的 Web 注册
    /// </summary>
    /// <param name="additional">配置额外服务</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(Action<IServiceCollection> additional
        , string urls = default
        , bool silence = false
        , bool logging = false
        , string[] args = default
        , CancellationToken cancellationToken = default)
    {
        return await RunAsync(urls, silence, logging, args, additional, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动默认 Web 主机，含最基础的 Web 注册
    /// </summary>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="additional">配置额外服务</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(string urls = default
        , bool silence = false
        , bool logging = false
        , string[] args = default
        , Action<IServiceCollection> additional = default)
    {
        return Run(RunOptions.Default
                     .WithArgs(args)
                     .Silence(silence, logging)
                     .ConfigureServices(additional)
                     .AddComponent<ServeServiceComponent>()
                     .UseComponent<ServeApplicationComponent>(), urls);
    }

    /// <summary>
    /// 启动默认 Web 主机，含最基础的 Web 注册
    /// </summary>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="additional">配置额外服务</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(string urls = default
        , bool silence = false
        , bool logging = false
        , string[] args = default
        , Action<IServiceCollection> additional = default
        , CancellationToken cancellationToken = default)
    {
        return await RunAsync(RunOptions.Default
                     .WithArgs(args)
                     .Silence(silence, logging)
                     .ConfigureServices(additional)
                     .AddComponent<ServeServiceComponent>()
                     .UseComponent<ServeApplicationComponent>(), urls, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动主机
    /// </summary>
    /// <remarks>通用方法</remarks>
    /// <param name="options"></param>
    /// <param name="urls"></param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(IRunOptions options, string urls = default)
    {
        IHost host;
        // .NET6+ 主机
        if (options is RunOptions runOptions)
        {
            host = Run(runOptions, urls);
        }
        // .NET6- 主机
        else if (options is LegacyRunOptions legacyRunOptions)
        {
            host = Run(legacyRunOptions, urls);
        }
        // 泛型主机
        else if (options is GenericRunOptions genericRunOptions)
        {
            host = Run(genericRunOptions);
        }
        else throw new InvalidCastException("Unsupported IRunOptions implementation type.");

        return host;
    }

    /// <summary>
    /// 启动主机
    /// </summary>
    /// <remarks>通用方法</remarks>
    /// <param name="options"></param>
    /// <param name="urls"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(IRunOptions options, string urls = default, CancellationToken cancellationToken = default)
    {
        IHost host;
        // .NET6+ 主机
        if (options is RunOptions runOptions)
        {
            host = await RunAsync(runOptions, urls, cancellationToken).ConfigureAwait(false);
        }
        // .NET6- 主机
        else if (options is LegacyRunOptions legacyRunOptions)
        {
            host = await RunAsync(legacyRunOptions, urls, cancellationToken).ConfigureAwait(false);
        }
        // 泛型主机
        else if (options is GenericRunOptions genericRunOptions)
        {
            host = await RunAsync(genericRunOptions, cancellationToken).ConfigureAwait(false);
        }
        else throw new InvalidCastException("Unsupported IRunOptions implementation type.");

        return host;
    }

    /// <summary>
    /// 启动通用泛型主机
    /// </summary>
    /// <param name="additional">配置额外服务</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost RunGeneric(Action<IServiceCollection> additional
        , bool silence = false
        , bool logging = false
        , string[] args = default)
    {
        return RunGeneric(silence, logging, args, additional);
    }

    /// <summary>
    /// 启动通用泛型主机
    /// </summary>
    /// <param name="additional">配置额外服务</param>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunGenericAsync(Action<IServiceCollection> additional
        , bool silence = false
        , bool logging = false
        , string[] args = default
        , CancellationToken cancellationToken = default)
    {
        return await RunGenericAsync(silence, logging, args, additional, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动通用泛型主机
    /// </summary>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="additional">配置额外服务</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost RunGeneric(bool silence = false
        , bool logging = false
        , string[] args = default
        , Action<IServiceCollection> additional = default)
    {
        return Run(GenericRunOptions.Default
             .WithArgs(args)
             .Silence(silence, logging)
             .ConfigureServices(services =>
             {
                 // 控制台日志美化
                 services.AddConsoleFormatter();

                 // 调用自定义配置
                 additional?.Invoke(services);
             }));
    }

    /// <summary>
    /// 启动通用泛型主机
    /// </summary>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <param name="args">启动参数</param>
    /// <param name="additional">配置额外服务</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunGenericAsync(bool silence = false
        , bool logging = false
        , string[] args = default
        , Action<IServiceCollection> additional = default
        , CancellationToken cancellationToken = default)
    {
        return await RunAsync(GenericRunOptions.Default
             .WithArgs(args)
             .Silence(silence, logging)
             .ConfigureServices(services =>
             {
                 // 控制台日志美化
                 services.AddConsoleFormatter();

                 // 调用自定义配置
                 additional?.Invoke(services);
             }), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动泛型 Web 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(LegacyRunOptions options, string urls = default)
    {
        return Run<FakeStartup>(options, urls);
    }

    /// <summary>
    /// 启动泛型 Web 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(LegacyRunOptions options, string urls = default, CancellationToken cancellationToken = default)
    {
        return await RunAsync<FakeStartup>(options, urls, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动泛型 Web 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <typeparam name="TStartup">启动 Startup 类</typeparam>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run<TStartup>(LegacyRunOptions options, string urls = default)
        where TStartup : class
    {
        // 构建 IHost 对象
        BuildApplication<TStartup>(options, urls, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            app.Run();
        }
        else
        {
            app.Start();
        }

        return app;
    }

    /// <summary>
    /// 启动泛型 Web 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <typeparam name="TStartup">启动 Startup 类</typeparam>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync<TStartup>(LegacyRunOptions options, string urls = default, CancellationToken cancellationToken = default)
        where TStartup : class
    {
        // 构建 IHost 对象
        BuildApplication<TStartup>(options, urls, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            await app.RunAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await app.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        return app;
    }

    /// <summary>
    /// 启动泛型通用主机
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(GenericRunOptions options)
    {
        // 构建 IHost 对象
        BuildApplication(options, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            app.Run();
        }
        else
        {
            app.Start();
        }

        return app;
    }

    /// <summary>
    /// 启动泛型通用主机
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(GenericRunOptions options, CancellationToken cancellationToken = default)
    {
        // 构建 IHost 对象
        BuildApplication(options, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            await app.RunAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await app.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        return app;
    }

    /// <summary>
    /// 启动 WebApplication 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <returns><see cref="IHost"/></returns>
    public static IHost Run(RunOptions options, string urls = default)
    {
        // 构建 WebApplication 对象
        BuildApplication(options, urls, out var startUrls, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            // 配置启动地址和端口
            app.Run(string.IsNullOrWhiteSpace(urls) ? null : startUrls);
        }
        else
        {
            app.Start();
        }

        return app;
    }

    /// <summary>
    /// 启动 WebApplication 主机
    /// </summary>
    /// <remarks>未包含 Web 基础功能，需手动注册服务/中间件</remarks>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="IHost"/></returns>
    public static async Task<IHost> RunAsync(RunOptions options, string urls = default, CancellationToken cancellationToken = default)
    {
        // 构建 WebApplication 对象
        BuildApplication(options, urls, out var startUrls, out var app);

        // 是否静默启动
        if (!options.IsSilence)
        {
            // 配置启动地址和端口
            await app.RunAsync(string.IsNullOrWhiteSpace(urls) ? null : startUrls).ConfigureAwait(false);
        }
        else
        {
            await app.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        return app;
    }

    /// <summary>
    /// 构建 WebApplication 对象
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="startUrls">Urls地址</param>
    /// <param name="app"><see cref="WebApplication"/></param>
    public static void BuildApplication(RunOptions options, string urls, out string startUrls, out WebApplication app)
    {
        // 获取命令行参数
        var args = options.Args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();



        // 初始化 WebApplicationBuilder
        var builder = (options.Options == null
            ? WebApplication.CreateBuilder(args)
            : WebApplication.CreateBuilder(options.Options));


        // 调用自定义配置服务
        options?.FirstActionBuilder?.Invoke(builder);


        // 注册 WebApplicationBuilder 组件
        if (options.WebComponents.Count > 0)
        {
            foreach (var (componentType, opt) in options.WebComponents)
            {
                builder.AddWebComponent(componentType, opt);
            }
        }

        // 静默启动排除指定日志类名
        if (options.IsSilence && !options.SilenceLogging)
        {
            builder.Logging.AddFilter((provider, category, logLevel) =>
            {
                return !SilenceExcludesOfLogCategoryName.Any(u => category.StartsWith(u));
            });
        }

        // 添加自定义配置
        options.ActionConfigurationManager?.Invoke(builder.Environment, builder.Configuration);

        // 初始化框架
        builder.Inject(options.ActionInject);

        // 注册服应用务组件
        if (options.ServiceComponents.Count > 0)
        {
            foreach (var (componentType, opt) in options.ServiceComponents)
            {
                builder.AddComponent(componentType, opt);
            }
        }

        // 解决部分主机不能正确读取 urls 参数命令问题
        startUrls = !string.IsNullOrWhiteSpace(urls) ? urls : builder.Configuration[nameof(urls)];

        // 自定义启动端口（只有静默模式才这样做）
        if (options.IsSilence && !string.IsNullOrWhiteSpace(startUrls))
        {
            builder.WebHost.UseUrls(startUrls);
        }

        // 调用自定义配置服务
        options?.ActionServices?.Invoke(builder.Services);

        // 调用自定义配置
        options?.ActionBuilder?.Invoke(builder);

        // 构建主机
        app = builder.Build();
        InternalApp.RootServices ??= app.Services;



        var applicationPartManager = app.Services.GetService<ApplicationPartManager>();

        applicationPartManager?.ApplicationParts?.RemoveWhere(p => App.BakImageNames.Any(b => b == p.Name));
        // 配置所有 Starup Configure
        UseStartups(app);
        UseStartups(app.Services);

        // 释放内存
        App.AppStartups.Clear();

        // 注册应用中间件组件
        if (options.ApplicationComponents.Count > 0)
        {
            foreach (var (componentType, opt) in options.ApplicationComponents)
            {
                app.UseComponent(app.Environment, componentType, opt);
            }
        }

        // 调用自定义配置
        options?.ActionConfigure?.Invoke(app);
    }

    /// <summary>
    /// 构建 IHost 对象
    /// </summary>
    /// <typeparam name="TStartup">启动 Startup 类</typeparam>
    /// <param name="options">配置选项</param>
    /// <param name="urls">默认 5000/5001 端口</param>
    /// <param name="app"><see cref="IHost"/></param>
    public static void BuildApplication<TStartup>(LegacyRunOptions options, string urls, out IHost app)
        where TStartup : class
    {
        // 获取命令行参数
        var args = options.Args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();

        var builder = Host.CreateDefaultBuilder(args);

        // 静默启动排除指定日志类名
        if (options.IsSilence && !options.SilenceLogging)
        {
            builder = builder.ConfigureLogging(logging =>
            {
                logging.AddFilter((provider, category, logLevel) =>
                {
                    return !SilenceExcludesOfLogCategoryName.Any(u => category.StartsWith(u));
                });
            });
        }

        // 配置 Web 主机
        builder = builder.ConfigureWebHostDefaults(webHostBuilder =>
        {
            // 注册 IWebHostBuilder 组件
            if (options.WebComponents.Count > 0)
            {
                foreach (var (componentType, opt) in options.WebComponents)
                {
                    webHostBuilder.AddWebComponent(componentType, opt);
                }
            }

            webHostBuilder = webHostBuilder.Inject(options.ActionWebInject);

            // 配置启动地址和端口
            var startUrls = !string.IsNullOrWhiteSpace(urls) ? urls : webHostBuilder.GetSetting(nameof(urls));

            // 自定义启动端口
            if (!string.IsNullOrWhiteSpace(startUrls))
            {
                webHostBuilder = webHostBuilder.UseUrls(startUrls);
            }

            // 配置服务
            if (options.ServiceComponents.Count > 0)
            {
                webHostBuilder = webHostBuilder.ConfigureServices(services =>
                {
                    // 注册应用服务组件
                    foreach (var (componentType, opt) in options.ServiceComponents)
                    {
                        services.AddComponent(componentType, opt);
                    }
                });
            }

            // 配置中间件
            if (options.ApplicationComponents.Count > 0)
            {
                webHostBuilder = webHostBuilder.Configure((context, app) =>
                {
                    // 注册应用中间件组件
                    foreach (var (componentType, opt) in options.ApplicationComponents)
                    {
                        app.UseComponent(context.HostingEnvironment, componentType, opt);
                    }
                });
            }

            // 解决 .NET5 项目必须配置 Startup 问题
            if (typeof(TStartup) != typeof(FakeStartup))
            {
                webHostBuilder = webHostBuilder.UseStartup<TStartup>();
            }

            // 调用自定义配置
            webHostBuilder = options?.ActionWebDefaultsBuilder?.Invoke(webHostBuilder) ?? webHostBuilder;
        });

        builder = builder.ConfigureServices(services =>
        {
            // 调用自定义配置服务
            options?.ActionServices?.Invoke(services);
        });

        // 调用自定义配置
        builder = options?.ActionBuilder?.Invoke(builder) ?? builder;

        // 构建主机
        app = builder.Build();
        InternalApp.RootServices ??= app.Services;

        var applicationPartManager = app.Services.GetService<ApplicationPartManager>();

        applicationPartManager?.ApplicationParts?.RemoveWhere(p => App.BakImageNames.Any(b => b == p.Name));
        // 配置所有 Starup Configure
        UseStartups(app.Services);
        // 释放内存
        App.AppStartups.Clear();
    }

    /// <summary>
    /// 构建 IHost 对象
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="app"><see cref="IHost"/></param>
    public static void BuildApplication(GenericRunOptions options, out IHost app)
    {
        // 获取命令行参数
        var args = options.Args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();

        var builder = Host.CreateDefaultBuilder(args);

        // 静默启动排除指定日志类名
        if (options.IsSilence && !options.SilenceLogging)
        {
            builder = builder.ConfigureLogging(logging =>
            {
                logging.AddFilter((provider, category, logLevel) =>
                {
                    return !SilenceExcludesOfLogCategoryName.Any(u => category.StartsWith(u));
                });
            });
        }

        // 初始化框架
        builder = builder.Inject(options.ActionInject);

        // 配置服务
        if (options.ServiceComponents.Count > 0)
        {
            builder = builder.ConfigureServices(services =>
            {
                // 注册应用服务组件
                foreach (var (componentType, opt) in options.ServiceComponents)
                {
                    services.AddComponent(componentType, opt);
                }
            });
        }

        builder = builder.ConfigureServices(services =>
        {
            // 调用自定义配置服务
            options?.ActionServices?.Invoke(services);
        });

        // 调用自定义配置
        builder = options?.ActionBuilder?.Invoke(builder) ?? builder;

        // 构建主机
        app = builder.Build();
        InternalApp.RootServices ??= app.Services;


        var applicationPartManager = app.Services.GetService<ApplicationPartManager>();
        applicationPartManager?.ApplicationParts?.RemoveWhere(p => App.BakImageNames.Any(b => b == p.Name));

        // 配置所有 Starup Configure
        UseStartups(app.Services);
        // 释放内存
        App.AppStartups.Clear();
    }



    /// <summary>
    /// 配置 Startup 的 Configure
    /// </summary>
    /// <param name="serviceProvider">应用构建器</param>
    private static void UseStartups(IServiceProvider serviceProvider)
    {
        // 反转，处理排序
        var startups = App.AppStartups.Reverse();
        if (!startups.Any()) return;

        UseStartups(startups, serviceProvider);
    }
    /// <summary>
    /// 配置 Startup 的 Configure
    /// </summary>
    /// <param name="app">应用构建器</param>
    private static void UseStartups(IApplicationBuilder app)
    {

        // 反转，处理排序
        var startups = App.AppStartups.Reverse();
        if (!startups.Any()) return;

        // 处理【部署】二级虚拟目录
        var virtualPath = App.Settings.VirtualPath;
        if (!string.IsNullOrWhiteSpace(virtualPath) && virtualPath.StartsWith('/'))
        {
            app.Map(virtualPath, _app => UseStartups(startups, _app));
            return;
        }

        UseStartups(startups, app);
    }
    /// <summary>
    /// 批量将自定义 AppStartup 添加到 Startup.cs 的 Configure 中
    /// </summary>
    /// <param name="startups"></param>
    /// <param name="serviceProvider"></param>
    private static void UseStartups(IEnumerable<AppStartup> startups, IServiceProvider serviceProvider)
    {
        // 遍历所有
        foreach (var startup in startups)
        {
            var type = startup.GetType();

            // 获取所有符合依赖注入格式的方法，如返回值 void，且第一个参数是 IServiceProvider 类型
            var configureMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters().First().ParameterType == typeof(IServiceProvider));

            if (!configureMethods.Any()) continue;

            // 自动安装属性调用
            foreach (var method in configureMethods)
            {
                method.Invoke(startup, ResolveMethodParameterInstances(serviceProvider, method));
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

    }
    /// <summary>
    /// 解析方法参数实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static object[] ResolveMethodParameterInstances(IServiceProvider serviceProvider, MethodInfo method)
    {
        // 获取方法所有参数
        var parameters = method.GetParameters();
        var parameterInstances = new object[parameters.Length];
        parameterInstances[0] = serviceProvider;

        // 解析服务
        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            parameterInstances[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
        }

        return parameterInstances;
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