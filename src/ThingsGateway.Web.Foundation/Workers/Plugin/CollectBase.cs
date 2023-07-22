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

using Microsoft.Extensions.Logging;

using System.Threading;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Web.Foundation;
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

    /// <inheritdoc cref="CollectBase"/>
    public CollectBase(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {

    }

    /// <summary>
    /// 返回是否支持读取
    /// </summary>
    /// <returns></returns>
    public abstract bool IsSupportRequest { get; }

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
    public abstract Task BeforStartAsync(CancellationToken cancellationToken);

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
                case ShareChannelEnum.SerialClient:
                    return OperResult.CreateSuccessResult(config.PortName);
                case ShareChannelEnum.TcpClient:
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
                case ShareChannelEnum.SerialClient:
                    TouchSocketConfig.SetSerialProperty(new()
                    {
                        PortName = config.PortName,
                        BaudRate = config.BaudRate,
                        DataBits = config.DataBits,
                        Parity = config.Parity,
                        StopBits = config.StopBits,
                    })
                .SetBufferLength(1024);
                    var serialClient = TouchSocketConfig.Container.Resolve<SerialClient>();
                    (serialClient).Setup(TouchSocketConfig);
                    return OperResult.CreateSuccessResult((object)serialClient);
                case ShareChannelEnum.TcpClient:
                    TouchSocketConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var tcpClient = TouchSocketConfig.Container.Resolve<TGTcpClient>();
                    (tcpClient).Setup(TouchSocketConfig);
                    return OperResult.CreateSuccessResult((object)tcpClient);
                case ShareChannelEnum.UdpSession:
                    TouchSocketConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var udpSession = TouchSocketConfig.BuildWithUdpSession<TGUdpSession>();
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
    /// 连读分包，返回实际通讯包信息<see cref="DeviceVariableSourceRead"/> 
    /// <br></br>每个驱动分包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    public abstract OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables);
    /// <summary>
    /// 采集驱动读取
    /// </summary>
    public virtual async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        if (IsSupportRequest)
        {
            OperResult<byte[]> read = await ReadAsync(deviceVariableSourceRead.Address, deviceVariableSourceRead.Length, cancellationToken);
            if (!read.IsSuccess)
            {
                deviceVariableSourceRead.DeviceVariables.ForEach(it =>
                {
                    var operResult = it.SetValue(null);
                    if (!operResult.IsSuccess)
                    {
                        _logger.LogWarning(operResult.Message);
                    }
                });
            }
            return ReadWriteHelpers.DealWithReadResult(read, content =>
            {
                ReadWriteHelpers.PraseStructContent(content, deviceVariableSourceRead.DeviceVariables);
            });
        }
        else
        {
            return new OperResult<byte[]>("不支持默认读取方式");
        }
    }

    /// <summary>
    /// 写入变量值
    /// </summary>
    /// <returns></returns>
    public abstract Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken);

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    /// <param name="client">链路对象，如TCPClient</param>
    protected abstract void Init(CollectDeviceRunTime device, object client = null);

    /// <summary>
    /// 底层日志输出
    /// </summary>
    protected override void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
    {
        if (arg1 >= LogType.Warning)
        {
            CurDevice.LastErrorMessage = arg3;
        }
        if (IsLogOut || arg1 >= LogType.Warning)
        {
            _logger.Log_Out(arg1, arg2, arg3, arg4);
        }

    }
    /// <summary>
    /// 返回全部内容字节数组
    /// <br></br>
    /// 通常使用<see cref="IReadWrite.ReadAsync(string, int, System.Threading.CancellationToken)"/>可以直接返回正确信息
    /// </summary>
    protected abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken);
}
