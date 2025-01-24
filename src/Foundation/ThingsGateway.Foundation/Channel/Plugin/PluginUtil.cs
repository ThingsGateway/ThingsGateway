//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static class PluginUtil
{
    /// <inheritdoc/>
    public static Action<IPluginManager> GetDtuClientPlugin(IDtuClient dtuClient)
    {
        Action<IPluginManager> action = a => { };

        action += a =>
        {
            var plugin = a.Add<HeartbeatAndReceivePlugin>();
            plugin.Heartbeat = dtuClient.Heartbeat;
            plugin.DtuId = dtuClient.DtuId;
            plugin.HeartbeatTime = dtuClient.HeartbeatTime;
        };
        return action;
    }

    /// <inheritdoc/>
    public static Action<IPluginManager> GetDtuPlugin(IDtu dtu)
    {
        Action<IPluginManager> action = a => { };

        action += a =>
        {
            a.UseCheckClear()
    .SetCheckClearType(CheckClearType.All)
    .SetTick(TimeSpan.FromMilliseconds(dtu.CheckClearTime))
    .SetOnClose((c, t) =>
    {
        c.TryShutdown();
        c.SafeClose($"{dtu.CheckClearTime}ms Timeout");
    });
        };

        action += a =>
        {
            var plugin = a.Add<DtuPlugin>();
            plugin.Heartbeat = dtu.Heartbeat;
        };
        return action;
    }

    /// <inheritdoc/>
    public static Action<IPluginManager> GetTcpServicePlugin(ITcpService tcpService)
    {
        Action<IPluginManager> action = a => { };

        action += a =>
        {
            a.UseCheckClear()
    .SetCheckClearType(CheckClearType.All)
    .SetTick(TimeSpan.FromMilliseconds(tcpService.CheckClearTime))
    .SetOnClose((c, t) =>
    {
        c.TryShutdown();
        c.SafeClose($"{tcpService.CheckClearTime}ms Timeout");
    });
        };

        return action;
    }

    /// <inheritdoc/>
    public static Action<IPluginManager> GetTcpReconnectionPlugin(ITcpClient tcpClient)
    {
        Action<IPluginManager> action = a => { };

        action += a =>
        {
            a.UseTcpReconnection();
        };

        return action;
    }
}
