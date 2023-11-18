#if NET6_0_OR_GREATER || NET481_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class DmtpRpcDispatchProxy<TClient, TAttribute> : RpcDispatchProxy<TClient, TAttribute> where TClient : IDmtpRpcActor where TAttribute : DmtpRpcAttribute
    {

    }

    /// <summary>
    /// DmtpRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class DmtpRpcDispatchProxy<TClient> : DmtpRpcDispatchProxy<TClient, DmtpRpcAttribute> where TClient : IDmtpRpcActor
    {

    }

    /// <summary>
    /// DmtpRpcDispatchProxy
    /// </summary>
    public abstract class DmtpRpcDispatchProxy : DmtpRpcDispatchProxy<IDmtpRpcActor>
    {

    }
}


#endif
