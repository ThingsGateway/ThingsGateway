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

using System.Collections;
using System.Linq.Expressions;

namespace ThingsGateway.Foundation
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// 实例生成
    /// </summary>
    public static class InstanceCreater
    {
        /// <summary>
        /// 根据对象类型创建对象实例
        /// </summary>
        /// <param name="key">对象类型</param>
        /// <returns></returns>
        public static object Create(Type key)
        {
            return Activator.CreateInstance(key);
        }
    }
#else

    /// <summary>
    /// 实例生成
    /// </summary>
    public static class InstanceCreater
    {
        private static readonly Hashtable m_paramCache = Hashtable.Synchronized(new Hashtable());//缓存

        /// <summary>
        /// 根据对象类型创建对象实例
        /// </summary>
        /// <param name="key">对象类型</param>
        /// <returns></returns>
        public static object Create(Type key)
        {
            var value = (Func<object>)m_paramCache[key];
            if (value == null)
            {
                value = CreateInstanceByType(key);
                m_paramCache[key] = value;
            }
            return value();
        }

        private static Func<object> CreateInstanceByType(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type), null).Compile();
        }
    }

#endif
}