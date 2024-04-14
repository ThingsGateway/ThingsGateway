﻿
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Diagnostics;

using ThingsGateway.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// 日志构建器拓展类
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// 添加控制台默认格式化器
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddConsoleFormatter(this ILoggingBuilder builder, Action<ConsoleFormatterExtendOptions> configure = default)
    {
        configure ??= (options) => { };

        return builder.AddConsole(options => options.FormatterName = "console-format")
                      .AddConsoleFormatter<ConsoleFormatterExtend, ConsoleFormatterExtendOptions>(configure);
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <param name="fileName">日志文件完整路径或文件名，推荐 .log 作为拓展名</param>
    /// <param name="append">追加到已存在日志文件或覆盖它们</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string fileName, bool append = true)
    {
        // 注册文件日志记录器提供器
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>((serviceProvider) =>
        {
            return new FileLoggerProvider(fileName ?? "application.log", append);
        }));

        return builder;
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <param name="fileName">日志文件完整路径或文件名，推荐 .log 作为拓展名</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string fileName, Action<FileLoggerOptions> configure)
    {
        // 注册文件日志记录器提供器
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>((serviceProvider) =>
        {
            var options = new FileLoggerOptions();
            configure?.Invoke(options);

            return new FileLoggerProvider(fileName ?? "application.log", options);
        }));

        return builder;
    }

    /// <summary>
    /// 添加文件日志记录器（从配置文件中）默认 Key 为："Logging:File"
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure = default)
    {
        return builder.AddFile(() => "Logging:File", configure);
    }

    /// <summary>
    /// 添加文件日志记录器（从配置文件中）
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <param name="configuraionKey">获取配置文件对应的 Key</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Func<string> configuraionKey, Action<FileLoggerOptions> configure = default)
    {
        // 注册文件日志记录器提供器
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>((serviceProvider) =>
        {
            return Penetrates.CreateFromConfiguration(configuraionKey, configure);
        }));

        return builder;
    }

    /// <summary>
    /// 添加数据库日志记录器
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="builder">日志构建器</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddDatabase<TDatabaseLoggingWriter>(this ILoggingBuilder builder, Action<DatabaseLoggerOptions> configure)
        where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        // 注册数据库日志写入器
        builder.Services.TryAddTransient<TDatabaseLoggingWriter, TDatabaseLoggingWriter>();

        // 注册数据库日志记录器提供器
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider>((serviceProvider) =>
        {
            // 解决在 IDatabaseLoggingWriter 实现类直接注册仓储导致死循环的问题
            var stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();

            if (frames.Any(u => u.HasMethod() && u.GetMethod()!.Name == "ResolveDbContext")
            || frames.Count(u => u.HasMethod() && u.GetMethod()!.Name.StartsWith("<AddDatabase>")) > 1)
            {
                return new EmptyLoggerProvider();
            }

            var options = new DatabaseLoggerOptions();
            configure?.Invoke(options);

            // 数据库日志记录器提供程序
            var databaseLoggerProvider = new DatabaseLoggerProvider(options);
            databaseLoggerProvider.SetServiceProvider(serviceProvider, typeof(TDatabaseLoggingWriter));

            return databaseLoggerProvider;
        }));

        return builder;
    }

    /// <summary>
    /// 添加数据库日志记录器
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="builder">日志构建器</param>
    /// <param name="configuraionKey">配置文件对于的 Key</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddDatabase<TDatabaseLoggingWriter>(this ILoggingBuilder builder, string configuraionKey = default, Action<DatabaseLoggerOptions> configure = default)
        where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        return builder.AddDatabase<TDatabaseLoggingWriter>(() => configuraionKey ?? "Logging:Database", configure);
    }

    /// <summary>
    /// 添加数据库日志记录器（从配置文件中）
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="builder">日志构建器</param>
    /// <param name="configuraionKey">获取配置文件对于的 Key</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggingBuilder"/></returns>
    public static ILoggingBuilder AddDatabase<TDatabaseLoggingWriter>(this ILoggingBuilder builder, Func<string> configuraionKey, Action<DatabaseLoggerOptions> configure = default)
        where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        // 注册数据库日志写入器
        builder.Services.TryAddTransient<TDatabaseLoggingWriter, TDatabaseLoggingWriter>();

        // 注册数据库日志记录器提供器
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider>((serviceProvider) =>
        {
            // 解决在 IDatabaseLoggingWriter 实现类直接注册仓储导致死循环的问题
            var stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();

            if (frames.Any(u => u.HasMethod() && u.GetMethod()!.Name == "ResolveDbContext")
            || frames.Count(u => u.HasMethod() && u.GetMethod()!.Name.StartsWith("<AddDatabase>")) > 1)
            {
                return new EmptyLoggerProvider();
            }

            // 创建数据库日志记录器提供程序
            var databaseLoggerProvider = Penetrates.CreateFromConfiguration(configuraionKey, configure);
            databaseLoggerProvider.SetServiceProvider(serviceProvider, typeof(TDatabaseLoggingWriter));

            return databaseLoggerProvider;
        }));

        return builder;
    }
}