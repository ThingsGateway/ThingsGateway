#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Reflection;

namespace ThingsGateway.Foundation.Extension;

/// <summary>
/// TypeExtension
/// </summary>
public static class TypeExtensions
{

    /// <summary>
    /// 是否是bool类型
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsBool(this Type self)
    {
        return self == typeof(bool);
    }

    /// <summary>
    /// 判断是否是 bool or bool?类型
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsBoolOrNullableBool(this Type self)
    {
        if (self == null)
        {
            return false;
        }
        if (self == typeof(bool) || self == typeof(bool?))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断是否为枚举
    /// </summary>
    /// <param name="self">Type类</param>
    /// <returns>判断结果</returns>
    public static bool IsEnum(this Type self)
    {
        return self.GetTypeInfo().IsEnum;
    }

    /// <summary>
    /// 判断是否为枚举或者可空枚举
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsEnumOrNullableEnum(this Type self)
    {
        if (self == null)
        {
            return false;
        }
        if (self.IsEnum)
        {
            return true;
        }
        else
        {
            if (self.IsGenericType && self.GetGenericTypeDefinition() == typeof(Nullable<>) && self.GetGenericArguments()[0].IsEnum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 判断是否是泛型
    /// </summary>
    /// <param name="self">Type类</param>
    /// <param name="innerType">泛型类型</param>
    /// <returns>判断结果</returns>
    public static bool IsGeneric(this Type self, Type innerType)
    {
        if (self.GetTypeInfo().IsGenericType && self.GetGenericTypeDefinition() == innerType)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断是否为Nullable类型
    /// </summary>
    /// <param name="self">Type类</param>
    /// <returns>判断结果</returns>
    public static bool IsNullable(Type self)
    {
        return self.IsGeneric(typeof(Nullable<>));
    }
    /// <summary>
    /// 是否是数值类型
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsNumber(Type self)
    {
        Type checktype = self;
        if (IsNullable(self))
        {
            checktype = self.GetGenericArguments()[0];
        }
        if (checktype == typeof(int) || checktype == typeof(short) || checktype == typeof(long) || checktype == typeof(float) || checktype == typeof(decimal) || checktype == typeof(double))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 判断是否为值类型
    /// </summary>
    /// <param name="self">Type类</param>
    /// <returns>判断结果</returns>
    public static bool IsPrimitive(this Type self)
    {
        return self.GetTypeInfo().IsPrimitive || self == typeof(decimal);
    }
}