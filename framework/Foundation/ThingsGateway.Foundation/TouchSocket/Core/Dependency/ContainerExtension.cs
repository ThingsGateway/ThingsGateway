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
using System.Data;
using System.Reflection;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// IContainerExtensions
    /// </summary>
    public static class ContainerExtension
    {
        #region RegisterSingleton

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom, TTo>(this IContainer container, TTo instance)
              where TFrom : class
              where TTo : class, TFrom
        {
            RegisterSingleton(container, typeof(TFrom), instance);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton(this IContainer container, object instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            RegisterSingleton(container, instance.GetType(), instance);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IContainer RegisterSingleton(this IContainer container, Type fromType)
        {
            if (fromType is null)
            {
                throw new ArgumentNullException(nameof(fromType));
            }

            RegisterSingleton(container, fromType, fromType);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom, TTo>(this IContainer container, string key, TTo instance)
              where TFrom : class
              where TTo : class, TFrom
        {
            RegisterSingleton(container, typeof(TFrom), instance, key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton(this IContainer container, Type fromType, object instance, string key = "")
        {
            container.Register(new DependencyDescriptor(fromType, instance), key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom>(this IContainer container, object instance, string key = "")
        {
            container.Register(new DependencyDescriptor(typeof(TFrom), instance), key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton(this IContainer container, object instance, string key = "")
        {
            container.Register(new DependencyDescriptor(instance.GetType(), instance), key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom>(this IContainer container, string key = "")
        {
            container.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Singleton), key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton(this IContainer container, Type fromType, Type toType, string key = "")
        {
            container.Register(new DependencyDescriptor(fromType, toType, Lifetime.Singleton), key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="func"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom>(this IContainer container, Func<IContainer, object> func, string key = "")
        {
            container.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Singleton)
            {
                ImplementationFactory = func
            }, key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="func"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton(this IContainer container, Type fromType, Func<IContainer, object> func, string key = "")
        {
            container.Register(new DependencyDescriptor(fromType, Lifetime.Singleton)
            {
                ImplementationFactory = func
            }, key);
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom, TTO>(this IContainer container)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterSingleton(container, typeof(TFrom), typeof(TTO));
            return container;
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterSingleton<TFrom, TTO>(this IContainer container, string key)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterSingleton(container, typeof(TFrom), typeof(TTO), key);
            return container;
        }

        #endregion RegisterSingleton

        #region Transient

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient<TFrom, TTO>(this IContainer container)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterTransient(container, typeof(TFrom), typeof(TTO));
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient<TFrom>(this IContainer container, string key = "")
             where TFrom : class
        {
            RegisterTransient(container, typeof(TFrom), typeof(TFrom), key);
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTO"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient<TFrom, TTO>(this IContainer container, string key = "")
            where TFrom : class
            where TTO : class, TFrom
        {
            RegisterTransient(container, typeof(TFrom), typeof(TTO), key);
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient(this IContainer container, Type fromType, string key = "")
        {
            RegisterTransient(container, fromType, fromType, key);
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient(this IContainer container, Type fromType, Type toType, string key = "")
        {
            container.Register(new DependencyDescriptor(fromType, toType, Lifetime.Transient), key);
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <param name="container"></param>
        /// <param name="func"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient<TFrom>(this IContainer container, Func<IContainer, object> func, string key = "")
        {
            container.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Transient)
            {
                ImplementationFactory = func
            }, key);
            return container;
        }

        /// <summary>
        /// 注册临时映射
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="func"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer RegisterTransient(this IContainer container, Type fromType, Func<IContainer, object> func, string key = "")
        {
            container.Register(new DependencyDescriptor(fromType, Lifetime.Transient)
            {
                ImplementationFactory = func
            }, key);
            return container;
        }

        #endregion Transient

        #region Unregister

        /// <summary>
        /// 移除注册信息
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer Unregister(this IContainer container, Type fromType, string key = "")
        {
            container.Unregister(new DependencyDescriptor(fromType), key);
            return container;
        }

        /// <summary>
        /// 移除注册信息
        /// </summary>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IContainer Unregister<TFrom>(this IContainer container, string key = "")
        {
            container.Unregister(new DependencyDescriptor(typeof(TFrom)), key);
            return container;
        }

        #endregion Unregister

        #region Resolve

        /// <summary>
        /// 创建类型对应的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Resolve<T>(this IContainer container, string key = "")
        {
            return (T)container.Resolve(typeof(T), key);
        }

        /// <summary>
        /// 创建<see cref="Lifetime.Transient"/>生命的未注册的根类型实例。一般适用于：目标类型没有注册，但是其成员类型已经注册的情况。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object ResolveWithoutRoot(this IContainer container, Type fromType)
        {
            object[] ops = null;
            var ctor = fromType.GetConstructors().FirstOrDefault(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
            if (ctor is null)
            {
                //如果没有被特性标记，那就取构造函数参数最多的作为注入目标
                if (fromType.GetConstructors().Length == 0)
                {
                    throw new Exception($"没有找到类型{fromType.FullName}的公共构造函数。");
                }
                ctor = fromType.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
            }
            DependencyTypeAttribute dependencyTypeAttribute = null;
            if (fromType.IsDefined(typeof(DependencyTypeAttribute), true))
            {
                dependencyTypeAttribute = fromType.GetCustomAttribute<DependencyTypeAttribute>();
            }

            var parameters = ctor.GetParameters();
            var ps = new object[parameters.Length];

            if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Constructor))
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (ops != null && ops.Length - 1 >= i)
                    {
                        ps[i] = ops[i];
                    }
                    else
                    {
                        if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                        {
                            ps[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : default;
                        }
                        else
                        {
                            if (parameters[i].IsDefined(typeof(DependencyInjectAttribute), true))
                            {
                                var attribute = parameters[i].GetCustomAttribute<DependencyInjectAttribute>();
                                var type = attribute.Type ?? parameters[i].ParameterType;
                                ps[i] = container.Resolve(type, attribute.Key);
                            }
                            else
                            {
                                ps[i] = container.Resolve(parameters[i].ParameterType, null);
                            }
                        }
                    }
                }
            }
            if (ps == null || ps.Length == 0)
            {
                return Activator.CreateInstance(fromType);
            }
            return Activator.CreateInstance(fromType, ps);
        }

        /// <summary>
        /// 创建<see cref="Lifetime.Transient"/>生命的未注册的根类型实例。一般适用于：目标类型没有注册，但是其成员类型已经注册的情况。
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T ResolveWithoutRoot<T>(this IContainer container)
        {
            return (T)ResolveWithoutRoot(container, typeof(T));
        }

        /// <summary>
        ///  尝试创建类型对应的实例，如果类型没有注册，则会返回null或者默认值类型。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object TryResolve(this IContainer container, Type fromType, string key = "")
        {
            if (container.IsRegistered(fromType))
            {
                return container.Resolve(fromType, key);
            }
            return default;
        }

        /// <summary>
        /// 尝试创建类型对应的实例，如果类型没有注册，则会返回null或者默认值类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T TryResolve<T>(this IContainer container, string key = "")
        {
            return (T)TryResolve(container, typeof(T), key);
        }

        #endregion Resolve
    }
}