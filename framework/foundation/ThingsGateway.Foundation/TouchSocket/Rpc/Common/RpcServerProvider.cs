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

using System.Reflection;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// RpcServerProvider
    /// </summary>
    public sealed class RpcServerProvider : IRpcServerProvider
    {
        private readonly IResolver m_containerProvider;
        private readonly ILog m_logger;
        private readonly RpcStore m_rpcStore;

        /// <summary>
        /// RpcServerProvider
        /// </summary>
        /// <param name="containerProvider"></param>
        /// <param name="logger"></param>
        /// <param name="rpcStore"></param>
        public RpcServerProvider(IResolver containerProvider, ILog logger, RpcStore rpcStore)
        {
            this.m_containerProvider = containerProvider;
            this.m_logger = logger;
            this.m_rpcStore = rpcStore;
        }

        /// <summary>
        /// 执行Rpc
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        public InvokeResult Execute(ICallContext callContext, object[] ps)
        {
            var invokeResult = new InvokeResult();
            try
            {
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutingAsync(callContext, ps, invokeResult)
                            .GetFalseAwaitResult();
                    }
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                var rpcServer = this.GetRpcServer(callContext);
                //调用
                switch (callContext.MethodInstance.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            callContext.MethodInstance.InvokeAsync(rpcServer, ps)
                                .GetFalseAwaitResult();
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = callContext.MethodInstance.InvokeObjectAsync(rpcServer, ps)
                                 .GetFalseAwaitResult();
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
                            .GetFalseAwaitResult();
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                invokeResult.Message = ex.InnerException != null ? "函数内部发生异常，信息：" + ex.InnerException.Message : "函数内部发生异常，信息：未知";
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex).GetFalseAwaitResult();
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
                        invokeResult = callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex).GetFalseAwaitResult();
                    }
                }
            }

            return invokeResult;
        }

        /// <summary>
        /// 异步执行Rpc
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="callContext"></param>
        /// <returns></returns>
        public async Task<InvokeResult> ExecuteAsync(ICallContext callContext, object[] ps)
        {
            var invokeResult = new InvokeResult();
            try
            {
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutingAsync(callContext, ps, invokeResult)
                            .ConfigureFalseAwait();
                    }
                }

                if (invokeResult.Status != InvokeStatus.Ready)
                {
                    return invokeResult;
                }

                var rpcServer = this.GetRpcServer(callContext);

                //调用
                switch (callContext.MethodInstance.TaskType)
                {
                    case TaskReturnType.Task:
                        {
                            await ((Task)callContext.MethodInstance.Invoke(rpcServer, ps)).ConfigureFalseAwait();
                        }
                        break;

                    case TaskReturnType.TaskObject:
                        {
                            invokeResult.Result = await callContext.MethodInstance.InvokeObjectAsync(rpcServer, ps)
                                .ConfigureFalseAwait();
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
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutedAsync(callContext, ps, invokeResult)
                            .ConfigureFalseAwait();
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                invokeResult.Status = InvokeStatus.InvocationException;
                invokeResult.Message = ex.InnerException != null ? "函数内部发生异常，信息：" + ex.InnerException.Message : "函数内部发生异常，信息：未知";
                if (callContext.MethodInstance.Filters != null)
                {
                    for (var i = 0; i < callContext.MethodInstance.Filters.Length; i++)
                    {
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex).ConfigureFalseAwait();
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
                        invokeResult = await callContext.MethodInstance.Filters[i].ExecutExceptionAsync(callContext, ps, invokeResult, ex).ConfigureFalseAwait();
                    }
                }
            }

            return invokeResult;
        }

        /// <inheritdoc/>
        public MethodInstance[] GetMethods()
        {
            return this.m_rpcStore.GetAllMethods();
        }

        private object GetRpcServer(ICallContext callContext)
        {
            try
            {
                var rpcServer = (IRpcServer)this.m_containerProvider.Resolve(callContext.MethodInstance.ServerFromType);
                if (rpcServer is ITransientRpcServer transientRpcServer)
                {
                    transientRpcServer.CallContext = callContext;
                }
                return rpcServer;
            }
            catch (Exception ex)
            {
                this.m_logger.Exception(ex);
                throw;
            }
        }
    }
}