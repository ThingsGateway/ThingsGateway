﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using ThingsGateway.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILoggerFactory"/> 拓展
/// </summary>
public static class ILoggerFactoryExtensions
{
    /// <summary>
    /// 添加数据库日志记录器
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="factory">日志工厂</param>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddDatabase<TDatabaseLoggingWriter>(this ILoggerFactory factory, IServiceProvider serviceProvider, Action<DatabaseLoggerOptions> configure)
         where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        var options = new DatabaseLoggerOptions();
        configure?.Invoke(options);

        var databaseLoggerProvider = new DatabaseLoggerProvider(options);

        // 解决数据库写入器中循环引用数据库仓储问题
        if (databaseLoggerProvider._serviceScope == null)
        {
            databaseLoggerProvider.SetServiceProvider(serviceProvider, typeof(TDatabaseLoggingWriter));
        }

        // 添加数据库日志记录器提供程序
        factory.AddProvider(databaseLoggerProvider);

        return factory;
    }

    /// <summary>
    /// 添加数据库日志记录器
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="factory">日志工厂</param>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="configuraionKey">配置文件对于的 Key</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddDatabase<TDatabaseLoggingWriter>(this ILoggerFactory factory, IServiceProvider serviceProvider, string configuraionKey = default, Action<DatabaseLoggerOptions> configure = default)
         where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        return factory.AddDatabase<TDatabaseLoggingWriter>(() => configuraionKey ?? "Logging:Database", serviceProvider, configure);
    }

    /// <summary>
    /// 添加数据库日志记录器
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="factory">日志工厂</param>
    /// <param name="configuraionKey">获取配置文件对应的 Key</param>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddDatabase<TDatabaseLoggingWriter>(this ILoggerFactory factory, Func<string> configuraionKey, IServiceProvider serviceProvider, Action<DatabaseLoggerOptions> configure = default)
        where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        // 创建数据库日志记录器提供程序
        var databaseLoggerProvider = Penetrates.CreateFromConfiguration(configuraionKey, configure);

        // 解决数据库写入器中循环引用数据库仓储问题
        if (databaseLoggerProvider._serviceScope == null)
        {
            databaseLoggerProvider.SetServiceProvider(serviceProvider, typeof(TDatabaseLoggingWriter));
        }

        // 添加数据库日志记录器提供程序
        factory.AddProvider(databaseLoggerProvider);

        return factory;
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="factory">日志工厂</param>
    /// <param name="fileName">日志文件完整路径或文件名，推荐 .log 作为拓展名</param>
    /// <param name="append">追加到已存在日志文件或覆盖它们</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, string fileName, bool append = true)
    {
        // 添加文件日志记录器提供程序
        factory.AddProvider(new FileLoggerProvider(fileName ?? "application.log", append));

        return factory;
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="factory">日志工厂</param>
    /// <param name="fileName">日志文件完整路径或文件名，推荐 .log 作为拓展名</param>
    /// <param name="configure"></param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, string fileName, Action<FileLoggerOptions> configure)
    {
        var options = new FileLoggerOptions();
        configure?.Invoke(options);

        // 添加文件日志记录器提供程序
        factory.AddProvider(new FileLoggerProvider(fileName ?? "application.log", options));

        return factory;
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="factory">日志工厂</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, Action<FileLoggerOptions> configure = default)
    {
        return factory.AddFile(() => "Logging:File", configure);
    }

    /// <summary>
    /// 添加文件日志记录器
    /// </summary>
    /// <param name="factory">日志工厂</param>
    /// <param name="configuraionKey">获取配置文件对应的 Key</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="ILoggerFactory"/></returns>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, Func<string> configuraionKey, Action<FileLoggerOptions> configure = default)
    {
        // 添加文件日志记录器提供程序
        factory.AddProvider(Penetrates.CreateFromConfiguration(configuraionKey, configure));

        return factory;
    }
}
