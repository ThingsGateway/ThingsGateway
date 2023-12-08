#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Furion.DependencyInjection;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Xml.Linq;

using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
public class DriverPluginService : ISingleton
{
    public const string DefaultKey = "默认";
    private readonly IServiceScope _serviceScope;
    private readonly ILogger _logger;

    /// <inheritdoc cref="DriverPluginService"/>
    public DriverPluginService(
    IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory)
    {
        Directory.CreateDirectory(AppContext.BaseDirectory.CombinePath(DriverPluginService.Plugins));//创建插件文件夹
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = loggerFactory.CreateLogger("驱动插件服务");
        _defaultDriverBaseDict = new(App.EffectiveTypes
            .Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
            .ToDictionary(a => a.Name));
        _stringConverter = new StringConverter();
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
    private StringConverter _stringConverter { get; }

    #region public

    /// <summary>
    /// 获取全部插件信息
    /// </summary>
    /// <returns></returns>
    public List<DriverPlugin> GetAllDriverPlugin(DriverEnum driverEnum)
    {
        var pluginList = GetAllDriverPlugin();
        // 筛选出DriverEnum属性为Collect的节点
        var filteredPlugins = pluginList.Select(p => new DriverPlugin
        {
            Name = p.Name,
            Children = p.Children.Where(c => c.DriverEnum == driverEnum).ToList()
        }).Where(p => p.Children.Any()).ToList();
        return filteredPlugins;
    }

    public const string Plugins = "Plugins";

    /// <summary>
    /// 获取全部插件信息
    /// </summary>
    /// <returns></returns>
    public List<DriverPlugin> GetAllDriverPlugin()
    {
        lock (this)
        {
            var type1 = this.GetType();
            var cacheKey = $"{CultureInfo.CurrentUICulture.Name}-{type1.FullName}-{type1.TypeHandle.Value}";
            var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().GetOrCreate($"{nameof(GetAllDriverPlugin)}", cacheKey, c =>
            {
                List<DriverPlugin> plugins = new List<DriverPlugin>();
                //默认上下文
                DriverPlugin defaultDriverPlugin = new();
                defaultDriverPlugin.Name = DefaultKey;
                defaultDriverPlugin.Children = new();
                foreach (var item in _defaultDriverBaseDict)
                {
                    FileInfo fileInfo = new FileInfo(this.GetType().Assembly.Location);
                    DateTime lastWriteTime = fileInfo.LastWriteTime;
                    defaultDriverPlugin.Children.Add(
                new DriverPlugin()
                {
                    Name = item.Key,
                    FileName = DefaultKey,
                    DriverEnum = (typeof(CollectBase).IsAssignableFrom(item.Value)) ? DriverEnum.Collect : DriverEnum.Upload,
                    Version = this.GetType().Assembly.GetName().Version.ToString(),
                    LastWriteTime = lastWriteTime,
                });
                }
                if (defaultDriverPlugin.Children.Count > 0)
                    plugins.Add(defaultDriverPlugin);

                string[] folderPaths = Directory.GetDirectories(AppContext.BaseDirectory.CombinePath(Plugins));

                foreach (string folderPath in folderPaths)
                {
                    DriverPlugin driverPlugin = new();
                    driverPlugin.Name = Path.GetFileName(folderPath);
                    try
                    {
                        FileInfo fileInfo = new FileInfo(folderPath.CombinePath($"{driverPlugin.Name}.dll"));
                        DateTime lastWriteTime = fileInfo.LastWriteTime;

                        var assembly = GetAssembly(folderPath, folderPath.CombinePath($"{driverPlugin.Name}.dll"), driverPlugin.Name);
                        var driverTypes = assembly.GetTypes().
        Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);
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
                            new DriverPlugin()
                            {
                                Name = type.Name,
                                FileName = $"{driverPlugin.Name}",
                                DriverEnum = (typeof(CollectBase).IsAssignableFrom(type)) ? DriverEnum.Collect : DriverEnum.Upload,
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
            }, true);
            return data;
        }
    }

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <param name="pluginName">主插件程序集文件名称.类型名称</param>
    /// <returns></returns>
    public DriverBase GetDriver(string pluginName)
    {
        lock (this)
        {
            var filtResult = DriverPluginServiceExtensions.GetFileNameAndTypeName(pluginName);
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
            var dir = AppContext.BaseDirectory.CombinePath(Plugins, filtResult.Item1);
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
                //根据采集/上传类型获取实际插件类
                var driverType = assembly.GetTypes().
                    Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract)
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
    }

    /// <summary>
    /// 获取插件方法
    /// </summary>
    public List<DependencyPropertyWithMethodInfo> GetDriverMethodInfo(DriverBase driver)
    {
        var type = driver.GetType();
        var cacheKey = $"{CultureInfo.CurrentUICulture.Name}-{type.FullName}-{type.TypeHandle.Value}";
        var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().GetOrCreate($"{nameof(GetDriverMethodInfo)}", cacheKey, c =>
        {
            var data = driver.GetType().GetMethods()?.SelectMany(it =>
    new[] { new { memberInfo = it, attribute = it.GetCustomAttribute<DeviceMethodAttribute>() } })
    ?.Where(x => x.attribute != null).ToList()
      ?.SelectMany(it => new[]
      {
                  new DependencyPropertyWithMethodInfo(){
                      PropertyName=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      MethodInfo=it.memberInfo,
                  }
      });
            return data?.ToList();
        }, true);
        return data;
    }

    private static T GetCustomAttributeRecursive<T>(PropertyInfo property) where T : Attribute
    {
        var attribute = property.GetCustomAttribute<T>(false);
        if (attribute == null && property.ReflectedType.BaseType != null && property.ReflectedType != typeof(UpDriverPropertyBase))
        {
            var baseProperty = property.ReflectedType.BaseType.GetProperties().FirstOrDefault(p => p.Name == property.Name);
            if (baseProperty != null)
            {
                attribute = GetCustomAttributeRecursive<T>(baseProperty);
            }
        }
        return attribute;
    }

    /// <summary>
    /// 获取插件的属性值
    /// </summary>
    public List<DependencyProperty> GetDriverProperties(DriverBase driver)
    {
        var type = driver.GetType();
        var cacheKey = $"{CultureInfo.CurrentUICulture.Name}-{type.FullName}-{type.TypeHandle.Value}";
        var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().GetOrCreate($"{nameof(GetDriverProperties)}", cacheKey, c =>
        {
            var data = driver.DriverPropertys?.GetType().GetProperties().SelectMany(it =>
    new[] { new { memberInfo = it, attribute = GetCustomAttributeRecursive<DevicePropertyAttribute>(it) } })
    .Where(x => x.attribute != null).ToList()
      .SelectMany(it => new[]
      {
                  new DependencyProperty(){
                      PropertyName=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      Value=_stringConverter.ConvertTo(it.memberInfo.GetValue(driver.DriverPropertys)),
                      //PropertyType=it.memberInfo.PropertyType,
                  }
      });
            return data.ToList();
        }, true);
        return data;
    }

    /// <summary>
    /// 获取插件的变量上传属性值
    /// </summary>
    public List<DependencyProperty> GetDriverVariableProperties(UpLoadBase driver)
    {
        var type = driver.GetType();
        var cacheKey = $"{CultureInfo.CurrentUICulture.Name}-{type.FullName}-{type.TypeHandle.Value}";
        var data = _serviceScope.ServiceProvider.GetService<MemoryCache>().GetOrCreate($"{nameof(GetDriverVariableProperties)}", cacheKey, c =>
        {
            var data = driver.VariablePropertys?.GetType().GetProperties()?.SelectMany(it =>
    new[] { new { memberInfo = it, attribute = GetCustomAttributeRecursive<VariablePropertyAttribute>(it) } })
    ?.Where(x => x.attribute != null).ToList()
      ?.SelectMany(it => new[]
      {
                  new DependencyProperty(){
                      PropertyName=it.memberInfo.Name,
                      Description=it.attribute.Description,
                      Remark=it.attribute.Remark,
                      Value=_stringConverter.ConvertTo(it.memberInfo.GetValue(driver.VariablePropertys)),
                      //PropertyType =it.memberInfo.PropertyType,
                  }
      });
            return data?.ToList();
        }, true);
        return data;
    }

    /// <summary>
    /// 分页显示插件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public SqlSugarPagedList<DriverPlugin> Page(DriverPluginPageInput input)
    {
        var query = GetAllDriverPlugin().SelectMany(a => a.Children)
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))//根据关键字查询
         .WhereIF(!string.IsNullOrEmpty(input.FileName), u => u.FileName.Contains(input.FileName));//根据关键字查询
        StringBuilder stringBuilder = new();
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append(",");
            if (!string.IsNullOrEmpty(input.SortField[i]))
                stringBuilder.Append(input.SortField[i] + (input.SortDesc[i] ? " desc" : " asc"));
        }
        if (stringBuilder.Length > 0)
        {
            var query1 = query.AsQueryable().OrderBy(stringBuilder.ToString()).ToList();
            var pageInfo = query1.ToPagedList(input);//分页
            return pageInfo;
        }
        else
        {
            var pageInfo = query.ToPagedList(input);//分页
            return pageInfo;
        }
    }

    public void Remove()
    {
        lock (this)
        {
            _driverBaseDict.Clear();
            _assemblyDict.Clear();
            foreach (var item in _assemblyLoadContextDict)
            {
                item.Value.Unload();
            }
            _assemblyLoadContextDict.Clear();
            GC.Collect();
            _serviceScope.ServiceProvider.GetService<MemoryCache>().RemoveByPrefix(nameof(GetAllDriverPlugin));
            _serviceScope.ServiceProvider.GetService<MemoryCache>().RemoveByPrefix(nameof(GetDriverMethodInfo));
            _serviceScope.ServiceProvider.GetService<MemoryCache>().RemoveByPrefix(nameof(GetDriverProperties));
            _serviceScope.ServiceProvider.GetService<MemoryCache>().RemoveByPrefix(nameof(GetDriverVariableProperties));
        }
    }

    /// <summary>
    /// 设置插件的属性值
    /// </summary>
    /// <param name="driver">插件实例</param>
    /// <param name="deviceProperties">插件属性，检索相同名称的属性后写入</param>
    public void SetDriverProperties(DriverBase driver, List<DependencyProperty> deviceProperties)
    {
        var pluginPropertys = driver.DriverPropertys?.GetType().GetProperties().Where(a => GetCustomAttributeRecursive<DevicePropertyAttribute>(a) != null)?.ToList();
        foreach (var propertyInfo in pluginPropertys ?? new())
        {
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.PropertyName == propertyInfo.Name);
            if (deviceProperty == null) continue;
            var value = _stringConverter.ConvertFrom(deviceProperty?.Value ?? "", propertyInfo.PropertyType);
            propertyInfo.SetValue(driver.DriverPropertys, value);
        }
    }

    /// <summary>
    /// 尝试添加插件，方法完成后会完全卸载插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public void TryAddDriver(DriverPluginAddInput plugin)
    {
        lock (this)
        {
            var assemblyLoadContext = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
            try
            {
                var maxFileSize = 100 * 1024 * 1024;//最大100m
                                                    //主程序集名称
                var mainFileName = Path.GetFileNameWithoutExtension(plugin.MainFile.Name);
                //插件文件夹绝对路径
                var fullDir = AppContext.BaseDirectory.CombinePath(Plugins, mainFileName);
                //主程序集绝对路径
                var fullPath = fullDir.CombinePathOS(plugin.MainFile.Name);

                //主程序集相对路径
                //获取文件流
                using var stream = plugin.MainFile.OpenReadStream(maxFileSize);
                stream.Seek(0, SeekOrigin.Begin);

                //获取主程序集
                var assembly = assemblyLoadContext.LoadFromStream(stream);
                foreach (var item in plugin.OtherFiles)
                {
                    using var otherStream = item.OpenReadStream(maxFileSize);
                    otherStream.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        assemblyLoadContext.LoadFromStream(otherStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"尝试加载附属程序集{item}失败，如果此程序集为DllImport，可以忽略此警告。错误信息：{(ex.Message)}");
                    }
                }
                if (assembly != null)
                {
                    //获取插件的相关信息
                    var driverTypes = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x) || typeof(UpLoadBase).IsAssignableFrom(x))
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

                stream.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(fullDir);//创建插件文件夹
                using FileStream fs = new(fullPath, FileMode.Create);
                stream.CopyTo(fs);
                foreach (var item in plugin.OtherFiles)
                {
                    using var otherStream = item.OpenReadStream(maxFileSize);
                    otherStream.Seek(0, SeekOrigin.Begin);
                    using FileStream fs1 = new(fullDir.CombinePathOS(item.Name), FileMode.Create);
                    otherStream.CopyTo(fs1);
                }
            }
            finally
            {
                assemblyLoadContext.Unload();
                _serviceScope.ServiceProvider.GetService<MemoryCache>().RemoveByPrefix(nameof(GetAllDriverPlugin));
            }
        }
    }

    #endregion public

    /// <summary>
    /// 删除插件域，卸载插件
    /// </summary>
    /// <param name="path">主程序集文件名称</param>
    private void DeleteDriver(string path)
    {
        if (_assemblyLoadContextDict.TryGetValue(path, out var assemblyLoadContext))
        {
            _driverBaseDict.RemoveWhere(a => path == DriverPluginServiceExtensions.GetFileNameAndTypeName(a.Key).Item1);
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