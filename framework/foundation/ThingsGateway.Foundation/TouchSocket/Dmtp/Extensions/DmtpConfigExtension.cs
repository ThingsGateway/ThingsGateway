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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// DmtpConfigExtensions
    /// </summary>
    public static class DmtpConfigExtension
    {
        /// <summary>
        /// 设置Dmtp相关配置。
        /// </summary>
        public static readonly DependencyProperty<DmtpOption> DmtpOptionProperty =
            DependencyProperty<DmtpOption>.Register("DmtpOption", new DmtpOption());

        /// <summary>
        /// 设置Dmtp相关配置。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetDmtpOption(this TouchSocketConfig config, DmtpOption value)
        {
            config.SetValue(DmtpOptionProperty, value);
            return config;
        }

        #region 创建TcpDmtp

        /// <summary>
        /// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithTcpDmtpClient<TClient>(this TouchSocketConfig config) where TClient : ITcpDmtpClient, new()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建<see cref="TcpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpDmtpClient BuildWithTcpDmtpClient(this TouchSocketConfig config)
        {
            return BuildWithTcpDmtpClient<TcpDmtpClient>(config);
        }

        /// <summary>
        /// 构建<see cref="ITcpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithTcpDmtpService<TService>(this TouchSocketConfig config) where TService : ITcpDmtpService, new()
        {
            return config.BuildService<TService>();
        }

        /// <summary>
        /// 构建<see cref="ITcpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TcpDmtpService BuildWithTcpDmtpService(this TouchSocketConfig config)
        {
            return config.BuildService<TcpDmtpService>();
        }

        #endregion 创建TcpDmtp

        #region 创建HttpDmtp

        /// <summary>
        /// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithHttpDmtpClient<TClient>(this TouchSocketConfig config) where TClient : IHttpDmtpClient, new()
        {
            return config.BuildClient<TClient>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpClient"/>类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpDmtpClient BuildWithHttpDmtpClient(this TouchSocketConfig config)
        {
            return config.BuildClient<HttpDmtpClient>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithHttpDmtpService<TService>(this TouchSocketConfig config) where TService : IHttpDmtpService, new()
        {
            return config.BuildService<TService>();
        }

        /// <summary>
        /// 构建<see cref="IHttpDmtpService"/>类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpDmtpService BuildWithHttpDmtpService(this TouchSocketConfig config)
        {
            return config.BuildService<HttpDmtpService>();
        }

        #endregion 创建HttpDmtp
    }
}