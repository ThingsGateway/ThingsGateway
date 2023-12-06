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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 手动IOC容器
    /// </summary>
    public abstract class ManualContainer : IContainer
    {
        private readonly ConcurrentDictionary<string, object> m_singletonInstances = new ConcurrentDictionary<string, object>();

        /// <inheritdoc/>
        public IResolver BuildResolver()
        {
            return this;
        }

        /// <inheritdoc/>
        public IEnumerable<DependencyDescriptor> GetDescriptors()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            return this.Resolve(serviceType);
        }

        /// <summary>
        /// 判断指定的类型是否已在容器中注册。
        /// <para>
        /// 在本容器中，一般均会返回<see langword="true"/>。
        /// </para>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool IsRegistered(Type fromType, string key)
        {
            return true;
        }

        /// <summary>
        /// 判断指定的类型是否已在容器中注册。
        /// <para>
        /// 在本容器中，一般均会返回<see langword="true"/>。
        /// </para>
        /// </summary>
        /// <param name="fromType"></param>
        /// <returns></returns>
        public virtual bool IsRegistered(Type fromType)
        {
            return true;
        }

        /// <summary>
        /// 注册描述符。
        /// <para>
        /// 一般情况下，本容器只会处理单例实例模式。
        /// </para>
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        public virtual void Register(DependencyDescriptor descriptor, string key)
        {
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    this.m_singletonInstances.AddOrUpdate($"{descriptor.FromType.FullName}{key}", descriptor.ToInstance, (k, v) => descriptor.ToInstance);
                }
            }
        }

        /// <summary>
        /// 注册描述符。
        /// <para>
        /// 一般情况下，本容器只会处理单例实例模式。
        /// </para>
        /// </summary>
        /// <param name="descriptor"></param>
        public virtual void Register(DependencyDescriptor descriptor)
        {
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    this.m_singletonInstances.AddOrUpdate(descriptor.FromType.FullName, descriptor.ToInstance, (k, v) => descriptor.ToInstance);
                }
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Exception"></exception>
        public object Resolve(Type fromType, string key)
        {
            if (fromType == typeof(IResolver) || fromType == typeof(IServiceProvider))
            {
                return this;
            }
            if (this.TryResolve(fromType, out var instance, key))
            {
                return instance;
            }

            throw new Exception($"没有解决容器所需类型：{fromType.FullName}");
        }

        /// <inheritdoc/>
        /// <exception cref="Exception"></exception>
        public object Resolve(Type fromType)
        {
            if (fromType == typeof(IResolver) || fromType == typeof(IServiceProvider))
            {
                return this;
            }
            if (this.TryResolve(fromType, out var instance))
            {
                return instance;
            }

            throw new Exception($"没有解决容器所需类型：{fromType.FullName}");
        }

        /// <summary>
        /// 默认不实现该功能
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Unregister(DependencyDescriptor descriptor, string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 默认不实现该功能
        /// </summary>
        /// <param name="descriptor"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Unregister(DependencyDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 尝试解决Ioc容器所需类型。
        /// <para>
        /// 本方法仅实现了在单例实例注册下的获取。
        /// </para>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool TryResolve(Type fromType, out object instance, string key)
        {
            if (key.IsNullOrEmpty())
            {
                return this.m_singletonInstances.TryGetValue(fromType.FullName, out instance);
            }
            return this.m_singletonInstances.TryGetValue($"{fromType.FullName}{key}", out instance);
        }

        /// <summary>
        /// 尝试解决Ioc容器所需类型。
        /// <para>
        /// 本方法仅实现了在单例实例注册下的获取。
        /// </para>
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected virtual bool TryResolve(Type fromType, out object instance)
        {
            return this.m_singletonInstances.TryGetValue(fromType.FullName, out instance);
        }
    }
}