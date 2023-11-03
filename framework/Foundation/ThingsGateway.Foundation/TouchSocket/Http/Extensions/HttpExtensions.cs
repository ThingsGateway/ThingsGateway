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
using System.Text;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http扩展辅助
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>
        /// 根据字符串获取枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetEnum<T>(string str, out T result) where T : struct
        {
            return Enum.TryParse<T>(str, out result);
        }

        #region HttpBase

        /// <summary>
        /// 添加Header参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TRequest AddHeader<TRequest>(this TRequest request, string key, string value) where TRequest : HttpBase
        {
            request.Headers.Add(key, value);
            return request;
        }

        /// <summary>
        /// 添加Header参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TRequest AddHeader<TRequest>(this TRequest request, HttpHeaders key, string value) where TRequest : HttpBase
        {
            request.Headers.Add(key, value);
            return request;
        }


        #region 设置内容

        /// <summary>
        /// 从Json
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromJson<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "application/json;charset=UTF-8");
            return request;
        }

        /// <summary>
        /// 从文本
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromText<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "text/plain;charset=UTF-8");
            return request;
        }

        /// <summary>
        /// 从Xml格式
        /// </summary>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromXML<T>(this T request, string value) where T : HttpBase
        {
            request.SetContent(Encoding.UTF8.GetBytes(value));
            request.Headers.Add(HttpHeaders.ContentType, "application/xml;charset=UTF-8");
            return request;
        }

        #endregion 设置内容

        /// <summary>
        /// 获取Body的字符串
        /// </summary>
        /// <param name="httpBase"></param>
        /// <returns></returns>
        public static string GetBody(this HttpBase httpBase)
        {
            return httpBase.TryGetContent(out var data) ? Encoding.UTF8.GetString(data) : throw new Exception("获取数据体错误。");
        }

        /// <summary>
        /// 当数据类型为multipart/form-data时，获取boundary
        /// </summary>
        /// <param name="httpBase"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetBoundary(this HttpBase httpBase)
        {
            if (httpBase.ContentType.IsNullOrEmpty())
            {
                return string.Empty;
            }
            var strs = httpBase.ContentType.Split(';');
            if (strs.Length == 2)
            {
                strs = strs[1].Split('=');
                if (strs.Length == 2)
                {
                    return strs[1].Trim();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="httpBase"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T SetContent<T>(this T httpBase, string content, Encoding encoding = null) where T : HttpBase
        {
            httpBase.SetContent(Encoding.UTF8.GetBytes(content));
            return httpBase;
        }

        /// <summary>
        /// 设置数据体长度
        /// </summary>
        /// <param name="httpBase"></param>
        /// <param name="value"></param>
        public static T SetContentLength<T>(this T httpBase, long value) where T : HttpBase
        {
            httpBase.ContentLength = value;
            return httpBase;
        }

        /// <summary>
        /// 从扩展名设置内容类型，必须以“.”开头
        /// </summary>
        /// <param name="httpBase"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static T SetContentTypeByExtension<T>(this T httpBase, string extension) where T : HttpBase
        {
            var type = HttpTools.GetContentTypeFromExtension(extension);
            httpBase.ContentType = type;
            return httpBase;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="httpBase"></param>
        /// <param name="buffer"></param>
        public static T WriteContent<T>(this T httpBase, byte[] buffer) where T : HttpBase
        {
            httpBase.WriteContent(buffer, 0, buffer.Length);
            return httpBase;
        }

        #endregion HttpBase

        #region HttpRequest

        /// <summary>
        /// 获取多文件集合。如果不存在，则返回null。
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static MultifileCollection GetMultifileCollection<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.GetBoundary().IsNullOrEmpty() ? null : new MultifileCollection(request);
        }

        /// <summary>
        /// 初始化常规的请求头。
        /// <para>包含：</para>
        /// <list type="number">
        /// <item>Connection:keep-alive</item>
        /// <item>Pragma:no-cache</item>
        /// <item>UserAgent:ThingsGateway.Foundation.Http</item>
        /// <item>Accept:*/*</item>
        /// <item>AcceptEncoding:deflate, br</item>
        /// </list>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TRequest InitHeaders<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Headers.Add(HttpHeaders.Connection, "keep-alive");
            request.Headers.Add(HttpHeaders.Pragma, "no-cache");
            request.Headers.Add(HttpHeaders.UserAgent, "ThingsGateway.Foundation.Http");
            request.Headers.Add(HttpHeaders.Accept, "*/*");
            request.Headers.Add(HttpHeaders.AcceptEncoding, "deflate, br");
            return request;
        }

        /// <summary>
        /// 添加Host请求头
        /// </summary>
        /// <param name="request"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static TRequest SetHost<TRequest>(this TRequest request, string host) where TRequest : HttpRequest
        {
            request.Headers.Add(HttpHeaders.Host, host);
            return request;
        }

        /// <summary>
        /// 添加Query参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TRequest AddQuery<TRequest>(this TRequest request, string key, string value) where TRequest : HttpRequest
        {
            request.Query.Add(key, value);
            return request;
        }

        /// <summary>
        /// 对比不包含参数的Url。其中有任意一方为null，则均返回False。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool UrlEquals<TRequest>(this TRequest request, string url) where TRequest : HttpRequest
        {
            if (string.IsNullOrEmpty(request.RelativeURL) || string.IsNullOrEmpty(url))
            {
                return false;
            }
            else
            {
                return request.RelativeURL.Equals(url, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        #region 设置函数

        /// <summary>
        /// 作为Delete访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TRequest AsDelete<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Delete;
            return request;
        }

        /// <summary>
        /// 作为Get访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TRequest AsGet<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Get;
            return request;
        }

        /// <summary>
        /// 作为指定函数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static TRequest AsMethod<TRequest>(this TRequest request, string method) where TRequest : HttpRequest
        {
            request.Method = new HttpMethod(method);
            return request;
        }

        /// <summary>
        /// 作为Post访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TRequest AsPost<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Post;
            return request;
        }

        /// <summary>
        /// 作为Put访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TRequest AsPut<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            request.Method = HttpMethod.Put;
            return request;
        }

        #endregion 设置函数

        #region 判断函数

        /// <summary>
        /// 是否作为Delete访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsDelete<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Delete;
        }

        /// <summary>
        /// 是否作为Get访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsGet<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Get;
        }

        /// <summary>
        /// 是否作为指定函数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsMethod<TRequest>(this TRequest request, string method) where TRequest : HttpRequest
        {
            return request.Method == new HttpMethod(method);
        }

        /// <summary>
        /// 是否作为Post访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsPost<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Post;
        }

        /// <summary>
        /// 是否作为Put访问
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsPut<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return request.Method == HttpMethod.Put;
        }

        #endregion 判断函数

        #region 判断属性

        /// <summary>
        /// 是否在headers中包含升级连接
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsUpgrade<TRequest>(this TRequest request) where TRequest : HttpRequest
        {
            return string.Equals(request.Headers.Get(HttpHeaders.Connection), HttpHeaders.Upgrade.GetDescription(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion 判断属性

        #endregion HttpRequest

        #region HttpResponse

        /// <summary>
        /// 设置文件类型。
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static TResponse SetContentTypeFromFileName<TResponse>(this TResponse response, string fileName) where TResponse : HttpResponse
        {
            var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName);
            response.Headers.Add(HttpHeaders.ContentDisposition, contentDisposition);
            return response;
        }

        /// <summary>
        /// 判断返回的状态码是否为成功。
        /// </summary>
        /// <param name="response"></param>
        /// <param name="status">
        /// 当不指定具体的状态码时，只要状态码在200-299之间则为<see langword="true"/>。
        /// 当指定时，状态码不仅必须要在200-299之间，还必须是指定的状态码才会返回<see langword="true"/>。
        /// </param>
        /// <returns></returns>
        public static bool IsSuccess<TResponse>(this TResponse response, int? status = default) where TResponse : HttpResponse
        {
            if (status.HasValue)
            {
                return response.StatusCode == status && response.StatusCode >= 200 && response.StatusCode < 300;
            }
            else
            {
                return response.StatusCode >= 200 && response.StatusCode < 300;
            }
        }

        /// <summary>
        /// 设置状态，并且附带时间戳。
        /// </summary>
        /// <param name="response"></param>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static TResponse SetStatus<TResponse>(this TResponse response, int status = 200, string msg = "Success") where TResponse : HttpResponse
        {
            response.StatusCode = status;
            response.StatusMessage = msg;
            response.Headers.Add(HttpHeaders.Server, $"ThingsGateway.Foundation.Http {HttpBase.ServerVersion}");
            response.Headers.Add(HttpHeaders.Date, DateTime.Now.ToGMTString("r"));
            return response;
        }

        /// <summary>
        /// 路径文件没找到
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static TResponse UrlNotFind<TResponse>(this TResponse response) where TResponse : HttpResponse
        {
            response.SetContent("<html><body><h1>404 -Not Found</h1></body></html>");
            response.SetStatus(404, "Not Found");
            response.ContentType = "text/html;charset=utf-8";
            return response;
        }

        #region FromFile

        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="response">响应</param>
        /// <param name="request">请求头，用于尝试续传，为null时则不续传。</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度（仅企业版有效）。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static HttpResponse FromFile(this HttpResponse response, string filePath, HttpRequest request, string fileName = null, int maxSpeed = 1024 * 1024 * 10, int bufferLen = 1024 * 64)
        {
            using (var reader = FilePool.GetReader(filePath))
            {
                response.SetContentTypeByExtension(Path.GetExtension(filePath));
                var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName ?? Path.GetFileName(filePath));
                response.Headers.Add(HttpHeaders.ContentDisposition, contentDisposition);
                response.Headers.Add(HttpHeaders.AcceptRanges, "bytes");

                if (response.CanWrite)
                {
                    HttpRange httpRange;
                    var range = request?.Headers.Get(HttpHeaders.Range);
                    if (string.IsNullOrEmpty(range))
                    {
                        response.SetStatus();
                        response.ContentLength = reader.FileStorage.FileInfo.Length;
                        httpRange = new HttpRange() { Start = 0, Length = reader.FileStorage.FileInfo.Length };
                    }
                    else
                    {
                        httpRange = HttpRange.GetRange(range, reader.FileStorage.FileInfo.Length);
                        if (httpRange == null)
                        {
                            response.ContentLength = reader.FileStorage.FileInfo.Length;
                            httpRange = new HttpRange() { Start = 0, Length = reader.FileStorage.FileInfo.Length };
                        }
                        else
                        {
                            response.SetContentLength(httpRange.Length)
                                .SetStatus(206, "Partial Content");
                            response.Headers.Add(HttpHeaders.ContentRange, string.Format("bytes {0}-{1}/{2}", httpRange.Start, httpRange.Length + httpRange.Start - 1, reader.FileStorage.FileInfo.Length));
                        }
                    }
                    reader.Position = httpRange.Start;
                    var surLen = httpRange.Length;
                    var flowGate = new FlowGate
                    {
                        Maximum = maxSpeed
                    };

                    using (var block = new ByteBlock(bufferLen))
                    {
                        while (surLen > 0)
                        {
                            var r = reader.Read(block.Buffer, 0, (int)Math.Min(bufferLen, surLen));
                            if (r == 0)
                            {
                                break;
                            }
                            flowGate.AddCheckWait(r);
                            response.WriteContent(block.Buffer, 0, r);
                            surLen -= r;
                        }
                    }
                }
                else
                {
                    if (reader.FileStorage.FileInfo.Length > 1024 * 1024)
                    {
                        throw new OverlengthException("当该对象不支持写入时，仅支持1Mb以内的文件。");
                    }

                    using (var byteBlock = new ByteBlock((int)reader.FileStorage.FileInfo.Length))
                    {
                        using (var block = new ByteBlock(bufferLen))
                        {
                            while (true)
                            {
                                var r = reader.Read(block.Buffer, 0, bufferLen);
                                if (r == 0)
                                {
                                    break;
                                }
                                byteBlock.Write(block.Buffer, 0, r);
                            }
                            response.SetContent(byteBlock.ToArray());
                        }
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度（仅企业版有效）。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static HttpResponse FromFile(this HttpContext context, string filePath, string fileName = null, int maxSpeed = 1024 * 1024 * 10, int bufferLen = 1024 * 64)
        {
            using (var reader = FilePool.GetReader(filePath))
            {
                context.Response.SetContentTypeByExtension(Path.GetExtension(filePath));
                var contentDisposition = "attachment;" + "filename=" + System.Web.HttpUtility.UrlEncode(fileName ?? Path.GetFileName(filePath));
                context.Response.Headers.Add(HttpHeaders.ContentDisposition, contentDisposition);
                context.Response.Headers.Add(HttpHeaders.AcceptRanges, "bytes");

                if (context.Response.CanWrite)
                {
                    HttpRange httpRange;
                    var range = context.Request?.Headers.Get(HttpHeaders.Range);
                    if (string.IsNullOrEmpty(range))
                    {
                        context.Response.SetStatus();
                        context.Response.ContentLength = reader.FileStorage.FileInfo.Length;
                        httpRange = new HttpRange() { Start = 0, Length = reader.FileStorage.FileInfo.Length };
                    }
                    else
                    {
                        httpRange = HttpRange.GetRange(range, reader.FileStorage.FileInfo.Length);
                        if (httpRange == null)
                        {
                            context.Response.ContentLength = reader.FileStorage.FileInfo.Length;
                            httpRange = new HttpRange() { Start = 0, Length = reader.FileStorage.FileInfo.Length };
                        }
                        else
                        {
                            context.Response.SetContentLength(httpRange.Length)
                                .SetStatus(206, "Partial Content");
                            context.Response.Headers.Add(HttpHeaders.ContentRange, string.Format("bytes {0}-{1}/{2}", httpRange.Start, httpRange.Length + httpRange.Start - 1, reader.FileStorage.FileInfo.Length));
                        }
                    }
                    reader.Position = httpRange.Start;
                    var surLen = httpRange.Length;
                    var flowGate = new FlowGate
                    {
                        Maximum = maxSpeed
                    };

                    using (var block = new ByteBlock(bufferLen))
                    {
                        while (surLen > 0)
                        {
                            var r = reader.Read(block.Buffer, 0, (int)Math.Min(bufferLen, surLen));
                            if (r == 0)
                            {
                                break;
                            }
                            flowGate.AddCheckWait(r);
                            context.Response.WriteContent(block.Buffer, 0, r);
                            surLen -= r;
                        }
                    }
                }
                else
                {
                    if (reader.FileStorage.FileInfo.Length > 1024 * 1024)
                    {
                        throw new OverlengthException("当该对象不支持写入时，仅支持1Mb以内的文件。");
                    }

                    using (var byteBlock = new ByteBlock((int)reader.FileStorage.FileInfo.Length))
                    {
                        using (var block = new ByteBlock(bufferLen))
                        {
                            while (true)
                            {
                                var r = reader.Read(block.Buffer, 0, bufferLen);
                                if (r == 0)
                                {
                                    break;
                                }
                                byteBlock.Write(block.Buffer, 0, r);
                            }
                            context.Response.SetContent(byteBlock.ToArray());
                        }
                    }
                }
            }
            return context.Response;
        }

        #endregion FromFile

        #region FromFileAsync

        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="response">响应</param>
        /// <param name="request">请求头，用于尝试续传，为null时则不续传。</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度（仅企业版有效）。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static Task<HttpResponse> FromFileAsync(this HttpResponse response, string filePath, HttpRequest request, string fileName = null, int maxSpeed = 1024 * 1024 * 10, int bufferLen = 1024 * 64)
        {
            return Task.Run(() =>
             {
                 return FromFile(response, filePath, request, fileName, maxSpeed, bufferLen);
             });
        }

        /// <summary>
        /// 从文件响应。
        /// <para>当response支持持续写入时，会直接回复响应。并阻塞执行，直到完成。所以在执行该方法之前，请确保已设置完成所有状态字</para>
        /// <para>当response不支持持续写入时，会填充Content，且不会响应，需要自己执行Build，并发送。</para>
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名，不设置时会获取路径文件名</param>
        /// <param name="maxSpeed">最大速度（仅企业版有效）。</param>
        /// <param name="bufferLen">读取长度。</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static Task<HttpResponse> FromFileAsync(this HttpContext context, string filePath, string fileName = null, int maxSpeed = 1024 * 1024 * 10, int bufferLen = 1024 * 64)
        {
            return Task.Run(() =>
            {
                return FromFile(context, filePath, fileName, maxSpeed, bufferLen);
            });
        }

        #endregion FromFileAsync

        #endregion HttpResponse
    }
}