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
using Furion.FriendlyException;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

using ThingsGateway.Application.Extensions;

using Yitter.IdGenerator;

namespace ThingsGateway.Application;
/// <summary>
/// 驱动插件服务  
/// </summary>
public class PluginSingletonService : ISingleton
{

    private readonly ILogger<PluginSingletonService> _logger;
    /// <inheritdoc cref="PluginSingletonService"/>
    public PluginSingletonService(ILogger<PluginSingletonService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 插件文件路径/插件程序集
    /// </summary>
    public ConcurrentDictionary<string, Assembly> AssemblyDict { get; private set; } = new();

    /// <summary>
    /// 插件文件路径/插件域
    /// </summary>
    public ConcurrentDictionary<string, AssemblyLoadContext> AssemblyLoadContextDict { get; private set; } = new();
    /// <summary>
    /// 插件ID/插件Type
    /// </summary>
    public ConcurrentDictionary<long, Type> DriverPluginDict { get; private set; } = new();

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public DriverBase GetDriver(DriverPlugin plugin)
    {
        lock (this)
        {
            //先判断是否已经拥有插件模块
            if (DriverPluginDict.ContainsKey(plugin.Id))
            {
                var driver = (DriverBase)Activator.CreateInstance(DriverPluginDict[plugin.Id]);
                driver.DriverPlugin = plugin;
                return driver;
            }
            Assembly assembly = null;

            _logger?.LogInformation($"添加插件文件：{plugin.FilePath}");

            //根据路径获取dll文件

            //主程序集路径
            var path = AppContext.BaseDirectory.CombinePathOS(plugin.FilePath);
            //全部程序集路径
            List<string> paths = new();
            Directory.GetFiles(Path.GetDirectoryName(path), "*.dll").ToList().
                ForEach(a => paths.Add(a.Replace("\\", "/")));

            if (AssemblyDict.ContainsKey(plugin.FilePath))
            {
                assembly = AssemblyDict[plugin.FilePath];
            }
            else
            {
                //新建插件域，并注明不可卸载
                var assemblyLoadContext = new AssemblyLoadContext(plugin.Id.ToString(), false);
                //获取插件程序集
                assembly = GetAssembly(path, paths, assemblyLoadContext);
                //添加到全局对象
                AssemblyLoadContextDict.TryAdd(plugin.FilePath, assemblyLoadContext);
                AssemblyDict.TryAdd(plugin.FilePath, assembly);
            }

            if (assembly != null)
            {
                //根据采集/上传类型获取实际插件类
                switch (plugin.DriverTypeEnum)
                {
                    case DriverEnum.Collect:
                        var driverType = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName);
                        if (driverType != null)
                        {
                            return GetDriver(plugin, driverType);
                        }
                        break;
                    case DriverEnum.Upload:
                        var upLoadType = assembly.GetTypes().Where(x => (typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).FirstOrDefault(it => it.Name == plugin.AssembleName);
                        if (upLoadType != null)
                        {
                            return GetDriver(plugin, upLoadType);
                        }
                        break;
                }

                throw new Exception($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 失败，{plugin.AssembleName}不存在");

            }
            else
            {
                throw new Exception($"加载驱动插件 {path} 失败，文件不存在");
            }
            DriverBase GetDriver(DriverPlugin plugin, Type driverType)
            {
                var driver = (DriverBase)Activator.CreateInstance(driverType);
                _logger?.LogInformation($"加载插件 {plugin.FilePath}-{plugin.AssembleName} 成功");
                DriverPluginDict.TryAdd(plugin.Id, driverType);
                driver.DriverPlugin = plugin;
                return driver;
            }
            Assembly GetAssembly(string path, List<string> paths, AssemblyLoadContext assemblyLoadContext)
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
                            _logger.LogWarning($"尝试加载附属程序集{item}失败，如果此程序集为非引用，比如非托管DllImport，可以忽略此警告。错误信息：{(ex.Message)}");
                        }
                    }
                }
                return assembly;
            }
        }
    }

    /// <summary>
    /// 获取插件的属性值
    /// </summary>
    public List<DependencyProperty> GetDriverProperties(DriverBase driver)
    {
        var data = driver.DriverPropertys?.GetType().GetPropertiesWithCache().SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<DevicePropertyAttribute>() } })
            .Where(x => x.devicePropertyAttribute != null).ToList()
              .SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Description,
                      Remark=it.devicePropertyAttribute.Remark,
                      Value=it.property.GetValue(driver.DriverPropertys)?.ToString(),
                  }
              });
        return data.ToList();
    }

    /// <summary>
    /// 获取插件的变量上传属性值
    /// </summary>
    public List<DependencyProperty> GetDriverVariableProperties(UpLoadBase driver)
    {
        var data = driver.VariablePropertys?.GetType().GetPropertiesWithCache()?.SelectMany(it =>
            new[] { new { property = it, devicePropertyAttribute = it.GetCustomAttribute<VariablePropertyAttribute>() } })
            ?.Where(x => x.devicePropertyAttribute != null).ToList()
              ?.SelectMany(it => new[]
              {
                  new DependencyProperty(){
                      PropertyName=it.property.Name,
                      Description=it.devicePropertyAttribute.Description,
                      Remark=it.devicePropertyAttribute.Remark,
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
    public List<MethodInfo> GetMethod(DriverBase driver)
    {
        return driver.GetType().GetMethods().Where(
               x => x.GetCustomAttribute(typeof(DeviceMethodAttribute)) != null).ToList();
    }

    /// <summary>
    /// 设置插件的属性值
    /// </summary>
    public void SetDriverProperties(DriverBase driver, List<DependencyProperty> deviceProperties)
    {
        var pluginPropertys = driver.DriverPropertys?.GetType().GetPropertiesWithCache().Where(a => a.GetCustomAttribute(typeof(DevicePropertyAttribute)) != null)?.ToList();
        foreach (var propertyInfo in pluginPropertys ?? new())
        {
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.PropertyName == propertyInfo.Name);
            if (deviceProperty == null) continue;
            var value = ReadWriteHelpers.ObjToTypeValue(propertyInfo, deviceProperty?.Value ?? "");
            propertyInfo.SetValue(driver.DriverPropertys, value);
        }
    }

    /// <summary>
    /// 尝试添加插件，返回插件表示类，方法完成后会完全卸载插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public async Task<List<DriverPlugin>> TestAddDriverAsync(DriverPluginAddInput plugin)
    {
        var assemblyLoadContext = new AssemblyLoadContext(YitIdHelper.NextId().ToString(), true);
        try
        {

            var driverPlugins = new List<DriverPlugin>();
            var maxFileSize = 100 * 1024 * 1024;//最大100m
            //主程序集名称
            var mainFileName = Path.GetFileNameWithoutExtension(plugin.MainFile.Name);
            //插件文件夹相对路径
            var dir = "Plugins".CombinePathOS(mainFileName);
            //插件文件夹绝对路径
            var fullDir = AppContext.BaseDirectory.CombinePathOS(dir);
            //主程序集相对路径
            var path = dir.CombinePathOS(plugin.MainFile.Name);
            //主程序集绝对路径
            var fullPath = fullDir.CombinePathOS(plugin.MainFile.Name);

            //主程序集相对路径
            //获取文件流
            using var stream = plugin.MainFile.OpenReadStream(maxFileSize);
            Directory.CreateDirectory(fullDir);//创建插件文件夹
            using FileStream fs = new(fullPath, FileMode.Create);
            await stream.CopyToAsync(fs);
            fs.Seek(0, SeekOrigin.Begin);

            //获取主程序集
            var assembly = assemblyLoadContext.LoadFromStream(fs);
            foreach (var item in plugin.OtherFiles)
            {
                using var otherStream = item.OpenReadStream(maxFileSize);
                using FileStream fs1 = new(fullDir.CombinePathOS(item.Name), FileMode.Create);
                await otherStream.CopyToAsync(fs1);
                fs1.Seek(0, SeekOrigin.Begin);
                try
                {
                    assemblyLoadContext.LoadFromStream(fs1);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"尝试加载附属程序集{item}失败，如果此程序集为非引用，比如非托管DllImport，可以忽略此警告。错误信息：{(ex.Message)}");
                }
            }
            if (assembly != null)
            {
                //获取插件的相关信息
                var collectBase = assembly.GetTypes().Where(x => (typeof(CollectBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).ToList();
                for (int i = 0; i < collectBase.Count; i++)
                {
                    var item = collectBase[i];
                    driverPlugins.Add(new DriverPlugin()
                    {
                        AssembleName = item.Name,
                        DriverTypeEnum = DriverEnum.Collect,
                        FilePath = path,
                        FileName = mainFileName,
                    });
                }
                collectBase.ForEach(a => a = null);
                collectBase.Clear();
                collectBase = null;
                var upLoadBase = assembly.GetTypes().Where(x => (typeof(UpLoadBase).IsAssignableFrom(x)) && x.IsClass && !x.IsAbstract).ToList();
                for (int i = 0; i < upLoadBase.Count; i++)
                {
                    var item = upLoadBase[i];
                    driverPlugins.Add(new DriverPlugin()
                    {
                        AssembleName = item.Name,
                        DriverTypeEnum = DriverEnum.Upload,
                        FilePath = path,
                        FileName = mainFileName,
                    });
                }
                upLoadBase.ForEach(a => a = null);
                upLoadBase.Clear();
                upLoadBase = null;
            }
            else
            {
                throw Oops.Bah("加载驱动文件失败");
            }
            if (driverPlugins.Count == 0)
            {
                throw Oops.Bah("找不到对应的驱动");
            }
            assembly = null;
            return driverPlugins;
        }
        finally
        {
            assemblyLoadContext.Unload();
        }
    }
}