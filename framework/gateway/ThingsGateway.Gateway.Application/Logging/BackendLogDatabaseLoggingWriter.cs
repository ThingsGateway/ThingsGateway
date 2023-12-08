#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

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
    private readonly ConcurrentQueue<BackendLog> _logQueues = new();
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="BackendLogDatabaseLoggingWriter"/>
    public BackendLogDatabaseLoggingWriter(IHostApplicationLifetime hostApplicationLifetime)
    {
        _appLifetime = hostApplicationLifetime;
        Task.Factory.StartNew(LogInsertAsync);
    }

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
            _logQueues.Enqueue(logRuntime);
        }
    }

    private async Task LogInsertAsync()
    {
        var db = DbContext.Db.CopyNew();
        while (!(_appLifetime.ApplicationStopping.IsCancellationRequested || _appLifetime.ApplicationStopped.IsCancellationRequested))
        {
            if (_logQueues.Count > 0)
            {
                try
                {
                    var data = _logQueues.ToListWithDequeue();
                    await db.InsertableWithAttr(data).ExecuteCommandAsync(_appLifetime.ApplicationStopping);//入库
                }
                catch
                {
                }
            }

            await Task.Delay(3000);
        }
    }
}