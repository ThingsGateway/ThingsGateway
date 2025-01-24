//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace ThingsGateway.Gateway.Application;

internal sealed class ChannelThreadManage : IChannelThreadManage
{
    private ILogger _logger;
    private static IDispatchService<ChannelRuntime> channelRuntimeDispatchService;
    private static IDispatchService<ChannelRuntime> ChannelRuntimeDispatchService
    {
        get
        {
            if (channelRuntimeDispatchService == null)
                channelRuntimeDispatchService = App.GetService<IDispatchService<ChannelRuntime>>();

            return channelRuntimeDispatchService;
        }
    }

    public ChannelThreadManage()
    {
        _logger = App.RootServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger($"ChannelThreadService");
    }

    public ConcurrentDictionary<long, IDeviceThreadManage> DeviceThreadManages { get; } = new();

    #region 设备管理

    private WaitLock NewChannelLock = new();
    /// <summary>
    /// 移除指定通道
    /// </summary>
    /// <param name="channelIds">要移除的通道ID</param>
    private async Task PrivateRemoveChannelsAsync(IEnumerable<long> channelIds)
    {

        await channelIds.ParallelForEachAsync(async (channelId, token) =>
          {
              try
              {
                  if (!DeviceThreadManages.TryRemove(channelId, out var deviceThreadManage)) return;

                  await deviceThreadManage.DisposeAsync().ConfigureAwait(false);

              }
              catch (Exception ex)
              {
                  _logger.LogWarning(ex, nameof(PrivateRemoveChannelsAsync));
              }
          }, Environment.ProcessorCount).ConfigureAwait(false);

    }

    /// <summary>
    /// 移除指定通道
    /// </summary>
    /// <param name="channelId">要移除的通道ID</param>
    public async Task RemoveChannelAsync(long channelId)
    {
        try
        {
            await NewChannelLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveChannelsAsync(Enumerable.Repeat(channelId, 1)).ConfigureAwait(false);
            ChannelRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewChannelLock.Release();

        }
    }

    /// <summary>
    /// 移除指定通道
    /// </summary>
    /// <param name="channelIds">要移除的通道ID</param>
    public async Task RemoveChannelAsync(IEnumerable<long> channelIds)
    {
        try
        {
            await NewChannelLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveChannelsAsync(channelIds).ConfigureAwait(false);
            ChannelRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewChannelLock.Release();

        }
    }



    private async Task PrivateRestartChannelAsync(IEnumerable<ChannelRuntime> channelRuntimes)
    {
        await PrivateRemoveChannelsAsync(channelRuntimes.Select(a => a.Id)).ConfigureAwait(false);


        await channelRuntimes.ParallelForEachAsync(async (channelRuntime, token) =>
        {
            try
            {
                if (channelRuntime.IsCollect == true)
                {
                    if (!GlobalData.StartCollectChannelEnable)
                    {
                        return;
                    }
                }
                else
                {
                    if (!GlobalData.StartBusinessChannelEnable)
                    {
                        return;
                    }
                }

                // 检查通道是否启用
                if (channelRuntime?.Enable != true)
                    return;

                // 创建新的通道线程，并将驱动程序添加到其中
                DeviceThreadManage deviceThreadManage = new DeviceThreadManage(channelRuntime);

                DeviceThreadManages.TryAdd(deviceThreadManage.ChannelId, deviceThreadManage);

                deviceThreadManage.ChannelThreadManage = this;

                await deviceThreadManage.RestartDeviceAsync(channelRuntime.DeviceRuntimes.Select(a => a.Value), false).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, nameof(PrivateRestartChannelAsync));
            }

        }, Environment.ProcessorCount).ConfigureAwait(false);


    }

    /// <summary>
    /// 添加通道
    /// </summary>
    public async Task RestartChannelAsync(ChannelRuntime channelRuntime)
    {
        try
        {
            await NewChannelLock.WaitAsync().ConfigureAwait(false);
            await PrivateRestartChannelAsync(Enumerable.Repeat(channelRuntime, 1)).ConfigureAwait(false);
            ChannelRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewChannelLock.Release();
        }
    }

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartChannelAsync(IEnumerable<ChannelRuntime> channelRuntimes)
    {

        try
        {
            await NewChannelLock.WaitAsync().ConfigureAwait(false);
            await PrivateRestartChannelAsync(channelRuntimes).ConfigureAwait(false);
            ChannelRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewChannelLock.Release();
        }
    }
    #endregion


}
