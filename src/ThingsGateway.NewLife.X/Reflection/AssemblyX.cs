﻿
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using NewLife.Collections;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NewLife.Reflection;

/// <summary>程序集辅助类。使用Create创建，保证每个程序集只有一个辅助类</summary>
public class AssemblyX
{
    #region 属性

    /// <summary>程序集</summary>
    public Assembly Asm { get; }

    private String? _Name;

    /// <summary>名称</summary>
    public String Name => _Name ??= "" + Asm.GetName().Name;

    private String? _Version;

    /// <summary>程序集版本</summary>
    public String Version => _Version ??= "" + Asm.GetName().Version;

    private String? _Title;

    /// <summary>程序集标题</summary>
    public String Title => _Title ??= "" + Asm.GetCustomAttributeValue<AssemblyTitleAttribute, String>();

    private String? _FileVersion;

    /// <summary>文件版本</summary>
    public String FileVersion
    {
        get
        {
            if (_FileVersion == null)
            {
                var ver = Asm.GetCustomAttributeValue<AssemblyInformationalVersionAttribute, String>();
                if (!ver.IsNullOrEmpty())
                {
                    var p = ver.IndexOf('+');
                    if (p > 0) ver = ver[..p];
                }
                _FileVersion = ver;
            }

            _FileVersion ??= Asm.GetCustomAttributeValue<AssemblyFileVersionAttribute, String>();

            _FileVersion ??= "";

            return _FileVersion;
        }
    }

    private DateTime? _Compile;

    /// <summary>编译时间</summary>
    public DateTime Compile
    {
        get
        {
            if (_Compile == null)
            {
                var time = GetCompileTime(Version);
                if (time == time.Date && FileVersion.Contains("-beta")) time = GetCompileTime(FileVersion);

                _Compile = time;
            }
            return _Compile.Value;
        }
    }

    private String? _Company;

    /// <summary>公司名称</summary>
    public String Company => _Company ??= "" + Asm.GetCustomAttributeValue<AssemblyCompanyAttribute, String>();

    private String? _Description;

    /// <summary>说明</summary>
    public String Description => _Description ??= "" + Asm.GetCustomAttributeValue<AssemblyDescriptionAttribute, String>();

    /// <summary>获取包含清单的已加载文件的路径或 UNC 位置。</summary>
    public String? Location
    {
        get
        {
            try
            {
                return Asm == null || Asm.IsDynamic ? null : Asm.Location;
            }
            catch { return null; }
        }
    }

    #endregion 属性

    #region 构造

    private AssemblyX(Assembly asm) => Asm = asm;

    private static readonly ConcurrentDictionary<Assembly, AssemblyX> cache = new();

    /// <summary>创建程序集辅助对象</summary>
    /// <param name="asm"></param>
    /// <returns></returns>
    public static AssemblyX? Create(Assembly? asm)
    {
        if (asm == null) return null;

        return cache.GetOrAdd(asm, key => new AssemblyX(key));
    }

    static AssemblyX()
    {
        //AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private static Assembly? OnAssemblyResolve(Object? sender, ResolveEventArgs args)
    {
        try
        {
            // 尝试在请求者所在目录加载
            var file = args.RequestingAssembly?.Location;
            if (!file.IsNullOrEmpty() && !args.Name.IsNullOrEmpty())
            {
                var name = args.Name;
                var p = name.IndexOf(',');
                if (p > 0) name = name[..p];

                file = Path.GetDirectoryName(file).CombinePath(name + ".dll");
                if (File.Exists(file)) return Assembly.LoadFrom(file);
            }
        }
        catch
        {
        }

        return null;
    }

    #endregion 构造

    #region 扩展属性

    /// <summary>是否系统程序集</summary>
    public Boolean IsSystemAssembly => CheckSystem(Asm);

    private static Boolean CheckSystem(Assembly asm)
    {
        if (asm == null) return false;

        var name = asm.FullName;
        if (name.IsNullOrEmpty()) return false;

        if (name.EndsWith("PublicKeyToken=b77a5c561934e089")) return true;
        if (name.EndsWith("PublicKeyToken=b03f5f7f11d50a3a")) return true;
        if (name.EndsWith("PublicKeyToken=89845dcd8080cc91")) return true;
        if (name.EndsWith("PublicKeyToken=31bf3856ad364e35")) return true;

        return false;
    }

    #endregion 扩展属性

    #region 静态属性

    /// <summary>入口程序集</summary>
    public static AssemblyX? Entry { get; set; } = Create(Assembly.GetEntryAssembly());

    /// <summary>
    /// 加载过滤器，如果返回 false 表示跳过加载。
    /// </summary>
    public static Func<String, Boolean>? ResolveFilter { get; set; }

    #endregion 静态属性

    #region 方法

    private readonly ConcurrentDictionary<String, Type?> typeCache2 = new();

    /// <summary>从程序集中查找指定名称的类型</summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public Type? GetType(String typeName)
    {
        if (String.IsNullOrEmpty(typeName)) throw new ArgumentNullException(nameof(typeName));

        return typeCache2.GetOrAdd(typeName, GetTypeInternal);
    }

    /// <summary>在程序集中查找类型</summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private Type? GetTypeInternal(String typeName)
    {
        var type = Asm.GetType(typeName);
        if (type != null) return type;

        // 如果没有包含圆点，说明其不是FullName
        if (!typeName.Contains('.'))
        {
            //try
            //{
            //    var types = Asm.GetTypes();
            //    if (types != null && types.Length > 0)
            //    {
            //        foreach (var item in types)
            //        {
            //            if (item.Name == typeName) return item;
            //        }
            //    }
            //}
            //catch (ReflectionTypeLoadException ex)
            //{
            //    if (XTrace.Debug)
            //    {
            //        //XTrace.WriteException(ex);
            //        XTrace.WriteLine("加载[{0}]{1}的类型时发生个{2}错误！", this, Location, ex.LoaderExceptions.Length);

            //        foreach (var item in ex.LoaderExceptions)
            //        {
            //            XTrace.WriteException(item);
            //        }
            //    }

            //    return null;
            //}
            //catch (Exception ex)
            //{
            //    if (XTrace.Debug) XTrace.WriteException(ex);

            //    return null;
            //}

            // 遍历所有类型，包括内嵌类型
            foreach (var item in Asm.GetTypes())
            {
                if (item.Name == typeName) return item;
            }
        }

        return null;
    }

    #endregion 方法

    #region 静态加载

    /// <summary>根据名称获取类型</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
    public static Type? GetType(String typeName, Boolean isLoadAssembly)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;

        // 数组
        if (typeName.EndsWith("[]"))
        {
            var elemType = GetType(typeName[0..^2], isLoadAssembly);
            if (elemType == null) return null;

            return elemType.MakeArrayType();
        }

        // 加速基础类型识别，忽略大小写
        if (!typeName.Contains('.'))
        {
            foreach (var item in Enum.GetNames(typeof(TypeCode)))
            {
                if (typeName.EqualIgnoreCase(item))
                {
                    type = Type.GetType("System." + item);
                    if (type != null) return type;
                }
            }
        }

        // 尝试本程序集
        var asms = new[] {
            Create(Assembly.GetExecutingAssembly()),
            Create(Assembly.GetCallingAssembly()),
            Create(Assembly.GetEntryAssembly()) };
        var loads = new List<AssemblyX>();

        foreach (var asm in asms)
        {
            if (asm == null || loads.Contains(asm)) continue;
            loads.Add(asm);

            type = asm.GetType(typeName);
            if (type != null) return type;
        }

        // 尝试所有程序集
        foreach (var asm in GetAssemblies())
        {
            if (loads.Contains(asm)) continue;
            loads.Add(asm);

            type = asm.GetType(typeName);
            if (type != null) return type;
        }

        // 尝试加载只读程序集
        if (!isLoadAssembly) return null;

        foreach (var asm in ReflectionOnlyGetAssemblies())
        {
            type = asm.GetType(typeName);
            if (type != null)
            {
                // 真实加载
                var file = asm.Asm.Location;
                try
                {
                    type = null;
                    var asm2 = Assembly.LoadFrom(file);
                    var type2 = Create(asm2)?.GetType(typeName);
                    if (type2 == null) continue;

                    type = type2;
                }
                catch
                {
                }

                return type;
            }
        }

        return null;
    }

    /// <summary>获取指定程序域所有程序集</summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public static IEnumerable<AssemblyX> GetAssemblies(AppDomain? domain = null)
    {
        domain ??= AppDomain.CurrentDomain;

        var asms = domain.GetAssemblies();
        if (asms == null || asms.Length <= 0) yield break;

        //return asms.Select(item => Create(item));
        foreach (var item in asms)
        {
            var rs = Create(item);
            if (rs != null) yield return rs;
        }
    }

    private static ICollection<String>? _AssemblyPaths;

    /// <summary>程序集目录集合</summary>
    public static ICollection<String> AssemblyPaths
    {
        [return: NotNull]
        get
        {
            if (_AssemblyPaths == null)
            {
                var set = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

                var basedir = AppDomain.CurrentDomain.BaseDirectory;
                if (!basedir.IsNullOrEmpty()) set.Add(basedir);

                _AssemblyPaths = set;
            }
            return _AssemblyPaths;
        }
        set => _AssemblyPaths = value;
    }

    /// <summary>获取当前程序域所有只反射程序集的辅助类。NETCore不支持只反射加载，该方法动态加载DLL后返回</summary>
    /// <returns></returns>
    public static IEnumerable<AssemblyX> ReflectionOnlyGetAssemblies()
    {
        var loadeds = GetAssemblies().ToList();

        // 先返回已加载的只加载程序集
        var loadeds2 = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select(e => Create(e)).ToList();
        foreach (var item in loadeds2)
        {
            if (item == null) continue;

            if (loadeds.Any(e => e.Location.EqualIgnoreCase(item.Location))) continue;
            // 尽管目录不一样，但这两个可能是相同的程序集
            // 这里导致加载了不同目录的同一个程序集，然后导致对象容器频繁报错
            //if (loadeds.Any(e => e.Asm.FullName.EqualIgnoreCase(item.Asm.FullName))) continue;
            // 相同程序集不同版本，全名不想等
            if (loadeds.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(item.Asm.GetName().Name))) continue;

            yield return item;
        }

        foreach (var item in AssemblyPaths)
        {
            foreach (var asm in ReflectionOnlyLoad(item)) yield return asm;
        }
    }

    private static readonly ConcurrentHashSet<String> _BakImages = new();

    /// <summary>只反射加载指定路径的所有程序集。NETCore不支持只反射加载，该方法动态加载DLL后返回</summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<AssemblyX> ReflectionOnlyLoad(String path)
    {
        if (!Directory.Exists(path)) yield break;

        // 先返回已加载的只加载程序集
        //var loadeds2 = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select(e => Create(e)).ToList();
        var loadeds2 = new List<AssemblyX>();
        foreach (var item in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
        {
            var ax = Create(item);
            if (ax != null) loadeds2.Add(ax);
        }

        // 再去遍历目录
        var ss = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        if (ss == null || ss.Length <= 0) yield break;

        var loadeds = GetAssemblies().ToList();

        var ver = new Version(Assembly.GetExecutingAssembly().ImageRuntimeVersion.TrimStart('v'));

        foreach (var item in ss)
        {
            // 仅尝试加载dll
            if (!item.EndsWithIgnoreCase(".dll")) continue;
            if (_BakImages.Contains(item)) continue;

            if (loadeds.Any(e => e.Location.EqualIgnoreCase(item)) ||
                loadeds2.Any(e => e.Location.EqualIgnoreCase(item))) continue;

            Assembly? asm = null;
            try
            {
                asm = Assembly.LoadFrom(item);
            }
            catch
            {
                _BakImages.TryAdd(item);
            }
            if (asm == null) continue;

            // 不搜索系统程序集，优化性能
            if (CheckSystem(asm)) continue;

            // 尽管目录不一样，但这两个可能是相同的程序集
            // 这里导致加载了不同目录的同一个程序集，然后导致对象容器频繁报错
            //if (loadeds.Any(e => e.Asm.FullName.EqualIgnoreCase(asm.FullName)) ||
            //    loadeds2.Any(e => e.Asm.FullName.EqualIgnoreCase(asm.FullName))) continue;
            // 相同程序集不同版本，全名不想等
            if (loadeds.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(asm.GetName().Name)) ||
                loadeds2.Any(e => e.Asm.GetName().Name.EqualIgnoreCase(asm.GetName().Name))) continue;

            var asmx = Create(asm);
            if (asmx != null) yield return asmx;
        }
    }

    /// <summary>获取当前应用程序的所有程序集，不包括系统程序集，仅限本目录</summary>
    /// <returns></returns>
    public static List<AssemblyX> GetMyAssemblies()
    {
        var list = new List<AssemblyX>();
        var hs = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
        var cur = AppDomain.CurrentDomain.BaseDirectory;
        foreach (var asmx in GetAssemblies())
        {
            // 加载程序集列表很容易抛出异常，全部屏蔽
            try
            {
                if (asmx.FileVersion.IsNullOrEmpty()) continue;

                var file = "";
                //file = asmx.Asm.CodeBase;
                if (file.IsNullOrEmpty()) file = asmx.Asm.Location;
                if (file.IsNullOrEmpty()) continue;

                if (file.StartsWith("file:///"))
                {
                    file = file.TrimStart("file:///");
                    if (Path.DirectorySeparatorChar == '\\')
                        file = file.Replace('/', '\\');
                    else
                        file = file.Replace('\\', '/').EnsureStart("/");
                }
                if (file.IsNullOrEmpty()) continue;
                if (!file.StartsWithIgnoreCase(cur)) continue;

                if (!hs.Contains(file))
                {
                    hs.Add(file);
                    list.Add(asmx);
                }
            }
            catch { }
        }
        return list;
    }

    #endregion 静态加载

    #region 重载

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString()
    {
        if (!String.IsNullOrEmpty(Title))
            return Title;
        else
            return Name;
    }

    ///// <summary>判断两个程序集是否相同，避免引用加载和执行上下文加载的相同程序集显示不同</summary>
    ///// <param name="asm1"></param>
    ///// <param name="asm2"></param>
    ///// <returns></returns>
    //public static Boolean Equal(Assembly asm1, Assembly asm2)
    //{
    //    if (asm1 == asm2) return true;

    //    return asm1.FullName == asm2.FullName;
    //}

    #endregion 重载

    #region 辅助

    /// <summary>根据版本号计算得到编译时间</summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static DateTime GetCompileTime(String version)
    {
        var ss = version?.Split(['.']);
        if (ss == null || ss.Length < 4) return DateTime.MinValue;

        var d = ss[2].ToInt();
        var s = ss[3].ToInt();
        var y = DateTime.Today.Year;

        // 指定年月日的版本格式 1.0.yyyy.mmdd-betaHHMM
        if (d <= y && d >= y - 10)
        {
            var dt = new DateTime(d, 1, 1);
            if (s > 0)
            {
                if (s >= 200) dt = dt.AddMonths(s / 100 - 1);
                s %= 100;
                if (s > 1) dt = dt.AddDays(s - 1);
            }
            else
            {
                var str = ss[3];
                var p = str.IndexOf('-');
                if (p > 0)
                {
                    s = str[..p].ToInt();
                    if (s > 0)
                    {
                        if (s >= 200) dt = dt.AddMonths(s / 100 - 1);
                        s %= 100;
                        if (s > 1) dt = dt.AddDays(s - 1);
                    }

                    if (str.Length >= 4 + 1 + 4)
                    {
                        s = str[^4..].ToInt();
                        if (s > 0) dt = dt.AddHours(s / 100).AddMinutes(s % 100).ToLocalTime();
                    }
                }
            }

            return dt;
        }
        else
        {
            var dt = new DateTime(2000, 1, 1);
            dt = dt.AddDays(d).AddSeconds(s * 2);

            return dt;
        }
    }

    #endregion 辅助
}