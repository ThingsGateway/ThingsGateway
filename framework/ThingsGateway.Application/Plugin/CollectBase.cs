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

using Furion;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Application;
/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// 读取字符串，DateTime等等不确定返回字节数量的方法属性特殊方法，需使用<see cref="DeviceMethodAttribute"/>特性标识
/// </summary>
public abstract class CollectBase : DriverBase
{
    /// <summary>
    /// 当前采集设备
    /// </summary>
    public CollectDeviceRunTime CurDevice;

    /// <summary>
    /// 返回是否支持读取
    /// </summary>
    /// <returns></returns>
    public abstract bool IsSupportRequest { get; }
    /// <summary>
    /// 一般底层驱动，也有可能为null
    /// </summary>
    protected abstract IReadWriteDevice PLC { get; }

    /// <summary>
    /// 数据转换器
    /// </summary>
    /// <returns></returns>
    public abstract IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 结束通讯后执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task AfterStopAsync();

    /// <summary>
    /// 开始通讯前执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task BeforStartAsync(CancellationToken token);

    /// <summary>
    /// 通道标识
    /// </summary>
    public virtual OperResult<string> GetChannelID()
    {
        var config = (CollectDriverPropertyBase)DriverPropertys;
        if (config.IsShareChannel)
        {
            switch (config.ShareChannel)
            {
                case ShareChannelEnum.SerialPort:
                    return OperResult.CreateSuccessResult(config.PortName);
                case ShareChannelEnum.TcpClientEx:
                case ShareChannelEnum.UdpSession:
                    var a = new IPHost($"{config.IP}:{config.Port}");
                    return OperResult.CreateSuccessResult(config.ShareChannel.ToString() + a.ToString());
            }
        }
        return new("不支持共享通道");
    }

    /// <summary>
    /// 共享通道类型
    /// </summary>
    public virtual OperResult<object> GetShareChannel()
    {
        var config = (CollectDriverPropertyBase)DriverPropertys;
        if (config.IsShareChannel)
        {
            switch (config.ShareChannel)
            {
                case ShareChannelEnum.None:
                    return new OperResult<object>("不支持共享链路");
                case ShareChannelEnum.SerialPort:
                    var data = new SerialProperty()
                    {
                        PortName = config.PortName,
                        BaudRate = config.BaudRate,
                        DataBits = config.DataBits,
                        Parity = config.Parity,
                        StopBits = config.StopBits,
                    };
                    FoundataionConfig.SetValue(SerialConfigExtension.SerialProperty, data);
                    var serialSession = new SerialsSession();
                    (serialSession).Setup(FoundataionConfig);
                    return OperResult.CreateSuccessResult((object)serialSession);
                case ShareChannelEnum.TcpClientEx:
                    FoundataionConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var tcpClient = new TcpClientEx();
                    (tcpClient).Setup(FoundataionConfig);
                    return OperResult.CreateSuccessResult((object)tcpClient);
                case ShareChannelEnum.UdpSession:
                    FoundataionConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var udpSession = new UdpSession();
                    return OperResult.CreateSuccessResult((object)udpSession);
            }

        }
        return new OperResult<object>("不支持共享链路");
    }


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ILogger logger, CollectDeviceRunTime device, object client = null)
    {
        _logger = logger;
        IsLogOut = device.IsLogOut;
        CurDevice = device;
        Init(device, client);
    }

    /// <summary>
    /// 共享链路需重新设置适配器时调用该方法
    /// </summary>
    public abstract void InitDataAdapter();

    /// <summary>
    /// 连读打包，返回实际通讯包信息<see cref="DeviceVariableSourceRead"/> 
    /// <br></br>每个驱动打包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    public abstract List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables);

    /// <summary>
    /// 采集驱动读取
    /// </summary>
    public virtual async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken token)
    {
        if (IsSupportRequest)
        {
            OperResult<byte[]> read = await ReadAsync(deviceVariableSourceRead.Address, deviceVariableSourceRead.Length, token);
            if (read == null || !read.IsSuccess)
            {
                deviceVariableSourceRead.DeviceVariables.ForEach(it =>
                {
                    var operResult = it.SetValue(null, isOnline: false);
                    if (!operResult.IsSuccess)
                    {
                        _logger.LogWarning("变量值更新失败：" + operResult.Message);
                    }
                });
                return read;
            }
            else
            {
                return ReadWriteHelpers.DealWithReadResult(read, content =>
                {
                    ReadWriteHelpers.PraseStructContent(content, deviceVariableSourceRead.DeviceVariables);
                });
            }

        }
        else
        {
            return new OperResult<byte[]>("不支持默认读取方式");
        }
    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果
    /// </summary>
    /// <returns></returns>
    public virtual async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<DeviceVariableRunTime, JToken> writeInfoLists, CancellationToken token)
    {
        if (PLC == null)
            throw new("未初始化成功");
        Dictionary<string, OperResult> operResults = new();
        foreach (var writeInfo in writeInfoLists)
        {
            var result = await PLC.WriteAsync(writeInfo.Key.VariableAddress, writeInfo.Key.DataType, writeInfo.Value.ToString(), token);
            await Task.Delay(10, token); //防止密集写入
            operResults.Add(writeInfo.Key.Name, result);
        }
        return operResults;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    /// <param name="client">链路对象，如TCPClient</param>
    protected abstract void Init(CollectDeviceRunTime device, object client = null);

    /// <summary>
    /// 底层日志输出
    /// </summary>
    protected override void Log_Out(TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (arg1 >= TouchSocket.Core.LogLevel.Warning)
        {
            CurDevice.SetDeviceStatus(lastErrorMessage: arg3);
        }
        if (IsLogOut || arg1 >= TouchSocket.Core.LogLevel.Warning)
        {
            _logger.Log_Out(arg1, arg2, arg3, arg4);
        }
    }

    internal override void NewMessage(TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsSaveLog)
        {
            if (arg3.StartsWith(FoundationConst.LogMessageHeader))
            {
                var customLevel = App.GetConfig<Microsoft.Extensions.Logging.LogLevel?>("Logging:LogLevel:BackendLog") ?? Microsoft.Extensions.Logging.LogLevel.Trace;
                if ((byte)arg1 < (byte)customLevel)
                {
                    var logRuntime = new BackendLog
                    {
                        LogLevel = (Microsoft.Extensions.Logging.LogLevel)arg1,
                        LogMessage = arg3,
                        LogSource = "采集设备:" + CurDevice.Name,
                        LogTime = SysDateTimeExtensions.CurrentDateTime,
                        Exception = null,
                    };
                    _logQueues.Enqueue(logRuntime);
                }
            }

        }
        base.NewMessage(arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// 返回全部内容字节数组
    /// <br></br>
    /// 通常使用<see cref="IReadWrite.ReadAsync(string, int, CancellationToken)"/>可以直接返回正确信息
    /// </summary>
    protected abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token);
}
