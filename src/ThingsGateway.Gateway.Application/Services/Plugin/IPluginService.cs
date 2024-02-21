//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
public interface IPluginService : ISingleton
{
    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    List<PluginOutput> GetList(PluginTypeEnum? pluginType = null);

    /// <summary>
    /// 根据插件全名称构建插件实例
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    DriverBase GetDriver(string pluginName);

    /// <summary>
    /// 分页查询插件信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    SqlSugarPagedList<PluginOutput> Page(PluginPageInput input);

    /// <summary>
    /// 清空全部插件信息
    /// </summary>
    void Remove();

    /// <summary>
    /// 设置插件动态属性
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="deviceProperties"></param>
    void SetDriverProperties(DriverBase driver, IEnumerable<DependencyProperty> deviceProperties);

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    Task AddAsync(PluginAddInput plugin);

    /// <summary>
    /// 获取插件的属性类型
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    Dictionary<string, DependencyPropertyWithInfo> GetDriverPropertyTypes(string pluginName, DriverBase? driverBase = null);

    /// <summary>
    /// 获取插件的变量业务属性类型
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    Dictionary<string, DependencyPropertyWithInfo> GetVariablePropertyTypes(string pluginName);

    /// <summary>
    /// 获取插件动态注册的方法
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    Dictionary<string, DependencyPropertyWithInfo> GetDriverMethodInfos(string pluginName, DriverBase? driverBase = null);
}