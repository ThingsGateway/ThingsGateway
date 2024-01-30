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

using Furion.Schedule;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_tgfilelog", Description = "清理网关通道文件日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tgfilelog", Description = "清理网关通道文件日志", RunOnStart = true)]
public class FileLogCleanJob : IJob
{
    /// <inheritdoc/>
    public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var channelService = context.ServiceProvider.GetService<IChannelService>();
        var data = channelService.GetCacheList().Select(a => a.Id.ToString());
        var dir = LoggerExtension.GetLogBasePath();
        Directory.CreateDirectory(dir);
        string[] dirs = Directory.GetDirectories(dir)
.Select(a => Path.GetFileName(a))
.ToArray();
        foreach (var item in dirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                if (!data.Contains(item))
                {
                    Directory.Delete(item, true);
                }
            }
            catch { }
        }

        var debugDir = LoggerExtension.GetDebugLogBasePath();
        Directory.CreateDirectory(debugDir);
        string[] debugDirs = Directory.GetDirectories(debugDir)
    .Select(a => Path.GetFileName(a))
    .ToArray();

        ChannelConfig channelConfig = TouchSocket.Core.AppConfigBase.GetNewDefault<ChannelConfig>();
        foreach (var item in debugDirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                if (!channelConfig.ChannelDatas.Select(a => a.Id.ToString()).Contains(item))
                {
                    Directory.Delete(item, true);
                }
            }
            catch { }
        }

        return Task.CompletedTask;
    }
}