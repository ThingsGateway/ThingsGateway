#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.FriendlyException;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 驱动插件服务  
/// <para>2023-4-13---插件实例化后无法卸载成功，卸载条件过于苛刻，所以去掉动态更新,优化部分方法,等待netCore更新后再行改进</para>
/// </summary>
public class PluginSingletonService : ISingleton
{
    private readonly ILogger<PluginSingletonService> _logger;
    private static IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="PluginSingletonService"/>
    public PluginSingletonService(ILogger<PluginSingletonService> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 插件文件名称/插件域
    /// </summary>
    public ConcurrentDictionary<string, AssemblyLoadContext> AssemblyLoadContextDict { get; private set; } = new();
    /// <summary>
    /// 插件文件名称/插件程序集
    /// </summary>
    public ConcurrentDictionary<string, Assembly> AssemblyDict { get; private set; } = new();
    /// <summary>
    /// 插件ID/插件Type
    /// </summary>
    public ConcurrentDictionary<long, Type> DriverPluginDict { get; private set; } = new();
    /// <summary>
    /// 插件ID/设备ID集合
    /// </summary>
    public ConcurrentDictionary<long, List<long>> DeviceOnDriverPluginDict { get; private set; } = new();
    /// <summary>
    /// 旧插件域
    /// </summary>
    public ConcurrentList<WeakReference> WeakReferences { get; private set; } = new();

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <param name="devId"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public object GetDriver(long devId, DriverPlugin plugin)
    {
        lock (this)
        {
            //先判断是否已经拥有插件模块
            if (DriverPluginDict.ContainsKey(plugin.Id))
            {
                object driver = Activator.CreateInstance(DriverPluginDict[plugin.Id], _scopeFactory);
                DeviceOnDriverPluginDict[plugin.Id].AddRangeIfNotContains(devId);
                return driver;
            }
            Assembly assembly = null;
            var driverFilePath = plugin.FilePath;
            _logger?.LogInformation($"添加插件文件：{driverFilePath}");

            var path = AppContext.BaseDirectory.CombinePathOS(driverFilePath);
            var parPath = Path.GetDirectoryName(path);
            List<string> paths = new();
            var pathArrays = Directory.GetFiles(parPath, "*.dll").ToList();
            pathArrays.ForEach(a => paths.Add(a.Replace("\\", "/")));

            if (AssemblyDict.ContainsKey(plugin.FileName))
            {
                assembly = AssemblyDict[plugin.FileName];
            }
            else
            {
                var assemblyLoadContext = new AssemblyLoadContext(plugin.Id.ToString(), true);
                assembly = GetAssembly(path, paths, assemblyLoadContext);
                AssemblyLoadContextDict.TryAdd(plugin.FileName, assemblyLoadContext);
                AssemblyDict.TryAdd(plugin.FileName, assembly);
            }

            //主程序集
            if (assembly != null)
            {
                switch (plugin.DriverTypeEnum)
                {
                    case DriverEnum.Collect:
                        var driverBase = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName.Replace(plugin.FileName + ".", ""));
                        if (driverBase != null)
                        {
                            object driver = Activator.CreateInstance(driverBase, _scopeFactory);
                            _logger?.LogInformation($"加载插件 {driverFilePath}-{plugin.AssembleName} 成功");
                            DriverPluginDict.TryAdd(plugin.Id, driverBase);
                            DeviceOnDriverPluginDict.TryAdd(plugin.Id, new() { devId });
                            return driver;
                        }
                        else
                        {
                            _logger?.LogError($"加载插件 {driverFilePath}-{plugin.AssembleName} 失败，{plugin.AssembleName}不存在");
                            return null;
                        }
                    case DriverEnum.Upload:
                        var upLoadBase = assembly.GetTypes().Where(x => (typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName.Replace(plugin.FileName + ".", ""));
                        if (upLoadBase != null)
                        {
                            object driver = Activator.CreateInstance(upLoadBase, _scopeFactory);
                            _logger?.LogInformation($"加载插件 {driverFilePath}-{plugin.AssembleName} 成功");
                            DriverPluginDict.TryAdd(plugin.Id, upLoadBase);
                            DeviceOnDriverPluginDict.TryAdd(plugin.Id, new() { devId });
                            return driver;
                        }
                        else
                        {
                            _logger?.LogError($"加载插件 {driverFilePath}-{plugin.AssembleName} 失败，{plugin.AssembleName}不存在");
                            return null;
                        }
                }
                _logger?.LogError($"加载驱动插件 {driverFilePath} 失败，{plugin.DriverTypeEnum}配置错误");
                return null;
            }
            else
            {
                _logger?.LogError($"加载驱动插件 {path} 失败，文件不存在");
                return null;
            }

            static Assembly GetAssembly(string path, List<string> paths, AssemblyLoadContext assemblyLoadContext)
            {
                Assembly assembly = null;
                foreach (var item in paths)
                {
                    using (var fs = new FileStream(item, FileMode.Open))
                    {
                        if (item == path)
                            assembly = assemblyLoadContext.LoadFromStream(fs);
                        else
                            assemblyLoadContext.LoadFromStream(fs);
                    }
                }
                return assembly;
            }
        }
    }
    /// <summary>
    /// 尝试添加插件，返回插件表示类，这个方法完成后会完全卸载插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public async Task<List<DriverPlugin>> TestAddDriverAsync(DriverPluginAddInput plugin)
    {
        var devId = YitIdHelper.NextId();
        var assemblyLoadContext = new AssemblyLoadContext(devId.ToString(), true);

        try
        {
            Assembly assembly = null;
            var driverPlugins = new List<DriverPlugin>();
            var mainFile = plugin.MainFile;
            var otherFiles = plugin.OtherFiles;
            var maxFileSize = 512000000;
            var mainFileName = Path.GetFileNameWithoutExtension(mainFile.Name);
            var fullDir = AppContext.BaseDirectory.CombinePathOS("Plugins", mainFileName);
            var dir = "Plugins".CombinePathOS(mainFileName);
            var path = fullDir.CombinePathOS(mainFile.Name);
            var stream = mainFile.OpenReadStream(maxFileSize);
            Directory.CreateDirectory(AppContext.BaseDirectory.CombinePathOS("Plugins", mainFileName));
            using FileStream fs = new(path, FileMode.Create);
            await stream.CopyToAsync(fs);
            fs.Position = 0;
            assembly = assemblyLoadContext.LoadFromStream(fs);
            foreach (var item in otherFiles)
            {
                var otherStream = item.OpenReadStream(maxFileSize);
                using FileStream fs1 = new(fullDir.CombinePathOS(item.Name), FileMode.Create);
                await otherStream.CopyToAsync(fs1);
                fs1.Position = 0;
                assemblyLoadContext.LoadFromStream(fs1);
            }
            if (assembly != null)
            {

                var driverBase = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);
                foreach (var item in driverBase)
                {
                    if (item != null)
                    {
                        driverPlugins.Add(new DriverPlugin()
                        {
                            AssembleName = item.ToString(),
                            DriverTypeEnum = DriverEnum.Collect,
                            FilePath = dir.CombinePathOS(mainFile.Name),
                            FileName = mainFileName,
                        });
                    }
                    else
                    {
                        //throw Oops.Bah("找不到对应的驱动");
                    }
                }
                var upLoadBase = assembly.GetTypes().Where(x => (typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);
                foreach (var item in upLoadBase)
                {
                    if (item != null)
                    {
                        driverPlugins.Add(new DriverPlugin()
                        {
                            AssembleName = item.ToString(),
                            FilePath = dir.CombinePathOS(mainFile.Name),
                            FileName = mainFileName,
                            DriverTypeEnum = DriverEnum.Upload,
                        });
                    }
                    else
                    {
                        //throw Oops.Bah("找不到对应的驱动");
                    }
                }

            }
            else
            {
                throw Oops.Bah("加载驱动文件失败");
            }
            if (driverPlugins.Count == 0)
            {
                throw Oops.Bah("找不到对应的驱动");
            }
            return driverPlugins;
        }
        finally
        {
            assemblyLoadContext.Unload();
        }
    }
    /// <summary>
    /// 删除插件，该方法已取消
    /// </summary>
    /// <param name="devId"></param>
    /// <param name="pluginId"></param>
    public void DeleteDriver(long devId, long pluginId)
    {
        //try
        //{
        //    foreach (WeakReference item in WeakReferences)
        //    {
        //        if (item.IsAlive)
        //        {
        //            GC.Collect();
        //            GC.WaitForPendingFinalizers();
        //        }
        //        else
        //        {
        //            WeakReferences.Remove(item);
        //        }
        //    }
        //}
        //catch
        //{

        //}
        //using var serviceScope = _scopeFactory.CreateScope();
        //var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        //var plugin = driverPluginService.GetDriverPluginById(pluginId);
        //var plugins = driverPluginService.GetCacheList();
        //var pluginGroups = plugins.GroupBy(a => a.FileName).ToList();
        //if (DeviceOnDriverPluginDict.ContainsKey(pluginId))
        //{
        //    DeviceOnDriverPluginDict[pluginId].Remove(devId);
        //    if (DeviceOnDriverPluginDict[pluginId].Count == 0)
        //    {
        //        DeviceOnDriverPluginDict.Remove(pluginId);
        //        DriverPluginDict.Remove(pluginId);
        //        if (pluginGroups.FirstOrDefault(a => a.Key == plugin.FileName).Where(a => DriverPluginDict.ContainsKey(a.Id)).Count() <= 0)
        //        {
        //            var assemblyLoadContext = AssemblyLoadContextDict.GetValueOrDefault(plugin.FileName);
        //            if (assemblyLoadContext != null)
        //            {
        //                AssemblyDict.Remove(plugin.FileName);
        //                AssemblyLoadContextDict.Remove(plugin.FileName);
        //                WeakReference alcWeakRef = new WeakReference(assemblyLoadContext, true);
        //                WeakReferences.Add(alcWeakRef);
        //                assemblyLoadContext.Unload();
        //                GC.Collect();
        //                GC.WaitForPendingFinalizers();
        //            }
        //        }

        //    }
        //}
    }
    /// <summary>
    /// 获取插件的属性值
    /// </summary>
    public List<DependencyProperty> GetDriverProperties(DriverBase driver)
    {
        var data = driver.DriverPropertys?.GetType().GetAllProps().SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<DevicePropertyAttribute>() } })
            .Where(x => x.devicePropertyAttribute != null).ToList()
              .SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Name,
                      Remark=it.devicePropertyAttribute.Description,
                      Value=it.property.GetValue(driver.DriverPropertys)?.ToString(),
                  }
              });
        return data.ToList();
    }

    /// <summary>
    /// 设置插件的属性值
    /// </summary>
    public void SetDriverProperties(DriverBase driver, List<DependencyProperty> deviceProperties)
    {
        var pluginPropertys = driver.DriverPropertys?.GetType().GetAllProps().Where(a => a.GetCustomAttribute(typeof(DevicePropertyAttribute)) != null)?.ToList();
        foreach (var propertyInfo in pluginPropertys ?? new())
        {
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.PropertyName == propertyInfo.Name);
            if (deviceProperty == null) continue;
            var value = ReadWriteHelpers.ObjToTypeValue(propertyInfo, deviceProperty?.Value ?? "");
            propertyInfo.SetValue(driver.DriverPropertys, value);
        }
    }


    /// <summary>
    /// 获取插件的变量上传属性值
    /// </summary>
    public List<DependencyProperty> GetDriverVariableProperties(UpLoadBase driver)
    {
        var data = driver.VariablePropertys?.GetType().GetAllProps()?.SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<VariablePropertyAttribute>() } })
            ?.Where(x => x.devicePropertyAttribute != null).ToList()
              ?.SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Name,
                      Remark=it.devicePropertyAttribute.Description,
                      Value=it.property.GetValue(driver.VariablePropertys)?.ToString(),
                  }
              });
        return data?.ToList();
    }

    /// <summary>
    /// 获取插件方法
    /// </summary>
    /// <param name="driver"></param>
    /// <returns></returns>
    public List<MethodInfo> GetMethod(object driver)
    {
        return driver.GetType().GetMethods().Where(
               x => x.GetCustomAttribute(typeof(DeviceMethodAttribute)) != null).ToList();
    }

}