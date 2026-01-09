//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using System.Collections.Concurrent;

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

public delegate void PluginEventHandler(PluginEventData data);

public class PluginEventData
{
    public string DeviceName { get; set; }
    public Newtonsoft.Json.Linq.JObject Value { get; set; }
    public string ValueType { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public object ObjectValue { get; set; }
    //public object ObjectValue => ValueType.IsNullOrEmpty() ? Value : Value.ToObject(Type.GetType(ValueType, false) ?? typeof(JObject));

    public PluginEventData(string deviceName, object value)
    {
        DeviceName = deviceName;
        ObjectValue = value;
        Value = Newtonsoft.Json.Linq.JObject.FromObject(value);
        ValueType = $"{value?.GetType().FullName},{value?.GetType().Assembly.GetName().Name}";
    }
}

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
    public static event VariableCollectEventHandler? VariableCollectChangeEvent;

    /// <summary>
    /// 报警变化事件
    /// </summary>
    public static event VariableAlarmEventHandler? AlarmChangedEvent;

    public static event PluginEventHandler? PluginEventHandler;

    public static Task<IEnumerable<ChannelRuntime>> GetCurrentUserChannels()
    {
        return GetCurrentUserChannels();

        static async PooledTask<IEnumerable<ChannelRuntime>> GetCurrentUserChannels()
        {
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            return ReadOnlyIdChannels.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
              .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Value);
        }
    }
    public static Task<IEnumerable<DeviceRuntime>> GetCurrentUserDevices()
    {
        return GetCurrentUserDevices();

        static async PooledTask<IEnumerable<DeviceRuntime>> GetCurrentUserDevices()
        {
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            return IdDevices.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
              .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Value);
        }
    }
    public static IEnumerable<long> GetCurrentUserDeviceIds(HashSet<long> dataScope)
    {
        return IdDevices.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Key);
    }
    public static Task<IEnumerable<VariableRuntime>> GetCurrentUserIdVariables()
    {
        return GetCurrentUserIdVariables();

        static async PooledTask<IEnumerable<VariableRuntime>> GetCurrentUserIdVariables()
        {
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

            return IdVariables
                .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.DeviceRuntime.CreateOrgId))//在指定机构列表查询
              .WhereIf(dataScope?.Count == 0, u => u.Value.DeviceRuntime.CreateUserId == UserManager.UserId).Select(a => a.Value);
        }
    }

    public static async Task CheckByDeviceNames(IEnumerable<string> deviceNames)
    {
        List<long> orgids = new();
        List<long> userIds = new();
        foreach (var deviceData in GlobalData.Devices.FilterByKeys(deviceNames))
        {
            orgids.Add(deviceData.Value.CreateOrgId);
            userIds.Add(deviceData.Value.CreateUserId);
        }
        await GlobalData.SysUserService.CheckApiDataScopeAsync(orgids, userIds).ConfigureAwait(false);
    }
    public static async Task CheckByDeviceIds(IEnumerable<long> deviceIds)
    {
        List<long> orgids = new();
        List<long> userIds = new();
        foreach (var deviceData in GlobalData.IdDevices.FilterByKeys(deviceIds))
        {
            orgids.Add(deviceData.Value.CreateOrgId);
            userIds.Add(deviceData.Value.CreateUserId);
        }
        await GlobalData.SysUserService.CheckApiDataScopeAsync(orgids, userIds).ConfigureAwait(false);
    }
    public static async Task CheckByVariableIds(IEnumerable<long> variableIds)
    {
        List<long> orgids = new();
        List<long> userIds = new();
        foreach (var deviceData in GlobalData.IdVariables.FilterByKeys(variableIds))
        {
            orgids.Add(deviceData.Value.DeviceRuntime.CreateOrgId);
            userIds.Add(deviceData.Value.DeviceRuntime.CreateUserId);
        }
        await GlobalData.SysUserService.CheckApiDataScopeAsync(orgids, userIds).ConfigureAwait(false);
    }
    public static async Task CheckByVariableId(long variableId)
    {
        if (GlobalData.IdVariables.TryGetValue(variableId, out var variable))
        {
            await GlobalData.SysUserService.CheckApiDataScopeAsync(variable.DeviceRuntime.CreateOrgId, variable.DeviceRuntime.CreateUserId).ConfigureAwait(false);
        }
    }
    public static Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync()
    {
        return GetCurrentUserRealAlarmVariablesAsync();

        static async PooledTask<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync()
        {
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            return RealAlarmIdVariables
                .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
              .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Value);
        }
    }

    public static Task<IEnumerable<VariableRuntime>> GetCurrentUserAlarmEnableVariables()
    {
        return GetCurrentUserAlarmEnableVariables();

        static async PooledTask<IEnumerable<VariableRuntime>> GetCurrentUserAlarmEnableVariables()
        {
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            return AlarmEnableIdVariables.WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.DeviceRuntime.CreateOrgId))//在指定机构列表查询
              .WhereIf(dataScope?.Count == 0, u => u.Value.DeviceRuntime.CreateUserId == UserManager.UserId).Select(a => a.Value);
        }
    }

    public static bool ContainsVariable(long businessDeviceId, VariableRuntime a)
    {
        if (GlobalData.IdDevices.TryGetValue(businessDeviceId, out var deviceRuntime))
        {

            if (deviceRuntime.Driver is BusinessBase businessBase)
            {
                if (businessBase.DriverProperties is IBusinessPropertyAllVariableBase property && property.IsAllVariable)
                {
                    return true;
                }
                else if (businessBase.RefreshRuntimeAlways)
                {
                    return true;
                }
            }

            if (deviceRuntime.Driver?.IdVariableRuntimes?.TryGetValue(a.Id, out var oldVariableRuntime) == true)
            {
                return true;
            }
        }

        return a.VariablePropertys?.ContainsKey(businessDeviceId) == true;
    }
    public static DeviceRuntime[] GetAllVariableBusinessDeviceRuntime()
    {
        var channelDevice = GlobalData.IdDevices.Where(a =>
          {
              if (a.Value.Driver is BusinessBase businessBase)
              {
                  if (businessBase.DriverProperties is IBusinessPropertyAllVariableBase property && property.IsAllVariable)
                  {
                      return true;
                  }
                  else if (businessBase.RefreshRuntimeAlways)
                  {
                      return true;
                  }
              }
              return false;

          }).Select(a => a.Value).ToArray();
        return channelDevice;
    }
    public static IDriver[] GetAllVariableBusinessDriver()
    {
        var channelDevice = GlobalData.IdDevices.Where(a =>
        {
            if (a.Value.Driver is BusinessBase businessBase)
            {
                if (businessBase.DriverProperties is IBusinessPropertyAllVariableBase property && property.IsAllVariable)
                {
                    return true;
                }
                else if (businessBase.RefreshRuntimeAlways)
                {
                    return true;
                }
            }
            return false;

        }).Select(a => a.Value.Driver).ToArray();
        return channelDevice;
    }
    /// <summary>
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IEnumerable<ChannelRuntime> GetEnableChannels()
    {
        return IdChannels.Where(a => a.Value.Enable).Select(a => a.Value);
    }

    public static VariableRuntime GetVariable(string deviceName, string variableName)
    {
        if (Devices.TryGetValue(deviceName, out var device))
        {
            if (device.VariableRuntimes.TryGetValue(variableName, out var variable))
            {
                return variable;
            }
        }
        return null;
    }
    public static bool TryGetVariable(string deviceName, string variableName, out VariableRuntime value)
    {

        value = GetVariable(deviceName, variableName);
        return value != null;
    }
    public static VariableRuntime GetVariable(string variableName)
    {

        if (string.IsNullOrEmpty(variableName))
            return null;
        var names = variableName.Split('.');
        if (names.Length > 2)
            return null;

        if (names.Length == 1)
        {
            if (MemoryVariableRuntimes.TryGetValue(names[0], out var variable))
            {
                return variable;
            }
        }
        else if (Devices.TryGetValue(names[0], out var device))
        {
            if (device.VariableRuntimes.TryGetValue(names[1], out var variable))
            {
                return variable;
            }
        }
        return null;
    }
    public static bool TryGetVariable(string variableName, out VariableRuntime value)
    {
        value = GetVariable(variableName);
        return value != null;
    }
    public static IEnumerable<DeviceRuntime> GetEnableDevices()
    {
        var idSet = GetRedundantDeviceIds();
        return IdDevices.Where(a => a.Value.Enable && !idSet.Contains(a.Value.Id)).Select(a => a.Value);
    }

    public static HashSet<long> GetRedundantDeviceIds()
    {
        return IdDevices.Select(a => a.Value).Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet();
    }

    public static bool IsRedundant(long deviceId)
    {
        if (GlobalData.IdDevices.TryGetValue(deviceId, out var deviceRuntime))
        {
            if (deviceRuntime.RedundantEnable && deviceRuntime.RedundantDeviceId != null)
                return true;

            var id = deviceRuntime.Id;
            foreach (var kv in GlobalData.IdDevices)
            {
                if (kv.Value.RedundantDeviceId == id)
                    return true;
            }
        }
        return false;
    }
    public static bool IsRedundantEnable(long deviceId)
    {
        if (GlobalData.IdDevices.TryGetValue(deviceId, out var deviceRuntime))
        {
            if (deviceRuntime.RedundantEnable && deviceRuntime.RedundantDeviceId != null)
                return true;
        }
        return false;
    }
    public static IEnumerable<VariableRuntime> GetEnableVariables()
    {
        return IdVariables.Where(a => a.Value.DeviceRuntime?.Enable != false && a.Value.DeviceRuntime?.ChannelRuntime?.Enable != false && a.Value?.Enable == true).Select(a => a.Value);
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

    public static IChannelThreadManage GetChannelThreadManage(ChannelRuntime channelRuntime)
    {
        if (channelRuntime.DeviceThreadManage?.ChannelThreadManage != null)
            return channelRuntime.DeviceThreadManage.ChannelThreadManage;
        else
            return GlobalData.ChannelThreadManage;
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

    private static IDispatchService<ChannelRuntime> channelRuntimeDispatchService;
    public static IDispatchService<ChannelRuntime> ChannelDeviceRuntimeDispatchService
    {
        get
        {
            if (channelRuntimeDispatchService == null)
                channelRuntimeDispatchService = App.GetService<IDispatchService<ChannelRuntime>>();

            return channelRuntimeDispatchService;
        }
    }
    private static IDispatchService<VariableRuntime> variableRuntimeDispatchService;
    public static IDispatchService<VariableRuntime> VariableRuntimeDispatchService
    {
        get
        {
            if (variableRuntimeDispatchService == null)
                variableRuntimeDispatchService = App.GetService<IDispatchService<VariableRuntime>>();

            return variableRuntimeDispatchService;
        }
    }

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
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IReadOnlyDictionary<long, ChannelRuntime> ReadOnlyIdChannels => IdChannels;
    /// <summary>
    /// 只读的通道字典，提供对通道的只读访问
    /// </summary>
    public static IReadOnlyDictionary<string, ChannelRuntime> ReadOnlyChannels => Channels;

    /// <summary>
    /// 内部使用的通道字典，用于存储通道对象
    /// </summary>
    internal static NonBlockingDictionary<long, ChannelRuntime> IdChannels { get; } = new();
    /// <summary>
    /// 内部使用的通道字典，用于存储通道对象
    /// </summary>
    internal static NonBlockingDictionary<string, ChannelRuntime> Channels { get; } = new();

    /// <summary>
    /// 只读的设备字典，提供对设备的只读访问
    /// </summary>
    public static IReadOnlyDictionary<long, DeviceRuntime> ReadOnlyIdDevices => IdDevices;

    /// <summary>
    /// 只读的设备字典，提供对设备的只读访问
    /// </summary>
    public static IReadOnlyDictionary<string, DeviceRuntime> ReadOnlyDevices => Devices;

    /// <summary>
    /// 内部使用的设备字典，用于存储设备对象
    /// </summary>
    internal static NonBlockingDictionary<string, DeviceRuntime> Devices { get; } = new();
    /// <summary>
    /// 内部使用的设备字典，用于存储设备对象
    /// </summary>
    internal static NonBlockingDictionary<long, DeviceRuntime> IdDevices { get; } = new();

    /// <summary>
    /// 内部使用的报警配置变量字典
    /// </summary>
    internal static NonBlockingDictionary<long, VariableRuntime> AlarmEnableIdVariables { get; } = new();

    /// <summary>
    /// 内部使用的报警配置变量字典
    /// </summary>
    internal static NonBlockingDictionary<long, AlarmVariable> RealAlarmIdVariables { get; } = new();

    /// <summary>
    /// 内部使用的变量字典，用于存储变量对象
    /// </summary>
    internal static NonBlockingDictionary<long, VariableRuntime> IdVariables { get; } = new();

    /// <summary>
    /// 实时报警列表
    /// </summary>
    public static IReadOnlyDictionary<long, AlarmVariable> ReadOnlyRealAlarmIdVariables => RealAlarmIdVariables;

    /// <summary>
    /// 只读的变量字典
    /// </summary>
    public static IReadOnlyDictionary<long, VariableRuntime> ReadOnlyIdVariables => IdVariables;
    public static MemoryChannelRuntime MemoryChannelRuntime { get; } = new MemoryChannelRuntime();
    public static MemoryDeviceRuntime MemoryDeviceRuntime { get; } = new MemoryDeviceRuntime();

    public static IReadOnlyDictionary<string, VariableRuntime> MemoryVariableRuntimes => MemoryDeviceRuntime.VariableRuntimes;
    #region 变化事件
    /// <summary>
    /// 报警状态变化处理方法，用于处理报警状态变化时的逻辑
    /// </summary>
    /// <param name="alarmVariable">报警变量</param>
    internal static void AlarmChange(AlarmVariable alarmVariable)
    {
        // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
        AlarmChangedEvent?.Invoke(alarmVariable);
    }

    /// <summary>
    /// 事件状态变化处理方法，用于处理事件状态变化时的逻辑
    /// </summary>
    public static void PluginEventChange(PluginEventData pluginEventData)
    {
        // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
        PluginEventHandler?.Invoke(pluginEventData);
    }
    /// <summary>
    /// 设备状态变化处理方法，用于处理设备状态变化时的逻辑
    /// </summary>
    /// <param name="deviceRuntime">设备运行时对象</param>
    internal static void DeviceStatusChange(DeviceRuntime deviceRuntime)
    {
        deviceRuntime.Driver?.LogMessage?.LogInformation($"Status changed: {deviceRuntime.DeviceStatus}");

        // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
        DeviceStatusChangeEvent?.Invoke(deviceRuntime, deviceRuntime.AdaptDeviceBasicData());
    }

    /// <summary>
    /// 变量值变化处理方法，用于处理变量值发生变化时的逻辑
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    internal static void VariableValueChange(VariableRuntime variableRuntime)
    {
        // 触发变量值变化事件，并将变量运行时对象转换为变量数据对象进行传递
        VariableValueChangeEvent?.Invoke(variableRuntime, variableRuntime.AdaptVariableBasicData());
    }
    /// <summary>
    /// 变量采集处理方法，用于处理变量进行采集时的逻辑
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    internal static void VariableCollectChange(VariableRuntime variableRuntime)
    {
        // 触发变量采集事件，并将变量运行时对象转换为变量数据对象进行传递
        VariableCollectChangeEvent?.Invoke(variableRuntime);
    }

    #endregion
}
