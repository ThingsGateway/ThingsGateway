
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using Microsoft.Extensions.Configuration;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Logging;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 日志数据库写入器，实现了 IDatabaseLoggingWriter 接口
/// </summary>
public class BackendLogDatabaseLoggingWriter : IDatabaseLoggingWriter
{
    // 自定义日志级别，默认为警告级别
    private LogLevel CustomLevel = App.Configuration.GetSection("Logging:BackendLog:LogLevel:Default").Get<LogLevel?>() ?? LogLevel.Warning;

    // SqlSugar客户端实例
    private SqlSugarClient SqlSugarClient;

    /// <inheritdoc/>
    /// <summary>
    /// 异步写入日志消息到数据库
    /// </summary>
    /// <param name="logMsg">日志消息对象</param>
    /// <param name="flush">是否立即入库标志</param>
    /// <returns>异步任务</returns>
    public async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        // 判断日志级别是否达到自定义的日志级别
        if (logMsg.LogLevel >= CustomLevel)
        {
            // 创建后端日志对象
            var logRuntime = new BackendLog
            {
                LogLevel = logMsg.LogLevel,         // 日志级别
                LogMessage = logMsg.State?.ToString(), // 日志消息内容
                LogSource = logMsg.LogName,         // 日志来源
                LogTime = logMsg.LogDateTime,       // 日志时间
                Exception = logMsg.Exception?.ToString(), // 异常信息
            };

            // 将日志对象加入日志消息队列
            _logQueues.Enqueue(logRuntime);

            // 如果需要立即入库
            if (flush)
            {
                // 如果SqlSugar客户端未初始化，则进行初始化
                SqlSugarClient ??= DbContext.Db.GetConnectionScopeWithAttr<BackendLog>().CopyNew();

                // 异步执行入库操作
                await SqlSugarClient.InsertableWithAttr(_logQueues.ToListWithDequeue()).ExecuteCommandAsync();

                // 延时1秒，避免过于频繁的数据库操作
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private readonly ConcurrentQueue<BackendLog> _logQueues = new();
}
