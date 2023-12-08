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
    /// ITargetRpcClient
    /// </summary>
    public interface ITargetRpcClient
    {
        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        Task InvokeAsync(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey"></param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        Task<object> InvokeAsync(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey"></param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey"></param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        object Invoke(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey"></param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        void Invoke(string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types);

        /// <summary>
        /// 调用对应Id的客户端Rpc
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="targetId">客户端Id</param>
        /// <param name="invokeKey"></param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        object Invoke(Type returnType, string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types);
    }
}