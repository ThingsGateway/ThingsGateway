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

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// IHttpDeletePlugin
    /// </summary>
    [Obsolete("该插件已被弃用，请考虑使用“IHttpPlugin”插件代替使用。本插件将在正式版发布时直接移除。", true)]
    public interface IHttpDeletePlugin<in TClient> : IPlugin where TClient : IHttpSocketClient
    {
        /// <summary>
        /// 在收到Delete时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpDelete(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpDeletePlugin
    /// </summary>
    [Obsolete("该插件已被弃用，请考虑使用“IHttpPlugin”插件代替使用。本插件将在正式版发布时直接移除。", true)]
    public interface IHttpDeletePlugin : IHttpDeletePlugin<IHttpSocketClient>
    {
    }
}