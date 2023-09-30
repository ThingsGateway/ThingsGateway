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
using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 表示属性的Getter
    /// </summary>
    public class MemberGetter
    {
        /// <summary>
        /// get方法委托
        /// </summary>
        private readonly Func<object, object> m_getFunc;

        /// <summary>
        /// 表示属性的Getter
        /// </summary>
        /// <param name="property">属性</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MemberGetter(PropertyInfo property)
        {
            this.m_getFunc = CreateGetterDelegate(property);
        }

        /// <summary>
        /// 表示类型字段或属性的Getter
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public MemberGetter(FieldInfo fieldInfo)
        {
            this.m_getFunc = CreateGetterDelegate(fieldInfo);
        }

        /// <summary>
        /// 获取属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns></returns>
        public object Invoke(object instance)
        {
            return this.m_getFunc.Invoke(instance);
        }

        private static Func<object, object> CreateGetterDelegate(PropertyInfo property)
        {
            var param_instance = Expression.Parameter(typeof(object));
            var body_instance = Expression.Convert(param_instance, property.DeclaringType);
            var body_property = Expression.Property(body_instance, property);
            var body_return = Expression.Convert(body_property, typeof(object));

            return Expression.Lambda<Func<object, object>>(body_return, param_instance).Compile();
        }

        private static Func<object, object> CreateGetterDelegate(FieldInfo fieldInfo)
        {
            var param_instance = Expression.Parameter(typeof(object));
            var body_instance = Expression.Convert(param_instance, fieldInfo.DeclaringType);
            var body_field = Expression.Field(body_instance, fieldInfo);
            var body_return = Expression.Convert(body_field, typeof(object));

            return Expression.Lambda<Func<object, object>>(body_return, param_instance).Compile();
        }
    }
}