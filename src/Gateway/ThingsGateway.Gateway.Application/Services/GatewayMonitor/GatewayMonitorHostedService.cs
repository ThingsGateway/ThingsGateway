//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道后台服务
/// </summary>
internal sealed class GatewayMonitorHostedService : BackgroundService, IGatewayMonitorHostedService
{
    private readonly ILogger _logger;
    /// <inheritdoc cref="AlarmHostedService"/>
    public GatewayMonitorHostedService(ILogger<GatewayMonitorHostedService> logger, IStringLocalizer<GatewayMonitorHostedService> localizer, IChannelThreadManage channelThreadManage)
    {
        _logger = logger;
        Localizer = localizer;
        ChannelThreadManage = channelThreadManage;
    }

    private IStringLocalizer Localizer { get; }


    private IChannelThreadManage ChannelThreadManage { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {

            //网关启动时，获取所有通道
            var channelRuntimes = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).Adapt<List<ChannelRuntime>>();
            var deviceRuntimes = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).Adapt<List<DeviceRuntime>>();
            var variableRuntimes = (await GlobalData.VariableService.GetAllAsync().ConfigureAwait(false)).Adapt<List<VariableRuntime>>();
            foreach (var channelRuntime in channelRuntimes)
            {
                try
                {
                    channelRuntime.Init();
                    var devRuntimes = deviceRuntimes.Where(x => x.ChannelId == channelRuntime.Id);
                    foreach (var item in devRuntimes)
                    {
                        item.Init(channelRuntime);

                        var varRuntimes = variableRuntimes.Where(x => x.DeviceId == item.Id);

                        varRuntimes.ParallelForEach(varItem =>
                        {
                            varItem.Init(item);
                        });

                    }

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Init Channel");
                }
            }

            var startCollectChannelEnable = GlobalData.StartCollectChannelEnable;
            var startBusinessChannelEnable = GlobalData.StartBusinessChannelEnable;

            var collectChannelRuntimes = channelRuntimes.Where(x => (x.Enable && x.IsCollect == true && startCollectChannelEnable));

            var businessChannelRuntimes = channelRuntimes.Where(x => (x.Enable && x.IsCollect == false && startBusinessChannelEnable));

            //根据初始冗余属性，筛选启动
            await ChannelThreadManage.RestartChannelAsync(businessChannelRuntimes).ConfigureAwait(false);
            await ChannelThreadManage.RestartChannelAsync(collectChannelRuntimes).ConfigureAwait(false);


        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Start error");
        }


    }

}
