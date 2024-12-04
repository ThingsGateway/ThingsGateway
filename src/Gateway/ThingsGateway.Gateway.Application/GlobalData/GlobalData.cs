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
using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备状态变化委托，用于通知设备状态发生变化时的事件
/// </summary>
/// <param name="deviceRunTime">设备运行时对象</param>
/// <param name="deviceData">设备数据对象</param>
public delegate void DelegateOnDeviceChanged(DeviceRunTime deviceRunTime, DeviceBasicData deviceData);

/// <summary>
/// 变量改变事件委托，用于通知变量值发生变化时的事件
/// </summary>
/// <param name="variableRunTime">变量运行时对象</param>
/// <param name="variableData">变量数据对象</param>
public delegate void VariableChangeEventHandler(VariableRunTime variableRunTime, VariableBasicData variableData);

/// <summary>
/// 变量采集事件委托，用于通知变量进行采集时的事件
/// </summary>
/// <param name="variableRunTime">变量运行时对象</param>
public delegate void VariableCollectEventHandler(VariableRunTime variableRunTime);

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
    public static event VariableChangeEventHandler? VariableValueChangeEvent;

    /// <summary>
    /// 变量采集事件，当变量进行采集时触发该事件
    /// </summary>
    internal static event VariableCollectEventHandler? VariableCollectChangeEvent;

    /// <summary>
    /// 只读的业务设备字典，提供对业务设备的只读访问
    /// </summary>
    public static IReadOnlyDictionary<string, DeviceRunTime> ReadOnlyBusinessDevices => BusinessDevices;

    /// <summary>
    /// 只读的采集设备字典，提供对采集设备的只读访问
    /// </summary>
    public static IReadOnlyDictionary<string, CollectDeviceRunTime> ReadOnlyCollectDevices => CollectDevices;

    /// <summary>
    /// 实时报警列表
    /// </summary>
    public static IReadOnlyDictionary<string, VariableRunTime> ReadOnlyRealAlarmVariables => AlarmHostedService.ReadOnlyRealAlarmVariables;

    /// <summary>
    /// 只读的变量字典
    /// </summary>
    public static IReadOnlyDictionary<string, VariableRunTime> ReadOnlyVariables => Variables;


    public static bool TryGetVariable(string key, [MaybeNullWhen(false)] out VariableRunTime value) => Variables.TryGetValue(key ,out value);
    public static bool TryGetCollectDevice(string key, [MaybeNullWhen(false)] out CollectDeviceRunTime value) => CollectDevices.TryGetValue(key, out value);
    public static bool TryGetBusinessDevice(string key, [MaybeNullWhen(false)] out DeviceRunTime value) => BusinessDevices.TryGetValue(key, out value);


    #region 单例服务

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

    private static IBusinessDeviceHostedService? businessDeviceHostedService;

    private static ICollectDeviceHostedService? collectDeviceHostedService;

    private static IHardwareJob? hardwareJob;

    public static IBusinessDeviceHostedService BusinessDeviceHostedService
    {
        get
        {
            businessDeviceHostedService ??= App.RootServices.GetRequiredService<IBusinessDeviceHostedService>();
            return businessDeviceHostedService;
        }
    }

    public static ICollectDeviceHostedService CollectDeviceHostedService
    {
        get
        {
            collectDeviceHostedService ??= App.RootServices.GetRequiredService<ICollectDeviceHostedService>();
            return collectDeviceHostedService;
        }
    }

    public static IHardwareJob HardwareJob
    {
        get
        {
            hardwareJob ??= App.RootServices.GetRequiredService<IHardwareJob>();
            return hardwareJob;
        }
    }


    #endregion

    /// <summary>
    /// 内部使用的业务设备字典，用于存储业务设备对象
    /// </summary>
    internal static ConcurrentDictionary<string, DeviceRunTime> BusinessDevices { get; } = new();

    /// <summary>
    /// 内部使用的采集设备字典，用于存储采集设备对象
    /// </summary>
    internal static ConcurrentDictionary<string, CollectDeviceRunTime> CollectDevices { get; } = new();

    /// <summary>
    /// 内部使用的变量字典，用于存储变量对象
    /// </summary>
    internal static ConcurrentDictionary<string, VariableRunTime> Variables { get; } = new();

    /// <summary>
    /// 设备状态变化处理方法，用于处理设备状态变化时的逻辑
    /// </summary>
    /// <param name="deviceRunTime">设备运行时对象</param>
    internal static void DeviceStatusChange(DeviceRunTime deviceRunTime)
    {
        if (DeviceStatusChangeEvent != null)
        {
            // 触发设备状态变化事件，并将设备运行时对象转换为设备数据对象进行传递
            DeviceStatusChangeEvent.Invoke(deviceRunTime, deviceRunTime.Adapt<DeviceBasicData>());
        }
    }

    /// <summary>
    /// 变量采集处理方法，用于处理变量进行采集时的逻辑
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    internal static void VariableCollectChange(VariableRunTime variableRunTime)
    {
        if (VariableCollectChangeEvent != null)
        {
            // 触发变量采集事件，并将变量运行时对象转换为变量数据对象进行传递
            VariableCollectChangeEvent.Invoke(variableRunTime);
        }
    }

    /// <summary>
    /// 变量值变化处理方法，用于处理变量值发生变化时的逻辑
    /// </summary>
    /// <param name="variableRunTime">变量运行时对象</param>
    internal static void VariableValueChange(VariableRunTime variableRunTime)
    {
        if (VariableValueChangeEvent != null)
        {
            // 触发变量值变化事件，并将变量运行时对象转换为变量数据对象进行传递
            VariableValueChangeEvent.Invoke(variableRunTime, variableRunTime.Adapt<VariableBasicData>());
        }
    }
}
