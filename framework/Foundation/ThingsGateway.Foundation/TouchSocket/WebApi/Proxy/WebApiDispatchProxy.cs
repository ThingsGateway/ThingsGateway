#if NET6_0_OR_GREATER ||NET481_OR_GREATER

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// WebApiDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class WebApiDispatchProxy<TClient, TAttribute> : RpcDispatchProxy<TClient, TAttribute> where TClient : IWebApiClientBase where TAttribute : WebApiAttribute
    {

    }

    /// <summary>
    /// WebApiDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class WebApiDispatchProxy<TClient> : WebApiDispatchProxy<TClient, WebApiAttribute> where TClient : IWebApiClientBase
    {

    }

    /// <summary>
    /// WebApiDispatchProxy
    /// </summary>
    public abstract class WebApiDispatchProxy : WebApiDispatchProxy<IWebApiClientBase>
    {

    }
}

#endif

