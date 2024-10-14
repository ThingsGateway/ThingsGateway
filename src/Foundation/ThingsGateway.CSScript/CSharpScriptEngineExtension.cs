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
using System.Text;

using CSScripting;

using CSScriptLib;

using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Threading;

namespace ThingsGateway.Gateway.Application;


/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    private static string CacheKey = $"{nameof(CSharpScriptEngineExtension)}-{nameof(Do)}";

    private static object m_waiterLock = new object();

    /// <summary>清理计时器</summary>
    private static TimerX? _clearTimer;
    static CSharpScriptEngineExtension()
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
    /// 执行脚本获取返回值
    /// </summary>
    public static T Do<T>(string source, params Assembly[] assemblies) where T : class
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<T>(field);
        if (runScript == null)
        {
            lock (m_waiterLock)
            {
                runScript = Instance.Get<T>(field);
                if (runScript == null)
                {

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
                    var evaluator = CSScript.Evaluator;
                    foreach (var item in assemblies)
                    {
                        evaluator = evaluator.ReferenceAssembly(item.Location);
                    }
                    // 动态加载并执行代码
                    runScript = evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<T>(
                       $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.NewLife;
        using ThingsGateway.NewLife.Extension;
        using ThingsGateway.Gateway.Application.Extensions;
        {_using}
        {_body}
    ");
                    GC.Collect();
                    Instance.Set(field, runScript);
                }
            }

        }
        Instance.SetExpire(field, TimeSpan.FromHours(1));

        return runScript;
    }



}


