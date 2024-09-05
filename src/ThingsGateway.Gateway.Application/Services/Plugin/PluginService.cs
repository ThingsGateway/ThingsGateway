//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

using ThingsGateway.Core.Extension;
using ThingsGateway.NewLife.X;

using TouchSocket.Core;

using UAParser;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
public class PluginService : IPluginService
{
    /// <summary>
    /// 主程序上下文驱动父名称
    /// </summary>
    public const string DefaultKey = "Default";

    /// <summary>
    /// 插件驱动文件夹名称
    /// </summary>
    public const string DirName = "Plugins";

    private const string _cacheKeyGetPluginOutputs = $"{ThingsGatewayCacheConst.Cache_Prefix}{nameof(PluginService)}{nameof(GetList)}";

    private readonly IDispatchService<PluginOutput> _dispatchService;
    private readonly EasyLock _locker = new();

    /// <summary>
    /// 驱动服务日志
    /// </summary>
    private readonly ILogger _logger;

    private IStringLocalizer Localizer;

    public PluginService(ILogger<PluginService> logger, IStringLocalizer<PluginService> localizer, IDispatchService<PluginOutput> dispatchService)
    {
        _dispatchService = dispatchService;
        Localizer = localizer;
        _logger = logger;

        //创建插件文件夹
        Directory.CreateDirectory(AppContext.BaseDirectory.CombinePathWithOs(PluginService.DirName));
        //主程序上下文驱动类字典
        _defaultDriverBaseDict = new(NetCoreApp.EffectiveTypes
            .Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
            .ToDictionary(a => a.Name));
    }

    /// <summary>
    /// 插件文件名称/插件域
    /// </summary>
    private ConcurrentDictionary<string, (AssemblyLoadContext AssemblyLoadContext, Assembly Assembly)> _assemblyLoadContextDict { get; } = new();

    /// <summary>
    /// 主程序上下文中的插件类型名称/插件Type
    /// </summary>
    private ReadOnlyDictionary<string, Type> _defaultDriverBaseDict { get; }

    /// <summary>
    /// 插件FullName/插件Type
    /// </summary>
    private ConcurrentDictionary<string, Type> _driverBaseDict { get; } = new();

    #region public

    /// <summary>
    /// 根据插件名称获取对应的驱动程序。
    /// </summary>
    /// <param name="pluginName">插件名称，格式为 主插件程序集文件名称.类型名称</param>
    /// <returns>获取到的驱动程序</returns>
    public DriverBase GetDriver(string pluginName)
    {
        try
        {
            // 等待锁的释放，确保线程安全
            _locker.Wait();

            // 解析插件名称，获取文件名和类型名
            var filtResult = PluginServiceUtil.GetFileNameAndTypeName(pluginName);

            // 检查是否为空或者默认键
            if (filtResult.FileName.IsNullOrEmpty() || filtResult.FileName == DefaultKey)
            {
                // 如果是默认键，则搜索主程序上下文中的类型
                if (_defaultDriverBaseDict.ContainsKey(filtResult.TypeName))
                {
                    var driver = (DriverBase)Activator.CreateInstance(_defaultDriverBaseDict[filtResult.TypeName]);
                    driver.Directory = AppContext.BaseDirectory;

                    return driver;
                }
                else
                {
                    // 抛出异常，插件类型不存在
                    throw new(Localizer[$"LoadTypeFail1", pluginName]);
                }
            }

            // 构建插件目录路径
            var dir = AppContext.BaseDirectory.CombinePathWithOs(DirName, filtResult.FileName);

            // 先判断是否已经拥有插件模块
            if (_driverBaseDict.ContainsKey(pluginName))
            {
                var driver = (DriverBase)Activator.CreateInstance(_driverBaseDict[pluginName]);
                driver.Directory = dir;
                return driver;
            }

            Assembly assembly = null;
            // 根据路径获取DLL文件
            var path = dir.CombinePathWithOs($"{filtResult.FileName}.dll");
            assembly = GetAssembly(path, filtResult.FileName);

            if (assembly != null)
            {
                // 根据采集/业务类型获取实际插件类
                var driverType = assembly.GetTypes()
                    .Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
                    .FirstOrDefault(it => it.Name == filtResult.TypeName);

                if (driverType != null)
                {
                    var driver = (DriverBase)Activator.CreateInstance(driverType);
                    _logger?.LogInformation(Localizer[$"LoadTypeSuccess", pluginName]);
                    _driverBaseDict.TryAdd(pluginName, driverType);
                    driver.Directory = dir;
                    return driver;
                }
                // 抛出异常，插件类型不存在
                throw new(Localizer[$"LoadTypeFail1", pluginName]);
            }
            else
            {
                // 抛出异常，插件文件不存在
                throw new(Localizer[$"LoadTypeFail2", path]);
            }
        }
        finally
        {
            // 释放锁
            _locker.Release();
        }
    }

    /// <summary>
    /// 获取指定插件的特殊方法。
    /// </summary>
    /// <param name="pluginName">插件名称。</param>
    /// <param name="driverBase">可选参数，插件的驱动基类对象，如果未提供，则会尝试从缓存中获取。</param>
    /// <returns>返回列表</returns>
    public List<DriverMethodInfo> GetDriverMethodInfos(string pluginName, DriverBase? driverBase = null)
    {
        // 线程安全地执行方法
        lock (this)
        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetDriverMethodInfos)}_{CultureInfo.CurrentUICulture.Name}";
            // 如果未提供驱动基类对象，则尝试根据插件名称获取驱动对象
            var dispose = driverBase == null; // 标记是否需要释放驱动对象
            driverBase ??= GetDriver(pluginName); // 如果未提供驱动对象，则根据插件名称获取驱动对象

            // 检查插件名称是否为空或null
            if (!pluginName.IsNullOrEmpty())
            {
                // 尝试从缓存中获取指定插件的属性信息
                var data = NetCoreApp.CacheService.HashGetAll<List<DriverMethodInfo>>(cacheKey);
                // 如果缓存中存在指定插件的属性信息，则直接返回
                if (data?.ContainsKey(pluginName) == true)
                {
                    return data[pluginName];
                }
            }

            // 如果未从缓存中获取到指定插件的属性信息，则尝试从驱动基类对象中获取
            return SetDriverMethodInfosCache(driverBase, pluginName, cacheKey, dispose); // 获取并设置属性信息缓存

            // 用于设置驱动方法信息缓存的内部方法
            List<DriverMethodInfo> SetDriverMethodInfosCache(DriverBase driverBase, string pluginName, string cacheKey, bool dispose)
            {
                // 获取驱动对象的方法信息，并筛选出带有 DynamicMethodAttribute 特性的方法
                var dependencyPropertyWithInfos = driverBase.GetType().GetMethods()?.SelectMany(it =>
                    new[] { new { memberInfo = it, attribute = it.GetCustomAttribute<DynamicMethodAttribute>() } })
                    .Where(x => x.attribute != null).ToList()
                    .SelectMany(it => new[]
                    {
                    new DriverMethodInfo(){
                        Name=it.memberInfo.Name,
                        Description=it.attribute.Description,
                        Remark=it.attribute.Remark,
                        MethodInfo=it.memberInfo,
                    }
                    });

                // 将方法信息转换为字典形式，并添加到缓存中
                var result = dependencyPropertyWithInfos.ToList();
                NetCoreApp.CacheService.HashAdd(cacheKey, pluginName, result);

                // 如果是通过方法内部创建的驱动对象，则在方法执行完成后释放该驱动对象
                if (dispose)
                    driverBase.SafeDispose();

                // 返回获取到的属性信息字典
                return result;
            }
        }
    }

    /// <summary>
    /// 获取指定插件的属性类型及其信息，将其缓存在内存中
    /// </summary>
    /// <param name="pluginName">插件名称</param>
    /// <param name="driverBase">驱动基类实例，可选参数</param>
    /// <returns>返回包含属性名称及其信息的字典</returns>
    public (IEnumerable<IEditorItem> EditorItems, object Model, Type PropertyUIType) GetDriverPropertyTypes(string pluginName, DriverBase? driverBase = null)
    {
        // 使用锁确保线程安全
        lock (this)
        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetDriverPropertyTypes)}_{CultureInfo.CurrentUICulture.Name}";

            var dispose = driverBase == null;
            driverBase ??= GetDriver(pluginName); // 如果 driverBase 为 null， 获取驱动实例
            // 检查插件名称是否为空或空字符串
            if (!pluginName.IsNullOrEmpty())
            {
                // 从缓存中获取属性类型数据
                var data = NetCoreApp.CacheService.HashGetAll<List<IEditorItem>>(cacheKey);
                // 如果缓存中存在数据
                if (data?.ContainsKey(pluginName) == true)
                {
                    // 返回缓存中存储的属性类型数据
                    var editorItems = data[pluginName];
                    return (editorItems, driverBase.DriverProperties, driverBase.DriverPropertyUIType);
                }
            }
            // 如果缓存中不存在该插件的数据，则重新获取并缓存

            return (SetCache(driverBase, pluginName, cacheKey, dispose), driverBase.DriverProperties, driverBase.DriverPropertyUIType); // 调用 SetCache 方法进行缓存并返回结果

            // 定义 SetCache 方法，用于设置缓存并返回
            IEnumerable<IEditorItem> SetCache(DriverBase driverBase, string pluginName, string cacheKey, bool dispose)
            {
                var editorItems = driverBase.PluginPropertyEditorItems;
                // 将结果存入缓存中，键为插件名称
                NetCoreApp.CacheService.HashAdd(cacheKey, pluginName, editorItems);
                // 如果 dispose 参数为 true，则释放 driverBase 对象
                if (dispose)
                    driverBase.SafeDispose();
                return editorItems;
            }
        }
    }

    /// <summary>
    /// 获取插件信息的方法，可以根据插件类型筛选插件列表。
    /// </summary>
    /// <param name="pluginType">要筛选的插件类型，可选参数</param>
    /// <returns>符合条件的插件列表</returns>
    public List<PluginOutput> GetList(PluginTypeEnum? pluginType = null)
    {
        // 获取完整的插件列表
        var pluginList = GetList();

        if (pluginType == null)
        {
            // 如果未指定插件类型，则返回完整的插件列表
            return pluginList;
        }

        // 筛选出指定类型的插件
        var filteredPlugins = pluginList.Where(c => c.PluginType == pluginType).ToList();

        return filteredPlugins;
    }

    /// <summary>
    /// 获取变量的属性类型
    /// </summary>
    public (IEnumerable<IEditorItem> EditorItems, object Model) GetVariablePropertyTypes(string pluginName, BusinessBase? businessBase = null)
    {
        lock (this)
        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetVariablePropertyTypes)}_{CultureInfo.CurrentUICulture.Name}";
            var dispose = businessBase == null;
            businessBase ??= (BusinessBase)GetDriver(pluginName); // 如果 driverBase 为 null， 获取驱动实例

            var data = NetCoreApp.CacheService.HashGetAll<List<IEditorItem>>(cacheKey);
            if (data?.ContainsKey(pluginName) == true)
            {
                return (data[pluginName], businessBase.VariablePropertys);
            }
            // 如果缓存中不存在该插件的数据，则重新获取并缓存
            return (SetCache(pluginName, cacheKey), businessBase.VariablePropertys);

            // 定义 SetCache 方法，用于设置缓存并返回
            IEnumerable<IEditorItem> SetCache(string pluginName, string cacheKey)
            {
                var editorItems = businessBase.PluginVariablePropertyEditorItems;
                // 将结果存入缓存中，键为插件名称
                NetCoreApp.CacheService.HashAdd(cacheKey, pluginName, editorItems);
                // 如果 dispose 参数为 true，则释放 driverBase 对象
                if (dispose)
                    businessBase.SafeDispose();
                return editorItems;
            }
        }
    }

    /// <summary>
    /// 分页显示插件
    /// </summary>
    public QueryData<PluginOutput> Page(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null)
    {
        //指定关键词搜索为插件FullName
        var query = GetList(pluginTypeEnum).WhereIF(!options.SearchText.IsNullOrWhiteSpace(), a => a.FullName.Contains(options.SearchText)).GetQueryData(options);
        return query;
    }

    /// <summary>
    /// 移除全部插件
    /// </summary>
    public void Remove()
    {
        try
        {
            _locker.Wait();
            _driverBaseDict.Clear();
            foreach (var item in _assemblyLoadContextDict)
            {
                item.Value.AssemblyLoadContext.Unload();
            }
            GC.Collect();
            ClearCache();
            _assemblyLoadContextDict.Clear();
        }
        finally
        {
            _locker.Release();
        }
    }

    /// <summary>
    /// 异步保存驱动程序信息。
    /// </summary>
    /// <param name="plugin">要保存的插件信息。</param>
    [OperDesc("SavePlugin", isRecordPar: false, localizerType: typeof(PluginAddInput))]
    public async Task SavePlugin(PluginAddInput plugin)
    {
        try
        {
            // 等待锁可用
            _locker.Wait();

            // 创建程序集加载上下文
            var assemblyLoadContext = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
            // 存储其他文件的内存流列表
            List<(string Name, MemoryStream MemoryStream)> otherFilesStreams = new();
            var maxFileSize = 100 * 1024 * 1024; // 最大100MB

            // 获取主程序集文件名
            var mainFileName = Path.GetFileNameWithoutExtension(plugin.MainFile.Name);
            // 构建插件文件夹绝对路径
            var fullDir = AppContext.BaseDirectory.CombinePathWithOs(DirName, mainFileName);
            try
            {
                // 构建主程序集绝对路径
                var fullPath = fullDir.CombinePathWithOs(plugin.MainFile.Name);

                // 获取主程序集文件流
                using var stream = plugin.MainFile.OpenReadStream(maxFileSize);
                using MemoryStream mainMemoryStream = new MemoryStream();
                await stream.CopyToAsync(mainMemoryStream).ConfigureAwait(false);
                mainMemoryStream.Seek(0, SeekOrigin.Begin);

                // 先加载到内存，如果成功添加后再装载到文件
                // 加载主程序集
                var assembly = assemblyLoadContext.LoadFromStream(mainMemoryStream);
                foreach (var item in plugin.OtherFiles ?? new())
                {
                    // 获取附属文件流
                    using var otherStream = item.OpenReadStream(maxFileSize);
                    MemoryStream memoryStream = new MemoryStream();
                    await otherStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    otherFilesStreams.Add((item.Name, memoryStream));
                    try
                    {
                        // 尝试加载附属程序集
                        assemblyLoadContext.LoadFromStream(memoryStream);
                    }
                    catch (Exception ex)
                    {
                        // 加载失败时记录警告信息
                        _logger?.LogWarning(ex, Localizer[$"LoadOtherFileFail", item]);
                    }
                }

                // 获取驱动类型信息
                var driverTypes = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x))
                && x.IsClass && !x.IsAbstract);

                // 检查是否存在驱动类型
                if (!driverTypes.Any())
                {
                    throw new(Localizer[$"PluginNotFound"]);
                }

                assembly = null;

                // 卸载相同文件的插件域
                DeletePlugin(mainFileName);

                // 将主程序集保存到文件
                mainMemoryStream.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(fullDir);// 创建插件文件夹
                using FileStream fs = new(fullPath, FileMode.Create);
                await mainMemoryStream.CopyToAsync(fs).ConfigureAwait(false);
            }
            finally
            {
                // 将其他文件保存到文件
                foreach (var item in otherFilesStreams)
                {
                    item.MemoryStream.Seek(0, SeekOrigin.Begin);
                    using FileStream fs1 = new(fullDir.CombinePathWithOs(item.Name), FileMode.Create);
                    await item.MemoryStream.CopyToAsync(fs1).ConfigureAwait(false);
                    await item.MemoryStream.DisposeAsync().ConfigureAwait(false);
                }
                // 卸载程序集加载上下文并清除缓存
                assemblyLoadContext.Unload();
                ClearCache();
            }
        }
        finally
        {
            // 释放锁资源
            _locker.Release();
        }
    }

    /// <summary>
    /// 设置插件的属性值。
    /// </summary>
    /// <param name="driver">插件实例。</param>
    /// <param name="deviceProperties">插件属性，检索相同名称的属性后写入。</param>
    public void SetDriverProperties(DriverBase driver, Dictionary<string, string> deviceProperties)
    {
        // 获取插件的属性信息列表
        var pluginProperties = driver.DriverProperties?.GetType().GetRuntimeProperties()
            // 筛选出带有 DynamicPropertyAttribute 特性的属性
            .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>(false) != null);

        // 遍历插件的属性信息列表
        foreach (var propertyInfo in pluginProperties ?? new List<PropertyInfo>())
        {
            // 在设备属性列表中查找与当前属性相同名称的属性
            if (!deviceProperties.TryGetValue(propertyInfo.Name, out var deviceProperty))
            {
                continue;
            }
            // 获取设备属性的值，如果设备属性值为空，则将其转换为当前属性类型的默认值
            var value = ThingsGatewayStringConverter.Default.Deserialize(null, deviceProperty, propertyInfo.PropertyType);
            // 设置插件属性的值
            propertyInfo.SetValue(driver.DriverProperties, value);
        }
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    private void ClearCache()
    {
        lock (this)
        {
            NetCoreApp.CacheService.Remove(_cacheKeyGetPluginOutputs);
            NetCoreApp.CacheService.DelByPattern($"{nameof(PluginService)}_");

            // 获取私有字段
            FieldInfo fieldInfo = typeof(ResourceManagerStringLocalizerFactory).GetField("_localizerCache", BindingFlags.Instance | BindingFlags.NonPublic);
            // 获取字段的值
            var dictionary = (ConcurrentDictionary<string, ResourceManagerStringLocalizer>)fieldInfo.GetValue(NetCoreApp.StringLocalizerFactory);
            foreach (var item in _assemblyLoadContextDict)
            {
                // 移除特定键
                dictionary.RemoveWhere(a => item.Value.Assembly.ExportedTypes.Select(b => b.AssemblyQualifiedName).Contains(a.Key));
            }
            _ = Task.Run(() =>
            {
                _dispatchService.Dispatch(new());
            });
        }
    }

    #endregion public

    /// <summary>
    /// 删除插件域，卸载插件
    /// </summary>
    /// <param name="path">主程序集文件名称</param>
    private void DeletePlugin(string path)
    {
        if (_assemblyLoadContextDict.TryGetValue(path, out var assemblyLoadContext))
        {
            //移除字典
            _driverBaseDict.RemoveWhere(a => path == PluginServiceUtil.GetFileNameAndTypeName(a.Key).FileName);
            _assemblyLoadContextDict.Remove(path);
            //卸载
            assemblyLoadContext.AssemblyLoadContext.Unload();
            GC.Collect();
        }
    }

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <param name="path">插件主文件绝对路径</param>
    /// <param name="fileName">插件主文件名称</param>
    /// <returns></returns>
    private Assembly GetAssembly(string path, string fileName)
    {
        Assembly assembly = null;
        _logger?.LogInformation(Localizer["AddPluginFile", path]);
        //全部程序集路径
        List<string> paths = new();
        Directory.GetFiles(Path.GetDirectoryName(path), "*.dll").ToList().ForEach(a => paths.Add(a));

        if (_assemblyLoadContextDict.ContainsKey(fileName))
        {
            assembly = _assemblyLoadContextDict[fileName].Assembly;
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
            _assemblyLoadContextDict.TryAdd(fileName, (assemblyLoadContext, assembly));
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
                assembly = assemblyLoadContext.LoadFromStream(fs); //加载主程序集，并获取
            else
            {
                try
                {
                    assemblyLoadContext.LoadFromStream(fs);//加载其余程序集
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, Localizer[$"LoadOtherFileFail", item]);
                }
            }
        }
        return assembly;
    }

    /// <summary>
    /// 获取全部插件信息
    /// </summary>
    /// <returns></returns>
    private List<PluginOutput> GetList()
    {
        try
        {
            // 等待锁可用，确保在多线程环境下不会出现并发访问问题
            _locker.Wait();

            // 从缓存中获取插件列表数据
            var data = NetCoreApp.CacheService.Get<List<PluginOutput>>(_cacheKeyGetPluginOutputs);

            // 如果缓存中没有数据，则调用 GetPluginOutputs 方法获取数据，并将其存入缓存
            if (data == null)
            {
                var pluginOutputs = GetPluginOutputs();
                NetCoreApp.CacheService.Set(_cacheKeyGetPluginOutputs, pluginOutputs);
                return pluginOutputs;
            }

            // 如果缓存中有数据，则直接返回
            return data;
        }
        finally
        {
            // 释放锁资源
            _locker.Release();
        }

        // 获取插件列表数据的私有方法
        List<PluginOutput> GetPluginOutputs()
        {
            List<PluginOutput> plugins = new List<PluginOutput>();
            // 主程序上下文

            // 遍历程序集上下文默认驱动字典，生成默认驱动插件信息
            foreach (var item in _defaultDriverBaseDict)
            {
                if (PluginServiceUtil.IsSupported(item.Value))
                {
                    FileInfo fileInfo = new FileInfo(GetType().Assembly.Location); //文件信息
                    DateTime lastWriteTime = fileInfo.LastWriteTime;//作为编译时间
                    plugins.Add(
                        new PluginOutput()
                        {
                            Name = item.Key,//插件名称
                            FileName = DefaultKey,//插件文件名称（分类）
                            PluginType = (typeof(CollectBase).IsAssignableFrom(item.Value)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business, //插件类型
                            Version = item.Value.Assembly.GetName().Version.ToString(), //插件版本
                            LastWriteTime = lastWriteTime, //编译时间
                        }
                    );
                }
            }

            // 获取插件文件夹路径列表
            string[] folderPaths = Directory.GetDirectories(AppContext.BaseDirectory.CombinePathWithOs(DirName));

            // 遍历插件文件夹
            foreach (string folderPath in folderPaths)
            {
                //当前插件文件夹

                try
                {
                    var driverMainName = Path.GetFileName(folderPath);
                    FileInfo fileInfo = new FileInfo(folderPath.CombinePathWithOs($"{driverMainName}.dll")); //插件主程序集名称约定为文件夹名称.dll
                    DateTime lastWriteTime = fileInfo.LastWriteTime;

                    // 加载插件程序集并获取其中的驱动类型信息
                    var assembly = GetAssembly(folderPath.CombinePathWithOs($"{driverMainName}.dll"), driverMainName);
                    var driverTypes = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);

                    // 遍历驱动类型，生成插件信息，并将其添加到插件列表中
                    foreach (var type in driverTypes)
                    {
                        if (PluginServiceUtil.IsSupported(type))
                        {
                            // 先判断是否已经拥有插件模块
                            if (!_driverBaseDict.ContainsKey($"{driverMainName}.{type.Name}"))
                            {
                                //添加到字典
                                _driverBaseDict.TryAdd($"{driverMainName}.{type.Name}", type);
                                _logger?.LogInformation(Localizer[$"LoadTypeSuccess", PluginServiceUtil.GetFullName(driverMainName, type.Name)]);
                            }
                            plugins.Add(
                                new PluginOutput()
                                {
                                    Name = type.Name, //类型名称
                                    FileName = $"{driverMainName}", //主程序集名称
                                    PluginType = (typeof(CollectBase).IsAssignableFrom(type)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business,//插件类型
                                    Version = assembly.GetName().Version.ToString(),//插件版本
                                    LastWriteTime = lastWriteTime, //编译时间
                                }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录加载插件失败的日志
                    _logger?.LogWarning(ex, Localizer[$"LoadPluginFail", Path.GetRelativePath(AppContext.BaseDirectory.CombinePathWithOs(DirName), folderPath)]);
                }
            }
            return plugins;
        }
    }
}
