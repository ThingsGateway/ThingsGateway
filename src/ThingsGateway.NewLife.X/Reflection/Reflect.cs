﻿
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace NewLife.Reflection;

/// <summary>反射工具类</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/reflect
/// </remarks>
public static class Reflect
{
    #region 静态

    /// <summary>当前反射提供者</summary>
    public static IReflect Provider { get; set; }

    static Reflect() => Provider = new DefaultReflect();// 如果需要使用快速反射，启用下面这一行//Provider = new EmitReflect();

    #endregion 静态

    #region 反射获取

    /// <summary>根据名称获取类型。可搜索当前目录DLL，自动加载</summary>
    /// <param name="typeName">类型名</param>
    /// <returns></returns>
    public static Type? GetTypeEx(this String typeName)
    {
        if (String.IsNullOrEmpty(typeName)) return null;

        var type = Type.GetType(typeName);
        if (type != null) return type;

        return Provider.GetType(typeName, false);
    }

    /// <summary>根据名称获取类型。可搜索当前目录DLL，自动加载</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
    [Obsolete("不再支持isLoadAssembly")]
    public static Type? GetTypeEx(this String typeName, Boolean isLoadAssembly)
    {
        if (String.IsNullOrEmpty(typeName)) return null;

        var type = Type.GetType(typeName);
        if (type != null) return type;

        return Provider.GetType(typeName, isLoadAssembly);
    }

    /// <summary>获取方法</summary>
    /// <remarks>用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用</remarks>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    public static MethodInfo? GetMethodEx(this Type type, String name, params Type[] paramTypes)
    {
        if (name.IsNullOrEmpty()) return null;

        // 如果其中一个类型参数为空，得用别的办法
        if (paramTypes.Length > 0 && paramTypes.Any(e => e == null)) return Provider.GetMethods(type, name, paramTypes.Length).FirstOrDefault();

        return Provider.GetMethod(type, name, paramTypes);
    }

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public static MethodInfo[] GetMethodsEx(this Type type, String name, Int32 paramCount = -1)
    {
        if (name.IsNullOrEmpty()) return new MethodInfo[0];

        return Provider.GetMethods(type, name, paramCount);
    }

    /// <summary>获取属性。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static PropertyInfo? GetPropertyEx(this Type type, String name, Boolean ignoreCase = false)
    {
        if (String.IsNullOrEmpty(name)) return null;

        return Provider.GetProperty(type, name, ignoreCase);
    }

    /// <summary>获取字段。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static FieldInfo? GetFieldEx(this Type type, String name, Boolean ignoreCase = false)
    {
        if (String.IsNullOrEmpty(name)) return null;

        return Provider.GetField(type, name, ignoreCase);
    }

    /// <summary>获取成员。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static MemberInfo? GetMemberEx(this Type type, String name, Boolean ignoreCase = false)
    {
        if (String.IsNullOrEmpty(name)) return null;

        return Provider.GetMember(type, name, ignoreCase);
    }

    /// <summary>获取用于序列化的字段</summary>
    /// <remarks>过滤<seealso cref="T:NonSerializedAttribute"/>特性的字段</remarks>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public static IList<FieldInfo> GetFields(this Type type, Boolean baseFirst) => Provider.GetFields(type, baseFirst);

    /// <summary>获取用于序列化的属性</summary>
    /// <remarks>过滤<seealso cref="T:XmlIgnoreAttribute"/>特性的属性和索引器</remarks>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public static IList<PropertyInfo> GetProperties(this Type type, Boolean baseFirst) => Provider.GetProperties(type, baseFirst);

    #endregion 反射获取

    #region 反射调用

    /// <summary>反射创建指定类型的实例</summary>
    /// <param name="type">类型</param>
    /// <param name="parameters">参数数组</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? CreateInstance(this Type type, params Object?[] parameters)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        return Provider.CreateInstance(type, parameters);
    }

    /// <summary>反射调用指定对象的方法。target为类型时调用其静态方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="name">方法名</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    public static Object? Invoke(this Object target, String name, params Object?[] parameters)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        if (TryInvoke(target, name, out var value, parameters)) return value;

        var type = GetType(target);
        throw new XException("Cannot find method named {1} in class {0}!", type, name);
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="name">方法名</param>
    /// <param name="value">数值</param>
    /// <param name="parameters">方法参数</param>
    /// <remarks>反射调用是否成功</remarks>
    public static Boolean TryInvoke(this Object target, String name, out Object? value, params Object?[] parameters)
    {
        value = null;

        if (String.IsNullOrEmpty(name)) return false;

        var type = GetType(target);

        // 参数类型数组
        var ps = parameters.Select(e => e?.GetType()).ToArray();

        // 如果参数数组出现null，则无法精确匹配，可按参数个数进行匹配
        var method = ps.Any(e => e == null) ? GetMethodEx(type, name) : GetMethodEx(type, name, ps!);
        method ??= GetMethodsEx(type, name, ps.Length > 0 ? ps.Length : -1).FirstOrDefault();
        if (method == null) return false;

        value = Invoke(target, method, parameters);

        return true;
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? Invoke(this Object? target, MethodBase method, params Object?[]? parameters)
    {
        //if (target == null) throw new ArgumentNullException("target");
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (!method.IsStatic && target == null) throw new ArgumentNullException(nameof(target));

        return Provider.Invoke(target, method, parameters);
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数字典</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? InvokeWithParams(this Object? target, MethodBase method, IDictionary? parameters)
    {
        //if (target == null) throw new ArgumentNullException("target");
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (!method.IsStatic && target == null) throw new ArgumentNullException(nameof(target));

        return Provider.InvokeWithParams(target, method, parameters);
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="throwOnError">出错时是否抛出异常</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? GetValue(this Object target, String name, Boolean throwOnError = true)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        if (TryGetValue(target, name, out var value)) return value;

        if (!throwOnError) return null;

        var type = GetType(target);
        throw new ArgumentException($"The [{name}] property or field does not exist in class [{type.FullName}].");
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns>是否成功获取数值</returns>
    internal static Boolean TryGetValue(this Object target, String name, out Object? value)
    {
        value = null;

        if (String.IsNullOrEmpty(name)) return false;

        var type = GetType(target);

        var mi = type.GetMemberEx(name, true);
        if (mi == null) return false;

        value = target.GetValue(mi);

        return true;
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? GetValue(this Object? target, MemberInfo member)
    {
        // 有可能跟普通的 PropertyInfo.GetValue(Object target) 搞混了
        if (member == null && target is MemberInfo mi)
        {
            member = mi;
            target = null;
        }

        //if (target is IModel model && member is PropertyInfo) return model[member.Name];

        if (member is PropertyInfo property)
            return Provider.GetValue(target, property);
        else if (member is FieldInfo field)
            return Provider.GetValue(target, field);
        else
            throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static Boolean SetValue(this Object target, String name, Object? value)
    {
        if (String.IsNullOrEmpty(name)) return false;

        //// 借助 IModel 优化取值赋值，有 IExtend 扩展属性的实体类过于复杂而不支持，例如IEntity就有脏数据问题
        //if (target is IModel model && target is not IExtend)
        //{
        //    model[name] = value;
        //    return true;
        //}

        var type = GetType(target);

        var mi = type.GetMemberEx(name, true);
        if (mi == null) return false;

        target.SetValue(mi, value);

        //throw new ArgumentException("The [{name}] property or field does not exist in class [{type.FullName}].");
        return true;
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static void SetValue(this Object target, MemberInfo member, Object? value)
    {
        //// 借助 IModel 优化取值赋值，有 IExtend 扩展属性的实体类过于复杂而不支持，例如IEntity就有脏数据问题
        //if (target is IModel model && target is not IExtend && member is PropertyInfo)
        //    model[member.Name] = value;
        //else
        if (member is PropertyInfo pi)
            Provider.SetValue(target, pi, value);
        else if (member is FieldInfo fi)
            Provider.SetValue(target, fi, value);
        else
            throw new ArgumentOutOfRangeException(nameof(member));
    }

    #endregion 反射调用

    #region 类型辅助

    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static Type? GetElementTypeEx(this Type type) => Provider.GetElementType(type);

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    public static Object? ChangeType(this Object? value, Type conversionType) => Provider.ChangeType(value, conversionType);

    /// <summary>类型转换</summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static TResult? ChangeType<TResult>(this Object? value)
    {
        if (value is TResult result) return result;

        return (TResult?)ChangeType(value, typeof(TResult));
    }

    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
    public static String GetName(this Type type, Boolean isfull = false) => Provider.GetName(type, isfull);

    /// <summary>从参数数组中获取类型数组</summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Type[] GetTypeArray(this Object?[]? args)
    {
        if (args == null) return Type.EmptyTypes;

        var typeArray = new Type[args.Length];
        for (var i = 0; i < typeArray.Length; i++)
        {
            var arg = args[i];
            if (arg == null)
                typeArray[i] = typeof(Object);
            else
                typeArray[i] = arg.GetType();
        }
        return typeArray;
    }

    /// <summary>获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身</summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static Type? GetMemberType(this MemberInfo member)
    {
        //return member.MemberType switch
        //{
        //    MemberTypes.Constructor => (member as ConstructorInfo).DeclaringType,
        //    MemberTypes.Field => (member as FieldInfo).FieldType,
        //    MemberTypes.Method => (member as MethodInfo).ReturnType,
        //    MemberTypes.Property => (member as PropertyInfo).PropertyType,
        //    MemberTypes.TypeInfo or MemberTypes.NestedType => member as Type,
        //    _ => null,
        //};

        if (member is ConstructorInfo ctor) return ctor.DeclaringType;
        if (member is FieldInfo field) return field.FieldType;
        if (member is MethodInfo method) return method.ReturnType;
        if (member is PropertyInfo property) return property.PropertyType;
        if (member is Type type) return type;

        return null;
    }

    /// <summary>获取类型代码</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static TypeCode GetTypeCode(this Type type) => Type.GetTypeCode(type);

    /// <summary>是否整数。Byte/Int16/Int32/Int64</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsInt(this Type type)
    {
        return type == typeof(Int32)
            || type == typeof(Int64)
            || type == typeof(Int16)
            || type == typeof(UInt32)
            || type == typeof(UInt64)
            || type == typeof(UInt16)
            || type == typeof(Byte)
            || type == typeof(SByte)
            ;
    }

    /// <summary>是否泛型列表</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsList(this Type type) => type != null && type.IsGenericType && type.As(typeof(IList<>));

    /// <summary>是否泛型字典</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsDictionary(this Type type) => type != null && type.IsGenericType && type.As(typeof(IDictionary<,>));

    #endregion 类型辅助

    #region 插件

    /// <summary>是否能够转为指定基类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static Boolean As(this Type type, Type baseType) => Provider.As(type, baseType);

    /// <summary>是否能够转为指定基类</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean As<T>(this Type type) => Provider.As(type, typeof(T));

    #endregion 插件

    #region 辅助方法

    /// <summary>获取类型，如果target是Type类型，则表示要反射的是静态成员</summary>
    /// <param name="target">目标对象</param>
    /// <returns></returns>
    private static Type GetType(Object target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        var type = target as Type;
        if (type == null)
            type = target.GetType();
        //else
        //    target = null;

        return type;
    }

    ///// <summary>判断某个类型是否可空类型</summary>
    ///// <param name="type">类型</param>
    ///// <returns></returns>
    //static Boolean IsNullable(Type type)
    //{
    //    //if (type.IsValueType) return false;

    //    if (type.IsGenericType && !type.IsGenericTypeDefinition &&
    //        Object.ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>))) return true;

    //    return false;
    //}

    /// <summary>把一个方法转为泛型委托，便于快速反射调用</summary>
    /// <typeparam name="TFunc"></typeparam>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TFunc? As<TFunc>(this MethodInfo method, Object? target = null)
    {
        if (method == null) return default;

        if (target == null)
            return (TFunc?)(Object?)Delegate.CreateDelegate(typeof(TFunc), method, true);
        else
            return (TFunc?)(Object?)Delegate.CreateDelegate(typeof(TFunc), target, method, true);
    }

    #endregion 辅助方法
}