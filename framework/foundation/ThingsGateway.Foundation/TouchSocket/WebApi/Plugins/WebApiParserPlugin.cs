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
using System.Net.Sockets;

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// WebApi解析器
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public class WebApiParserPlugin : PluginBase, IRpcParser
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiParserPlugin(IContainer container, IPluginManager pluginManager)
        {
            if (container.IsRegistered(typeof(RpcStore)))
            {
                this.RpcStore = container.Resolve<RpcStore>();
            }
            else
            {
                this.RpcStore = new RpcStore(container);
            }

            if (pluginManager is null)
            {
                throw new ArgumentNullException(nameof(pluginManager));
            }

            pluginManager.Add(nameof(IHttpPlugin.OnHttpRequest), this.OnHttpRequest);

            this.GetRouteMap = new ActionMap(true);
            this.PostRouteMap = new ActionMap(true);
            this.Converter = new StringConverter();

            this.RpcStore.AddRpcParser(this);
        }

        /// <summary>
        /// 转化器
        /// </summary>
        public StringConverter Converter { get; private set; }

        /// <summary>
        /// 配置转换器。可以实现json序列化或者xml序列化。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public WebApiParserPlugin ConfigureConverter(Action<StringConverter> action)
        {
            action.Invoke(this.Converter);
            return this;
        }

        /// <summary>
        /// 获取Get函数路由映射图
        /// </summary>
        public ActionMap GetRouteMap { get; private set; }

        /// <summary>
        /// 获取Post函数路由映射图
        /// </summary>
        public ActionMap PostRouteMap { get; private set; }

        /// <summary>
        /// 所属服务器
        /// </summary>
        public RpcStore RpcStore { get; private set; }

        private async Task OnHttpGet(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.GetRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;
                var callContext = new WebApiCallContext()
                {
                    Caller = client,
                    HttpContext = e.Context,
                    MethodInstance = methodInstance
                };
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        ps = new object[methodInstance.Parameters.Length];
                        var i = 0;
                        if (methodInstance.IncludeCallContext)
                        {
                            ps[i] = callContext;
                            i++;
                        }
                        if (e.Context.Request.Query == null)
                        {
                            for (; i < methodInstance.Parameters.Length; i++)
                            {
                                ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                            }
                        }
                        else
                        {
                            for (; i < methodInstance.Parameters.Length; i++)
                            {
                                var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                                if (!value.IsNullOrEmpty())
                                {
                                    ps[i] = this.Converter.ConvertFrom(value, methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.ToString();
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    var rpcServer = methodInstance.ServerFactory.Create(callContext, ps);
                    if (rpcServer is ITransientRpcServer transientRpcServer)
                    {
                        transientRpcServer.CallContext = callContext;
                    }
                    invokeResult = await RpcStore.ExecuteAsync(rpcServer, ps, callContext);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }
                var httpResponse = e.Context.Response;
                switch (invokeResult.Status)
                {
                    case InvokeStatus.Success:
                        {
                            httpResponse.FromJson(this.Converter.ConvertTo(invokeResult.Result)).SetStatus();
                            break;
                        }
                    case InvokeStatus.UnFound:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(404);
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(405);
                            break;
                        }
                    case InvokeStatus.InvocationException:
                    case InvokeStatus.Exception:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(422);
                            break;
                        }
                }

                using (var byteBlock = new ByteBlock())
                {
                    httpResponse.Build(byteBlock);
                    client.DefaultSend(byteBlock);
                }

                if (!e.Context.Request.KeepAlive)
                {
                    client.TryShutdown(SocketShutdown.Both);
                }
            }
            await e.InvokeNext();
        }

        private async Task OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.PostRouteMap.TryGetMethodInstance(e.Context.Request.RelativeURL, out var methodInstance))
            {
                e.Handled = true;

                var invokeResult = new InvokeResult();
                object[] ps = null;
                var callContext = new WebApiCallContext()
                {
                    Caller = client,
                    HttpContext = e.Context,
                    MethodInstance = methodInstance
                };
                if (methodInstance.IsEnable)
                {
                    try
                    {
                        int index;
                        ps = new object[methodInstance.Parameters.Length];
                        var i = 0;
                        if (methodInstance.IncludeCallContext)
                        {
                            ps[i] = callContext;
                            i++;
                            index = methodInstance.Parameters.Length - 2;
                        }
                        else
                        {
                            index = methodInstance.Parameters.Length - 1;
                        }
                        if (e.Context.Request.Query == null)
                        {
                            for (; i < methodInstance.Parameters.Length - 1; i++)
                            {
                                ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                            }
                        }
                        else
                        {
                            for (; i < methodInstance.Parameters.Length - 1; i++)
                            {
                                var value = e.Context.Request.Query.Get(methodInstance.ParameterNames[i]);
                                if (!value.IsNullOrEmpty())
                                {
                                    ps[i] = this.Converter.ConvertFrom(value, methodInstance.ParameterTypes[i]);
                                }
                                else
                                {
                                    ps[i] = methodInstance.ParameterTypes[i].GetDefault();
                                }
                            }
                        }

                        if (index >= 0)
                        {
                            var str = e.Context.Request.GetBody();
                            if (methodInstance.IncludeCallContext)
                            {
                                index++;
                            }
                            ps[index] = this.Converter.ConvertFrom(str, methodInstance.ParameterTypes[index]);
                        }
                    }
                    catch (Exception ex)
                    {
                        invokeResult.Status = InvokeStatus.Exception;
                        invokeResult.Message = ex.ToString();
                    }
                }
                else
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
                if (invokeResult.Status == InvokeStatus.Ready)
                {
                    var rpcServer = methodInstance.ServerFactory.Create(callContext, ps);
                    if (rpcServer is ITransientRpcServer transientRpcServer)
                    {
                        transientRpcServer.CallContext = callContext;
                    }
                    invokeResult = await RpcStore.ExecuteAsync(rpcServer, ps, callContext);
                }

                if (e.Context.Response.Responsed)
                {
                    return;
                }
                var httpResponse = e.Context.Response;
                switch (invokeResult.Status)
                {
                    case InvokeStatus.Success:
                        {
                            httpResponse.FromJson(this.Converter.ConvertTo(invokeResult.Result)).SetStatus();
                            break;
                        }
                    case InvokeStatus.UnFound:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(404, invokeResult.Status.ToString());
                            break;
                        }
                    case InvokeStatus.UnEnable:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(405, invokeResult.Status.ToString());
                            break;
                        }
                    case InvokeStatus.InvocationException:
                    case InvokeStatus.Exception:
                        {
                            var jsonString = this.Converter.ConvertTo(new ActionResult() { Status = invokeResult.Status, Message = invokeResult.Message });
                            httpResponse.FromJson(jsonString).SetStatus(422, invokeResult.Status.ToString());
                            break;
                        }
                }

                using (var byteBlock = new ByteBlock())
                {
                    httpResponse.Build(byteBlock);
                    client.DefaultSend(byteBlock);
                }

                if (!e.Context.Request.KeepAlive)
                {
                    client.TryShutdown(SocketShutdown.Both);
                }
            }
            await e.InvokeNext();
        }

        private Task OnHttpRequest(object sender, PluginEventArgs args)
        {
            var client = (IHttpSocketClient)sender;
            var e = (HttpContextEventArgs)args;

            if (e.Context.Request.Method == Http.HttpMethod.Get)
            {
                return this.OnHttpGet(client, e);
            }
            else if (e.Context.Request.Method == Http.HttpMethod.Post)
            {
                return this.OnHttpPost(client, e);
            }
            else
            {
                return e.InvokeNext();
            }
        }

        #region Rpc解析器

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
                {
                    var actionUrls = attribute.GetRouteUrls(methodInstance);
                    if (actionUrls != null)
                    {
                        foreach (var item in actionUrls)
                        {
                            if (attribute.Method == HttpMethodType.GET)
                            {
                                this.GetRouteMap.Add(item, methodInstance);
                            }
                            else if (attribute.Method == HttpMethodType.POST)
                            {
                                this.PostRouteMap.Add(item, methodInstance);
                            }
                        }
                    }
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<WebApiAttribute>() is WebApiAttribute attribute)
                {
                    var actionUrls = attribute.GetRouteUrls(methodInstance);
                    if (actionUrls != null)
                    {
                        foreach (var item in actionUrls)
                        {
                            if (attribute.Method == HttpMethodType.GET)
                            {
                                this.GetRouteMap.Remove(item);
                            }
                            else if (attribute.Method == HttpMethodType.POST)
                            {
                                this.PostRouteMap.Remove(item);
                            }
                        }
                    }
                }
            }
        }

        #endregion Rpc解析器
    }
}