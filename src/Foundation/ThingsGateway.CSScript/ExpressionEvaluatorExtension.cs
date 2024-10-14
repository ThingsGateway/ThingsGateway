//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScripting;

using CSScriptLib;

using System.Text;

using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Threading;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 读写表达式脚本
/// </summary>
public interface ReadWriteExpressions
{
    /// <summary>
    /// 获取新值
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    object GetNewValue(object a);
}

/// <summary>
/// 表达式扩展
/// </summary>
public static class ExpressionEvaluatorExtension
{
    private static string CacheKey = $"{nameof(ExpressionEvaluatorExtension)}-{nameof(GetReadWriteExpressions)}";

    private static object m_waiterLock = new object();

    /// <summary>清理计时器</summary>
    private static TimerX? _clearTimer;
    static ExpressionEvaluatorExtension()
    {
        if (_clearTimer == null)
        {
            _clearTimer = new TimerX(RemoveNotAlive, null, 30 * 1000, 60 * 1000) { Async = true };
        }
    }

    private static void RemoveNotAlive(Object? state)
    {
        //检测缓存
        try
        {
            var data = Instance.GetAll();
            lock (m_waiterLock)
            {

                foreach (var item in data)
                {
                    if (item.Value!.ExpiredTime < item.Value.VisitTime + 1800_000)
                    {
                        Instance.Remove(item.Key);
                        item.Value?.Value?.GetType().Assembly.Unload();
                        GC.Collect();
                    }
                }
            }
        }
        catch
        {
        }

    }

    private static MemoryCache Instance { get; set; } = new MemoryCache();

    /// <summary>
    /// 添加或获取脚本，非线程安全
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReadWriteExpressions GetOrAddScript(string source)
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            if (!source.Contains("return"))
            {
                source = $"return {source}";//只判断简单脚本中可省略return字符串
            }

            var src = source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var _using = new StringBuilder();
            var _body = new StringBuilder();
            src.ToList().ForEach(l =>
            {
                if (l.StartsWith("using "))
                {
                    _using.AppendLine(l);
                }
                else
                {
                    _body.AppendLine(l);
                }

            });
            // 动态加载并执行代码
            runScript = CSScript.Evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<ReadWriteExpressions>(
                $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.NewLife;
        using ThingsGateway.NewLife.Extension;
        using ThingsGateway.Gateway.Application.Extensions;
        {_using}
        public class Script:ReadWriteExpressions
        {{
            public object GetNewValue(object raw)
            {{
                   {_body};
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
    public static object GetExpressionsResult(this string expressions, object rawvalue)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            return rawvalue;
        }
        var readWriteExpressions = GetReadWriteExpressions(expressions);
        var value = readWriteExpressions.GetNewValue(rawvalue);
        return value;
    }

    /// <summary>
    /// 执行脚本获取返回值ReadWriteExpressions
    /// </summary>
    public static ReadWriteExpressions GetReadWriteExpressions(string source)
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            lock (m_waiterLock)
            {
                runScript = GetOrAddScript(source);
            }
        }
        Instance.SetExpire(field, TimeSpan.FromHours(1));

        return runScript;
    }
}
