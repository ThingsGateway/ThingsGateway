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
using Microsoft.CodeAnalysis.Text;

using System.Reflection;
using System.Text;

namespace ThingsGateway.Foundation;

internal sealed class VariableCodeBuilder
{
    private readonly INamedTypeSymbol m_pluginClass;

    public VariableCodeBuilder(INamedTypeSymbol pluginClass)
    {
        m_pluginClass = pluginClass;
    }

    public string Prefix { get; set; }

    public IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using ThingsGateway.Foundation;";
            yield return "using System.Threading.Tasks;";
        }
    }

    public string GetFileName()
    {
        return m_pluginClass.ToDisplayString() + "Generator";
    }

    public bool TryToSourceText(out SourceText sourceText)
    {
        var code = ToString();
        if (string.IsNullOrEmpty(code))
        {
            sourceText = null;
            return false;
        }
        sourceText = SourceText.From(code, Encoding.UTF8);
        return true;
    }

    public override string ToString()
    {
        var propertys = FindPropertys().ToList();
        if (propertys.Count == 0)
        {
            return null;
        }
        var codeString = new StringBuilder();
        codeString.AppendLine("/*");
        codeString.AppendLine("此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码");
        codeString.AppendLine("*/");
        codeString.AppendLine("#pragma warning disable");

        foreach (var item in Usings)
        {
            codeString.AppendLine(item);
        }

        codeString.AppendLine($"namespace {m_pluginClass.ContainingNamespace}");
        codeString.AppendLine("{");
        codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"ThingsGateway.Foundation\",\"{Assembly.GetExecutingAssembly().GetName().Version}\")]");
        codeString.AppendLine($"partial class {m_pluginClass.Name}");
        codeString.AppendLine("{");
        foreach (var item in propertys)
        {
            BuildMethod(codeString, item);
        }
        codeString.AppendLine("}");
        codeString.AppendLine("}");

        return codeString.ToString();
    }

    private void BuildMethod(StringBuilder stringBuilder, IPropertySymbol propertySymbol)
    {
        var attributeData = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == VariableSyntaxReceiver.VariableRuntimeAttributeTypeName);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"public ValueTask<OperResult> Write{propertySymbol.Name}Async({propertySymbol.Type} value,CancellationToken cancellationToken=default)");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine($"return WriteValueAsync(\"{propertySymbol.Name}\",value,cancellationToken);");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
    }

    private IEnumerable<IPropertySymbol> FindPropertys()
    {
        return m_pluginClass
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(m =>
            {
                return m.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == VariableSyntaxReceiver.VariableRuntimeAttributeTypeName);
            });
    }
}

#endif
