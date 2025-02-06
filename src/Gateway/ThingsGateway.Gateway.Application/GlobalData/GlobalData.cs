//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using System.Collections.Concurrent;

using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备状态变化委托，用于通知设备状态发生变化时的事件
/// </summary>
/// <param name="deviceRuntime">设备运行时对象</param>
/// <param name="deviceData">设备数据对象</param>
public delegate void DelegateOnDeviceChanged(DeviceRuntime deviceRuntime, DeviceBasicData deviceData);

/// <summary>
/// 变量改变事件委托，用于通知变量值发生变化时的事件
/// </summary>
/// <param name="variableRuntime">变量运行时对象</param>
/// <param name="variableData">变量数据对象</param>
public delegate void VariableChangeEventHandler(VariableRuntime variableRuntime, VariableBasicData variableData);
/// <summary>
/// 变量采集事件委托，用于通知变量进行采集时的事件
/// </summary>
/// <param name="variableRuntime">变量运行时对象</param>
public delegate void VariableCollectEventHandler(VariableRuntime variableRuntime);

/// <summary>
/// 变量报警事件委托
/// </summary>
public delegate void VariableAlarmEventHandler(AlarmVariable alarmVariable);

/// <summary>
/// 采集设备值与状态全局提供类，用于提供全局的设备状态和变量数据的管理
/// </summary>
public static class GlobalData
{
    /// <summary>
    /// 设备状态变化事件，当设备状态发生变化时触发该事件
    /// </summary>
    public static event DelegateOnDeviceChanged DeviceStatusChangeEvent;

    /// <summary>
    /// 变量值改变事件，当变量值发生改变时触发该事件
    /// </summary>
    public static event VariableChangeEventHandler VariableValueChangeEvent;
    /// <summary>
    /// 变量采集事件，当变量进行采集时触发该事件
    /// </summary>
    internal static event VariableCollectEventHandler? VariableCollectChangeEvent;

    /// <summary>
    /// 报警变化事件
    /// </summary>
    public static event VariableAlarmEventHandler? AlarmChangedEvent;

    /// <summary>
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IReadOnlyDictionary<long, ChannelRuntime> ReadOnlyChannels => Channels;

    public static async Task<IEnumerable<KeyValuePair<long, ChannelRuntime>>> GetCurrentUserChannels()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return ReadOnlyChannels.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId);
    }
    public static async Task<IEnumerable<KeyValuePair<long, DeviceRuntime>>> GetCurrentUserDevices()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return ReadOnlyDevices.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId);
    }
    public static async Task<IEnumerable<KeyValuePair<long, VariableRuntime>>> GetCurrentUserIdVariables()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return IdVariables.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId);
    }
    public static async Task<IEnumerable<KeyValuePair<string, VariableRuntime>>> GetCurrentUserVariables()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return Variables.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId);
    }
    public static async Task<IEnumerable<KeyValuePair<string, VariableRuntime>>> GetCurrentUserRealAlarmVariables()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return RealAlarmVariables.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId);
    }

    /// <summary>
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IEnumerable<KeyValuePair<long, ChannelRuntime>> GetEnableChannels()
    {
        return Channels.Where(a => a.Value.Enable);
    }

    /// <summary>
    /// 只读的设备字典，提供对设备的只读访问
    /// </summary>
    public static IReadOnlyDictionary<long, DeviceRuntime> ReadOnlyDevices => Devices;

    /// <summary>
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IEnumerable<KeyValuePair<long, DeviceRuntime>> GetEnableDevices()
    {
        var idSet = GetRedundantDeviceIds();
        return Devices.Where(a => a.Value.Enable && !idSet.Contains(a.Value.Id));
    }
    public static HashSet<long> GetRedundantDeviceIds()
    {
        return Devices.Select(a => a.Value).Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet();
    }
    /// <summary>
    /// 实时报警列表
    /// </summary>
    public static IReadOnlyDictionary<string, VariableRuntime> ReadOnlyRealAlarmVariables => RealAlarmVariables;
    /// <summary>
    /// 只读的变量字典
    /// </summary>
    public static IReadOnlyDictionary<string, VariableRuntime> ReadOnlyVariables => Variables;
    /// <summary>
    /// 只读的变量字典
    /// </summary>
    public static IReadOnlyDictionary<long, VariableRuntime> ReadOnlyIdVariables => IdVariables;

    public static IEnumerable<KeyValuePair<string, VariableRuntime>> GetEnableVariables()
    {
        var idSet = Devices.Select(a => a.Value).Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet();

        return Variables.Where(a => a.Value.Enable && !idSet.Contains(a.Value.DeviceId));
    }

    public static IEnumerable<KeyValuePair<long, VariableRuntime>> GetEnableIdVariables()
    {
        var idSet = Devices.Select(a => a.Value).Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet();

        return IdVariables.Where(a => a.Value.Enable && !idSet.Contains(a.Value.DeviceId));
    }

    public static bool TryGetDeviceThreadManage(DeviceRuntime deviceRuntime, out IDeviceThreadManage deviceThreadManage)
    {
        if (deviceRuntime.Driver?.DeviceThreadManage != null)
        {
            deviceThreadManage = deviceRuntime.Driver.DeviceThreadManage;
            return true;
        }
        return GlobalData.ChannelThreadManage.DeviceThreadManages.TryGetValue(deviceRuntime.ChannelId, out deviceThreadManage);
    }
    public static Dictionary<IDeviceThreadManage, List<DeviceRuntime>> GetDeviceThreadManages(IEnumerable<DeviceRuntime> deviceRuntimes)
    {
        Dictionary<IDeviceThreadManage, List<DeviceRuntime>> deviceThreadManages = new();

        foreach (var item in deviceRuntimes)
        {
            if (TryGetDeviceThreadManage(item, out var deviceThreadManage))
            {
                if (deviceThreadManages.TryGetValue(deviceThreadManage, out List<DeviceRuntime>? value))
                {
                    value.Add(item);
                }
                else
                {
                    deviceThreadManages.Add(deviceThreadManage, new List<DeviceRuntime> { item });
                }
            }
        }
        return deviceThreadManages;
    }

    #region 单例服务

    private static ISysUserService sysUserService;
    public static ISysUserService SysUserService
    {
        get
        {
            if (sysUserService == null)
            {
                sysUserService = App.RootServices.GetRequiredService<ISysUserService>();
            }
            return sysUserService;
        }
    }


    private static IVariableRuntimeService variableRuntimeService;
    public static IVariableRuntimeService VariableRuntimeService
    {
        get
        {
            if (variableRuntimeService == null)
            {
                variableRuntimeService = App.RootServices.GetRequiredService<IVariableRuntimeService>();
            }
            return variableRuntimeService;
        }
    }


    private static IDeviceRuntimeService deviceRuntimeService;
    public static IDeviceRuntimeService DeviceRuntimeService
    {
        get
        {
            if (deviceRuntimeService == null)
            {
                deviceRuntimeService = App.RootServices.GetRequiredService<IDeviceRuntimeService>();
            }
            return deviceRuntimeService;
        }
    }

    private static IChannelRuntimeService channelRuntimeService;
    public static IChannelRuntimeService ChannelRuntimeService
    {
        get
        {
            if (channelRuntimeService == null)
            {
                channelRuntimeService = App.RootServices.GetRequiredService<IChannelRuntimeService>();
            }
            return channelRuntimeService;
        }
    }
    private static IChannelThreadManage channelThreadManage;
    public static IChannelThreadManage ChannelThreadManage
    {
        get
        {
            if (channelThreadManage == null)
            {
                channelThreadManage = App.RootServices.GetRequiredService<IChannelThreadManage>();
            }
            return channelThreadManage;
        }
    }


    private static IGatewayMonitorHostedService gatewayMonitorHostedService;
    public static IGatewayMonitorHostedService GatewayMonitorHostedService
    {
        get
        {
            if (gatewayMonitorHostedService == null)
            {
                gatewayMonitorHostedService = App.RootServices.GetRequiredService<IGatewayMonitorHostedService>();
            }
            return gatewayMonitorHostedService;
        }
    }

    private static IRpcService rpcService;
    public static IRpcService RpcService
    {
        get
        {
            if (rpcService == null)
            {
                rpcService = App.RootServices.GetRequiredService<IRpcService>();
            }
            return rpcService;
        }
    }

    private static IAlarmHostedService alarmHostedService;
    public static IAlarmHostedService AlarmHostedService
    {
        get
        {
            if (alarmHostedService == null)
            {
                alarmHostedService = App.RootServices.GetRequiredService<IAlarmHostedService>();
            }
            return alarmHostedService;
        }
    }

    private static IHardwareJob? hardwareJob;

    public static IHardwareJob HardwareJob
    {
        get
        {
            hardwareJob ??= App.RootServices.GetRequiredService<IHardwareJob>();
            return hardwareJob;
        }
    }


    private static IPluginService? pluginService;
    public static IPluginService PluginService
    {
        get
        {
            pluginService ??= App.RootServices.GetRequiredService<IPluginService>();
            return pluginService;
        }
    }


    private static IChannelService? channelService;
    internal static IChannelService ChannelService
    {
        get
        {
            channelService ??= App.RootServices.GetRequiredService<IChannelService>();
            return channelService;
        }
    }


    private static IDeviceService? deviceService;
    internal static IDeviceService DeviceService
    {
        get
        {
            deviceService ??= App.RootServices.GetRequiredService<IDeviceService>();
            return deviceService;
        }
    }


    private static IVariableService? variableService;
    internal static IVariableService VariableService
    {
        get
        {
            variableService ??= App.RootServices.GetRequiredService<IVariableService>();
            return variableService;
        }
    }

    private static IGatewayRedundantSerivce? gatewayRedundantSerivce;
    private static IGatewayRedundantSerivce? GatewayRedundantSerivce
    {
        get
        {
            gatewayRedundantSerivce ??= App.RootServices.GetService<IGatewayRedundantSerivce>();
            return gatewayRedundantSerivce;
        }
    }

    /// <summary>
    /// 采集通道是否可用
    /// </summary>
    public static bool StartCollectChannelEnable => GatewayRedundantSerivce?.StartCollectChannelEnable ?? true;

    /// <summary>
    /// 业务通道是否可用
    /// </summary>
    public static bool StartBusinessChannelEnable => GatewayRedundantSerivce?.StartBusinessChannelEnable ?? true;
    #endregion



    /// <summary>
    /// 内部使用的通道字典，用于存储通道对象
    /// </summary>
    internal static ConcurrentDictionary<long, ChannelRuntime> Channels { get; } = new();

    /// <summary>
    /// 内部使用的设备字典，用于存储设备对象
    /// </summary>
    internal static ConcurrentDictionary<long, DeviceRuntime> Devices { get; } = new();
    /// <summary>
    /// 内部使用的变量字典，用于存储变量对象
    /// </summary>
    internal static ConcurrentDictionary<long, VariableRuntime> IdVariables { get; } = new();
    /// <summary>
    /// 内部使用的变量字典，用于存储变量对象
    /// </summary>
    internal static ConcurrentDictionary<string, VariableRuntime> Variables { get; } = new();
    /// <summary>
    /// 内部使用的报警配置变量字典
    /// </summary>
    internal static ConcurrentDictionary<long, VariableRuntime> AlarmEnableVariables { get; } = new();

    /// <summary>
    /// 内部使用的报警配置变量字典
    /// </summary>
    internal static ConcurrentDictionary<string, VariableRuntime> RealAlarmVariables { get; } = new();

    /// <summary>
    /// 报警状态变化处理方法，用于处理报警状态变化时的逻辑
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    internal static void AlarmChange(AlarmVariable alarmVariable)
    {
        if (AlarmChangedEvent != null)
        {
            // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
            AlarmChangedEvent.Invoke(alarmVariable);
        }
    }

    /// <summary>
    /// 设备状态变化处理方法，用于处理设备状态变化时的逻辑
    /// </summary>
    /// <param name="deviceRuntime">设备运行时对象</param>
    internal static void DeviceStatusChange(DeviceRuntime deviceRuntime)
    {
        if (DeviceStatusChangeEvent != null)
        {
            // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
            DeviceStatusChangeEvent.Invoke(deviceRuntime, deviceRuntime.Adapt<DeviceBasicData>());
        }
    }

    /// <summary>
    /// 变量值变化处理方法，用于处理变量值发生变化时的逻辑
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    internal static void VariableValueChange(VariableRuntime variableRuntime)
    {
        if (VariableValueChangeEvent != null)
        {
            // 触发变量值变化事件，并将变量运行时对象转换为变量数据对象进行传递
            VariableValueChangeEvent.Invoke(variableRuntime, variableRuntime.Adapt<VariableBasicData>());
        }
    }
    /// <summary>
    /// 变量采集处理方法，用于处理变量进行采集时的逻辑
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    internal static void VariableCollectChange(VariableRuntime variableRuntime)
    {
        if (VariableCollectChangeEvent != null)
        {
            // 触发变量采集事件，并将变量运行时对象转换为变量数据对象进行传递
            VariableCollectChangeEvent.Invoke(variableRuntime);
        }
    }
}
