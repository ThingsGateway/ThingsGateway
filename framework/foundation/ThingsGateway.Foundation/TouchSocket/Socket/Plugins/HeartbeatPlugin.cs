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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// HeartbeatPlugin
    /// </summary>
    public abstract class HeartbeatPlugin : PluginBase
    {
        /// <summary>
        /// 最大失败次数，默认3。
        /// </summary>
        public int MaxFailCount { get; set; } = 3;

        /// <summary>
        /// 心跳间隔。默认3秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(3);
    }

    /// <summary>
    /// HeartbeatPluginExtension
    /// </summary>
    public static class HeartbeatPluginExtension
    {
        /// <summary>
        /// 设置心跳间隔。默认3秒。
        /// </summary>
        /// <typeparam name="THeartbeatPlugin"></typeparam>
        /// <param name="heartbeatPlugin"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static THeartbeatPlugin SetTick<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, TimeSpan timeSpan)
            where THeartbeatPlugin : HeartbeatPlugin
        {
            heartbeatPlugin.Tick = timeSpan;
            return heartbeatPlugin;
        }

        /// <summary>
        /// 设置最大失败次数，默认3。
        /// </summary>
        /// <typeparam name="THeartbeatPlugin"></typeparam>
        /// <param name="heartbeatPlugin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static THeartbeatPlugin SetMaxFailCount<THeartbeatPlugin>(this THeartbeatPlugin heartbeatPlugin, int value)
             where THeartbeatPlugin : HeartbeatPlugin
        {
            heartbeatPlugin.MaxFailCount = value;
            return heartbeatPlugin;
        }
    }
}