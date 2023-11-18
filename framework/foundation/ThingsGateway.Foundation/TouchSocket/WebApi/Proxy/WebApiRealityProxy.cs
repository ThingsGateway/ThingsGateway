#if NET45_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// WebApiRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class WebApiRealityProxy<T, TClient, TAttribute> : RpcRealityProxy<T, TClient, TAttribute> where TClient : IWebApiClientBase where TAttribute : WebApiAttribute
    {

    }

    /// <summary>
    /// WebApiRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class WebApiRealityProxy<T, TClient> : WebApiRealityProxy<T, TClient, WebApiAttribute> where TClient : IWebApiClientBase
    {

    }

    /// <summary>
    /// WebApiRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WebApiRealityProxy<T> : WebApiRealityProxy<T, IWebApiClientBase>
    {

    }
}

#endif
