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

using Mapster;

using System.Collections.Concurrent;

using ThingsGateway.Extension;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Threading;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务设备运行状态
/// </summary>
public class DeviceRuntime : Device, IDisposable
{
    protected volatile DeviceStatusEnum _deviceStatus = DeviceStatusEnum.Default;

    private string? _lastErrorMessage;

    /// <summary>
    /// 设备活跃时间
    /// </summary>
    public DateTime ActiveTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 插件名称
    /// </summary>
    public virtual string PluginName => ChannelRuntime?.PluginName;

    /// <summary>
    /// 插件名称
    /// </summary>
    public virtual PluginTypeEnum? PluginType => ChannelRuntime?.PluginInfo?.PluginType;

    /// <summary>
    /// 是否采集
    /// </summary>
    public bool? IsCollect => PluginType == null ? null : PluginType == PluginTypeEnum.Collect;

    /// <summary>
    /// 通道
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public ChannelRuntime? ChannelRuntime { get; set; }

    /// <summary>
    /// 通道名称
    /// </summary>
    public string? ChannelName => ChannelRuntime?.Name;
    public string LogPath => Id.GetLogPath();

    /// <summary>
    /// 设备状态
    /// </summary>
    public virtual DeviceStatusEnum DeviceStatus
    {
        get
        {
            if (!Pause)
                return _deviceStatus;
            else
                return DeviceStatusEnum.Pause;
        }
        set
        {
            lock (this)
            {
                if (_deviceStatus != value)
                {
                    _deviceStatus = value;
                    GlobalData.DeviceStatusChange(this);
                }
            }
        }
    }

    /// <summary>
    /// 设备变量数量
    /// </summary>
    public int DeviceVariableCount { get => VariableRuntimes == null ? 0 : VariableRuntimes.Count; }

    /// <summary>
    /// 暂停
    /// </summary>
    public bool Pause { get; set; } = false;

    /// <summary>
    /// 最后一次失败原因
    /// </summary>
    public string? LastErrorMessage
    {
        get
        {
            return _lastErrorMessage;
        }
        set
        {
            if (!value.IsNullOrWhiteSpace())
                _lastErrorMessage = TimerX.Now.ToDefaultDateTimeFormat() + " - " + value;
        }
    }

    /// <summary>
    /// 设备属性数量
    /// </summary>
    public int PropertysCount { get => DevicePropertys == null ? 0 : DevicePropertys.Count; }

    /// <summary>
    /// 冗余状态
    /// </summary>
    public RedundantTypeEnum? RedundantType { get; set; } = null;

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IReadOnlyDictionary<long, VariableRuntime>? ReadVariableRuntimes => VariableRuntimes;

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    internal ConcurrentDictionary<long, VariableRuntime>? VariableRuntimes { get; set; } = new(Environment.ProcessorCount, 1000);

    #region 采集
    /// <summary>
    /// 特殊方法数量
    /// </summary>
    public int MethodVariableCount { get; set; }

    /// <summary>
    /// 特殊方法变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableMethod>? ReadVariableMethods { get; set; }

    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    public int SourceVariableCount => VariableSourceReads?.Count ?? 0;

    /// <summary>
    /// 打包变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableSourceRead>? VariableSourceReads { get; set; }

    /// <summary>
    /// 特殊地址变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableScriptRead>? VariableScriptReads { get; set; }


    public volatile bool CheckEnable;

    #endregion 采集

    /// <summary>
    /// 传入设备的状态信息
    /// </summary>
    /// <param name="activeTime"></param>
    /// <param name="error"></param>
    /// <param name="lastErrorMessage"></param>
    public void SetDeviceStatus(DateTime? activeTime = null, bool? error = null, string lastErrorMessage = null)
    {
        if (activeTime != null)
            ActiveTime = activeTime.Value;
        if (error == true)
        {
            DeviceStatus = DeviceStatusEnum.OffLine;
        }
        else if (error == false)
        {
            DeviceStatus = DeviceStatusEnum.OnLine;
        }
        if (lastErrorMessage != null)
            LastErrorMessage = lastErrorMessage;
    }



    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public IDriver? Driver { get; internal set; }

    public void Init(ChannelRuntime channelRuntime)
    {
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);

        ChannelRuntime = channelRuntime;
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);
        ChannelRuntime.DeviceRuntimes.TryAdd(Id, this);

        GlobalData.IdDevices.TryRemove(Id, out _);
        GlobalData.IdDevices.TryAdd(Id, this);
        GlobalData.Devices.TryRemove(Name, out _);
        GlobalData.Devices.TryAdd(Name, this);

    }

    public void Dispose()
    {
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);

        GlobalData.IdDevices.TryRemove(Id, out _);
        GlobalData.Devices.TryRemove(Name, out _);

        Driver = null;
        GC.SuppressFinalize(this);
    }

}
