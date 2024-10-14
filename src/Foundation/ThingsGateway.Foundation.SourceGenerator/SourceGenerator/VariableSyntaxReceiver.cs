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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThingsGateway.Foundation;

internal sealed class VariableSyntaxReceiver : ISyntaxReceiver
{
    public const string GeneratorVariableAttributeTypeName = "ThingsGateway.Foundation.GeneratorVariableAttribute";
    public const string VariableRuntimeAttributeTypeName = "ThingsGateway.Foundation.VariableRuntimeAttribute";

    /// <summary>
    /// 接口列表
    /// </summary>
    private readonly List<ClassDeclarationSyntax> m_classSyntaxList = new List<ClassDeclarationSyntax>();

    /// <summary>
    /// 访问语法树
    /// </summary>
    /// <param name="syntaxNode"></param>
    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax syntax)
        {
            m_classSyntaxList.Add(syntax);
        }
    }

    public INamedTypeSymbol GeneratorVariableAttributeAttribute { get; private set; }

    /// <summary>
    /// 获取所有插件符号
    /// </summary>
    /// <param name="compilation"></param>
    /// <returns></returns>
    public IEnumerable<INamedTypeSymbol> GetVariableObjectTypes(Compilation compilation)
    {
        GeneratorVariableAttributeAttribute = compilation.GetTypeByMetadataName(GeneratorVariableAttributeTypeName)!;
        if (GeneratorVariableAttributeAttribute == null)
        {
            yield break;
        }
        foreach (var classSyntax in m_classSyntaxList)
        {
            var @class = compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);
            if (@class != null && IsVariableObject(@class))
            {
                yield return @class;
            }
        }
    }

    /// <summary>
    /// 是否为变量类
    /// </summary>
    /// <param name="class"></param>
    /// <returns></returns>
    public bool IsVariableObject(INamedTypeSymbol @class)
    {
        if (GeneratorVariableAttributeAttribute is null)
        {
            return false;
        }

        if (@class.IsAbstract)
        {
            return false;
        }
        return HasAttribute(@class, GeneratorVariableAttributeAttribute);
    }

    /// <summary>
    /// 返回是否声明指定的特性
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attribute)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && (attrClass.AllInterfaces.Contains(attribute) || SymbolEqualityComparer.Default.Equals(attrClass, attribute)))
            {
                return true;
            }
        }
        return false;
    }
}

#endif
