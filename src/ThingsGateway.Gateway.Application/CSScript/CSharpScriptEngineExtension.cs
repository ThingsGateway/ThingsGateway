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

using System.Reflection;
using System.Text;
using ThingsGateway.Foundation.Common.Caching;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    private const string CacheKey = $"{nameof(CSharpScriptEngineExtension)}-{nameof(Do)}";

    private static readonly object m_waiterLock = new object();

    static CSharpScriptEngineExtension()
    {
        var temp = Environment.GetEnvironmentVariable("CSS_CUSTOM_TEMPDIR");
        if (string.IsNullOrWhiteSpace(temp))
        {
            var tempDir = Path.Combine(AppContext.BaseDirectory, "CSSCRIPT");
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir);
                }
                catch
                {
                }
            }

            Directory.CreateDirectory(tempDir);//重新创建，防止缓存的一些目录信息错误
            Environment.SetEnvironmentVariable("CSS_CUSTOM_TEMPDIR", tempDir); //传入变量
        }
        Instance.KeyExpired += Instance_KeyExpired;
    }

    private static void Instance_KeyExpired(object sender, KeyEventArgs e)
    {
        try
        {
            if (Instance.GetAll().TryGetValue(e.Key, out var item))
            {
                item?.Value?.TryDispose();
                item?.Value?.GetType().Assembly.Unload();
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

    /// <summary>
    /// 执行脚本获取返回值
    /// </summary>
    public static T Do<T>(string source, TimeSpan timeSpan, params Assembly[] assemblies) where T : class
    {
        if (source.IsNullOrEmpty()) return null;
        var field = $"{CacheKey}-{source}";
        var exfield = $"{CacheKey}-Exception-{source}";
        var runScript = Instance.Get<T>(field);
        if (runScript == null)
        {
            lock (m_waiterLock)
            {
                var hasValue = Instance.TryGetValue<T>(field, out runScript);
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

                    var evaluator = CSScript.Evaluator;
                    foreach (var item in assemblies)
                    {
                        evaluator = evaluator.ReferenceAssembly(item.Location);
                    }
                    try
                    {
                        // 动态加载并执行代码
                        runScript = evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<T>(
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
    ");
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Instance.Set(field, runScript);

                    }
                    catch (NullReferenceException)
                    {
                        //如果编译失败，应该不重复编译，避免oom
                        Instance.Set<T>(field, null, timeSpan);

                        string exString = string.Format("无法识别正确的接口类，需要实现 {0} 类型", typeof(T).FullName);
                        throw new(exString);
                    }
                    catch (Exception ex)
                    {
                        //如果编译失败，应该不重复编译，避免oom
                        Instance.Set<T>(field, null, timeSpan);
                        Instance.Set(exfield, ex, timeSpan);
                        throw;
                    }
                }
            }
        }
        Instance.SetExpire(field, timeSpan);
        Instance.SetExpire(exfield, timeSpan);
        if (runScript == null)
        {
            throw (Instance.Get<Exception>(exfield) ?? new Exception("compilation error"));
        }
        return runScript;
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
        Instance.SetExpire(field, timeSpan ?? TimeSpan.FromHours(1));
    }
}
