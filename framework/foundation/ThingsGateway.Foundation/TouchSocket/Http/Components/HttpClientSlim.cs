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

#if !NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Core;

using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
    /// </summary>
    public class HttpClientSlim : SetupConfigObject
    {
        private readonly System.Net.Http.HttpClient m_httpClient;

        /// <summary>
        /// 这是基于<see cref="System.Net.Http.HttpClient"/>的通讯模型。
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpClientSlim(System.Net.Http.HttpClient httpClient = default)
        {
            httpClient ??= new System.Net.Http.HttpClient();
            this.m_httpClient = httpClient;
        }

        /// <summary>
        /// 通讯客户端
        /// </summary>
        public System.Net.Http.HttpClient HttpClient => this.m_httpClient;

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.m_httpClient.BaseAddress ??= config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            base.LoadConfig(config);
        }
    }
}
#endif
