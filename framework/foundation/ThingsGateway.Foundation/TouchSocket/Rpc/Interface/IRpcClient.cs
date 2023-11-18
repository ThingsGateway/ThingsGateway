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

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc接口
    /// </summary>
    public interface IRpcClient
    {
        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        object Invoke(Type returnType, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types);

        /// <summary>
        /// Rpc调用
        /// </summary>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types);

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        Task InvokeAsync(string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// Rpc调用
        /// <para>如果调用端为客户端，则会调用服务器Rpc服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端Rpc服务。</para>
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="invokeKey">调用键</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">Rpc调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">Rpc异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        Task<object> InvokeAsync(Type returnType, string invokeKey, IInvokeOption invokeOption, params object[] parameters);
    }
}