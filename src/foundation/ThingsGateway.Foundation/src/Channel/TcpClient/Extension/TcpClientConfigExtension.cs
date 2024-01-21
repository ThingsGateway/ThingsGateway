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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TcpClientConfigExtension
    /// </summary>
    public static class TcpClientConfigExtension
    {
        /// <summary>
        /// 获取一个新的Tcp客户端通道。传入远程服务端地址
        /// </summary>
        /// <returns></returns>
        public static TgTcpClient GetTcpClientWithIPHost(this TouchSocketConfig config, IPHost remoteUrl, string? bindUrl = default)
        {
            if (remoteUrl == null)
                throw new ArgumentNullException(nameof(IPHost));
            config.SetRemoteIPHost(remoteUrl);
            if (!string.IsNullOrEmpty(bindUrl))
                config.SetBindIPHost(bindUrl);
            //载入配置
            TgTcpClient tgTcpClient = new TgTcpClient();
            tgTcpClient.Setup(config);
            return tgTcpClient;
        }

        /// <summary>
        /// 获取一个新的Tcp服务端通道。传入绑定地址
        /// </summary>
        /// <returns></returns>
        public static TgTcpService GetTcpServiceWithBindIPHost(this TouchSocketConfig config, IPHost bindUrl)
        {
            if (bindUrl == null) throw new ArgumentNullException(nameof(IPHost));
            config.SetListenIPHosts(new IPHost[] { bindUrl });
            //载入配置
            TgTcpService tgTcpService = new TgTcpService();
            tgTcpService.Setup(config);
            return tgTcpService;
        }
    }
}