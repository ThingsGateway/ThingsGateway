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
    /// 基于Dmtp协议的Tcp客户端接口
    /// </summary>
    public interface ITcpDmtpClient : ITcpDmtpClientBase, ITcpClient
    {
        /// <summary>
        /// 建立Tcp连接，并且执行握手。
        /// </summary>
        /// <param name="cancellationToken">可取消令箭</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        ITcpDmtpClient Connect(CancellationToken cancellationToken, int timeout = 5000);

        /// <summary>
        /// 建立Tcp连接，并且执行握手。
        /// </summary>
        /// <param name="cancellationToken">可取消令箭</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task<ITcpDmtpClient> ConnectAsync(CancellationToken cancellationToken, int timeout = 5000);
    }
}