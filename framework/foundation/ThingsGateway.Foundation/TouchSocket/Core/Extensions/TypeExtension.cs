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

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// TypeExtension
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object GetDefault(this Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetRefOutType(this Type type)
        {
            return type.IsByRef ? type.GetElementType() : type;
        }

        /// <summary>
        /// 检查类型是否是匿名类型
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsAnonymous(this Type type)
        {
            // 检查是否贴有 [CompilerGenerated] 特性
            if (!type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            {
                return false;
            }

            // 类型限定名是否以 <> 开头且以 AnonymousType 结尾
            return type.FullName.HasValue()
                && type.FullName.StartsWith("<>")
                && type.FullName.Contains("AnonymousType");
        }

        /// <summary>
        /// 检查类型是否是小数类型
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsDecimal(this Type type)
        {
            // 如果是浮点类型则直接返回
            if (type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(float))
            {
                return true;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 检查类型是否是字典类型
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsDictionary(this Type type)
        {
            return TouchSocketCoreUtility.dicType.IsAssignableFrom(type);
        }

        /// <summary>
        /// 检查类型是否可实例化
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsInstantiable(this Type type)
        {
            return type is { IsClass: true, IsAbstract: false }
                && !type.IsStatic();
        }

        /// <summary>
        /// 检查类型是否是整数类型
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsInteger(this Type type)
        {
            // 如果是枚举或浮点类型则直接返回
            if (type.IsEnum || type.IsDecimal())
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 是否是<see cref="List{T}"/>类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsList(this Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type theType)
        {
            return (theType.IsGenericType && theType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType));
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this PropertyInfo propertyInfo)
        {
            var att = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null
                ? true
                : propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        /// <summary>
        /// 判断该类型是否为可空类型
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(this FieldInfo fieldInfo)
        {
            var att = fieldInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return att != null
                ? true
                : fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.
              GetGenericTypeDefinition().Equals
              (TouchSocketCoreUtility.nullableType);
        }

        /// <summary>
        /// 检查类型是否是数值类型
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsNumeric(this Type type)
        {
            return type.IsInteger()
                || type.IsDecimal();
        }

        /// <summary>
        /// 判断是否为静态类。
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStatic(this Type targetType)
        {
            return targetType.IsAbstract && targetType.IsSealed;
        }

        /// <summary>
        /// 判断为结构体
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type targetType)
        {
            return !targetType.IsPrimitive && !targetType.IsClass && !targetType.IsEnum && targetType.IsValueType;
        }

        /// <summary>
        /// 判断该类型是否为值元组类型
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsValueTuple(this Type theType)
        {
            return theType.IsValueType &&
                 theType.IsGenericType &&
                 theType.FullName.StartsWith("System.ValueTuple");
        }
    }
}