// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public interface IDeviceThreadManage : IAsyncDisposable
{
    long ChannelId { get; }
    bool? IsCollectChannel { get; }
    ChannelRuntime CurrentChannel { get; }
    LoggerGroup LogMessage { get; }
    string LogPath { get; }
    IChannelThreadManage ChannelThreadManage { get; }

    Task SetLogAsync(bool enable, LogLevel? logLevel = null, bool upDataBase = true);
    Task RestartDeviceAsync(DeviceRuntime deviceRuntime, bool deleteCache);
    Task RestartDeviceAsync(IEnumerable<DeviceRuntime> deviceRuntimes, bool deleteCache);

    Task RemoveDeviceAsync(IEnumerable<long> deviceIds);
    Task RemoveDeviceAsync(long deviceId);
    Task DeviceRedundantThreadAsync(long deviceId);
}