//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

public sealed class ChannelThreadManage : IChannelThreadManage
{
    private ILogger _logger;

    public ChannelThreadManage()
    {
        _logger = App.RootServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger($"ChannelThreadService");
    }

    public NonBlockingDictionary<long, IDeviceThreadManage> DeviceThreadManages { get; } = new();

    #region 设备管理

    private WaitLock NewChannelLock = new(nameof(ChannelThreadManage));
    /// <summary>
    /// 移除指定通道
    /// </summary>
    /// <param name="channelIds">要移除的通道ID</param>
    private async Task PrivateRemoveChannelsAsync(IList<long> channelIds)
    {
        await channelIds.ParallelForEachAsync(async (channelId, token) =>
          {
              try
              {
                  if (!DeviceThreadManages.TryRemove(channelId, out var deviceThreadManage)) return;

                  await deviceThreadManage.SafeDisposeAsync().ConfigureAwait(false);
              }
              catch (Exception ex)
              {
                  _logger.LogWarning(ex, nameof(PrivateRemoveChannelsAsync));
              }
          }).ConfigureAwait(false);
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

            await PrivateRemoveChannelsAsync([channelId]).ConfigureAwait(false);
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
    public async Task RemoveChannelAsync(IList<long> channelIds)
    {
        try
        {
            await NewChannelLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveChannelsAsync(channelIds).ConfigureAwait(false);
        }
        finally
        {
            NewChannelLock.Release();
        }
    }

    private async Task PrivateRestartChannelAsync(IList<ChannelRuntime> channelRuntimes)
    {
        await PrivateRemoveChannelsAsync(channelRuntimes.Select(a => a.Id).ToArray()).ConfigureAwait(false);

        await channelRuntimes.ParallelForEachAsync(async (channelRuntime, token) =>
        {
            try
            {
                if (App.HostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    return;
                }

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

                await deviceThreadManage.RestartDeviceAsync(channelRuntime.DeviceRuntimes.Select(a => a.Value).ToList(), false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, nameof(PrivateRestartChannelAsync));
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 添加通道
    /// </summary>
    public async Task RestartChannelAsync(ChannelRuntime channelRuntime)
    {
        try
        {
            await NewChannelLock.WaitAsync(App.HostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
            await PrivateRestartChannelAsync([channelRuntime]).ConfigureAwait(false);
        }
        finally
        {
            NewChannelLock.Release();
        }
    }

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartChannelAsync(IList<ChannelRuntime> channelRuntimes)
    {
        try
        {
            await NewChannelLock.WaitAsync(App.HostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
            await PrivateRestartChannelAsync(channelRuntimes).ConfigureAwait(false);
        }
        finally
        {
            NewChannelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await RemoveChannelAsync(DeviceThreadManages.Keys.ToList()).ConfigureAwait(false);
    }

    #endregion

}
