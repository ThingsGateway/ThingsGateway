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

using ThingsGateway.Cache;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 清理网关业务设备缓存文件
/// </summary>
[JobDetail("job_tglitedb", Description = "清理网关业务设备缓存文件", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tgfilelog", Description = "清理网关业务设备缓存文件", RunOnStart = true)]
public class LiteDbCleanJob : IJob
{
    /// <inheritdoc/>
    public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var channelService = context.ServiceProvider.GetService<IDeviceService>();
        var data = channelService.GetCacheList().Where(a => a.PluginType == PluginTypeEnum.Business).Select(a => a.Id);
        var dir = LiteDBCacheUtil.GetFileBasePath();
        string[] dirs = Directory.GetDirectories(dir);
        foreach (var item in dirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                var id = Path.GetFileName(item).ToLong();
                if (id > 0)
                {
                    if (!data.Contains(id))
                    {
                        Directory.Delete(item, true);
                    }
                }
            }
            catch { }
        }

        return Task.CompletedTask;
    }
}