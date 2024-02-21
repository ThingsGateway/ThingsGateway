//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CodingSeb.ExpressionEvaluator;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 表达式扩展
/// </summary>
[System.Security.SecuritySafeCritical]
public static class ExpressionEvaluatorExtension
{
    private static readonly GlobalData GlobalData = App.RootServices.GetService<GlobalData>();

    /// <summary>
    /// 计算表达式：例如：raw*100，raw为原始值
    /// </summary>
    public static object GetExpressionsResult(this string expressions, object? rawvalue)
    {
        if (string.IsNullOrEmpty(expressions))
        {
            return rawvalue;
        }

        //JArray内部转换
        //
        //JArray newValue = new JArray(raw);
        //for (int i = 0; i < raw.Count; i++)
        //{
        //newValue[i] = JToken.FromObject(Extensions.Value<int>(raw[i]) * 1000);
        //}
        //return newValue;
        //
        ExpressionEvaluator expressionEvaluator = new();
        expressionEvaluator.OptionScriptNeedSemicolonAtTheEndOfLastExpression = false;
        expressionEvaluator.PreEvaluateVariable += Evaluator_PreEvaluateVariable;
        expressionEvaluator.Assemblies.Add(typeof(Newtonsoft.Json.JsonConverter).Assembly);
        expressionEvaluator.Namespaces.Add("Newtonsoft.Json.Linq");
        expressionEvaluator.Namespaces.Add("Newtonsoft.Json");
        expressionEvaluator.Variables = new Dictionary<string, object>()
            {
              { "Raw", rawvalue},
              { "raw", rawvalue},
            };
        var value = expressionEvaluator.ScriptEvaluate(expressions);
        return value;
    }

    /// <summary>
    /// 表达式的扩展变量来源
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void Evaluator_PreEvaluateVariable(object? sender, VariablePreEvaluationEventArg e)
    {
        var obj = GlobalData.AllVariables.FirstOrDefault(it => it.Name == e.Name);
        if (obj == null)
        {
            return;
        }
        if (obj.Value != null)
            e.Value = obj.Value;
    }
}