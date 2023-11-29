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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// HttpConfigExtensions
    /// </summary>
    public static class HttpConfigExtensions
    {
        #region 创建

        /// <summary>
        /// 构建Http类客户端，并连接
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TClient BuildWithHttpClient<TClient>(this TouchSocketConfig config) where TClient : IHttpClient
        {
            var client = Activator.CreateInstance<TClient>();
            client.Setup(config);
            client.Connect();
            return client;
        }

        /// <summary>
        /// 构建Http类客户端，并连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Http.HttpClient BuildWithHttpClient(this TouchSocketConfig config)
        {
            return BuildWithHttpClient<Http.HttpClient>(config);
        }

        /// <summary>
        /// 构建Http类服务器，并启动。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TService BuildWithHttpService<TService>(this TouchSocketConfig config) where TService : IHttpService
        {
            var service = Activator.CreateInstance<TService>();
            service.Setup(config);
            service.Start();
            return service;
        }

        /// <summary>
        /// 构建Http类服务器，并启动。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HttpService BuildWithHttpService(this TouchSocketConfig config)
        {
            return BuildWithHttpService<HttpService>(config);
        }

        #endregion 创建

        /// <summary>
        /// Http代理
        /// </summary>
        public static readonly DependencyProperty<HttpProxy> HttpProxyProperty =
            DependencyProperty<HttpProxy>.Register("HttpProxy", null);

        /// <summary>
        ///设置Http代理
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetHttpProxy(this TouchSocketConfig config, HttpProxy value)
        {
            config.SetValue(HttpProxyProperty, value);
            return config;
        }
    }
}