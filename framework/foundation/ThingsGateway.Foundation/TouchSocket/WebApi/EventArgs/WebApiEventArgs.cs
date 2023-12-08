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

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// WebApiEventArgs
    /// </summary>
    public partial class WebApiEventArgs : PluginEventArgs
    {
        /// <summary>
        /// WebApiEventArgs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public WebApiEventArgs(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;
            this.IsHttpMessage = false;
        }

        /// <summary>
        /// 是否以HttpMessage请求
        /// </summary>
        public bool IsHttpMessage { get; set; }

        /// <summary>
        /// Http请求
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Http响应
        /// </summary>
        public HttpResponse Response { get; }
    }

#if !NET45
    public partial class WebApiEventArgs
    {
        /// <summary>
        /// Http请求
        /// </summary>
        public System.Net.Http.HttpRequestMessage RequestMessage { get; }

        /// <summary>
        /// WebApiEventArgs
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="responseMessage"></param>
        public WebApiEventArgs(System.Net.Http.HttpRequestMessage requestMessage, System.Net.Http.HttpResponseMessage responseMessage)
        {
            this.RequestMessage = requestMessage;
            this.ResponseMessage = responseMessage;
            this.IsHttpMessage = true;
        }

        /// <summary>
        /// Http响应
        /// </summary>
        public System.Net.Http.HttpResponseMessage ResponseMessage { get; }
    }
#endif
}