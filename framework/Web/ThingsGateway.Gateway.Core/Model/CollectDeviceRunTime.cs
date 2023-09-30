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

using System.ComponentModel;

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 采集设备状态表示
/// </summary>
public class CollectDeviceRunTime : CollectDevice
{
    /// <summary>
    /// 设备驱动名称
    /// </summary>
    [Description("设备驱动名称")]
    public string PluginName { get; set; }
    /// <summary>
    /// 设备属性数量
    /// </summary>
    [Description("属性数量")]
    public int PropertysCount { get => DevicePropertys == null ? 0 : DevicePropertys.Count; }
    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<DeviceVariableRunTime> DeviceVariableRunTimes { get; set; }
    /// <summary>
    /// 设备变量数量
    /// </summary>
    [Description("变量数量")]
    public int DeviceVariableCount { get => DeviceVariableRunTimes == null ? 0 : DeviceVariableRunTimes.Count; }

    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    [Description("打包数量")]
    public int SourceVariableCount { get; set; }
    /// <summary>
    /// 设备特殊方法数量
    /// </summary>
    [Description("特殊方法数量")]
    public int MethodVariableCount { get; set; }
    /// <summary>
    /// 设备活跃时间
    /// </summary>
    [Description("活跃时间")]
    public DateTime ActiveTime { get; private set; } = DateTime.MinValue;
    /// <summary>
    /// 设备状态
    /// </summary>
    [Description("设备状态")]
    public DeviceStatusEnum DeviceStatus
    {
        get
        {
            if (KeepRun)
                return deviceStatus;
            else
                return DeviceStatusEnum.Pause;
        }
        private set
        {
            if (deviceStatus != value)
            {
                deviceStatus = value;
                DeviceStatusChange?.Invoke(this);
            }
        }
    }
    /// <summary>
    /// 冗余状态
    /// </summary>
    [Description("冗余状态")]
    public RedundantEnum Redundant { get; set; } = RedundantEnum.Primary;
    /// <summary>
    /// 运行
    /// </summary>
    [Description("运行")]
    public bool KeepRun { get; set; } = true;

    private int errorCount;
    /// <summary>
    /// 距上次成功时的读取失败次数,超过3次设备更新为离线，等于0时设备更新为在线
    /// </summary>
    [Description("失败次数")]
    public int ErrorCount
    {
        get
        {
            return errorCount;
        }
        private set
        {
            errorCount = value;
            if (errorCount > 3)
            {
                DeviceStatus = DeviceStatusEnum.OffLine;
            }
            else if (errorCount == 0)
            {
                DeviceStatus = DeviceStatusEnum.OnLine;
            }
        }
    }

    private DeviceStatusEnum deviceStatus = DeviceStatusEnum.None;
    /// <summary>
    /// 传入设备的状态信息
    /// </summary>
    /// <param name="activeTime"></param>
    /// <param name="errorCount"></param>
    /// <param name="lastErrorMessage"></param>
    public void SetDeviceStatus(DateTime? activeTime = null, int? errorCount = null, string lastErrorMessage = null)
    {
        if (activeTime != null)
            ActiveTime = activeTime.Value;
        if (errorCount != null)
            ErrorCount = errorCount.Value;
        if (lastErrorMessage != null)
            LastErrorMessage = lastErrorMessage;
    }

    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event DelegateOnDeviceChanged DeviceStatusChange;

    private string lastErrorMessage;
    /// <summary>
    /// 最后一次失败原因
    /// </summary>
    [Description("最后一次失败原因")]
    public string LastErrorMessage
    {
        get
        {
            return lastErrorMessage;
        }
        private set
        {
            lastErrorMessage = DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + value;
        }
    }
}
/// <summary>
/// 设备变化委托
/// </summary>
/// <param name="collectDeviceRunTime"></param>
public delegate void DelegateOnDeviceChanged(CollectDeviceRunTime collectDeviceRunTime);

