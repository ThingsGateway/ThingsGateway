//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 获取后台服务
/// </summary>
public static class HostedServiceUtil
{
    private static AlarmHostedService? alarmHostedService;

    public static AlarmHostedService AlarmHostedService
    {
        get
        {
            alarmHostedService ??= GetHostedService<AlarmHostedService>();
            return alarmHostedService;
        }
    }

    private static CollectDeviceHostedService? collectDeviceHostedService;

    public static CollectDeviceHostedService CollectDeviceHostedService
    {
        get
        {
            collectDeviceHostedService ??= GetHostedService<CollectDeviceHostedService>();
            return collectDeviceHostedService;
        }
    }

    private static BusinessDeviceHostedService? businessDeviceHostedService;

    public static BusinessDeviceHostedService BusinessDeviceHostedService
    {
        get
        {
            businessDeviceHostedService ??= GetHostedService<BusinessDeviceHostedService>();
            return businessDeviceHostedService;
        }
    }

    private static HardwareInfoService? hardwareInfoHostedService;

    public static HardwareInfoService HardwareInfoHostedService
    {
        get
        {
            hardwareInfoHostedService ??= GetHostedService<HardwareInfoService>();
            return hardwareInfoHostedService;
        }
    }

    private static ManagementHostedService? managementHostedService;

    public static ManagementHostedService ManagementHostedService
    {
        get
        {
            managementHostedService ??= GetHostedService<ManagementHostedService>();
            return managementHostedService;
        }
    }

    /// <summary>
    /// 获取HostedService
    /// </summary>
    public static T GetHostedService<T>() where T : class, IHostedService
    {
        var hostedService = App.RootServices.GetServices<IHostedService>().FirstOrDefault(it => it is T) as T;
        return hostedService;
    }
}
