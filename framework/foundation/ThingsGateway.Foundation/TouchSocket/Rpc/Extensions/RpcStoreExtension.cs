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
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// RpcStoreExtensions
    /// </summary>
    public static class RpcStoreExtension
    {
#if !NET6_0_OR_GREATER
        /// <summary>
        /// 注册所有服务
        /// </summary>
        /// <returns>返回搜索到的服务数</returns>
        public static int RegisterAllServer(this RpcStore rpcStore)
        {
            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var t1 = assembly.GetTypes().Where(p => typeof(IRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass).ToArray();
                    types.AddRange(t1);
                }
                catch
                {
                }
            }

            foreach (var type in types)
            {
                rpcStore.RegisterServer(type);
            }
            return types.Count;
        }
#endif


        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this RpcStore rpcStore) where T : IRpcServer
#else
        public static void RegisterServer<T>(this RpcStore rpcStore) where T : IRpcServer
#endif

        {
            rpcStore.RegisterServer(typeof(T));
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="rpcStore"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer(this RpcStore rpcStore, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type providerType)
#else
        public static void RegisterServer(this RpcStore rpcStore, Type providerType)
#endif
        {
            rpcStore.RegisterServer(providerType, providerType);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFrom, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTo>(this RpcStore rpcStore) where TFrom : class, IRpcServer where TTo : TFrom
#else
        public static void RegisterServer<TFrom, TTo>(this RpcStore rpcStore) where TFrom : class, IRpcServer where TTo : TFrom
#endif

        {
            rpcStore.RegisterServer(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// 注册为单例服务
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        public static void RegisterServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFrom>(this RpcStore rpcStore, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFrom rpcServer) where TFrom : class, IRpcServer
#else
        public static void RegisterServer<TFrom>(this RpcStore rpcStore, TFrom rpcServer) where TFrom : class, IRpcServer
#endif

        {
            rpcStore.RegisterServer(typeof(TFrom), rpcServer);
        }
    }
}