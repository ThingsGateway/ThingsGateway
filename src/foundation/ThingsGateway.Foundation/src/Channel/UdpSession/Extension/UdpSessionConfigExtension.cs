//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// UdpSessionConfigExtension
    /// </summary>
    public static class UdpSessionConfigExtension
    {
        /// <summary>
        /// 获取一个新的Udp通道。传入默认远程服务端地址，绑定地址
        /// </summary>
        public static TgUdpSession GetUdpSessionWithIPHost(this TouchSocketConfig config, string? remoteUrl, string? bindUrl)
        {
            if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl)) throw new ArgumentNullException(nameof(IPHost));
            if (!string.IsNullOrEmpty(remoteUrl))
                config.SetRemoteIPHost(remoteUrl);
            if (!string.IsNullOrEmpty(bindUrl))
                config.SetBindIPHost(bindUrl);
            else
                config.SetBindIPHost(new IPHost(0));

            //载入配置
            TgUdpSession tgUdpSession = new TgUdpSession();
            tgUdpSession.Setup(config);
            return tgUdpSession;
        }
    }
}