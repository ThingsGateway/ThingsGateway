using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusRtuOverUdp : CollectBase
{
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtuOverUdp _plc;

    private ModbusRtuOverUdpProperty driverPropertys = new ModbusRtuOverUdpProperty();

    public ModbusRtuOverUdp(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    public override void AfterStop()
    {
        _plc.Disconnect();
    }

    public override Task BeforStartAsync()
    {
        return _plc.ConnectAsync();
    }

    public override void Dispose()
    {
        _plc?.Disconnect();
    }

    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }
    public override OperResult IsConnected()
    {
        return _plc?.UdpSession?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override bool IsSupportAddressRequest()
    {
        return true;
    }

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
    {
        return deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, driverPropertys.MaxPack);
    }

    public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
    {
        return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value);
    }

    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            TouchSocketConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                .SetBufferLength(1024);
            client = TouchSocketConfig.BuildWithUdpSession<UdpSession>();
        }
        //载入配置
        _plc = new((UdpSession)client);
        _plc.Crc16CheckEnable = driverPropertys.Crc16CheckEnable;
        _plc.DataFormat = driverPropertys.DataFormat;
        _plc.ConnectTimeOut = driverPropertys.ConnectTimeOut;
        _plc.Station = driverPropertys.Station;
        _plc.TimeOut = driverPropertys.TimeOut;
    }
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length);
    }

}

public class ModbusRtuOverUdpProperty : ModbusRtuOverTcpProperty
{
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.UdpSession;

}
