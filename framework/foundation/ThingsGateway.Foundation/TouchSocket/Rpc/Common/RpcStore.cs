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

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc仓库
    /// </summary>
    public sealed class RpcStore
    {
        private readonly IRegistrator m_registrator;
        private readonly ConcurrentDictionary<Type, List<MethodInstance>> m_serverTypes = new ConcurrentDictionary<Type, List<MethodInstance>>();

        /// <summary>
        /// 实例化一个Rpc仓库。
        /// </summary>
        public RpcStore(IRegistrator registrator)
        {
            registrator.Register(new DependencyDescriptor(typeof(IRpcServerProvider), typeof(RpcServerProvider), Lifetime.Singleton)
            {
                ImplementationFactory = (provider) => new RpcServerProvider(
                    provider.Resolve<IResolver>(),
                    provider.Resolve<ILog>(),
                    provider.Resolve<RpcStore>())
            });
            this.m_registrator = registrator;
        }

        /// <summary>
        /// 服务类型
        /// </summary>
        public Type[] ServerTypes => this.m_serverTypes.Keys.ToArray();

        /// <summary>
        /// 获取所有已注册的函数。
        /// </summary>
        public MethodInstance[] GetAllMethods()
        {
            var methods = new List<MethodInstance>();
            foreach (var item in this.m_serverTypes.Values)
            {
                methods.AddRange(item);
            }

            return methods.ToArray();
        }

        /// <summary>
        /// 本地获取代理
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="attrbuteTypes"></param>
        /// <returns></returns>
        public string GetProxyCodes(string @namespace, params Type[] attrbuteTypes)
        {
            var cellCodes = this.GetProxyInfo(attrbuteTypes);
            return CodeGenerator.ConvertToCode(@namespace, cellCodes);
        }

        /// <summary>
        /// 获取生成的代理
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public string GetProxyCodes<TAttribute>(string @namespace) where TAttribute : RpcAttribute
        {
            var cellCodes = this.GetProxyInfo(new Type[] { typeof(TAttribute) });
            return CodeGenerator.ConvertToCode(@namespace, cellCodes);
        }

        /// <summary>
        /// 从本地获取代理
        /// </summary>
        /// <param name="attrbuteType"></param>
        /// <returns></returns>
        public ServerCellCode[] GetProxyInfo(Type[] attrbuteType)
        {
            var codes = new List<ServerCellCode>();

            foreach (var attrbute in attrbuteType)
            {
                foreach (var item in this.m_serverTypes.Keys)
                {
                    var serverCellCode = CodeGenerator.Generator(item, attrbute);
                    codes.Add(serverCellCode);
                }
            }
            return codes.ToArray();
        }

        /// <summary>
        /// 获取服务类型对应的服务方法。
        /// </summary>
        /// <param name="serverType"></param>
        /// <returns></returns>
        public MethodInstance[] GetServerMethodInstances(Type serverType)
        {
            return this.m_serverTypes[serverType].ToArray();
        }

        #region 注册

        /// <summary>
        /// 注册为单例服务
        /// </summary>
        /// <param name="serverFromType"></param>
        /// <param name="rpcServer"></param>
        /// <returns></returns>
        public void RegisterServer(Type serverFromType, IRpcServer rpcServer)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
            {
                throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
            }

            if (!serverFromType.IsAssignableFrom(rpcServer.GetType()))
            {
                throw new RpcException("实例类型必须与注册类型有继承关系。");
            }
            foreach (var item in this.m_serverTypes.Keys)
            {
                if (item.FullName == serverFromType.FullName)
                {
                    return;
                }
            }

            var methodInstances = CodeGenerator.GetMethodInstances(serverFromType, rpcServer.GetType());

            this.m_serverTypes.TryAdd(serverFromType, new List<MethodInstance>(methodInstances));
            this.m_registrator.RegisterSingleton(serverFromType, rpcServer);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serverFromType"></param>
        /// <param name="serverToType"></param>
        /// <returns></returns>
        public void RegisterServer(Type serverFromType, Type serverToType)
        {
            if (!typeof(IRpcServer).IsAssignableFrom(serverFromType))
            {
                throw new RpcException($"注册类型必须与{nameof(IRpcServer)}有继承关系");
            }

            if (!serverFromType.IsAssignableFrom(serverToType))
            {
                throw new RpcException("实例类型必须与注册类型有继承关系。");
            }

            foreach (var item in this.m_serverTypes.Keys)
            {
                if (item.FullName == serverFromType.FullName)
                {
                    return;
                }
            }

            if (typeof(ITransientRpcServer).IsAssignableFrom(serverFromType))
            {
                this.m_registrator.RegisterTransient(serverFromType, serverToType);
            }
            else
            {
                this.m_registrator.RegisterSingleton(serverFromType, serverToType);
            }
            var methodInstances = CodeGenerator.GetMethodInstances(serverFromType, serverToType);

            this.m_serverTypes.TryAdd(serverFromType, new List<MethodInstance>(methodInstances));
        }

        #endregion 注册
    }
}