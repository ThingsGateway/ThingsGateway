//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.Logging;

using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 数据库写入器
/// </summary>
public class BackendLogDatabaseLoggingWriter : IDatabaseLoggingWriter
{
    /// <inheritdoc/>
    public void Write(LogMessage logMsg, bool flush)
    {
        var customLevel = App.GetConfig<LogLevel?>("Logging:BackendLog:LogLevel:Default") ?? LogLevel.Trace;
        if (logMsg.LogLevel >= customLevel)
        {
            var logRuntime = new BackendLog
            {
                LogLevel = logMsg.LogLevel,
                LogMessage = logMsg.State.ToString(),
                LogSource = logMsg.LogName,
                LogTime = logMsg.LogDateTime,
                Exception = logMsg.Exception?.ToString(),
            };
            WriteToQueue(logRuntime);
        }
    }

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private readonly ConcurrentQueue<BackendLog> _logQueues = new();

    private IServiceProvider _serviceProvider;

    public BackendLogDatabaseLoggingWriter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // 创建长时间运行的后台任务，并将日志消息队列中数据写入存储中
        Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    private void WriteToQueue(BackendLog logMsg)
    {
        _logQueues.Enqueue(logMsg);
    }

    /// <summary>
    /// 将日志消息写入数据库中
    /// </summary>
    private async Task ProcessQueue()
    {
        var db = DbContext.Db.CopyNew();
        var appLifetime = _serviceProvider.GetService<IHostApplicationLifetime>();
        while (!(appLifetime.ApplicationStopping.IsCancellationRequested || appLifetime.ApplicationStopped.IsCancellationRequested))
        {
            if (_logQueues.Count > 0)
            {
                await db.InsertableWithAttr(_logQueues.ToListWithDequeue()).ExecuteCommandAsync();//入库
            }
            await Task.Delay(3000);
        }
    }
}