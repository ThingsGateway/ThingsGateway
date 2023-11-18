#if NET45_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// RpcRealityProxyBase
    /// </summary>
    public abstract class RpcRealityProxyBase<T>: RealProxy
    {
        /// <summary>
        /// RpcRealityProxyBase
        /// </summary>
        public RpcRealityProxyBase():base(typeof(T))
        {

        }

        /// <summary>
        /// 返回当前实例的透明代理。
        /// </summary>
        /// <returns></returns>
        public new T GetTransparentProxy()
        {
            return (T)base.GetTransparentProxy();
        }
    }
}

#endif
