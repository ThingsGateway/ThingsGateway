// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Schedule;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("gatewayjob_log", Description = "清理网关日志", GroupName = "Log", Concurrent = false)]
[Daily(TriggerId = "trigger_gatewaylog", Description = "清理网关日志", RunOnStart = false)]
public class LogJob : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var gatewayLogOptions = App.GetOptions<GatewayLogOptions>();
        var rpcLogDaysdaysAgo = gatewayLogOptions?.RpcLogDaysAgo ?? 30;
        var backendLogdaysAgo = gatewayLogOptions?.BackendLogDaysAgo ?? 30;
        await DeleteRpcLog(rpcLogDaysdaysAgo, stoppingToken).ConfigureAwait(false);
        await DeleteBackendLog(backendLogdaysAgo, stoppingToken).ConfigureAwait(false);
        await DeleteTextLog(stoppingToken).ConfigureAwait(false);
        DeleteLocalDB(stoppingToken);
    }

    private static async Task DeleteRpcLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.GetDB<RpcLog>();
        var time = DateTime.Now.AddDays(-daysAgo);
        await db.DeleteableWithAttr<RpcLog>().Where(u => u.LogTime < time).ExecuteCommandAsync(stoppingToken).ConfigureAwait(false); // 删除操作日志
    }

    private static async Task DeleteBackendLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.GetDB<BackendLog>();
        var time = DateTime.Now.AddDays(-daysAgo);
        await db.DeleteableWithAttr<BackendLog>().Where(u => u.LogTime < time).ExecuteCommandAsync(stoppingToken).ConfigureAwait(false); // 删除操作日志
    }

    private static async Task DeleteTextLog(CancellationToken stoppingToken)
    {
        //网关通道日志以通道id命名
        var channelNames = (GlobalData.Channels.Keys).ToHashSet();
        var deviceNames = (GlobalData.Devices.Keys).ToHashSet();
        var channelBaseDir = LoggerHelper.GetChannelLogBasePath();
        Directory.CreateDirectory(channelBaseDir);
        var deviceBaseDir = LoggerHelper.GetDeviceLogBasePath();
        Directory.CreateDirectory(deviceBaseDir);

        Delete(channelBaseDir, channelNames, stoppingToken);
        Delete(deviceBaseDir, deviceNames, stoppingToken);

        //网关通道日志以通道id命名
        var rulesService = App.RootServices.GetService<IRulesService>();
        var ruleNames = (await rulesService.GetFromDBAsync(a => a.Name).ConfigureAwait(false)).ToHashSet();
        var ruleBaseDir = RulesEngineHostedService.LogDir;
        Directory.CreateDirectory(ruleBaseDir);

        Delete(ruleBaseDir, ruleNames, stoppingToken);

        //底层调试
        var debugDir = LoggerHelper.GetDebugLogBasePath();
        Directory.CreateDirectory(debugDir);
        string[] debugDirs = Directory.GetDirectories(debugDir)
    .Select(a => Path.GetFileName(a))
    .ToArray();

        foreach (var item in debugDirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            //删除文件夹
            try
            {
                Directory.Delete(PathHelper.CombinePathReplace(debugDir, item), true);
            }
            catch { }
        }
    }

    private static void Delete(string baseDir, HashSet<string> strings, CancellationToken stoppingToken)
    {
        var channelDir = Directory.GetDirectories(baseDir)
  .Select(a => Path.GetFileName(a))
  .ToArray();
        foreach (var dir in channelDir)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            //删除文件夹
            try
            {
                if (!strings.Contains(dir))
                {
                    Directory.Delete(PathHelper.CombinePathReplace(baseDir, dir), true);
                }
            }
            catch { }
        }
    }

    public void DeleteLocalDB(CancellationToken stoppingToken)
    {
        var data = (GlobalData.Devices.Keys).ToHashSet();
        var dir = CacheDBUtil.GetCacheFileBasePath();
        string[] dirs = Directory.GetDirectories(dir);
        foreach (var item in dirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            //删除文件夹
            try
            {
                var id = Path.GetFileName(item);
                if (!data.Contains(id))
                {
                    Directory.Delete(item, true);
                }
            }
            catch { }
        }
    }
}
