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

namespace ThingsGateway.Foundation;

/// <summary>
/// 源生成
/// </summary>
[Generator]
public class VariableObjectSourceGenerator : ISourceGenerator
{
    private string m_generatorVariableAttribute = @"
using System;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 使用源生成变量写入方法的调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class GeneratorVariableAttribute:Attribute
    {
    }
}

";

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        //Debugger.Launch();
        context.RegisterForPostInitialization(a =>
        {
            a.AddSource(nameof(m_generatorVariableAttribute), m_generatorVariableAttribute);
        });
        context.RegisterForSyntaxNotifications(() => new VariableSyntaxReceiver());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

        if (context.SyntaxReceiver is VariableSyntaxReceiver receiver)
        {
            var builders = receiver
                .GetVariableObjectTypes(context.Compilation)
                .Select(i => new VariableCodeBuilder(i))
                .Distinct();
            foreach (var builder in builders)
            {
                if (builder.TryToSourceText(out var sourceText))
                {
                    var tree = CSharpSyntaxTree.ParseText(sourceText);
                    var root = tree.GetRoot().NormalizeWhitespace();
                    var ret = root.ToFullString();
                    context.AddSource($"{builder.GetFileName()}.g.cs", ret);
                }
            }
        }
    }
}

#endif
