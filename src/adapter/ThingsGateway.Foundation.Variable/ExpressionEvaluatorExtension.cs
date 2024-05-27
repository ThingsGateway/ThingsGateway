//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScriptLib;

namespace ThingsGateway.Foundation;

/// <summary>
/// 读写表达式脚本
/// </summary>
public interface ReadWriteExpressions
{
    object GetNewValue(dynamic a);
}

/// <summary>
/// 表达式扩展
/// </summary>
public static class ExpressionEvaluatorExtension
{
    /// <summary>
    /// 执行脚本获取返回值
    /// </summary>
    public static ReadWriteExpressions GetReadWriteExpressions(string source)
    {
        // 生成缓存键
        var cacheKey = $"{nameof(ExpressionEvaluatorExtension)}-{nameof(GetReadWriteExpressions)}-{source}";
        var runScript = NewLife.Caching.MemoryCache.Instance.GetOrAdd(cacheKey, c =>
        {
            // 清理输入源字符串
            source = source.Trim();
            if (!source.Contains("return"))
            {
                source = $"return {source}";//只判断简单脚本中可省略return字符串
            }

            // 动态加载并执行代码
            var runScript = CSScript.Evaluator.LoadCode<ReadWriteExpressions>(
                $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using ThingsGateway.Foundation;
        using ThingsGateway.Foundation.Json.Extension;
        public class Script:ReadWriteExpressions
        {{
            public object GetNewValue(dynamic raw)
            {{
                {source};
            }}
        }}
    ");
            return runScript;
        });
        return runScript;
    }

    /// <summary>
    /// 计算表达式：例如：raw*100，raw为原始值
    /// </summary>
    public static object GetExpressionsResult(this string expressions, object? rawvalue)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            return rawvalue;
        }

        var readWriteExpressions = GetReadWriteExpressions(expressions);
        var value = readWriteExpressions.GetNewValue(rawvalue);
        return value;
    }
}
