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

#if !Management

using Riok.Mapperly.Abstractions;

using System.Collections.Concurrent;

namespace ThingsGateway.Gateway.Application;
#else

namespace ThingsGateway.Management.Application;
#endif

/// <summary>
/// 业务设备运行状态
/// </summary>
public class DeviceRuntime : Device
#if !Management
    , IDisposable
#endif
{
    protected volatile DeviceStatusEnum _deviceStatus = DeviceStatusEnum.Default;

    private string? _lastErrorMessage;

    private readonly object _lockObject = new object();

    /// <summary>
    /// 设备活跃时间
    /// </summary>
    public DateTime ActiveTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 是否采集
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public bool? IsCollect => PluginType == null ? null : PluginType == PluginTypeEnum.Collect;


    [AutoGenerateColumn(Ignore = true)]
    public string LogPath => Name.GetDeviceLogPath();

#if !Management

    /// <summary>
    /// 插件名称
    /// </summary>
    public virtual string PluginName => ChannelRuntime?.PluginName;

    /// <summary>
    /// 插件名称
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public virtual PluginTypeEnum? PluginType => ChannelRuntime?.PluginInfo?.PluginType;


    /// <summary>
    /// 通道名称
    /// </summary>
    public string? ChannelName => ChannelRuntime?.Name;
    /// <summary>
    /// 通道
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public ChannelRuntime? ChannelRuntime { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    public virtual bool IsMemory => ChannelRuntime?.IsMemory ?? false;


    [AutoGenerateColumn(Ignore = true)]
    public bool Started => Driver?.DeviceThreadManage != null;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    public DateTime DeviceStatusChangeTime = DateTime.MinValue;

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
            lock (_lockObject)
            {
                if (_deviceStatus != value)
                {
                    _deviceStatus = value;
                    DeviceStatusChangeTime = TimerX.Now;
                    GlobalData.DeviceStatusChange(this);
                }
            }
        }
    }


    /// <summary>
    /// 设备变量数量
    /// </summary>
    public int DeviceVariableCount { get => Driver == null ? VariableRuntimes?.Count ?? 0 : Driver.IdVariableRuntimes.Count; }

    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    public int SourceVariableCount => VariableSourceReads?.Count ?? 0;

#else

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual bool IsMemory { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    public virtual string PluginName { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public virtual PluginTypeEnum? PluginType { get; set; }


    /// <summary>
    /// 通道名称
    /// </summary>
    public string? ChannelName { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    public bool Started { get; set; }


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
            lock (_lockObject)
            {
                if (_deviceStatus != value)
                {
                    _deviceStatus = value;
                }
            }
        }
    }


    /// <summary>
    /// 设备变量数量
    /// </summary>
    public int DeviceVariableCount { get; set; }
    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    public int SourceVariableCount { get; set; }


#endif

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
#if !Management
            if (!string.IsNullOrWhiteSpace(value))
                _lastErrorMessage = TimerX.Now.ToDefaultDateTimeFormat() + " - " + value;
#else
            if (!value.IsNullOrWhiteSpace())
                _lastErrorMessage = value;
#endif
        }
    }

    /// <summary>
    /// 设备属性数量
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public int PropertysCount { get => DevicePropertys == null ? 0 : DevicePropertys.Count; }

    /// <summary>
    /// 冗余状态
    /// </summary>
    public RedundantTypeEnum? RedundantType { get; set; } = null;



    /// <summary>
    /// 特殊方法数量
    /// </summary>
    public int MethodVariableCount { get; set; }




#if !Management

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IReadOnlyDictionary<string, VariableRuntime>? ReadOnlyVariableRuntimes => VariableRuntimes;

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    internal NonBlockingDictionary<string, VariableRuntime>? VariableRuntimes { get; } = new();

    /// <summary>
    /// 特殊方法变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public List<VariableMethod>? ReadVariableMethods { get; set; }

    /// <summary>
    /// 打包变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public List<VariableSourceRead>? VariableSourceReads { get; set; }

    /// <summary>
    /// 特殊地址变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public List<VariableScriptRead>? VariableScriptReads { get; set; }

#endif



#if !Management

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
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IDriver? Driver { get; internal set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IRpcDriver? RpcDriver { get; set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public string? Tag { get; set; }

    public virtual void Init(ChannelRuntime channelRuntime)
    {
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);

        ChannelRuntime = channelRuntime;
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);
        ChannelRuntime?.DeviceRuntimes?.TryAdd(Id, this);

        GlobalData.IdDevices.TryRemove(Id, out _);
        GlobalData.IdDevices.TryAdd(Id, this);
        GlobalData.Devices.TryRemove(Name, out _);
        GlobalData.Devices.TryAdd(Name, this);
    }

    public virtual void Dispose()
    {
        ChannelRuntime?.DeviceRuntimes?.TryRemove(Id, out _);

        GlobalData.IdDevices.TryRemove(Id, out _);
        GlobalData.Devices.TryRemove(Name, out _);

        Driver = null;
        VariableSourceReads?.Clear();
        VariableScriptReads?.Clear();
        ReadVariableMethods?.Clear();

        GC.SuppressFinalize(this);
    }


#endif
}
