//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Runtime.Loader;
using System.Text;
using ThingsGateway.Foundation.Common.Caching;
using Westwind.Scripting;
using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 读写表达式脚本
/// </summary>
public abstract class MemoryReadWriteExpressions
{
    public WeakReference<TouchSocket.Core.ILog> Log { get; set; }
    public TouchSocket.Core.ILog? Logger => Log.TryGetTarget(out var log) ? log : null;
    public VariableRuntime Tag(string device, string variable)
    {
        Tags.Add((device, variable));
        var result = GlobalData.GetVariable(device, variable);
        if (result == null)
        {
            throw new Exception($"can not found variable：{device}.{variable}");
        }
        return result;
    }

    public HashSet<(string, string)> Tags = new();
    /// <summary>
    /// 获取新值
    /// </summary>
    public abstract object GetNewValue();
}

/// <summary>
/// 表达式扩展
/// </summary>
public static class MemoryExpressionEvaluatorExtension
{
    private static readonly object m_waiterLock = new object();
    public static readonly string ExpressionEvaluatorExtensionDir =
  Path.Combine(AppContext.BaseDirectory, "CSSCRIPT");
    static MemoryExpressionEvaluatorExtension()
    {
        Directory.CreateDirectory(ExpressionEvaluatorExtensionDir);
        Instance.KeyExpired += Instance_KeyExpired;
    }

    private static void Instance_KeyExpired(object sender, KeyEventArgs e)
    {
        try
        {
            if (Instance.GetAll().TryGetValue(e.Key, out var item))
            {
                var data = (CacheItem)item?.Value;
                data.Obj?.TryDispose();
                data.ALC?.Unload();
                CSharpScriptExecution.MarkDelete(data.Path);
            }
        }
        catch
        {
        }
    }

    private static MemoryCache Instance { get; set; } = new MemoryCache();
    static TimeSpan time = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 添加或获取脚本，非线程安全
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static MemoryReadWriteExpressions GetOrAddScript(string source)
    {
        if (string.IsNullOrEmpty(source)) return null;
        var key = source.GetHashCode().ToString();
        var runScript = Instance.Get<CacheItem>(key);
        if (runScript.Obj == null)
        {
            var hasValue = Instance.TryGetValue<CacheItem>(key, out runScript);
            if (!hasValue)
            {

                if (!source.Contains("return"))
                {
                    source = $"return {source}";//只判断简单脚本中可省略return字符串
                }
                var src = source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                using var _using = new ValueStringBuilder();
                using var _body = new ValueStringBuilder();
                foreach (var item in src)
                {
                    if (item.StartsWith("using "))
                    {
                        _using.AppendLine(item);
                    }
                    else
                    {
                        _body.AppendLine(item);
                    }
                }
                // 动态加载并执行代码
                try
                {
                    var context = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
                    var script = new CSharpScriptExecution();
                    script.AlternateAssemblyLoadContext = context;
                    var code =
                    $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.Foundation.Common.StringExtension;
        using ThingsGateway.Foundation.Common;
        using ThingsGateway.Foundation.Common.Extension;
        using ThingsGateway.Foundation.Common.Json.Extension;
        using ThingsGateway.Gateway.Application.Extensions;
        {_using.ToString()}
        public class Script:MemoryReadWriteExpressions
        {{
            public override object GetNewValue()
            {{
                   {_body.ToString()};
            }}

        }}
    ";

                    var readWriteExpressions = script.CompileClassWithFile(code) as MemoryReadWriteExpressions;
                    if (readWriteExpressions == null)
                    {
                        CSharpScriptExecution.MarkDelete(script.OutputAssembly);
                        throw new Exception("compilation error");
                    }
                    runScript.Obj = readWriteExpressions;
                    runScript.ALC = context;
                    runScript.Path = script.OutputAssembly;

                    Instance.Set(key, runScript);

                }
                catch (Exception ex)
                {
                    //如果编译失败，应该不重复编译，避免oom
                    Instance.Set<CacheItem>(key, default, time);
                    var exfield = $"Exception-{key}";
                    Instance.Set(exfield, ex, time);
                    throw;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        Instance.SetExpire(key, time);
        if (runScript.Obj == null)
        {
            var exfield = $"Exception-{key}";
            throw (Instance.Get<Exception>(exfield) ?? new Exception("compilation error"));
        }
        return (MemoryReadWriteExpressions)runScript.Obj;
    }

    public static object GetMemoryExpressionsResult(this string expressions)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            throw new ArgumentNullException(nameof(expressions));
        }
        var readWriteExpressions = GetMemoryReadWriteExpressions(expressions);
        var value = readWriteExpressions.GetNewValue();
        return value;
    }

    public static object GetMemoryExpressionsResult(this string expressions, TouchSocket.Core.ILog logger)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            throw new ArgumentNullException(nameof(expressions));
        }
        var readWriteExpressions = GetMemoryReadWriteExpressions(expressions);
        readWriteExpressions.Log = new WeakReference<TouchSocket.Core.ILog>(logger);
        var value = readWriteExpressions.GetNewValue();
        return value;
    }

    /// <summary>
    /// 执行脚本获取返回值MemoryReadWriteExpressions
    /// </summary>
    public static MemoryReadWriteExpressions GetMemoryReadWriteExpressions(string source)
    {
        var field = source;
        var runScript = Instance.Get<MemoryReadWriteExpressions>(field);
        if (runScript == null)
        {
            lock (m_waiterLock)
            {
                runScript = GetOrAddScript(source);
            }
        }
        Instance.SetExpire(field, time);

        return runScript;
    }
    public static void SetExpire(string source, TimeSpan? timeSpan = null)
    {
        var field = source;
        Instance.SetExpire(field, timeSpan ?? TimeSpan.FromHours(1));
    }
}
