//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Xml.Linq;

using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;

using UAParser;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class PluginService : IPluginService
{
    public const string DefaultKey = "默认";
    public const string DirName = "Plugins";
    private const string _cacheKeyGetPluginOutputs = $"{ThingsGatewayCacheConst.Cache_Prefix}{nameof(PluginService)}{nameof(GetList)}";
    private const string _cacheKeyGetDriverMethodInfos = $"{nameof(PluginService)}_{nameof(GetDriverMethodInfos)}";
    private const string _cacheKeyGetDriverPropertyTypes = $"{nameof(PluginService)}_{nameof(GetDriverPropertyTypes)}";
    private const string _cacheKeyGetVariablePropertyTypes = $"{nameof(PluginService)}_{nameof(GetVariablePropertyTypes)}";

    private readonly IServiceScope _serviceScope;
    private readonly ISimpleCacheService _simpleCacheService;
    private readonly ILogger _logger;
    private readonly EasyLock _locker = new();

    /// <inheritdoc cref="PluginService"/>
    public PluginService(
    IServiceScopeFactory serviceScopeFactory, ISimpleCacheService simpleCacheService,
        ILoggerFactory loggerFactory)
    {
        _simpleCacheService = simpleCacheService;
        Directory.CreateDirectory(AppContext.BaseDirectory.CombinePath(PluginService.DirName));//创建插件文件夹
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = loggerFactory.CreateLogger("驱动插件服务");
        _defaultDriverBaseDict = new(App.EffectiveTypes
            .Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
            .ToDictionary(a => a.Name));
        _stringConverter = new ThingsGatewayStringConverter();
        _stringConverter.Add(new StringToEncodingConverter());
    }

    /// <summary>
    /// 插件文件名称/插件程序集
    /// </summary>
    private ConcurrentDictionary<string, Assembly> _assemblyDict { get; } = new();

    /// <summary>
    /// 插件文件名称/插件域
    /// </summary>
    private ConcurrentDictionary<string, AssemblyLoadContext> _assemblyLoadContextDict { get; } = new();

    /// <summary>
    /// 默认上下文中的插件文件名称.类型名称/插件Type
    /// </summary>
    private ReadOnlyDictionary<string, Type> _defaultDriverBaseDict { get; }

    /// <summary>
    /// 插件文件名称.类型名称/插件Type
    /// </summary>
    private ConcurrentDictionary<string, Type> _driverBaseDict { get; } = new();

    /// <summary>
    /// 字符串转换器，默认支持基础类型和Json。
    /// </summary>
    private ThingsGatewayStringConverter _stringConverter { get; }

    #region public

    /// <summary>
    /// 获取插件信息
    /// </summary>
    /// <returns></returns>
    public List<PluginOutput> GetList(PluginTypeEnum? pluginType = null)
    {
        var pluginList = GetList();
        if (pluginType == null)
        {
            return pluginList;
        }
        // 筛选出PluginTypeEnum属性节点
        var filteredPlugins = pluginList.Select(p => new PluginOutput
        {
            Name = p.Name,
            Children = p.Children.Where(c => c.PluginType == pluginType).ToList()
        }).Where(p => p.Children.Any()).ToList();
        return filteredPlugins;
    }

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <param name="pluginName">主插件程序集文件名称.类型名称</param>
    /// <returns></returns>
    public DriverBase GetDriver(string pluginName)
    {
        try
        {
            _locker.Wait();

            var filtResult = PluginServiceExtension.GetFileNameAndTypeName(pluginName);
            if (filtResult.Item1.IsNullOrEmpty() || filtResult.Item1 == DefaultKey)
            {
                //搜索默认上下文中的类型
                if (_defaultDriverBaseDict.ContainsKey(pluginName))
                {
                    var driver = (DriverBase)Activator.CreateInstance(_defaultDriverBaseDict[pluginName]);
                    driver.Directory = AppContext.BaseDirectory;
                    return driver;
                }
                else
                {
                    throw new($"加载插件 {AppContext.BaseDirectory}-{filtResult.Item2} 失败，插件类型不存在");
                }
            }
            var dir = AppContext.BaseDirectory.CombinePath(DirName, filtResult.Item1);
            //先判断是否已经拥有插件模块
            if (_driverBaseDict.ContainsKey(pluginName))
            {
                var driver = (DriverBase)Activator.CreateInstance(_driverBaseDict[pluginName]);
                driver.Directory = dir;
                return driver;
            }

            Assembly assembly = null;
            //根据路径获取dll文件
            //主程序集路径
            var path = dir.CombinePath($"{filtResult.Item1}.dll");
            assembly = GetAssembly(dir, path, filtResult.Item1);

            if (assembly != null)
            {
                //根据采集/业务类型获取实际插件类
                var driverType = assembly.GetTypes().
                    Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
                    .FirstOrDefault(it => it.Name == filtResult.Item2);
                if (driverType != null)
                {
                    var driver = (DriverBase)Activator.CreateInstance(driverType);
                    _logger?.LogInformation($"加载插件 {path}-{filtResult.Item2} 成功");
                    _driverBaseDict.TryAdd(pluginName, driverType);
                    driver.Directory = dir;
                    return driver;
                }
                throw new($"加载插件 {path}-{filtResult.Item2} 失败，插件类型不存在");
            }
            else
            {
                throw new Exception($"加载插件文件 {path} 失败，文件不存在");
            }
        }
        finally
        {
            _locker.Release();
        }
    }

    /// <summary>
    /// 获取插件的属性类型
    /// </summary>
    public Dictionary<string, DependencyPropertyWithInfo> GetDriverPropertyTypes(string pluginName, DriverBase? driverBase = null)
    {
        lock (this)
        {
            if (!pluginName.IsNullOrEmpty())
            {
                var data = _simpleCacheService.HashGetAll<Dictionary<string, DependencyPropertyWithInfo>>(_cacheKeyGetDriverPropertyTypes);
                if (data != null)
                {
                    if (data.ContainsKey(pluginName))
                    {
                        return data[pluginName];
                    }
                    else
                    {
                        var dispose = driverBase == null;
                        driverBase ??= this.GetDriver(pluginName);
                        return SetCache(driverBase, pluginName, _cacheKeyGetDriverPropertyTypes, dispose);
                    }
                }
                else
                {
                    var dispose = driverBase == null;
                    driverBase ??= this.GetDriver(pluginName);
                    return SetCache(driverBase, pluginName, _cacheKeyGetDriverPropertyTypes, dispose);
                }
            }
            else
            {
                var dispose = driverBase == null;
                driverBase ??= this.GetDriver(pluginName);
                return SetCache(driverBase, pluginName, _cacheKeyGetDriverPropertyTypes, dispose);
            }
            Dictionary<string, DependencyPropertyWithInfo> SetCache(DriverBase driverBase, string pluginName, string cacheKey, bool dispose)
            {
                var dependencyPropertyWithInfos = driverBase.DriverPropertys?.GetType().GetProperties().SelectMany(it =>
        new[] { new { memberInfo = it, attribute = GetCustomAttributeRecursive<DynamicPropertyAttribute>(it) } })
        .Where(x => x.attribute != null).ToList()
          .SelectMany(it => new[]
          {
                  new DependencyPropertyWithInfo(){
                      Name=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      Value=_stringConverter.Serialize(null, it.memberInfo.GetValue(driverBase.DriverPropertys)),
                      PropertyType=it.memberInfo,
                  }
          });
                var result = dependencyPropertyWithInfos.ToDictionary(a => a.Name);
                _simpleCacheService.HashAdd(cacheKey, pluginName, result);
                if (dispose)
                    driverBase.SafeDispose();
                return result;
            }
        }
    }

    /// <summary>
    /// 获取变量的属性类型
    /// </summary>
    public Dictionary<string, DependencyPropertyWithInfo> GetVariablePropertyTypes(string pluginName)
    {
        lock (this)
        {
            var data = _simpleCacheService.HashGetAll<Dictionary<string, DependencyPropertyWithInfo>>(_cacheKeyGetVariablePropertyTypes);
            if (data != null)
            {
                if (data.ContainsKey(pluginName))
                {
                    return data[pluginName];
                }
                else
                {
                    return SetCache(pluginName, _cacheKeyGetVariablePropertyTypes);
                }
            }
            else
            {
                return SetCache(pluginName, _cacheKeyGetVariablePropertyTypes);
            }

            Dictionary<string?, DependencyPropertyWithInfo> SetCache(string pluginName, string cacheKey)
            {
                var driverBase = (BusinessBase)this.GetDriver(pluginName);
                var dependencyPropertyWithInfos = driverBase.VariablePropertys?.GetType().GetProperties().SelectMany(it =>
        new[] { new { memberInfo = it, attribute = GetCustomAttributeRecursive<DynamicPropertyAttribute>(it) } })
        .Where(x => x.attribute != null).ToList()
          .SelectMany(it => new[]
          {
                  new DependencyPropertyWithInfo(){
                      Name=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      Value=_stringConverter.Serialize(null, it.memberInfo.GetValue(driverBase.VariablePropertys)),
                      PropertyType=it.memberInfo,
                  }
          });
                var result = dependencyPropertyWithInfos.ToDictionary(a => a.Description!);
                _simpleCacheService.HashAdd(cacheKey, pluginName, result);
                driverBase.SafeDispose();
                return result;
            }
        }
    }

    /// <summary>
    /// 获取插件的属性类型
    /// </summary>
    public Dictionary<string, DependencyPropertyWithInfo> GetDriverMethodInfos(string pluginName, DriverBase? driverBase = null)
    {
        lock (this)
        {
            if (!pluginName.IsNullOrEmpty())
            {
                var data = _simpleCacheService.HashGetAll<Dictionary<string, DependencyPropertyWithInfo>>(_cacheKeyGetDriverMethodInfos);
                if (data != null)
                {
                    if (data.ContainsKey(pluginName))
                    {
                        return data[pluginName];
                    }
                    else
                    {
                        var dispose = driverBase == null;
                        driverBase ??= this.GetDriver(pluginName);
                        return SetDriverMethodInfosCache(driverBase, pluginName, _cacheKeyGetDriverMethodInfos, dispose);
                    }
                }
                else
                {
                    var dispose = driverBase == null;
                    driverBase ??= this.GetDriver(pluginName);
                    return SetDriverMethodInfosCache(driverBase, pluginName, _cacheKeyGetDriverMethodInfos, dispose);
                }
            }
            else
            {
                var dispose = driverBase == null;
                driverBase ??= this.GetDriver(pluginName);
                return SetDriverMethodInfosCache(driverBase, pluginName, _cacheKeyGetDriverMethodInfos, dispose);
            }

            Dictionary<string, DependencyPropertyWithInfo> SetDriverMethodInfosCache(DriverBase driverBase, string pluginName, string cacheKey, bool dispose)
            {
                var dependencyPropertyWithInfos = driverBase.GetType().GetMethods()?.SelectMany(it =>
            new[] { new { memberInfo = it, attribute = it.GetCustomAttribute<DynamicMethodAttribute>() } })
            .Where(x => x.attribute != null).ToList()
              .SelectMany(it => new[]
              {
                  new DependencyPropertyWithInfo(){
                      Name=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      MethodInfo=it.memberInfo,
                  }
              });
                var result = dependencyPropertyWithInfos.ToDictionary(a => a.Name);
                _simpleCacheService.HashAdd(cacheKey, pluginName, result);
                if (dispose)
                    driverBase.SafeDispose();
                return result;
            }
        }
    }

    /// <summary>
    /// 分页显示插件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public SqlSugarPagedList<PluginOutput> Page(PluginPageInput input)
    {
        var query = GetList(input.PluginType).SelectMany(a => a.Children)
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))//根据关键字查询
         .WhereIF(!string.IsNullOrEmpty(input.FileName), u => u.FileName.Contains(input.FileName));//根据关键字查询

        var pageInfo = query.ToPagedList(input);//分页
        return pageInfo;
    }

    public void Remove()
    {
        try
        {
            _locker.Wait();
            _driverBaseDict.Clear();
            _assemblyDict.Clear();
            foreach (var item in _assemblyLoadContextDict)
            {
                item.Value.Unload();
            }
            _assemblyLoadContextDict.Clear();
            GC.Collect();
            ClearCache();
        }
        finally
        {
            _locker.Release();
        }
    }

    private void ClearCache()
    {
        _simpleCacheService.Remove(_cacheKeyGetDriverMethodInfos);
        _simpleCacheService.Remove(_cacheKeyGetDriverPropertyTypes);
        _simpleCacheService.Remove(_cacheKeyGetVariablePropertyTypes);
        _simpleCacheService.Remove(_cacheKeyGetPluginOutputs);
    }

    /// <summary>
    /// 设置插件的属性值
    /// </summary>
    /// <param name="driver">插件实例</param>
    /// <param name="deviceProperties">插件属性，检索相同名称的属性后写入</param>
    public void SetDriverProperties(DriverBase driver, IEnumerable<DependencyProperty> deviceProperties)
    {
        var pluginPropertys = driver.DriverPropertys?.GetType().GetProperties().Where(a => GetCustomAttributeRecursive<DynamicPropertyAttribute>(a) != null)?.ToList();
        foreach (var propertyInfo in pluginPropertys ?? new())
        {
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.Name == propertyInfo.Name);
            if (deviceProperty == null) continue;
            var value = _stringConverter.Deserialize(null, deviceProperty?.Value ?? string.Empty, propertyInfo.PropertyType);
            propertyInfo.SetValue(driver.DriverPropertys, value);
        }
    }

    /// <summary>
    /// 尝试添加插件，方法完成后会完全卸载插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    [OperDesc("添加/更新插件文件")]
    public async Task AddAsync(PluginAddInput plugin)
    {
        try
        {
            _locker.Wait();
            var assemblyLoadContext = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
            try
            {
                var maxFileSize = 100 * 1024 * 1024;//最大100m

                //主程序集名称
                var mainFileName = Path.GetFileNameWithoutExtension(plugin.MainFile.Name);
                //插件文件夹绝对路径
                var fullDir = AppContext.BaseDirectory.CombinePath(DirName, mainFileName);
                //主程序集绝对路径
                var fullPath = fullDir.CombinePathOS(plugin.MainFile.Name);

                //主程序集相对路径
                //获取文件流
                using var stream = plugin.MainFile.OpenReadStream(maxFileSize);
                using MemoryStream memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                List<(string, MemoryStream)> otherFilesStreams = new();
                //先加载到内存，如果成功添加后再装载到文件
                //获取主程序集
                var assembly = assemblyLoadContext.LoadFromStream(memoryStream);
                foreach (var item in plugin.OtherFiles ?? new())
                {
                    using var otherStream = item.OpenReadStream(maxFileSize);
                    MemoryStream memoryStream1 = new MemoryStream();
                    await otherStream.CopyToAsync(memoryStream1);
                    memoryStream1.Seek(0, SeekOrigin.Begin);
                    otherFilesStreams.Add((item.Name, memoryStream1));
                    try
                    {
                        assemblyLoadContext.LoadFromStream(memoryStream1);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"尝试加载附属程序集{item}失败，如果此程序集为DllImport，可以忽略此警告。错误信息：{(ex.Message)}");
                    }
                }
                if (assembly != null)
                {
                    //获取插件的相关信息
                    var driverTypes = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x))
                    && x.IsClass && !x.IsAbstract);
                    if (!driverTypes.Any())
                    {
                        throw new Exception("找不到对应的驱动");
                    }
                }
                else
                {
                    throw new Exception("加载驱动文件失败");
                }
                assembly = null;

                //卸载相同文件的插件域
                DeleteDriver(mainFileName);

                //保存到文件
                memoryStream.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(fullDir);//创建插件文件夹
                using FileStream fs = new(fullPath, FileMode.Create);
                await memoryStream.CopyToAsync(fs);
                foreach (var item in otherFilesStreams)
                {
                    item.Item2.Seek(0, SeekOrigin.Begin);
                    using FileStream fs1 = new(fullDir.CombinePathOS(item.Item1), FileMode.Create);
                    await item.Item2.CopyToAsync(fs1);
                    await item.Item2.DisposeAsync();
                }
            }
            finally
            {
                assemblyLoadContext.Unload();
                ClearCache();
            }
        }
        finally
        {
            _locker.Release();
        }
    }

    #endregion public

    private static T GetCustomAttributeRecursive<T>(PropertyInfo property) where T : Attribute
    {
        var attribute = property.GetCustomAttribute<T>(false);
        //if (attribute == null && property.ReflectedType.BaseType != null && property.ReflectedType != typeof(BusinessPropertyBase))
        //{
        //    var baseProperty = property.ReflectedType.BaseType.GetProperties().FirstOrDefault(p => p.Name == property.Name);
        //    if (baseProperty != null)
        //    {
        //        attribute = GetCustomAttributeRecursive<T>(baseProperty);
        //    }
        //}
        return attribute;
    }

    /// <summary>
    /// 获取全部插件信息
    /// </summary>
    /// <returns></returns>
    private List<PluginOutput> GetList()
    {
        try
        {
            _locker.Wait();
            var data = _simpleCacheService.Get<List<PluginOutput>>(_cacheKeyGetPluginOutputs);
            if (data == null)
            {
                var pluginOutputs = GetPluginOutputs();
                _simpleCacheService.Set<List<PluginOutput>>(_cacheKeyGetPluginOutputs, pluginOutputs);
                return pluginOutputs;
            }
            return data;
        }
        finally
        {
            _locker.Release();
        }

        List<PluginOutput> GetPluginOutputs()
        {
            List<PluginOutput> plugins = new List<PluginOutput>();
            //默认上下文
            PluginOutput defaultDriverPlugin = new();
            defaultDriverPlugin.Name = DefaultKey;
            defaultDriverPlugin.Children = new();
            foreach (var item in _defaultDriverBaseDict)
            {
                FileInfo fileInfo = new FileInfo(this.GetType().Assembly.Location);
                DateTime lastWriteTime = fileInfo.LastWriteTime;
                defaultDriverPlugin.Children.Add(
            new PluginOutput()
            {
                Name = item.Key,
                FileName = DefaultKey,
                PluginType = (typeof(CollectBase).IsAssignableFrom(item.Value)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business,
                Version = this.GetType().Assembly.GetName().Version.ToString(),
                LastWriteTime = lastWriteTime,
            });
            }
            if (defaultDriverPlugin.Children.Count > 0)
                plugins.Add(defaultDriverPlugin);

            string[] folderPaths = Directory.GetDirectories(AppContext.BaseDirectory.CombinePath(DirName));

            foreach (string folderPath in folderPaths)
            {
                //linux 环境下 OPCDA 不可用
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (folderPath.ToLower().Contains("opcda"))
                    {
                        continue;
                    }
                }

                PluginOutput driverPlugin = new();
                driverPlugin.Name = Path.GetFileName(folderPath);
                try
                {
                    FileInfo fileInfo = new FileInfo(folderPath.CombinePath($"{driverPlugin.Name}.dll"));
                    DateTime lastWriteTime = fileInfo.LastWriteTime;

                    var assembly = GetAssembly(folderPath, folderPath.CombinePath($"{driverPlugin.Name}.dll"), driverPlugin.Name);
                    var driverTypes = assembly.GetTypes().
    Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);
                    driverPlugin.Children = new();
                    foreach (var type in driverTypes)
                    {
                        //先判断是否已经拥有插件模块
                        if (!_driverBaseDict.ContainsKey($"{driverPlugin.Name}.{type.Name}"))
                        {
                            _driverBaseDict.TryAdd($"{driverPlugin.Name}.{type.Name}", type);
                            _logger?.LogInformation($"加载插件 {folderPath.CombinePath($"{driverPlugin.Name}.dll")}-{type.Name} 成功");
                        }
                        driverPlugin.Children.Add(
                        new PluginOutput()
                        {
                            Name = type.Name,
                            FileName = $"{driverPlugin.Name}",
                            PluginType = (typeof(CollectBase).IsAssignableFrom(type)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business,
                            Version = assembly.GetName().Version.ToString(),
                            LastWriteTime = lastWriteTime,
                        }
                        );
                    }
                    if (driverPlugin.Children.Count > 0)
                        plugins.Add(driverPlugin);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"加载插件 {folderPath} 失败");
                }
            }
            return plugins;
        }
    }

    /// <summary>
    /// 删除插件域，卸载插件
    /// </summary>
    /// <param name="path">主程序集文件名称</param>
    private void DeleteDriver(string path)
    {
        if (_assemblyLoadContextDict.TryGetValue(path, out var assemblyLoadContext))
        {
            _driverBaseDict.RemoveWhere(a => path == PluginServiceExtension.GetFileNameAndTypeName(a.Key).Item1);
            _assemblyDict.Remove(path);
            _assemblyLoadContextDict.Remove(path);
            assemblyLoadContext.Unload();
            GC.Collect();
        }
    }

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <param name="dir">插件文件夹绝对路径</param>
    /// <param name="path">插件主文件绝对路径</param>
    /// <param name="fileName">插件主文件名称</param>
    /// <returns></returns>
    private Assembly GetAssembly(string dir, string path, string fileName)
    {
        Assembly assembly = null;
        _logger?.LogInformation($"添加插件文件：{path}");
        //全部程序集路径
        List<string> paths = new();
        Directory.GetFiles(Path.GetDirectoryName(path), "*.dll").ToList().ForEach(a => paths.Add(a));

        if (_assemblyDict.ContainsKey(fileName))
        {
            assembly = _assemblyDict[fileName];
        }
        else
        {
            //新建插件域，并注明可卸载
            var assemblyLoadContext = new AssemblyLoadContext(fileName, true);
            //获取插件程序集
            assembly = GetAssembly(path, paths, assemblyLoadContext);
            if (assembly == null)
            {
                assemblyLoadContext.Unload();
                return null;
            }
            //添加到全局对象
            _assemblyLoadContextDict.TryAdd(fileName, assemblyLoadContext);
            _assemblyDict.TryAdd(fileName, assembly);
        }
        return assembly;
    }

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <param name="path">主程序的路径</param>
    /// <param name="paths">全部文件的路径</param>
    /// <param name="assemblyLoadContext">当前插件域</param>
    /// <returns></returns>
    private Assembly GetAssembly(string path, List<string> paths, AssemblyLoadContext assemblyLoadContext)
    {
        Assembly assembly = null;
        foreach (var item in paths)
        {
            using var fs = new FileStream(item, FileMode.Open);
            if (item == path)
                assembly = assemblyLoadContext.LoadFromStream(fs);
            else
            {
                try
                {
                    assemblyLoadContext.LoadFromStream(fs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"尝试加载附属程序集{item}失败，如果此程序集为非引用程序集，可以忽略此警告。错误信息：{(ex.Message)}");
                }
            }
        }
        return assembly;
    }
}