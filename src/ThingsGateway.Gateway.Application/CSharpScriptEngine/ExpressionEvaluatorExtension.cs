//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScripting;

using CSScriptLib;

using NewLife.Caching;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 读写表达式脚本
/// </summary>
public interface ReadWriteExpressions
{
    object GetNewValue(object a);
}

/// <summary>
/// 表达式扩展
/// </summary>
public static class ExpressionEvaluatorExtension
{
    static ExpressionEvaluatorExtension()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                await Task.Delay(30000);
                //检测缓存
                try
                {
                    var data = Instance.GetAll();
                    m_waiterLock.Wait();

                    foreach (var item in data)
                    {
                        if (item.Value.ExpiredTime < item.Value.VisitTime + 1800_000)
                        {
                            Instance.Remove(item.Key);
                            item.Value?.Value?.GetType().Assembly.Unload();
                            GC.Collect();
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    m_waiterLock.Release();
                }

                await Task.Delay(30000);
            }
        });
    }

    private static MemoryCache Instance { get; set; } = new MemoryCache();
    private static EasyLock m_waiterLock = new EasyLock();
    private static string CacheKey = $"{nameof(ExpressionEvaluatorExtension)}-{nameof(GetReadWriteExpressions)}";

    /// <summary>
    /// 执行脚本获取返回值ReadWriteExpressions
    /// </summary>
    public static ReadWriteExpressions GetReadWriteExpressions(string source)
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            try
            {
                m_waiterLock.Wait();
                {
                    runScript = AddScript(source);
                }
            }
            finally
            {
                m_waiterLock.Release();
            }
        }
        Instance.SetExpire(field, TimeSpan.FromHours(1));

        return runScript;
    }

    internal static ReadWriteExpressions AddScript(string source)
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            if (!source.Contains("return"))
            {
                source = $"return {source}";//只判断简单脚本中可省略return字符串
            }
            // 动态加载并执行代码
            runScript = CSScript.Evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<ReadWriteExpressions>(
                $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.Foundation;
        using ThingsGateway.Gateway.Application.Extensions;
        public class Script:ReadWriteExpressions
        {{
            public object GetNewValue(object raw)
            {{
                {source};
            }}
        }}
    ");
            GC.Collect();
            Instance.Set(field, runScript);
        }
        return runScript;
    }

    /// <summary>
    /// 计算表达式：例如：(int)raw*100，raw为原始值
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
