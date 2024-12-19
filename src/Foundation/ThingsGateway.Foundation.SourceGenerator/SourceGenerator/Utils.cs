
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------




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

#if !NET45_OR_GREATER

using Microsoft.CodeAnalysis;

namespace ThingsGateway.Foundation;

internal static class Utils
{
    public static bool IsInheritFrom(this ITypeSymbol typeSymbol, string baseType)
    {
        if (typeSymbol.ToDisplayString() == baseType)
        {
            return true;
        }

        if (typeSymbol.BaseType != null)
        {
            var b = IsInheritFrom(typeSymbol.BaseType, baseType);
            if (b)
            {
                return true;
            }
        }

        foreach (var item in typeSymbol.AllInterfaces)
        {
            var b = IsInheritFrom(item, baseType);
            if (b)
            {
                return true;
            }
        }

        return false;
    }

    public static string RenameCamelCase(this string str)
    {
        var firstChar = str[0];

        if (firstChar == char.ToLowerInvariant(firstChar))
        {
            return str;
        }

        var name = str.ToCharArray();
        name[0] = char.ToLowerInvariant(firstChar);

        return new string(name);
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && attrClass.ToDisplayString() == attribute.ToDisplayString())
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, string attribute, out AttributeData attributeData)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && attrClass.ToDisplayString() == attribute)
            {
                attributeData = attr;
                return true;
            }
        }
        attributeData = default;
        return false;
    }

    public static bool HasFlags(int value, int flag)
    {
        return (value & flag) == flag;
    }

    public static bool HasReturn(this IMethodSymbol method)
    {
        if (method.ReturnsVoid || method.ReturnType.ToDisplayString() == typeof(Task).FullName)
        {
            return false;
        }
        return true;
    }
}

#endif