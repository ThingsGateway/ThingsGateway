using Microsoft.Extensions.Logging;

using System.Threading;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 采集插件，继承实现不同PLC通讯
/// 属性暴露使用<see cref="DevicePropertyAttribute"/>特性标识
/// 如果设备属性需要密码输入，属性名称中需包含Password字符串
/// 读取字符串，DateTime等等不确定返回字节数量的方法属性特殊方法，需使用<see cref="DeviceMethodAttribute"/>特性标识
/// </summary>
public abstract class DriverBase : IDisposable
{

    public TouchSocketConfig TouchSocketConfig;
    protected ILogger _logger;
    private bool isLogOut;
    private ILogger privateLogger;
    private IServiceScopeFactory _scopeFactory;
    public DriverBase(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        TouchSocketConfig = new TouchSocketConfig();
        TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(new EasyLogger(Log_Out)));
    }

    public bool IsLogOut
    {
        get => isLogOut;
        set
        {
            isLogOut = value;
            if (value)
            {
                _logger = privateLogger;
            }
            else
            {
                _logger = null;
            }
        }
    }

    ///// <summary>
    ///// 独立链路
    ///// </summary>
    //[DeviceProperty("独立链路")]
    //public bool IsAloneLink { get; set; } = true;

    ///// <summary>
    ///// 获取串口链路描述
    ///// </summary>
    ///// <returns></returns>
    //public virtual SerialProperty GetSerialProperty()
    //{
    //    return null;
    //}
    ///// <summary>
    ///// 获取Tcp链路描述
    ///// </summary>
    ///// <returns></returns>
    //public virtual IPHost GetTcpProperty()
    //{
    //    return null;
    //}


    /// <summary>
    /// 数据转换器
    /// </summary>
    /// <returns></returns>
    public abstract IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 结束通讯后执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract void AfterStop();

    /// <summary>
    /// 开始通讯前执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task BeforStart();


    public virtual Type DriverUI { get; }

    public abstract void Dispose();
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ILogger logger, CollectDeviceRunTime device, object client = null)
    {
        privateLogger = logger;
        if (IsLogOut)
            _logger = privateLogger;
        Init(device, client);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    /// <param name="client">链路对象，如TCPClient</param>
    protected abstract void Init(CollectDeviceRunTime device, object client = null);

    /// <summary>
    /// 返回是否支持读取
    /// </summary>
    /// <returns></returns>
    public abstract bool IsSupportAddressRequest();

    /// <summary>
    /// 连读分包，返回实际通讯包信息<see cref="DeviceVariableSourceRead"/> 
    /// <br></br>每个驱动分包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    public abstract OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables);

    /// <summary>
    /// 采集驱动读取
    /// </summary>
    /// <param name="deviceVariableSourceRead"></param>
    /// <returns></returns>
    public virtual async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        ushort length;
        if (!ushort.TryParse(deviceVariableSourceRead.Length, out length))
            return new OperResult<byte[]>("解析失败 长度[" + deviceVariableSourceRead.Length + "] 解析失败 :");
        OperResult<byte[]> read = await ReadAsync(deviceVariableSourceRead.Address, length, cancellationToken);
        if (!read.IsSuccess)
            deviceVariableSourceRead.DeviceVariables.ForEach(it => it.SetValue(null));
        return ReadWriteHelpers.DealWithReadResult(read, content =>
        ReadWriteHelpers.PraseStructContent(content, deviceVariableSourceRead.DeviceVariables));
    }

    /// <summary>
    /// 写入变量值
    /// </summary>
    /// <param name="deviceVariable">变量实体</param>
    /// <param name="value">变量写入值</param>
    /// <returns></returns>
    public abstract Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value);

    /// <summary>
    /// 返回全部内容字节数组
    /// <br></br>
    /// 通常使用<see cref="IReadWrite.ReadAsync(string, int, System.Threading.CancellationToken)"/>可以直接返回正确信息
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <returns></returns>
    protected abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken);

    private void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
    {
        switch (arg1)
        {
            case LogType.None:
                _logger?.Log(LogLevel.None, 0, arg4, arg3);
                break;
            case LogType.Trace:
                _logger?.Log(LogLevel.Trace, 0, arg4, arg3);
                break;
            case LogType.Debug:
                _logger?.Log(LogLevel.Debug, 0, arg4, arg3);
                break;
            case LogType.Info:
                _logger?.Log(LogLevel.Information, 0, arg4, arg3);
                break;
            case LogType.Warning:
                privateLogger?.Log(LogLevel.Warning, 0, arg4, arg3);
                break;
            case LogType.Error:
                privateLogger?.Log(LogLevel.Error, 0, arg4, arg3);
                break;
            case LogType.Critical:
                privateLogger?.Log(LogLevel.Critical, 0, arg4, arg3);
                break;
            default:
                break;
        }
    }

}
