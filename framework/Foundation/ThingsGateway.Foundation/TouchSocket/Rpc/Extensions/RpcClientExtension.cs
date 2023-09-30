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

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// RpcClientExtension
    /// </summary>
    public static class RpcClientExtension
    {
        #region RpcClient

        /// <inheritdoc cref="IRpcClient.Invoke(Type, string, IInvokeOption, object[])"/>
        public static T InvokeT<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)client.Invoke(typeof(T), invokeKey, invokeOption, parameters);
        }

        /// <inheritdoc cref="IRpcClient.Invoke(Type, string, IInvokeOption, ref object[], Type[])"/>
        public static T InvokeT<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return (T)client.Invoke(typeof(T), invokeKey, invokeOption, ref parameters, types);
        }

        /// <inheritdoc cref="IRpcClient.InvokeAsync(Type, string, IInvokeOption, object[])"/>
        public static async Task<T> InvokeTAsync<T>(this IRpcClient client, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)await client.InvokeAsync(typeof(T), invokeKey, invokeOption, parameters);
        }

        #endregion RpcClient

        #region ITargetRpcClient

        /// <inheritdoc cref="ITargetRpcClient.Invoke(Type, string, string, IInvokeOption, object[])"/>
        public static T InvokeT<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)client.Invoke(typeof(T), targetId, invokeKey, invokeOption, parameters);
        }

        /// <inheritdoc cref="ITargetRpcClient.Invoke(Type, string, string, IInvokeOption, ref object[], Type[])"/>
        public static T InvokeT<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return (T)client.Invoke(typeof(T), targetId, invokeKey, invokeOption, ref parameters, types);
        }

        /// <inheritdoc cref="ITargetRpcClient.InvokeAsync(Type, string, string, IInvokeOption, object[])"/>
        public static async Task<T> InvokeTAsync<T>(this ITargetRpcClient client, string targetId, string invokeKey, IInvokeOption invokeOption, params object[] parameters)
        {
            return (T)await client.InvokeAsync(typeof(T), targetId, invokeKey, invokeOption, parameters);
        }

        #endregion ITargetRpcClient
    }
}