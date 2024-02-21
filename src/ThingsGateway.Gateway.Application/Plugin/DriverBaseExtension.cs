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

public static class DriverBaseExtension
{
    /// <summary>
    /// 创建插件实例，并且根据设备属性设置实例
    /// </summary>
    /// <returns></returns>
    public static DriverBase CreatDriver(this DeviceRunTime deviceRunTime, IPluginService pluginService)
    {
        var driver = pluginService.GetDriver(deviceRunTime.PluginName);
        //设置插件配置项
        driver.Init(deviceRunTime);
        pluginService.SetDriverProperties(driver, deviceRunTime.DevicePropertys);
        return driver;
    }

    /// <summary>
    /// 获取设备的属性值
    /// </summary>
    public static DependencyProperty GetDevicePropertyValue(this DeviceRunTime collectDeviceRunTime, string propertyName)
    {
        if (collectDeviceRunTime == null)
            return null;
        return collectDeviceRunTime.DevicePropertys.FirstOrDefault(a => a.Name == propertyName);
    }

    /// <summary>
    /// 获取变量的业务属性值
    /// </summary>
    /// <param name="variableRunTime">当前变量</param>
    /// <param name="deviceRunTime">对应业务设备Id</param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static DependencyProperty GetPropertyValue(this VariableRunTime variableRunTime, long businessId, string propertyName)
    {
        if (variableRunTime == null)
            return null;
        if (variableRunTime.VariablePropertys.ContainsKey(businessId))
        {
            var data = variableRunTime.VariablePropertys[businessId]
                .FirstOrDefault(a => a.Name == propertyName);
            return data;
        }
        return null;
    }
}