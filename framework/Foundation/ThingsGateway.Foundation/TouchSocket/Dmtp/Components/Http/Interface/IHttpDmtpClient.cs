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

using System.Threading;
using System.Threading.Tasks;
using ThingsGateway.Foundation.Http;

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// IHttpDmtpClient
    /// </summary>
    public interface IHttpDmtpClient : IHttpClient, IHttpDmtpClientBase
    {
        /// <summary>
        /// 建立Tcp，并发送Http请求，最后完成Dmtp握手连接。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IHttpDmtpClient Connect(CancellationToken token, int timeout = 5000);

        /// <summary>
        /// 建立Tcp，并发送Http请求，最后完成Dmtp握手连接。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<IHttpDmtpClient> ConnectAsync(CancellationToken token, int timeout = 5000);
    }
}