﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace ThingsGateway.Logging;

/// <summary>
/// 数据库日志记录器提供程序
/// </summary>
/// <remarks>https://docs.microsoft.com/zh-cn/dotnet/core/extensions/custom-logging-provider</remarks>
[ProviderAlias("Database")]
public sealed class DatabaseLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>
    /// 数据库日志写入器作用域范围
    /// </summary>
    internal IServiceScope _serviceScope;

    /// <summary>
    /// 存储多日志分类日志记录器
    /// </summary>
    private readonly ConcurrentDictionary<string, DatabaseLogger> _databaseLoggers = new();

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private readonly BlockingCollection<LogMessage> _logMessageQueue = new(12000);

    /// <summary>
    /// 数据库日志写入器
    /// </summary>
    private IDatabaseLoggingWriter _databaseLoggingWriter;

    /// <summary>
    /// 长时间运行的后台任务
    /// </summary>
    /// <remarks>实现不间断写入</remarks>
    private Task _processQueueTask;

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    private IExternalScopeProvider _scopeProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="databaseLoggerOptions">数据库日志记录器配置选项</param>
    public DatabaseLoggerProvider(DatabaseLoggerOptions databaseLoggerOptions)
    {
        LoggerOptions = databaseLoggerOptions;
    }

    /// <summary>
    /// 数据库日志记录器配置选项
    /// </summary>
    internal DatabaseLoggerOptions LoggerOptions { get; private set; }

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    internal IExternalScopeProvider ScopeProvider
    {
        get
        {
            _scopeProvider ??= new LoggerExternalScopeProvider();
            return _scopeProvider;
        }
    }

    /// <summary>
    /// 创建数据库日志记录器
    /// </summary>
    /// <param name="categoryName">日志分类名</param>
    /// <returns><see cref="ILogger"/></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _databaseLoggers.GetOrAdd(categoryName, name => new DatabaseLogger(name, this));
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    /// <remarks>控制日志消息队列</remarks>
    public void Dispose()
    {
        // 标记日志消息队列停止写入
        _logMessageQueue.CompleteAdding();

        try
        {
            // 设置 1.5秒的缓冲时间，避免还有日志消息没有完成写入数据库中
            _processQueueTask?.Wait(1500);
        }
        catch (OperationCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is OperationCanceledException) { }
        catch { }

        // 清空数据库日志记录器
        _databaseLoggers.Clear();

        // 释放数据库写入器作用域范围
        _serviceScope?.Dispose();
    }

    /// <summary>
    /// 设置作用域提供器
    /// </summary>
    /// <param name="scopeProvider"></param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// 设置服务提供器
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="databaseLoggingWriterType"></param>
    internal void SetServiceProvider(IServiceProvider serviceProvider, Type databaseLoggingWriterType)
    {
        // 解析服务作用域工厂服务
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        // 创建服务作用域
        _serviceScope = serviceScopeFactory.CreateScope();

        // 基于当前作用域创建数据库日志写入器
        _databaseLoggingWriter = (_serviceScope.ServiceProvider.GetRequiredService(databaseLoggingWriterType)! as IDatabaseLoggingWriter)!;

        // 创建长时间运行的后台任务，并将日志消息队列中数据写入存储中
        _processQueueTask = Task.Factory.StartNew(state => ((DatabaseLoggerProvider)state!).ProcessQueueAsync()
            , this, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    internal void WriteToQueue(LogMessage logMsg)
    {
        // 只有队列可持续入队才写入
        if (!_logMessageQueue.IsAddingCompleted)
        {
            try
            {
                _logMessageQueue.Add(logMsg);
                return;
            }
            catch (InvalidOperationException) { }
            catch { }
        }
    }

    /// <summary>
    /// 将日志消息写入数据库中
    /// </summary>
    /// <remarks></remarks>
    private async Task ProcessQueueAsync()
    {
        var lifetime = _serviceScope.ServiceProvider.GetService<IHostApplicationLifetime>();
        try
        {
            foreach (var logMsg in _logMessageQueue.GetConsumingEnumerable(lifetime.ApplicationStopping))
            {
                try
                {
                    // 调用数据库写入器写入数据库方法
                    await _databaseLoggingWriter.WriteAsync(logMsg, _logMessageQueue.Count == 0);
                }
                catch (Exception ex)
                {
                    // 处理数据库写入错误
                    if (LoggerOptions.HandleWriteError != null)
                    {
                        var databaseWriteError = new DatabaseWriteError(ex);
                        LoggerOptions.HandleWriteError(databaseWriteError);
                    }
                    // 这里不抛出异常，避免中断日志写入
                    else { }
                }
                finally { }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
