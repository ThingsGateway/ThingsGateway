//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Reflection;

namespace ThingsGateway.Foundation.TypeExtension;

/// <summary>
/// TypeExtension
/// </summary>
public static class TypeExtension
{
    /// <summary>
    /// IsNullable
    /// </summary>
    public static bool IsNullable(this PropertyInfo property)
    {
#if NET6_0_OR_GREATER
        return new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable;
#else
        return IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);
#endif
    }

    /// <summary>
    /// IsNullable
    /// </summary>
    public static bool IsNullable(this FieldInfo field)
    {
#if NET6_0_OR_GREATER
        return new NullabilityInfoContext().Create(field).WriteState is NullabilityState.Nullable;
#else
        return IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);
#endif
    }

    /// <summary>
    /// IsNullable
    /// </summary>
    public static bool IsNullable(this ParameterInfo parameter)
    {
#if NET6_0_OR_GREATER
        return new NullabilityInfoContext().Create(parameter).WriteState is NullabilityState.Nullable;
#else
        return IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);
#endif
    }

    private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) != null;

        var nullable = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2;
            }
        }

        for (var type = declaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context != null &&
                context.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
            {
                return (byte)context.ConstructorArguments[0].Value! == 2;
            }
        }

        // Couldn't find a suitable attribute
        return false;
    }
}