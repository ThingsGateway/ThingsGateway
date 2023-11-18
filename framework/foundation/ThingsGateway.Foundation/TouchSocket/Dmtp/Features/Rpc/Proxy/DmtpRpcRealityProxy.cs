#if NET45_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class DmtpRpcRealityProxy<T, TClient, TAttribute> : RpcRealityProxy<T, TClient, TAttribute> where TClient : IDmtpRpcActor where TAttribute : DmtpRpcAttribute
    {

    }

    /// <summary>
    /// DmtpRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class DmtpRpcRealityProxy<T, TClient> : DmtpRpcRealityProxy<T, TClient, DmtpRpcAttribute> where TClient : IDmtpRpcActor
    {

    }

    /// <summary>
    /// DmtpRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DmtpRpcRealityProxy<T> : DmtpRpcRealityProxy<T, IDmtpRpcActor>
    {

    }
}

#endif
