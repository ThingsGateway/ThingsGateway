//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion;

using NewLife;
using NewLife.Configuration;

namespace ThingsGateway.Cache;

/// <summary>
/// AppStartup启动类
/// </summary>
public class Startup : AppStartup
{
    /// <summary>
    /// ConfigureServices中不能解析服务，比如App.GetService()，尤其是不能在ConfigureServices中获取诸如缓存等数据进行初始化，应该在Configure中进行
    /// 服务都还没初始化完成，会导致内存中存在多份 IOC 容器！！
    /// 正确应该在 Configure 中，这个时候服务（IServiceCollection 已经完成 BuildServiceProvider() 操作了
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISimpleCacheService, MemoryCacheService>();

        var provider = new JsonConfigProvider();
        provider.FileName = "newlife.config.json";
        Setting.Provider = provider;
        Setting.Provider.Name = "newlife.config";
        Setting.Current = new();
        Setting.Current.Debug = false;
        Setting.Current.LogLevel = NewLife.Log.LogLevel.Warn;
        Setting.Current.PluginServer = "null";
        Setting.Current.ServiceAddress = string.Empty;
        Setting.Current.Save();
    }
}