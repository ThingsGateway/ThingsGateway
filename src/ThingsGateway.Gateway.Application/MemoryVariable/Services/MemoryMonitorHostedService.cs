//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道后台服务
/// </summary>
internal sealed class MemoryMonitorHostedService : BackgroundService
{
    public ILogger Logger { get; }
    /// <inheritdoc cref="AlarmHostedService"/>
    public MemoryMonitorHostedService(ILogger<MemoryMonitorHostedService> logger)
    {
        Logger = logger;
    }




    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            GlobalData.MemoryChannelRuntime.Id = MemoryConst.MemoryChannelId;
            GlobalData.MemoryChannelRuntime.Name = MemoryConst.MemoryName;
            GlobalData.MemoryChannelRuntime.PluginName = typeof(MemoryDriver).FullName;
            GlobalData.MemoryChannelRuntime.ChannelType = ChannelTypeEnum.Other;
            GlobalData.MemoryChannelRuntime.LogLevel = TouchSocket.Core.LogLevel.Debug;
            GlobalData.MemoryChannelRuntime.Init();
            GlobalData.MemoryDeviceRuntime.Id = MemoryConst.MemoryDeviceId;
            GlobalData.MemoryDeviceRuntime.ChannelId = MemoryConst.MemoryChannelId;
            GlobalData.MemoryDeviceRuntime.Name = MemoryConst.MemoryName;
            GlobalData.MemoryDeviceRuntime.LogLevel = TouchSocket.Core.LogLevel.Debug;
            GlobalData.MemoryDeviceRuntime.Init(GlobalData.MemoryChannelRuntime);

            var variableRuntimes = App.GetService<IMemoryVariableService>().GetAllVariableRuntime();


            try
            {
                variableRuntimes.ParallelForEach(varItem => varItem.Init(GlobalData.MemoryDeviceRuntime));
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Init MemoryVariable");
            }

            await GlobalData.MemoryChannelThreadManage.RestartChannelAsync(GlobalData.MemoryChannelRuntime).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Start error");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await GlobalData.MemoryChannelThreadManage.SafeDisposeAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
