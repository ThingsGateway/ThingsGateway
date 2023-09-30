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

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpDtu : CollectBase
{
    /// <inheritdoc/>
    protected override IReadWrite PLC => _plc;
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusTcpDtu _plc;

    private readonly ModbusTcpProperty driverPropertys = new();

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ModbusTcpDebugPage);

    /// <inheritdoc/>
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;
    /// <inheritdoc/>
    public override bool IsSupportRequest => true;
    /// <inheritdoc/>
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        _plc?.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await _plc?.ConnectAsync(cancellationToken);
    }
    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return _plc?.TcpService?.ServerState == ServerState.Running;
    }

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(deviceVariables, driverPropertys.MaxPack);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        _plc?.SafeDispose();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            IPHost iPHost = new(driverPropertys.Port);
            if (!string.IsNullOrEmpty(driverPropertys.IP))
            {
                iPHost = new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}");
            }
            FoundataionConfig.SetListenIPHosts(new IPHost[] { iPHost });
            client = new TcpService();
            ((TcpService)client).Setup(FoundataionConfig);
        }
        //载入配置
        _plc = new((TcpService)client)
        {
            DataFormat = driverPropertys.DataFormat,
            FrameTime = driverPropertys.FrameTime,
            CacheTimeout = driverPropertys.CacheTimeout,
            ConnectTimeOut = driverPropertys.ConnectTimeOut,
            Station = driverPropertys.Station,
            TimeOut = driverPropertys.TimeOut,
            IsCheckMessageId = driverPropertys.MessageIdCheckEnable
        };
    }
    /// <inheritdoc/>
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length, cancellationToken);
    }
}
