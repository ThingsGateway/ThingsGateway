//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using System.Text;
using ThingsGateway.Foundation.Common.Caching;
using ThingsGateway.Gateway.Application.Extensions;
using Westwind.Scripting;
using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    private const string CacheKey = $"{nameof(CSharpScriptEngineExtension)}-{nameof(Do)}";

    private static readonly object m_waiterLock = new object();
    public static readonly string ExpressionEvaluatorExtensionDir =
  Path.Combine(AppContext.BaseDirectory, "CSSCRIPT");
    static CSharpScriptEngineExtension()
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
                if (item.Expired)
                {
                    var data = (CacheItem)item?.Value;
                    data.Obj?.TryDispose();
                    data.ALC?.Unload();
                    CSharpScriptExecution.MarkDelete(data.Path);
                }
            }
        }
        catch
        {
        }
    }

    public static void Remove(string source)
    {
        Instance.Remove($"{CacheKey}-{source}");
    }

    private static MemoryCache Instance { get; } = new MemoryCache();
    static TimeSpan time = TimeSpan.FromDays(7);

    /// <summary>
    /// 执行脚本获取返回值
    /// </summary>
    public static T Do<T>(string source, TimeSpan timeSpan, params Assembly[] assemblies) where T : class
    {
        if (source.IsNullOrEmpty()) return null;
        var key = source.GetHashCode().ToString();
        var exfield = $"Exception-{key}";
        var runScript = Instance.Get<CacheItem>(key);
        if (runScript.Obj == null)
        {
            var hasValue = Instance.TryGetValue<CacheItem>(key, out runScript);
            if (!hasValue)
            {
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

                var context = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
                var script = new CSharpScriptExecution();
                script.AlternateAssemblyLoadContext = context;
                foreach (var item in assemblies)
                {
                    script.AddAssembly(item);
                }
                try
                {
                    // 动态加载并执行代码
                    var code =
                               $@"
        using System;
        using System.Linq;
        using System.Threading.Tasks;
        using System.Threading;
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
        {_body.ToString()}
    ";


                    var readWriteExpressions = script.CompileClassWithFile(code) as T;
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
                catch (NullReferenceException)
                {
                    //如果编译失败，应该不重复编译，避免oom
                    Instance.Set<CacheItem>(key, default, timeSpan);

                    string exString = string.Format("无法识别正确的接口类，需要实现 {0} 类型", typeof(T).FullName);
                    throw new(exString);
                }
                catch (Exception ex)
                {
                    //如果编译失败，应该不重复编译，避免oom
                    Instance.Set<CacheItem>(key, default, timeSpan);
                    Instance.Set(exfield, ex, timeSpan);
                    throw;
                }


                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        Instance.SetExpire(key, timeSpan);
        Instance.SetExpire(exfield, timeSpan);
        if (runScript.Obj == null)
        {
            var ex = ((Instance.Get<Exception>(exfield)) ?? new Exception("compilation error"));
            ExceptionDispatchInfo.Capture(ex).Throw();
        }
        return (T)runScript.Obj;
    }
    /// <summary>
    /// 执行脚本获取返回值
    /// </summary>
    public static T Do<T>(string source, params Assembly[] assemblies) where T : class
    {
        return Do<T>(source, TimeSpan.FromDays(7), assemblies);
    }

    public static void SetExpire(string source, TimeSpan? timeSpan = null)
    {
        var field = $"{CacheKey}-{source}";
        Instance.SetExpire(field, timeSpan ?? time);
    }
}
