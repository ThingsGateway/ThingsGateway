//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Runtime.Loader;

using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
internal sealed class PluginService : IPluginService
{
    /// <summary>
    /// 插件驱动文件夹名称
    /// </summary>
    public const string DirName = "GatewayPlugins";

    private const string CacheKeyGetPluginOutputs = $"ThingsGateway.Gateway.Application.{nameof(PluginService)}GetList";
    private const string SaveEx = ".save";
    private const string DelEx = ".del";

    private readonly IDispatchService<PluginInfo> _dispatchService;
    private readonly WaitLock _locker = new(nameof(PluginService));
    private readonly ILogger _logger;

    public PluginService(ILogger<PluginService> logger, IDispatchService<PluginInfo> dispatchService)
    {
        _logger = logger;

        _dispatchService = dispatchService;
        //创建插件文件夹
        Directory.CreateDirectory(PathHelper.CombinePathReplace(AppContext.BaseDirectory, PluginService.DirName));
        //主程序上下文驱动类字典
        _defaultDriverBaseDict = new(App.EffectiveTypes
     .Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
     .ToDictionary(a => $"{Path.GetFileNameWithoutExtension(new FileInfo(a.Assembly.Location).Name)}.{a.Name}"));

        DeleteBackup(DirName);
        DeleteBackup(AppContext.BaseDirectory);
    }

    /// <summary>
    /// 插件文件名称/插件域
    /// </summary>
    private NonBlockingDictionary<string, (AssemblyLoadContext AssemblyLoadContext, Assembly Assembly)> _assemblyLoadContextDict { get; } = new();

    /// <summary>
    /// 主程序上下文中的插件FullName/插件Type
    /// </summary>
    private System.Collections.ObjectModel.ReadOnlyDictionary<string, Type> _defaultDriverBaseDict { get; }


    /// <summary>
    /// 插件FullName/插件Type
    /// </summary>
    private NonBlockingDictionary<string, Type> _driverBaseDict { get; } = new();

    #region public
    public List<Assembly> GetLoadContextAssemblyList()
    {
        GetPluginList();
        return _assemblyLoadContextDict.Select(a => a.Value.Assembly).ToList();
    }
    public Type GetDebugUI(string pluginName)
    {
        var driver = GetDriver(pluginName);
        return driver?.DriverDebugUIType;
    }
    public Type GetAddressUI(string pluginName)
    {
        var driver = GetDriver(pluginName);
        return driver?.DriverVariableAddressUIType;
    }

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
            var filtResult = PluginInfoUtil.GetFileNameAndTypeName(pluginName);

            // 如果是默认键，则搜索主程序上下文中的类型
            if (_defaultDriverBaseDict.TryGetValue(pluginName, out var type))
            {
                var driver = (DriverBase)Activator.CreateInstance(type);

                return driver;
            }

            // 构建插件目录路径
            var dir = PathHelper.CombinePathReplace(AppContext.BaseDirectory, DirName, filtResult.FileName);

            // 先判断是否已经拥有插件模块
            if (_driverBaseDict.TryGetValue(pluginName, out var value))
            {
                var driver = (DriverBase)Activator.CreateInstance(value);
                return driver;
            }

            Assembly assembly = null;
            // 根据路径获取DLL文件
            var path = PathHelper.CombinePathReplace(dir, $"{filtResult.FileName}.dll");
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
                    if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information) == true)
                        _logger?.LogInformation(string.Format(AppResource.LoadTypeSuccess, pluginName));
                    _driverBaseDict.TryAdd(pluginName, driverType);
                    return driver;
                }
                // 抛出异常，插件类型不存在
                throw new(string.Format(AppResource.LoadTypeFail1, pluginName));
            }
            else
            {
                // 抛出异常，插件文件不存在
                throw new(string.Format(AppResource.LoadTypeFail2, path));
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
    /// <param name="driver">可选参数，插件的驱动基类对象，如果未提供，则会尝试从缓存中获取。</param>
    /// <returns>返回列表</returns>
    public List<DriverMethodInfo> GetDriverMethodInfos(string pluginName, IDriver? driver = null)
    {

        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetDriverMethodInfos)}_{System.Globalization.CultureInfo.CurrentUICulture.Name}";
            // 如果未提供驱动基类对象，则尝试根据插件名称获取驱动对象
            driver ??= GetDriver(pluginName); // 如果未提供驱动对象，则根据插件名称获取驱动对象

            // 检查插件名称是否为空或null
            if (!pluginName.IsNullOrEmpty())
            {
                // 尝试从缓存中获取指定插件的属性信息
                var data = App.CacheService.HashGetAll<List<DriverMethodInfo>>(cacheKey);
                // 如果缓存中存在指定插件的属性信息，则直接返回
                if (data?.ContainsKey(pluginName) == true)
                {
                    return data[pluginName];
                }
            }

            // 如果未从缓存中获取到指定插件的属性信息，则尝试从驱动基类对象中获取
            return SetDriverMethodInfosCache(driver, pluginName, cacheKey); // 获取并设置属性信息缓存

            // 用于设置驱动方法信息缓存的内部方法
            List<DriverMethodInfo> SetDriverMethodInfosCache(IDriver driver, string pluginName, string cacheKey)
            {
                // 获取驱动对象的方法信息，并筛选出带有 DynamicMethodAttribute 特性的方法
                var dependencyPropertyWithInfos = driver.GetType().GetMethods()?.SelectMany(it =>
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
                App.CacheService.HashAdd(cacheKey, pluginName, result);

                // 返回获取到的属性信息字典
                return result;
            }
        }
    }

    /// <summary>
    /// 获取指定插件的属性类型及其信息，将其缓存在内存中
    /// </summary>
    /// <param name="pluginName">插件名称</param>
    /// <param name="driver">驱动基类实例，可选参数</param>
    /// <returns>返回包含属性名称及其信息的字典</returns>
    public (IEnumerable<IEditorItem> EditorItems, object Model, Type PropertyUIType) GetDriverPropertyTypes(string pluginName, IDriver? driver = null)
    {

        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetDriverPropertyTypes)}_{System.Globalization.CultureInfo.CurrentUICulture.Name}";

            driver ??= GetDriver(pluginName); // 如果 driver 为 null， 获取驱动实例
            // 检查插件名称是否为空或空字符串
            if (!pluginName.IsNullOrEmpty())
            {
                // 从缓存中获取属性类型数据
                var data = App.CacheService.HashGetAll<List<IEditorItem>>(cacheKey);
                // 如果缓存中存在数据
                if (data?.ContainsKey(pluginName) == true)
                {
                    // 返回缓存中存储的属性类型数据
                    var editorItems = data[pluginName];
                    return (editorItems, driver.DriverProperties, driver.DriverPropertyUIType);
                }
            }
            // 如果缓存中不存在该插件的数据，则重新获取并缓存

            return (SetCache(driver, pluginName, cacheKey), driver.DriverProperties, driver.DriverPropertyUIType); // 调用 SetCache 方法进行缓存并返回结果

            // 定义 SetCache 方法，用于设置缓存并返回
            IEnumerable<IEditorItem> SetCache(IDriver driver, string pluginName, string cacheKey)
            {
                var editorItems = PluginServiceUtil.GetEditorItems(driver.DriverProperties?.GetType()).ToList();
                // 将结果存入缓存中，键为插件名称
                App.CacheService.HashAdd(cacheKey, pluginName, editorItems);

                return editorItems;
            }
        }
    }
    /// <summary>
    /// 获取插件信息的方法，可以根据插件类型筛选插件列表。
    /// </summary>
    /// <param name="pluginType">要筛选的插件类型，可选参数</param>
    /// <returns>符合条件的插件列表</returns>
    public Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null)
    {
        // 获取完整的插件列表
        var pluginList = PrivateGetList();

        if (pluginType == null)
        {
            // 如果未指定插件类型，则返回完整的插件列表
            return Task.FromResult(pluginList.ToList());
        }

        // 筛选出指定类型的插件
        var filteredPlugins = pluginList.Where(c => c.PluginType == pluginType).ToList();

        return Task.FromResult(filteredPlugins);
    }

    /// <summary>
    /// 获取插件信息的方法，可以根据插件类型筛选插件列表。
    /// </summary>
    /// <param name="pluginType">要筛选的插件类型，可选参数</param>
    /// <returns>符合条件的插件列表</returns>
    public List<PluginInfo> GetPluginList(PluginTypeEnum? pluginType = null)
    {
        // 获取完整的插件列表
        var pluginList = PrivateGetList();

        if (pluginType == null)
        {
            // 如果未指定插件类型，则返回完整的插件列表
            return pluginList.ToList();
        }

        // 筛选出指定类型的插件
        var filteredPlugins = pluginList.Where(c => c.PluginType == pluginType).ToList();

        return filteredPlugins;
    }


    /// <summary>
    /// 获取变量的属性类型
    /// </summary>
    public (IEnumerable<IEditorItem> EditorItems, object Model, Type VariablePropertyUIType) GetVariablePropertyTypes(string pluginName, BusinessBase? businessBase = null)
    {

        {
            string cacheKey = $"{nameof(PluginService)}_{nameof(GetVariablePropertyTypes)}_{System.Globalization.CultureInfo.CurrentUICulture.Name}";
            businessBase ??= (BusinessBase)GetDriver(pluginName); // 如果 driver 为 null， 获取驱动实例

            var data = App.CacheService.HashGetAll<List<IEditorItem>>(cacheKey);
            if (data?.ContainsKey(pluginName) == true)
            {
                return (data[pluginName], businessBase.VariablePropertys, businessBase.DriverVariablePropertyUIType);
            }
            // 如果缓存中不存在该插件的数据，则重新获取并缓存
            return (SetCache(pluginName, cacheKey), businessBase.VariablePropertys, businessBase.DriverVariablePropertyUIType);

            // 定义 SetCache 方法，用于设置缓存并返回
            IEnumerable<IEditorItem> SetCache(string pluginName, string cacheKey)
            {
                var editorItems = businessBase.PluginVariablePropertyEditorItems;
                // 将结果存入缓存中，键为插件名称
                App.CacheService.HashAdd(cacheKey, pluginName, editorItems);
                // 如果 dispose 参数为 true，则释放 driver 对象
                return editorItems;
            }
        }
    }

    /// <summary>
    /// 分页显示插件
    /// </summary>
    public async Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginType = null)
    {
        //指定关键词搜索为插件FullName
        var query = (await GetPluginsAsync(pluginType).ConfigureAwait(false)).WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.FullName.Contains(options.SearchText)).Where(a => a.Display).GetQueryData(options);
        return query;
    }

    /// <summary>
    /// 移除全部插件
    /// </summary>
    public async Task ReloadPluginAsync()
    {
        try
        {
            await _locker.WaitAsync().ConfigureAwait(false);
            _driverBaseDict.Clear();
            foreach (var item in _assemblyLoadContextDict)
            {
                item.Value.AssemblyLoadContext.Unload();
            }
            ClearCache();
            _assemblyLoadContextDict.Clear();
        }
        finally
        {
            _locker.Release();
        }

    }



    private WaitLock SaveLock = new(nameof(PluginService));
    /// <summary>
    /// 异步保存驱动程序信息。
    /// </summary>
    /// <param name="pluginAddPathInput">要保存的插件信息。</param>
    [OperDesc("SavePlugin", isRecordPar: false, localizerType: typeof(PluginAddInput))]
    public async Task SavePluginByPathAsync(PluginAddPathInput pluginAddPathInput)
    {
        try
        {
            // 等待锁可用
            await SaveLock.WaitAsync().ConfigureAwait(false);
            var tempDir = DirName;
            // 创建程序集加载上下文
            var assemblyLoadContext = new AssemblyLoadContext(CommonUtils.GetSingleId().ToString(), true);
            // 存储其他文件的内存流列表
            List<(string Name, MemoryStream MemoryStream)> otherFilesStreams = new();

            // 获取主程序集文件名
            var mainFileName = Path.GetFileNameWithoutExtension(pluginAddPathInput.MainFilePath);
            string fullDir = string.Empty;
            bool isDefaultDriver = false;
            //判定是否上下文程序集
            var defaultDriver = _defaultDriverBaseDict.FirstOrDefault(a => Path.GetFileNameWithoutExtension(new FileInfo(a.Value.Assembly.Location).Name) == mainFileName);
            if (defaultDriver.Value != null)
            {
                var filtResult = PluginInfoUtil.GetFileNameAndTypeName(defaultDriver.Key);
                fullDir = Path.GetDirectoryName(filtResult.FileName);
                isDefaultDriver = true;
            }
            else
            {
                // 构建插件文件夹绝对路径
                fullDir = PathHelper.CombinePathReplace(AppContext.BaseDirectory, tempDir, mainFileName);
                isDefaultDriver = false;
            }
            // 获取主程序集文件流
            MemoryStream mainMemoryStream = new MemoryStream();
            try
            {
                // 构建主程序集绝对路径
                var fullPath = PathHelper.CombinePathReplace(fullDir, Path.GetFileName(pluginAddPathInput.MainFilePath));


                using (var stream = File.Open(pluginAddPathInput.MainFilePath, FileMode.Open, FileAccess.Read))
                {

                    await stream.CopyToAsync(mainMemoryStream).ConfigureAwait(false);
                }
                mainMemoryStream.Seek(0, SeekOrigin.Begin);

                #region
                // 先加载到内存，如果成功添加后再装载到文件
                // 加载主程序集
                var assembly = assemblyLoadContext.LoadFromStream(mainMemoryStream);
                foreach (var item in pluginAddPathInput.OtherFilePaths ?? new())
                {
                    // 获取附属文件流
                    using (var otherStream = File.Open(pluginAddPathInput.MainFilePath, FileMode.Open, FileAccess.Read))
                    {

                        MemoryStream memoryStream = new MemoryStream();
                        await otherStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        otherFilesStreams.Add((Path.GetFileName(item), memoryStream));
                        try
                        {
                            // 尝试加载附属程序集
                            assemblyLoadContext.LoadFromStream(memoryStream);
                        }
                        catch (Exception ex)
                        {
                            // 加载失败时记录警告信息
                            if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning) == true)
                                _logger?.LogWarning(ex, string.Format(AppResource.LoadOtherFileFail, item));
                        }
                    }
                }
                #endregion

                // 获取驱动类型信息
                var driverTypes = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x))
                && x.IsClass && !x.IsAbstract);

                // 检查是否存在驱动类型
                if (!driverTypes.Any())
                {
                    throw new(AppResource.PluginNotFound);
                }

                assembly = null;

                if (isDefaultDriver)
                {
                    // 将主程序集保存到文件
                    await MarkSave(fullPath, mainMemoryStream).ConfigureAwait(false);
                    // 将其他文件保存到文件
                    foreach (var item in otherFilesStreams)
                    {
                        await MarkSave(PathHelper.CombinePathReplace(fullDir, item.Name), item.MemoryStream).ConfigureAwait(false);
                    }
                }
                else
                {
                    // 将主程序集保存到文件
                    mainMemoryStream.Seek(0, SeekOrigin.Begin);
                    Directory.CreateDirectory(fullDir);// 创建插件文件夹
                    using FileStream fs = new(fullPath, FileMode.Create);
                    await mainMemoryStream.CopyToAsync(fs).ConfigureAwait(false);
                    // 将其他文件保存到文件
                    foreach (var item in otherFilesStreams)
                    {
                        item.MemoryStream.Seek(0, SeekOrigin.Begin);
                        using FileStream fs1 = new(PathHelper.CombinePathReplace(fullDir, item.Name), FileMode.Create);
                        await item.MemoryStream.CopyToAsync(fs1).ConfigureAwait(false);
                        await item.MemoryStream.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (mainMemoryStream != null)
                    await mainMemoryStream.SafeDisposeAsync().ConfigureAwait(false);
                otherFilesStreams.ForEach(async a =>
                {
                    if (a.MemoryStream != null)
                    {
                        await a.MemoryStream.SafeDisposeAsync().ConfigureAwait(false);
                    }
                });

                // 卸载程序集加载上下文并清除缓存
                assemblyLoadContext.Unload();

                ClearCache();

                try
                {
                    // 卸载相同文件的插件域
                    DeletePlugin(mainFileName);
                }
                catch
                {
                }
            }
        }
        finally
        {
            if (File.Exists(pluginAddPathInput.MainFilePath))
            {
                File.Delete(pluginAddPathInput.MainFilePath);
            }
            foreach (var item in pluginAddPathInput.OtherFilePaths)
            {
                if (File.Exists(item))
                {
                    File.Delete(item);
                }
            }
            // 释放锁资源
            SaveLock.Release();
        }
    }

    private static async Task MarkSave(string fullPath, MemoryStream stream)
    {
        var has = MarkDeletePlugin(fullPath);
        var saveEx = string.Empty;
        stream.Seek(0, SeekOrigin.Begin);
        using FileStream fs = new($"{fullPath}{saveEx}", FileMode.Create);
        await stream.CopyToAsync(fs).ConfigureAwait(false);
        await stream.DisposeAsync().ConfigureAwait(false);
    }
    /// <summary>
    /// 标记删除插件
    /// </summary>
    /// <param name="path">主程序集文件名称</param>
    private static bool MarkDeletePlugin(string path)
    {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
            fileInfo.MoveTo($"{path}{CommonUtils.GetSingleId()}{DelEx}", true);
        else
            return false;
        return true;
    }

    /// <summary>删除备份文件</summary>
    /// <param name="dest">目标目录</param>
    private static void DeleteBackup(String dest)
    {
        // 删除备份
        var di = dest.AsDirectory();
        var fs = di.GetAllFiles($"*{DelEx}", true);
        foreach (var item in fs)
        {
            try
            {
                item.Delete();
            }
            catch { }
        }
    }

    /// <summary>
    /// 设置插件的属性值。
    /// </summary>
    /// <param name="protperties">插件属性实例。</param>
    /// <param name="deviceProperties">插件属性，检索相同名称的属性后写入。</param>
    public void SetDriverProperties(object protperties, Dictionary<string, string> deviceProperties)
    {
        // 获取插件的属性信息列表
        var pluginProperties = protperties?.GetType().GetRuntimeProperties()
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
            propertyInfo.SetValue(protperties, value);
        }
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    private void ClearCache()
    {

        {
            App.CacheService.Remove(CacheKeyGetPluginOutputs);
            App.CacheService.DelByPattern($"{nameof(PluginService)}_");

            //多语言缓存清理
            try
            {
                // 获取私有字段
                FieldInfo fieldInfo = typeof(ResourceManagerStringLocalizerFactory).GetField("_localizerCache", BindingFlags.Instance | BindingFlags.NonPublic);
                // 获取字段的值
                var dictionary = (ConcurrentDictionary<string, ResourceManagerStringLocalizer>)fieldInfo.GetValue(App.StringLocalizerFactory);
                foreach (var item in _assemblyLoadContextDict)
                {
                    var ids = item.Value.Assembly.ExportedTypes.Select(b => b.AssemblyQualifiedName).ToHashSet();
                    // 移除特定键
                    dictionary.RemoveWhere(a => ids.Contains(a.Key));
                }
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            _ = Task.Run(() => _dispatchService.Dispatch(null));
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
            _driverBaseDict.RemoveWhere(a => path == PluginInfoUtil.GetFileNameAndTypeName(a.Key).FileName);
            _assemblyLoadContextDict.Remove(path, out _);
            //卸载
            assemblyLoadContext.AssemblyLoadContext.Unload();
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
        try
        {
            Assembly assembly = null;
            //全部程序集路径
            List<string> paths = new();
            Directory.GetFiles(Path.GetDirectoryName(path), "*.dll").ToList().ForEach(a => paths.Add(a));

            if (_assemblyLoadContextDict.TryGetValue(fileName, out (AssemblyLoadContext AssemblyLoadContext, Assembly Assembly) value))
            {
                assembly = value.Assembly;
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
                if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information) == true)
                    _logger?.LogInformation(string.Format(AppResource.AddPluginFile, path));
            }
            return assembly;
        }
        catch
        {
            return null;
        }
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
        //var cacheId = CommonUtils.GetSingleId();
        //foreach (var item in paths)
        //{
        //    var dir = Path.GetDirectoryName(item).CombinePathWithOs($"Cache{cacheId}");
        //    var cachePath = dir.CombinePath(Path.GetFileName(item));
        //    Directory.CreateDirectory(dir);
        //    File.Copy(item, cachePath, true);
        //}
        foreach (var item in paths)
        {
            using var fs = new FileStream(item, FileMode.Open);
            //var cachePath = Path.GetDirectoryName(item).CombinePathWithOs($"Cache{cacheId}").CombinePath(Path.GetFileName(item));
            if (Path.GetFullPath(item) == Path.GetFullPath(path))
            {
                assembly = assemblyLoadContext.LoadFromStream(fs); //加载主程序集，并获取

                //修改为从文件创立，满足roslyn引擎的要求
                //assembly = assemblyLoadContext.LoadFromAssemblyPath(cachePath);

            }
            else
            {
                try
                {
                    //assemblyLoadContext.LoadFromAssemblyPath(cachePath);
                    assemblyLoadContext.LoadFromStream(fs); //加载主程序集，并获取
                }
                catch (Exception ex)
                {
                    if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning) == true)
                        _logger?.LogWarning(ex, string.Format(AppResource.LoadOtherFileFail, item));
                }
            }
        }
        return assembly;
    }
    /// <summary>
    /// 获取全部插件信息
    /// </summary>
    /// <returns></returns>
    private IEnumerable<PluginInfo> PrivateGetList()
    {
        try
        {
            _locker.Wait();

            // 从缓存中获取插件列表数据
            var data = App.CacheService.Get<List<PluginInfo>>(CacheKeyGetPluginOutputs);

            // 如果缓存中没有数据，则调用 GetPluginOutputs 方法获取数据，并将其存入缓存
            if (data == null)
            {
                var pluginInfos = GetPluginOutputs();
                App.CacheService.Set(CacheKeyGetPluginOutputs, pluginInfos.ToList());
                return pluginInfos;
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
        IEnumerable<PluginInfo> GetPluginOutputs()
        {
            var plugins = new List<PluginInfo>();
            // 主程序上下文
            string tempDir = DirName;

            // 遍历程序集上下文默认驱动字典，生成默认驱动插件信息
            foreach (var item in _defaultDriverBaseDict)
            {
                if (PluginServiceUtil.IsSupported(item.Value))
                {
                    FileInfo fileInfo = new FileInfo(item.Value.Assembly.Location); //文件信息
                    DateTime lastWriteTime = fileInfo.LastWriteTime;//作为编译时间

                    var pluginInfo = new PluginInfo()
                    {
                        Name = item.Value.Name,//插件名称
                        FileName = Path.GetFileNameWithoutExtension(fileInfo.Name),//插件文件名称（分类）
                        PluginType = (typeof(CollectBase).IsAssignableFrom(item.Value)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business, //插件类型
                        EducationPlugin = PluginServiceUtil.IsEducation(item.Value),
                        Display = PluginServiceUtil.IsEducation(item.Value),
                        Version = item.Value.Assembly.GetName().Version.ToString(), //插件版本

                        LastWriteTime = lastWriteTime, //编译时间
                    };
                    if (!item.Value.Assembly.Location.IsNullOrEmpty())
                        pluginInfo.Directory = Path.GetRelativePath(AppContext.BaseDirectory, Path.GetDirectoryName(item.Value.Assembly.Location));

                    plugins.Add(pluginInfo);
                }
            }

            // 获取插件文件夹路径列表
            string[] folderPaths = Directory.GetDirectories(PathHelper.CombinePathReplace(AppContext.BaseDirectory, tempDir));

            // 遍历插件文件夹
            foreach (string folderPath1 in folderPaths)
            {
                //当前插件文件夹
                var folderPath = Path.GetRelativePath(AppContext.BaseDirectory, folderPath1);
                try
                {
                    var driverMainName = Path.GetFileName(folderPath);
                    FileInfo fileInfo = new FileInfo(PathHelper.CombinePathReplace(folderPath, $"{driverMainName}.dll")); //插件主程序集名称约定为文件夹名称.dll
                    DateTime lastWriteTime = fileInfo.LastWriteTime;

                    // 加载插件程序集并获取其中的驱动类型信息
                    var assembly = GetAssembly(PathHelper.CombinePathReplace(folderPath, $"{driverMainName}.dll"), driverMainName);

                    if (assembly == null) continue;
                    var driverTypes = assembly?.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(BusinessBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);

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
                                if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information) == true)
                                    _logger?.LogInformation(string.Format(AppResource.LoadTypeSuccess, PluginInfoUtil.GetFullName(driverMainName, type.Name)));
                            }
                            var plugin = new PluginInfo()
                            {
                                Name = type.Name, //类型名称
                                FileName = $"{driverMainName}", //主程序集名称
                                PluginType = (typeof(CollectBase).IsAssignableFrom(type)) ? PluginTypeEnum.Collect : PluginTypeEnum.Business,//插件类型
                                Version = assembly.GetName().Version.ToString(),//插件版本
                                LastWriteTime = lastWriteTime, //编译时间
                                EducationPlugin = PluginServiceUtil.IsEducation(type),
                            };
                            plugin.Directory = folderPath;

                            plugins.Add(plugin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录加载插件失败的日志
                    if (_logger?.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning) == true)
                        _logger?.LogWarning(ex, string.Format(AppResource.LoadPluginFail, Path.GetRelativePath(PathHelper.CombinePathReplace(AppContext.BaseDirectory, tempDir), folderPath)));
                }
            }
            return plugins.DistinctBy(a => a.FullName).OrderBy(a => a.EducationPlugin);
        }
    }
    //public Task SavePlugin(PluginAddInput plugin)
    //{
    //    return PluginInfoUtil.SavePlugin(plugin);
    //}
}
