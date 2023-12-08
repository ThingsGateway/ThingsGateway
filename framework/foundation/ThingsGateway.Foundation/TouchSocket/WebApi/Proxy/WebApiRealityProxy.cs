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
