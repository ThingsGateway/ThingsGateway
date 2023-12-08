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

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// IWebSocketClosingPlugin
    /// </summary>
    public interface IWebSocketClosingPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示收到断开连接报文。如果对方直接断开连接，此方法则不会触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        Task OnWebSocketClosing(TClient client, MsgPermitEventArgs e);
    }

    /// <summary>
    /// IWebSocketClosingPlugin
    /// </summary>
    public interface IWebSocketClosingPlugin : IWebSocketClosingPlugin<IHttpClientBase>
    {
    }
}