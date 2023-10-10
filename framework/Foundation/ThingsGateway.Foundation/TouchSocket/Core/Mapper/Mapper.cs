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
using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 映射数据
    /// </summary>

    public static partial class Mapper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, Property>> m_typeToProperty = new ConcurrentDictionary<Type, Dictionary<string, Property>>();

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TTarget Map<TTarget>(this object source, MapperOption option = default) where TTarget : class, new()
        {
            return (TTarget)Map(source, typeof(TTarget), option);
        }

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TTarget Map<TTarget>(this TTarget source, MapperOption option = default) where TTarget : class, new()
        {
            return (TTarget)Map(source, typeof(TTarget), option);
        }

        /// <summary>
        /// 简单映射
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TTarget Map<TSource, TTarget>(this TSource source, MapperOption option = default) where TTarget : class, new()
        {
            return (TTarget)Map(source, typeof(TTarget), option);
        }

        /// <summary>
        /// 简单对象映射
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static object Map(this object source, Type targetType, MapperOption option = default)
        {
            return Map(source, Activator.CreateInstance(targetType), option);
        }

        /// <summary>
        /// 简单对象映射
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static object Map(this object source, object target, MapperOption option = default)
        {
            if (source is null)
            {
                return default;
            }
            var sourceType = source.GetType();
            if (sourceType.IsPrimitive || sourceType.IsEnum || sourceType == TouchSocketCoreUtility.stringType)
            {
                return source;
            }
            var sourcePairs = m_typeToProperty.GetOrAdd(sourceType, (k) =>
               {
                   var pairs = new Dictionary<string, Property>();
                   var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                   foreach (var item in ps)
                   {
                       pairs.Add(item.Name, new Property(item));
                   }
                   return pairs;
               });

            var targetPairs = m_typeToProperty.GetOrAdd(target.GetType(), (k) =>
            {
                var pairs = new Dictionary<string, Property>();
                var ps = k.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var item in ps)
                {
                    pairs.Add(item.Name, new Property(item));
                }
                return pairs;
            });

            foreach (var item in sourcePairs)
            {
                if (item.Value.CanRead)
                {
                    var pkey = item.Key;
                    if (option != null && option.MapperProperties != null && option.MapperProperties.ContainsKey(pkey))
                    {
                        pkey = option.MapperProperties[pkey];
                    }

                    if (option?.IgnoreProperties?.Contains(pkey) == true)
                    {
                        continue;
                    }
                    if (targetPairs.TryGetValue(pkey, out var property))
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(target, item.Value.GetValue(source));
                        }
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// 映射List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="list"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static IEnumerable<T1> MapList<T, T1>(this IEnumerable<T> list, MapperOption option = default) where T : class where T1 : class, new()
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var result = new List<T1>();
            foreach (var item in list)
            {
                result.Add(Map<T, T1>(item, option));
            }
            return result;
        }

        /// <summary>
        /// 映射List
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="list"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<T1> MapList<T1>(this IEnumerable<object> list, MapperOption option = default) where T1 : class, new()
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var result = new List<T1>();
            foreach (var item in list)
            {
                result.Add(Map<T1>(item, option));
            }
            return result;
        }
    }
}