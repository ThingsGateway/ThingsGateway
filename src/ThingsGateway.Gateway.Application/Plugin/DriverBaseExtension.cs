//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.Collection;
using ThingsGateway.NewLife.X.Extension;

namespace ThingsGateway.Gateway.Application;

public static class DriverBaseExtension
{
    /// <summary>
    /// 创建插件实例，并根据设备属性设置实例
    /// </summary>
    /// <param name="deviceRunTime">当前设备</param>
    /// <param name="pluginService">插件服务</param>
    /// <returns>插件实例</returns>
    public static DriverBase CreateDriver(this DeviceRunTime deviceRunTime, IPluginService pluginService)
    {
        var driver = pluginService.GetDriver(deviceRunTime.PluginName);

        // 初始化插件配置项
        driver.Init(deviceRunTime);

        // 设置设备属性到插件实例
        pluginService.SetDriverProperties(driver, deviceRunTime.DevicePropertys);

        return driver;
    }

    public static void RefreshCollectDeviceRuntime(this CollectDeviceRunTime newDevice, long oldDeviceId)
    {
        // 从全局设备字典中移除具有相同 Id 的设备
        GlobalData.CollectDevices.RemoveWhere(it => it.Value.Id == oldDeviceId);

        // 尝试向全局设备字典中添加当前设备，使用设备名称作为键
        GlobalData.CollectDevices.TryAdd(newDevice.Name, newDevice);

        // 从全局变量字典中移除与当前设备关联的变量
        GlobalData.Variables.RemoveWhere(it => it.Value.DeviceId == oldDeviceId);

        // 遍历当前设备的变量运行时集合，将其中的变量添加到全局变量字典中
        foreach (var item in newDevice.VariableRunTimes)
        {
            GlobalData.Variables.TryAdd(item.Key, item.Value);
        }
    }
    public static void RefreshBusinessDeviceRuntime(this DeviceRunTime newDevice, long oldDeviceId)
    {
        // 移除全局业务设备中与当前设备相同Id的项
        GlobalData.BusinessDevices.RemoveWhere(it => it.Value.Id == oldDeviceId);

        // 添加当前设备到全局业务设备字典中
        GlobalData.BusinessDevices.TryAdd(newDevice.Name, newDevice);
    }

    public static void RemoveCollectDeviceRuntime(this IEnumerable<DriverBase> driverBases)
    {
        GlobalData.CollectDevices.RemoveWhere(it => driverBases.Any(a => a.DeviceId == it.Value.Id));
        GlobalData.Variables.RemoveWhere(it => driverBases.Any(a => a.DeviceId == it.Value.DeviceId));
    }
    public static void RemoveBusinessDeviceRuntime(this IEnumerable<DriverBase> driverBases)
    {
        GlobalData.BusinessDevices.RemoveWhere(it => driverBases.Any(a => a.DeviceId == it.Value.Id));
    }
    public static void RemoveCollectDeviceRuntime(this DriverBase driverBase)
    {
        GlobalData.CollectDevices.RemoveWhere(it => driverBase.DeviceId == it.Value.Id);
        GlobalData.Variables.RemoveWhere(it => driverBase.DeviceId == it.Value.DeviceId);
    }
    public static void RemoveBusinessDeviceRuntime(this DriverBase driverBase)
    {
        GlobalData.BusinessDevices.RemoveWhere(it => driverBase.DeviceId == it.Value.Id);
    }

    /// <summary>
    /// 获取设备的属性值
    /// </summary>
    /// <param name="collectDeviceRunTime">当前设备</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性值，如果不存在则返回null</returns>
    public static string? GetDevicePropertyValue(this DeviceRunTime collectDeviceRunTime, string propertyName)
    {
        if (collectDeviceRunTime == null || propertyName.IsNullOrWhiteSpace())
            return null;

        // 尝试获取指定属性的值
        collectDeviceRunTime.DevicePropertys.TryGetValue(propertyName, out var value);
        return value; // 返回属性值
    }

    /// <summary>
    /// 获取变量的业务属性值
    /// </summary>
    /// <param name="variableRunTime">当前变量</param>
    /// <param name="businessId">对应业务设备Id</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性值，如果不存在则返回null</returns>
    public static string? GetPropertyValue(this VariableRunTime variableRunTime, long businessId, string propertyName)
    {
        if (variableRunTime == null || propertyName.IsNullOrWhiteSpace())
            return null;

        // 检查是否存在对应的业务设备Id
        if (variableRunTime.VariablePropertys?.ContainsKey(businessId) == true)
        {
            variableRunTime.VariablePropertys[businessId].TryGetValue(propertyName, out var value);
            return value; // 返回属性值
        }

        return null; // 未找到对应的业务设备Id，返回null
    }
}
