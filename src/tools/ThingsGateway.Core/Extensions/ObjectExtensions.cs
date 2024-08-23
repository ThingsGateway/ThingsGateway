//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ThingsGateway.Core.Extension;

public static class ObjectExtensions
{
    /// <summary>
    /// 判断类型是否实现某个泛型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="generic">泛型类型</param>
    /// <returns>bool</returns>
    public static bool HasImplementedRawGeneric(this Type type, Type generic)
    {
        // 检查接口类型
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;

        // 检查类型
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType!;
        }

        return false;

        // 判断逻辑
        bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
    }

    /// <summary>
    /// 将一个对象转换为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T ChangeType<T>(this object obj)
    {
        return (T)ChangeType(obj, typeof(T))!;
    }

    /// <summary>
    /// 将一个对象转换为指定类型
    /// </summary>
    /// <param name="obj">待转换的对象</param>
    /// <param name="type">目标类型</param>
    /// <returns>转换后的对象</returns>
    public static object? ChangeType(this object? obj, Type type)
    {
        if (type == null) return obj;
        if (type == typeof(string)) return obj?.ToString();
        if (type == typeof(Guid) && obj != null) return Guid.Parse(obj.ToString());
        if (type == typeof(bool) && obj != null && obj is not bool)
        {
            var objStr = obj.ToString().ToLower();
            if (objStr == "1" || objStr == "true" || objStr == "yes" || objStr == "on") return true;
            return false;
        }
        if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (type.IsAssignableFrom(obj.GetType())) return obj;
        else if ((underlyingType ?? type).IsEnum)
        {
            if (underlyingType != null && string.IsNullOrWhiteSpace(obj.ToString())) return null;
            else return Enum.Parse(underlyingType ?? type, obj.ToString());
        }
        // 处理 DateTime -> DateTimeOffset 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
        {
            return ((DateTime)obj).ConvertToDateTimeOffset();
        }
        // 处理 DateTimeOffset -> DateTime 类型
        else if (obj.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
        {
            return ((DateTimeOffset)obj).ConvertToDateTime();
        }
        // 处理 DateTime -> DateOnly 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateOnly)))
        {
            return DateOnly.FromDateTime(((DateTime)obj));
        }
        // 处理 DateTime -> TimeOnly 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(TimeOnly)))
        {
            return TimeOnly.FromDateTime(((DateTime)obj));
        }
        else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
        {
            try
            {
                return Convert.ChangeType(obj, underlyingType ?? type, null);
            }
            catch
            {
                return underlyingType == null ? Activator.CreateInstance(type) : null;
            }
        }
        else
        {
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(obj.GetType())) return converter.ConvertFrom(obj);

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                var o = constructor.Invoke(null);
                var propertys = type.GetProperties();
                var oldType = obj.GetType();

                foreach (var property in propertys)
                {
                    var p = oldType.GetProperty(property.Name);
                    if (property.CanWrite && p != null && p.CanRead)
                    {
                        property.SetValue(o, ChangeType(p.GetValue(obj, null), property.PropertyType), null);
                    }
                }
                return o;
            }
        }

        return obj;
    }

    /// <summary>
    /// 获取所有祖先类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetAncestorTypes(this Type type)
    {
        var ancestorTypes = new List<Type>();
        while (type != null && type != typeof(object))
        {
            if (IsNoObjectBaseType(type))
            {
                var baseType = type.BaseType!;
                ancestorTypes.Add(baseType);
                type = baseType;
            }
            else break;
        }

        return ancestorTypes;

        static bool IsNoObjectBaseType(Type type) => type.BaseType != typeof(object);
    }

    /// <summary>
    /// 查找方法指定特性，如果没找到则继续查找声明类
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="method"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit)
        where TAttribute : Attribute
    {
        // 获取方法所在类型
        var declaringType = method.DeclaringType!;

        var attributeType = typeof(TAttribute);

        // 判断方法是否定义指定特性，如果没有再查找声明类
        var foundAttribute = method.IsDefined(attributeType, inherit)
            ? method.GetCustomAttribute<TAttribute>(inherit)
            : (
                declaringType.IsDefined(attributeType, inherit)
                ? declaringType.GetCustomAttribute<TAttribute>(inherit)
                : default
            );

        return foundAttribute;
    }

    /// <summary>
    /// 获取方法真实返回类型
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static Type GetRealReturnType(this MethodInfo method)
    {
        // 判断是否是异步方法
        var isAsyncMethod = method.IsAsync();

        // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
        var returnType = method.ReturnType;
        return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
    }

    /// <summary>
    /// 获取类型自定义特性
    /// </summary>
    /// <typeparam name="TAttribute">特性类型</typeparam>
    /// <param name="type">类类型</param>
    /// <param name="inherit">是否继承查找</param>
    /// <returns>特性对象</returns>
    public static TAttribute GetTypeAttribute<TAttribute>(this Type type, bool inherit = false)
        where TAttribute : Attribute
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);

        // 检查特性并获取特性对象
        return type.IsDefined(typeof(TAttribute), inherit)
            ? type.GetCustomAttribute<TAttribute>(inherit)
            : default;
    }

    /// <summary>
    /// 判断是否是匿名类型
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static bool IsAnonymous(this object obj)
    {
        var type = obj is Type t ? t : obj.GetType();

        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }

    /// <summary>
    /// 判断方法是否是异步
    /// </summary>
    /// <param name="method">方法</param>
    /// <returns></returns>
    public static bool IsAsync(this MethodInfo method)
    {
        return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null
            || method.ReturnType.ToString().StartsWith(typeof(Task).FullName!);
    }

    /// <summary>
    /// 判断集合是否为空
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="collection">集合对象</param>
    /// <returns><see cref="bool"/> 实例，true 表示空集合，false 表示非空集合</returns>
    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// 判断是否是富基元类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static bool IsRichPrimitive(this Type? type)
    {
        if (type == null) return false;

        // 处理元组类型
        if (type.IsValueTuple()) return false;

        // 处理数组类型，基元数组类型也可以是基元类型
        if (type.IsArray) return type.GetElementType()?.IsRichPrimitive() ?? false;

        // 基元类型或值类型或字符串类型
        if (type.IsPrimitive || type.IsValueType || type == typeof(string)) return true;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return type.GenericTypeArguments[0].IsRichPrimitive();

        return false;
    }

    /// <summary>
    /// 判断是否是元组类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static bool IsValueTuple(this Type type)
    {
        return type.Namespace == "System" && type.Name.Contains("ValueTuple`");
    }

    /// <summary>
    /// JsonElement 转 Object
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    public static object ToObject(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                return jsonElement.GetString();

            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return default;

            case JsonValueKind.Number:
                return jsonElement.GetDecimal();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();

            case JsonValueKind.Object:
                var enumerateObject = jsonElement.EnumerateObject();
                var dic = new Dictionary<string, object>();
                foreach (var item in enumerateObject)
                {
                    dic.Add(item.Name, item.Value.ToObject());
                }
                return dic;

            case JsonValueKind.Array:
                var enumerateArray = jsonElement.EnumerateArray();
                var list = new List<object>();
                foreach (var item in enumerateArray)
                {
                    list.Add(item.ToObject());
                }
                return list;

            default:
                return default;
        }
    }
}
