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
    /// ServiceExtension
    /// </summary>
    public static class ServiceExtension
    {
        #region ITcpService

        /// <inheritdoc cref="IService.Start"/>
        public static void Start<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpService
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetListenIPHosts(iPHosts);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetListenIPHosts(iPHosts);
            }
            service.Start();
        }
        /// <inheritdoc cref="IService.StartAsync"/>
        public static async Task StartAsync<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpService
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetListenIPHosts(iPHosts);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetListenIPHosts(iPHosts);
            }
            await service.StartAsync();
        }
        #endregion ITcpService

        #region Udp

        /// <inheritdoc cref="IService.Start"/>
        public static void Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetBindIPHost(iPHost);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetBindIPHost(iPHost);
            }
            service.Start();
        }
        /// <inheritdoc cref="IService.Start"/>
        public static async Task StartAsync<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetBindIPHost(iPHost);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetBindIPHost(iPHost);
            }
            await service.StartAsync();
        }
        #endregion Udp
    }
}