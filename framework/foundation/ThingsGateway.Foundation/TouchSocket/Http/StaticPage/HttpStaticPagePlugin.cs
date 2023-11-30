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

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http静态内容插件
    /// </summary>
    [PluginOption(Singleton = false, NotRegister = true)]
    public class HttpStaticPagePlugin : PluginBase, IHttpPlugin
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpStaticPagePlugin()
        {
            this.FileCache = new FileCachePool();
            this.SetNavigateAction(request =>
            {
                return request.RelativeURL;
            });
        }

        /// <summary>
        /// 提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        public IContentTypeProvider ContentTypeProvider { get; set; }

        /// <summary>
        /// 静态文件缓存。
        /// </summary>
        public FileCachePool FileCache { get; private set; }

        /// <summary>
        /// 重新导航。
        /// </summary>
        public Func<HttpRequest, Task<string>> NavigateAction { get; set; }

        /// <summary>
        /// 在响应之前调用
        /// </summary>
        public Func<HttpContext, Task> ResponseAction { get; set; }

        /// <summary>
        /// 添加静态
        /// </summary>
        /// <param name="path">Static content path</param>
        /// <param name="prefix">Cache prefix (default is "/")</param>
        /// <param name="filter">Cache filter (default is "*.*")</param>
        /// <param name="timeout">Refresh cache timeout (default is 1 hour)</param>
        public void AddFolder(string path, string prefix = "/", string filter = "*.*", TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromHours(1);
            this.FileCache.InsertPath(path, prefix, filter, timeout.Value, null);
        }
        /// <inheritdoc/>
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            var url = await this.NavigateAction.Invoke(e.Context.Request);
            if (this.FileCache.Find(url, out var data))
            {
                var response = e.Context.Response;
                response.SetStatus();
                if (this.ContentTypeProvider?.TryGetContentType(url, out var result) != true)
                {
                    result = HttpTools.GetContentTypeFromExtension(url);
                }
                response.ContentType = result;
                response.SetContent(data);
                if (this.ResponseAction != null)
                {
                    await this.ResponseAction.Invoke(e.Context);
                }

                await response.AnswerAsync();
                e.Handled = true;
            }
            else
            {
                await e.InvokeNext();
            }
        }
        /// <summary>
        /// Clear static content cache
        /// </summary>
        public void ClearFolder()
        {
            this.FileCache.Clear();
        }
        /// <summary>
        /// Remove static content cache
        /// </summary>
        /// <param name="path">Static content path</param>
        public void RemoveFolder(string path)
        {
            this.FileCache.RemovePath(path);
        }

        /// <summary>
        /// 设置提供文件扩展名和MIME类型之间的映射。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetContentTypeProvider(IContentTypeProvider provider)
        {
            this.ContentTypeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            return this;
        }

        /// <summary>
        /// 设定重新导航
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetNavigateAction(Func<HttpRequest, string> func)
        {
            this.NavigateAction = (request) =>
            {
                return Task.FromResult(func(request));
            };
            return this;
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetResponseAction(Func<HttpContext, Task> func)
        {
            this.ResponseAction = func;
            return this;
        }

        /// <summary>
        /// 在响应之前调用。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public HttpStaticPagePlugin SetResponseAction(Action<HttpContext> action)
        {
            this.ResponseAction = (response) =>
            {
                action.Invoke(response);
                return EasyTask.CompletedTask;
            };
            return this;
        }
    }
}