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
public class ChannelRuntime : Channel
#if !Management
    ,
    IChannelOptions,
    IDisposable
#endif
{

#if !Management


    [AutoGenerateColumn(Ignore = true)]
    public virtual bool IsMemory { get; protected set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public WaitLock WaitLock { get; private set; } = new WaitLock(maxCount:1);

    /// <inheritdoc/>
    [MinValue(1)]
    public override int MaxConcurrentCount
    {
        get
        {
            return _maxConcurrentCount;
        }
        set
        {
            if (value > 0)
            {
                _maxConcurrentCount = value;

                if (WaitLock?.MaxCount != MaxConcurrentCount)
                {
                    var _lock = WaitLock;
                    WaitLock = new WaitLock(maxCount:_maxConcurrentCount);
                    _lock?.TryDispose();
                }
            }
        }
    }

    private volatile int _maxConcurrentCount = 1;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public TouchSocket.Core.TouchSocketConfig Config { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IReadOnlyDictionary<long, DeviceRuntime>? ReadDeviceRuntimes => DeviceRuntimes;

    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    internal NonBlockingDictionary<long, DeviceRuntime>? DeviceRuntimes { get; } = new();

    /// <summary>
    /// 设备数量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public int? DeviceRuntimeCount => DeviceRuntimes?.Count;

    [AutoGenerateColumn(Ignore = true)]
    public bool Started => DeviceThreadManage != null;
#else



    /// <inheritdoc/>
    [MinValue(1)]
    public override int MaxConcurrentCount { get; set; }

    /// <summary>
    /// 设备数量
    /// </summary>
    public int? DeviceRuntimeCount { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    public bool Started { get; set; }
#endif



    /// <summary>
    /// 插件信息
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public PluginInfo? PluginInfo { get; set; }

    /// <summary>
    /// 是否采集
    /// </summary>
    public PluginTypeEnum? PluginType => PluginInfo?.PluginType;

    /// <summary>
    /// 是否采集
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public bool? IsCollect => PluginInfo == null ? null : PluginInfo?.PluginType == PluginTypeEnum.Collect;



    public override string ToString()
    {
        if (ChannelType == ChannelTypeEnum.Other)
        {
            return Name;
        }
        return $"{Name}[{base.ToString()}]";
    }

    [AutoGenerateColumn(Ignore = true)]
    public string LogPath => Name.GetChannelLogPath();


#if !Management
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IDeviceThreadManage? DeviceThreadManage { get; internal set; }





    public virtual void Init()
    {
        // 通过插件名称获取插件信息
        PluginInfo = GlobalData.PluginService.GetPluginList().FirstOrDefault(A => A.FullName == PluginName);

        GlobalData.IdChannels.TryRemove(Id, out _);
        GlobalData.Channels.TryRemove(Name, out _);

        GlobalData.IdChannels.TryAdd(Id, this);
        GlobalData.Channels.TryAdd(Name, this);
    }

    public virtual void Dispose()
    {
        //Config?.SafeDispose();

        GlobalData.IdChannels.TryRemove(Id, out _);
        GlobalData.Channels.TryRemove(Name, out _);
        DeviceThreadManage = null;
        GC.SuppressFinalize(this);
    }



    public IChannel GetChannel(TouchSocket.Core.TouchSocketConfig config, IReceivedDevice receivedDevice = null)
    {
        lock (GlobalData.IdChannels)
        {
            if (DeviceThreadManage?.Channel?.DisposedValue == false)
                return DeviceThreadManage?.Channel;

            if (ChannelType == ChannelTypeEnum.TcpService
                || ChannelType == ChannelTypeEnum.SerialPort
                || ChannelType == ChannelTypeEnum.UdpSession
                )
            {
                //获取相同配置的Tcp服务或Udp服务或COM
                var same = GlobalData.IdChannels.Where(a =>
                 {
                     if (a.Value == this)
                         return false;
                     if (a.Value.DeviceThreadManage?.Channel?.DisposedValue == true || a.Value.DeviceThreadManage?.Channel?.DisposedValue == null)
                         return false;

                     if (a.Value.ChannelType == ChannelType)
                     {
                         if (a.Value.ChannelType == ChannelTypeEnum.TcpService)
                             if (a.Value.BindUrl == BindUrl)
                                 return true;
                         if (a.Value.ChannelType == ChannelTypeEnum.UdpSession)
                             if ((!BindUrl.IsNullOrWhiteSpace()) && a.Value.BindUrl == BindUrl)
                                 return true;
                         if (a.Value.ChannelType == ChannelTypeEnum.SerialPort)
                             if (a.Value.PortName == PortName)
                                 return true;
                     }
                     return false;
                 }).FirstOrDefault(a => a.Value.IsCollect == true).Value;

                if (same != null)
                {
                    return same.GetChannel(config, receivedDevice);
                }
            }

            if (DeviceThreadManage?.Channel?.DisposedValue == false)
                return DeviceThreadManage?.Channel;

            if (receivedDevice != null)
            {
                var ichannel = receivedDevice.CreateChannel(config, this);
                return ichannel;
            }
            else
            {
                var ichannel = config.GetChannel(this);
                return ichannel;
            }
        }
    }


#endif
}
