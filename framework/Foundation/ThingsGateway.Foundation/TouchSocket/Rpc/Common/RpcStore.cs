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
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc仓库
    /// </summary>
    public class RpcStore : DisposableObject, IEnumerable<IRpcParser>
    {
        private readonly ConcurrentList<IRpcParser> m_parsers = new ConcurrentList<IRpcParser>();
        private readonly ConcurrentDictionary<Type, List<MethodInstance>> m_serverTypes = new ConcurrentDictionary<Type, List<MethodInstance>>();

        /// <summary>
        /// 实例化一个Rpc仓库。
        /// <para>需要指定<see cref="IContainer"/>容器。一般和对应的服务器、客户端共用一个容器比较好。</para>
        /// </summary>
        public RpcStore(IContainer container)
        {
            this.Container = container ?? throw new ArgumentNullException(nameof(container));
            if (!container.IsRegistered(typeof(IRpcServerFactory)))
            {
                this.Container.RegisterSingleton<IRpcServerFactory, RpcServerFactory>();
            }
        }

        /// <summary>
        /// 内置IOC容器
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public Type[] ServerTypes => this.m_serverTypes.Keys.ToArray();

        /// <summary>
        /// 执行Rpc
        /// </summary>
        /// <param name="rpcServer"></param>
        /// <param name="ps"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        public static InvokeResult Execute(IRpcServer rpcServer, object[] ps, ICallContext callContext)
        {
            var invokeResult = new InvokeResult();
            try
            {
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutingAsync(callContext, ps, invokeResult)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                //调用
                switch (callContext.MethodInstance.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            callContext.MethodInstance.InvokeAsync(rpcServer, ps)
                                .ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = callContext.MethodInstance.InvokeObjectAsync(rpcServer, ps)
                                 .ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        break;

                    default:
                    case TaskReturnType.None:
                        {
                            if (callContext.MethodInstance.HasReturn)
                            {
                                invokeResult.Result = callContext.MethodInstance.Invoke(rpcServer, ps);
                            }
                            else
                            {
                                callContext.MethodInstance.Invoke(rpcServer, ps);
                            }
                        }
                        break;
                }

                invokeResult.Status = InvokeStatus.Success;
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutedAsync(callContext, ps, invokeResult)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                invokeResult.Message = ex.InnerException != null ? $"函数内部发生异常，信息：{ex.InnerException.Message}" : "函数内部发生异常，信息：未知";
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex)
                             .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            }

            return invokeResult;
        }

        /// <summary>
        /// 异步执行Rpc
        /// </summary>
        /// <param name="rpcServer"></param>
        /// <param name="ps"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        public static async Task<InvokeResult> ExecuteAsync(IRpcServer rpcServer, object[] ps, ICallContext callContext)
        {
            var invokeResult = new InvokeResult();
            try
            {
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutingAsync(callContext, ps, invokeResult);
                    }
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                //调用
                switch (callContext.MethodInstance.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            await (Task)callContext.MethodInstance.Invoke(rpcServer, ps);
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = await callContext.MethodInstance.InvokeObjectAsync(rpcServer, ps);
                        }
                        break;

                    default:
                    case TaskReturnType.None:
                        {
                            if (callContext.MethodInstance.HasReturn)
                            {
                                invokeResult.Result = callContext.MethodInstance.Invoke(rpcServer, ps);
                            }
                            else
                            {
                                callContext.MethodInstance.Invoke(rpcServer, ps);
                            }
                        }
                        break;
                }

                invokeResult.Status = InvokeStatus.Success;
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutedAsync(callContext, ps, invokeResult);
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                invokeResult.Message = ex.InnerException != null ? $"函数内部发生异常，信息：{ex.InnerException.Message}" : "函数内部发生异常，信息：未知";
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex);
                    }
                }
            }

            return invokeResult;
        }

        /// <summary>
        /// 添加Rpc解析器
        /// </summary>
        /// <param name="parser">解析器实例</param>
        /// <param name="applyServer">是否应用已注册服务</param>
        public void AddRpcParser(IRpcParser parser, bool applyServer = true)
        {
            this.ThrowIfDisposed();
            this.m_parsers.Add(parser);
            //parser.SetRpcStore(this);
            if (applyServer)
            {
                foreach (var item in this.m_serverTypes)
                {
                    parser.OnRegisterServer(item.Value.ToArray());
                }
            }
        }

        /// <summary>
        /// 获取所有已注册的函数。
        /// </summary>
        public MethodInstance[] GetAllMethods()
        {
            var methods = new List<MethodInstance>();
            foreach (var item in this.m_serverTypes.Values)
            {
                methods.AddRange(item);
            }

            return methods.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_parsers.GetEnumerator();
        }

        /// <summary>
        /// 返回枚举对象
        /// </summary>
        /// <returns></returns>
        IEnumerator<IRpcParser> IEnumerable<IRpcParser>.GetEnumerator()
        {
            return this.m_parsers.GetEnumerator();
        }

        /// <summary>
        /// 本地获取代理
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="attrbuteTypes"></param>
        /// <returns></returns>
        public string GetProxyCodes(string @namespace, params Type[] attrbuteTypes)
        {
            var cellCodes = this.GetProxyInfo(attrbuteTypes);
            return CodeGenerator.ConvertToCode(@namespace, cellCodes);
        }

        /// <summary>
        /// 获取生成的代理
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public string GetProxyCodes<TAttribute>(string @namespace) where TAttribute : RpcAttribute
        {
            var cellCodes = this.GetProxyInfo(new Type[] { typeof(TAttribute) });
            return CodeGenerator.ConvertToCode(@namespace, cellCodes);
        }

        /// <summary>
        /// 从本地获取代理
        /// </summary>
        /// <param name="attrbuteType"></param>
        /// <returns></returns>
        public ServerCellCode[] GetProxyInfo(Type[] attrbuteType)
        {
            if (this.DisposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            var codes = new List<ServerCellCode>();

            foreach (var attrbute in attrbuteType)
            {
                foreach (var item in this.m_serverTypes.Keys)
                {
                    var serverCellCode = CodeGenerator.Generator(item, attrbute);
                    codes.Add(serverCellCode);
                }
            }
            return codes.ToArray();
        }

        /// <summary>
        /// 获取服务类型对应的服务方法。
        /// </summary>
        /// <param name="serverType"></param>
        /// <returns></returns>
        public MethodInstance[] GetServerMethodInstances(Type serverType)
        {
            return this.m_serverTypes[serverType].ToArray();
        }

        /// <summary>
        /// 移除Rpc解析器
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool RemoveRpcParser(IRpcParser parser)
        {
            return this.m_parsers.Remove(parser);
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int UnregisterServer(IRpcServer provider)
        {
            return this.UnregisterServer(provider.GetType());
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public int UnregisterServer(Type providerType)
        {
            this.ThrowIfDisposed();
            if (!typeof(IRpcServer).IsAssignableFrom(providerType))
            {
                throw new RpcException("类型不相符");
            }

            if (this.RemoveServer(providerType, out var instances))
            {
                foreach (var parser in this)
                {
                    parser.OnUnregisterServer(instances);
                }

                return instances.Length;
            }
            return 0;
        }

        /// <summary>
        /// 移除注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int UnregisterServer<T>() where T : IRpcServer
        {
            return this.UnregisterServer(typeof(T));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!this.DisposedValue)
            {
                foreach (var item in this)
                {
                    item.SafeDispose();
                }
            }

            base.Dispose(disposing);
        }

        private bool RemoveServer(Type type, out MethodInstance[] methodInstances)
        {
            foreach (var newType in this.m_serverTypes.Keys)
            {
                if (newType.FullName == type.FullName)
                {
                    this.m_serverTypes.TryRemove(newType, out var list);
                    methodInstances = list.ToArray();
                    return true;
                }
            }
            methodInstances = null;
            return false;
        }

        #region 注册

        /// <summary>
        /// 注册为单例服务
        /// </summary>
        /// <param name="serverFromType"></param>
        /// <param name="rpcServer"></param>
        /// <returns></returns>
        public void RegisterServer(Type serverFromType, IRpcServer rpcServer)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
            {
                throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
            }

            if (!serverFromType.IsAssignableFrom(rpcServer.GetType()))
            {
                throw new RpcException("实例类型必须与注册类型有继承关系。");
            }
            foreach (var item in this.m_serverTypes.Keys)
            {
                if (item.FullName == serverFromType.FullName)
                {
                    return;
                }
            }

            var methodInstances = CodeGenerator.GetMethodInstances(serverFromType, rpcServer.GetType());
            foreach (var item in methodInstances)
            {
                item.ServerFactory = this.Container.Resolve<IRpcServerFactory>() ?? throw new ArgumentNullException($"{nameof(IRpcServerFactory)}");
            }
            this.m_serverTypes.TryAdd(serverFromType, new List<MethodInstance>(methodInstances));
            this.Container.RegisterSingleton(serverFromType, rpcServer);

            foreach (var parser in this)
            {
                parser.OnRegisterServer(methodInstances);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverFromType"></param>
        /// <param name="serverToType"></param>
        /// <returns></returns>
        public void RegisterServer(Type serverFromType, Type serverToType)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
            {
                throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
            }

            if (!serverFromType.IsAssignableFrom(serverToType))
            {
                throw new RpcException("实例类型必须与注册类型有继承关系。");
            }

            foreach (var item in this.m_serverTypes.Keys)
            {
                if (item.FullName == serverFromType.FullName)
                {
                    return;
                }
            }


            if (typeof(ITransientRpcServer).IsAssignableFrom(serverFromType))
            {
                this.Container.RegisterTransient(serverFromType, serverToType);
            }
            else
            {
                this.Container.RegisterSingleton(serverFromType, serverToType);
            }
            var methodInstances = CodeGenerator.GetMethodInstances(serverFromType, serverToType);
            foreach (var item in methodInstances)
            {
                item.ServerFactory = this.Container.Resolve<IRpcServerFactory>() ?? throw new ArgumentNullException($"{nameof(IRpcServerFactory)}");
            }

            this.m_serverTypes.TryAdd(serverFromType, new List<MethodInstance>(methodInstances));

            foreach (var parser in this)
            {
                parser.OnRegisterServer(methodInstances);
            }
        }

        #endregion 注册
    }
}