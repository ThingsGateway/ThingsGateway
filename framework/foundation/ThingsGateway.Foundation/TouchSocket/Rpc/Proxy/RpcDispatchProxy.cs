#if NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// RpcDispatchProxy
    /// </summary>
    public abstract class RpcDispatchProxy<TClient, TAttribute> : DispatchProxy where TClient : IRpcClient where TAttribute : RpcAttribute
    {
        private readonly ConcurrentDictionary<MethodInfo, ProxyModel> m_methods = new ConcurrentDictionary<MethodInfo, ProxyModel>();
        private readonly MethodInfo m_fromResultMethod;

        /// <summary>
        /// RpcDispatchProxy
        /// </summary>
        public RpcDispatchProxy()
        {
            this.m_fromResultMethod = typeof(Task).GetMethod("FromResult");
        }

        /// <summary>
        /// 获取调用Rpc的客户端。
        /// </summary>
        public abstract TClient GetClient();

        /// <inheritdoc/>
        protected sealed override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var value = this.m_methods.GetOrAdd(targetMethod, this.AddMethod);
            var methodInstance = value.MethodInstance;
            var invokeKey = value.InvokeKey;

            var invokeOption = value.InvokeOption ? (IInvokeOption)args.Last() : InvokeOption.WaitInvoke;

            object[] ps;
            if (value.InvokeOption)
            {
                var pslist = new List<object>();

                for (var i = 0; i < args.Length; i++)
                {
                    if (i < args.Length - 1)
                    {
                        pslist.Add(args[i]);
                    }
                }

                ps = pslist.ToArray();
            }
            else
            {
                ps = args;
            }

            this.OnBefore(targetMethod, value.InvokeKey, ref ps);

            object result = default;

            switch (methodInstance.TaskType)
            {
                case TaskReturnType.Task:
                    {
                        this.GetClient().Invoke(invokeKey, invokeOption, ref ps, methodInstance.ParameterTypes);
                        result = EasyTask.CompletedTask;
                        break;
                    }
                case TaskReturnType.TaskObject:
                    {
                        var obj = this.GetClient().Invoke(methodInstance.ReturnType, invokeKey, invokeOption, ref ps, methodInstance.ParameterTypes);
                        result = value.GenericMethod.Invoke(default, obj);
                        break;
                    }
                case TaskReturnType.None:
                default:
                    {
                        if (methodInstance.HasReturn)
                        {
                            result = this.GetClient().Invoke(methodInstance.ReturnType, invokeKey, invokeOption, ref ps, methodInstance.ParameterTypes);
                        }
                        else
                        {
                            this.GetClient().Invoke(invokeKey, invokeOption, ref ps, methodInstance.ParameterTypes);
                        }
                        break;
                    }
            }
            if (methodInstance.IsByRef)
            {
                for (var i = 0; i < ps.Length; i++)
                {
                    args[i] = ps[i];
                }
            }

            this.OnAfter(targetMethod, invokeKey, ref args, ref result);

            return result;
        }


        private ProxyModel AddMethod(MethodInfo info)
        {
            var attribute = info.GetCustomAttribute<TAttribute>(true) ?? throw new Exception($"在方法{info.Name}中没有找到{typeof(TAttribute)}的特性。");
            var methodInstance = new MethodInstance(info);
            var invokeKey = attribute.GetInvokenKey(methodInstance);
            var invokeOption = false;
            if (info.GetParameters().Length > 0 && typeof(IInvokeOption).IsAssignableFrom(info.GetParameters().Last().ParameterType))
            {
                invokeOption = true;
            }
            return new ProxyModel()
            {
                InvokeKey = invokeKey,
                MethodInstance = methodInstance,
                InvokeOption = invokeOption,
                GenericMethod = methodInstance.TaskType == TaskReturnType.TaskObject ? new Method(this.m_fromResultMethod.MakeGenericMethod(methodInstance.ReturnType)) : default
            };
        }

        /// <summary>
        /// 方法调用前
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual void OnBefore(MethodInfo method, string invokeKey, ref object[] args)
        {

        }

        /// <summary>
        /// 方法调用后
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeKey"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        protected virtual void OnAfter(MethodInfo method, string invokeKey, ref object[] args, ref object result)
        {

        }
    }
}
#endif