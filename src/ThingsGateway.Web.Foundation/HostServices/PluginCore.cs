using Furion.FriendlyException;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 驱动插件服务
/// </summary>
public class PluginCore : ISingleton
{
    private readonly ILogger<PluginCore> _logger;
    private static IServiceScopeFactory _scopeFactory;
    public PluginCore(ILogger<PluginCore> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 插件ID/插件域
    /// </summary>
    public ConcurrentDictionary<long, AssemblyLoadContext> AssemblyLoadContexts { get; private set; } = new();
    /// <summary>
    /// 旧插件域
    /// </summary>
    public ConcurrentList<WeakReference> WeakReferences { get; private set; } = new();
    /// <summary>
    /// 插件ID/插件Type
    /// </summary>
    public ConcurrentDictionary<long, Type> DriverPlugins { get; private set; } = new();
    /// <summary>
    /// 插件ID/设备ID集合
    /// </summary>
    public ConcurrentDictionary<long, List<long>> DeviceOnDriverPlugins { get; private set; } = new();

    public object AddDriver(long devId, DriverPlugin plugin)
    {
        //先判断是否已经拥有插件模块
        if (DriverPlugins.ContainsKey(plugin.Id))
        {
            object driver = Activator.CreateInstance(DriverPlugins[plugin.Id], _scopeFactory);
            DeviceOnDriverPlugins[plugin.Id].AddRangeIfNotContains(devId);
            return driver;
        }

        Assembly assembly = null;
        var driverFiles = plugin.FilePath;
        _logger?.LogInformation($"添加插件文件：{driverFiles}");
        var path = Path.Combine(AppContext.BaseDirectory, plugin.FilePath);
        var parPath = Path.GetDirectoryName(path);
        var paths = Directory.GetFiles(parPath, "*.dll");
        if (AssemblyLoadContexts.ContainsKey(plugin.Id))
        {
            var assemblyLoadContext = AssemblyLoadContexts[plugin.Id];
            assembly = GetAssembly(path, paths, assemblyLoadContext);
        }
        else
        {
            var assemblyLoadContext = new AssemblyLoadContext(plugin.Id.ToString(), true);
            AssemblyLoadContexts.TryAdd(plugin.Id, assemblyLoadContext);
            assembly = GetAssembly(path, paths, assemblyLoadContext);
        }

        if (assembly != null)
        {
            switch (plugin.DriverTypeEnum)
            {
                case DriverEnum.Collect:
                    var driverBase = assembly.GetTypes().Where(x => (typeof(DriverBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName.Replace(plugin.FileName + ".", ""));
                    if (driverBase != null)
                    {
                        object driver = Activator.CreateInstance(driverBase, _scopeFactory);
                        _logger?.LogInformation($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 成功");
                        DriverPlugins.TryAdd(plugin.Id, driverBase);
                        DeviceOnDriverPlugins.TryAdd(plugin.Id, new() { devId });
                        return driver;
                    }
                    else
                    {
                        _logger?.LogError($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 失败，{plugin.AssembleName}不存在");
                        return null;
                    }
                case DriverEnum.Upload:
                    var upLoadBase = assembly.GetTypes().Where(x => (typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName.Replace(plugin.FileName + ".", ""));
                    if (upLoadBase != null)
                    {
                        object driver = Activator.CreateInstance(upLoadBase, _scopeFactory);
                        _logger?.LogInformation($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 成功");
                        DriverPlugins.TryAdd(plugin.Id, upLoadBase);
                        DeviceOnDriverPlugins.TryAdd(plugin.Id, new() { devId });
                        return driver;
                    }
                    else
                    {
                        _logger?.LogError($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 失败，{plugin.AssembleName}不存在");
                        return null;
                    }
            }
            _logger?.LogError($"加载驱动插件 {plugin.FilePath} 失败，{plugin.DriverTypeEnum}配置错误");
            return null;
        }
        else
        {
            _logger?.LogError($"加载驱动插件 {plugin.FilePath} 失败，文件不存在");
            return null;
        }

        static Assembly GetAssembly(string path, string[] paths, AssemblyLoadContext assemblyLoadContext)
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

    public async Task<List<DriverPlugin>> TestAddDriver(DriverPluginAddInput plugin)
    {
        var devId = YitIdHelper.NextId();
        var assemblyLoadContext = new AssemblyLoadContext(devId.ToString(), true);
        try
        {
            Assembly assembly = null;
            var driverPlugins = new List<DriverPlugin>();
            var mainFile = plugin.MainFile;
            var otherFiles = plugin.OtherFiles;
            var maxFileSize = 5120000;
            var mainFileName = Path.GetFileNameWithoutExtension(mainFile.Name);
            var fullDir = Path.Combine(AppContext.BaseDirectory, "Plugins", mainFileName);
            var dir = Path.Combine("Plugins", mainFileName);
            var path = Path.Combine(fullDir, mainFile.Name);
            var stream = mainFile.OpenReadStream(maxFileSize);
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Plugins", mainFileName));
            using FileStream fs = new(path, FileMode.Create);
            await stream.CopyToAsync(fs);
            fs.Position = 0;
            assembly = assemblyLoadContext.LoadFromStream(fs);
            foreach (var item in otherFiles)
            {
                var otherStream = item.OpenReadStream(maxFileSize);
                using FileStream fs1 = new(Path.Combine(fullDir, item.Name), FileMode.Create);
                await otherStream.CopyToAsync(fs1);
                fs1.Position = 0;
                assemblyLoadContext.LoadFromStream(fs1);
            }
            if (assembly != null)
            {

                var driverBase = assembly.GetTypes().Where(x => (typeof(DriverBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract);
                foreach (var item in driverBase)
                {
                    if (item != null)
                    {
                        //object driver = Activator.CreateInstance(item, _scopeFactory);
                        driverPlugins.Add(new DriverPlugin()
                        {
                            AssembleName = item.ToString(),
                            DriverTypeEnum = DriverEnum.Collect,
                            FilePath = Path.Combine(dir, mainFile.Name),
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
                        //object driver = Activator.CreateInstance(item, _scopeFactory);
                        driverPlugins.Add(new DriverPlugin()
                        {
                            AssembleName = item.ToString(),
                            FilePath = Path.Combine(dir, mainFile.Name),
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

    public void DeleteDriver(long devId, long pluginId)
    {
        try
        {
            foreach (WeakReference item in WeakReferences)
            {
                if (item.IsAlive)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                else
                {
                    WeakReferences.Remove(item);
                }
            }
        }
        catch
        {

        }

        if (DeviceOnDriverPlugins.ContainsKey(pluginId))
        {
            DeviceOnDriverPlugins[pluginId].Remove(devId);
            if (DeviceOnDriverPlugins[pluginId].Count == 0)
            {
                DeviceOnDriverPlugins.Remove(pluginId);
                DriverPlugins.Remove(pluginId);
                var assemblyLoadContext = AssemblyLoadContexts.GetValueOrDefault(pluginId);
                if (assemblyLoadContext != null)
                {
                    AssemblyLoadContexts.Remove(pluginId);
                    WeakReference alcWeakRef = new WeakReference(assemblyLoadContext, true);
                    WeakReferences.Add(alcWeakRef);
                    assemblyLoadContext.Unload();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }

    /// <summary>
    /// 获取插件的属性值
    /// </summary>
    public List<DependencyProperty> GetDriverProperties(object driver)
    {
        var data = driver.GetType().GetProperties().SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<DevicePropertyAttribute>() } })
            .Where(x => x.devicePropertyAttribute != null).ToList()
              .SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Name,
                      Remark=it.devicePropertyAttribute.Remark,
                      Value=it.property.GetValue(driver)?.ToString(),
                  }
              });
        return data.ToList();
    }

    /// <summary>
    /// 获取插件的变量上传属性值
    /// </summary>
    public List<DependencyProperty> GetDriverVariableProperties(object driver)
    {
        var data = driver.GetType().GetProperties().SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<VariablePropertyAttribute>() } })
            .Where(x => x.devicePropertyAttribute != null).ToList()
              .SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Name,
                      Remark=it.devicePropertyAttribute.Remark,
                      Value=it.property.GetValue(driver)?.ToString(),
                  }
              });
        return data.ToList();
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