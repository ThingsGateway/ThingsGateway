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
/// 上传设备运行状态
/// </summary>
public class DeviceRunTime : CollectDevice
{
    protected DeviceStatusEnum _deviceStatus = DeviceStatusEnum.None;

    protected int _errorCount;

    private string _lastErrorMessage;

    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event DelegateOnDeviceChanged DeviceStatusChange;

    /// <summary>
    /// 设备活跃时间
    /// </summary>
    [Description("活跃时间")]
    public DateTime ActiveTime { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// 设备状态
    /// </summary>
    [Description("设备状态")]
    public virtual DeviceStatusEnum DeviceStatus
    {
        get
        {
            if (KeepRun)
                return _deviceStatus;
            else
                return DeviceStatusEnum.Pause;
        }
        protected set
        {
            if (_deviceStatus != value)
            {
                _deviceStatus = value;
                DeviceStatusChange?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// 设备变量数量
    /// </summary>
    [Description("变量数量")]
    public int DeviceVariableCount { get => DeviceVariableRunTimes == null ? 0 : DeviceVariableRunTimes.Count; }

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<DeviceVariableRunTime> DeviceVariableRunTimes { get; set; }
    /// <summary>
    /// 距上次成功时的读取失败次数,超过3次设备更新为离线，等于0时设备更新为在线
    /// </summary>
    [Description("失败次数")]
    public virtual int ErrorCount
    {
        get
        {
            return _errorCount;
        }
        protected set
        {
            _errorCount = value;
            if (_errorCount > 3)
            {
                DeviceStatus = DeviceStatusEnum.OffLine;
            }
            else if (_errorCount == 0)
            {
                DeviceStatus = DeviceStatusEnum.OnLine;
            }
        }
    }

    /// <summary>
    /// 运行
    /// </summary>
    [Description("运行")]
    public bool KeepRun { get; set; } = true;

    /// <summary>
    /// 最后一次失败原因
    /// </summary>
    [Description("最后一次失败原因")]
    public string LastErrorMessage
    {
        get
        {
            return _lastErrorMessage;
        }
        protected set
        {
            _lastErrorMessage = DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + value;
        }
    }
    /// <summary>
    /// 日志接口，初始化时传入
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public ILog Log { get; set; }

    /// <summary>
    /// 设备属性数量
    /// </summary>
    [Description("属性数量")]
    public int PropertysCount { get => DevicePropertys == null ? 0 : DevicePropertys.Count; }
    /// <summary>
    /// 冗余状态
    /// </summary>
    [Description("冗余状态")]
    public RedundantEnum Redundant { get; set; } = RedundantEnum.Primary;

    /// <summary>
    /// 传入设备的状态信息
    /// </summary>
    /// <param name="activeTime"></param>
    /// <param name="errorCount"></param>
    /// <param name="lastErrorMessage"></param>
    public void SetDeviceStatus(DateTime? activeTime = null, int? errorCount = null, string lastErrorMessage = null)
    {
        lock (this)
        {
            if (activeTime != null)
                ActiveTime = activeTime.Value;
            if (errorCount != null)
                ErrorCount = errorCount.Value;
            if (lastErrorMessage != null)
                LastErrorMessage = lastErrorMessage;
        }
    }
}

/// <summary>
/// 设备变化委托
/// </summary>
/// <param name="collectDeviceRunTime"></param>
public delegate void DelegateOnDeviceChanged(DeviceRunTime collectDeviceRunTime);