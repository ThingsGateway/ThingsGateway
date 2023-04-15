using System.Text;

using ThingsGateway.Foundation;

namespace ThingsGateway.Siemens;


public class SiemensProperty : DriverPropertyBase
{
    [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
    [DeviceProperty("端口", "")] public int Port { get; set; } = 102;
    [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
    [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
    [DeviceProperty("LocalTSAP", "为0时不写入，通常默认0即可")] public int LocalTSAP { get; set; } = 0;
    [DeviceProperty("DestTSAP", "为0时不写入，通常默认0即可")] public int DestTSAP { get; set; } = 0;
}

public abstract class S7 : CollectBase
{

    protected SiemensS7PLC _plc;
    protected SiemensProperty driverPropertys = new SiemensProperty();

    protected S7(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    public override void AfterStop()
    {
        _plc?.Disconnect();
    }

    public override async Task BeforStartAsync()
    {
        await _plc.ConnectAsync();
    }

    public override void Dispose()
    {
        _plc?.Disconnect();
    }

    public override OperResult IsConnected()
    {
        return _plc?.TGTcpClient?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override bool IsSupportAddressRequest()
    {
        return true;
    }

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
    {
        Init(null);
        _plc.Connect();
        var data = deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, _plc);
        _plc?.Disconnect();
        return data;
    }

    [DeviceMethod("ReadDate", "")]
    public Task<OperResult<System.DateTime>> ReadDateAsync(string address)
    {
        return _plc?.ReadDateAsync(address);
    }
    [DeviceMethod("ReadDateTime", "")]
    public Task<OperResult<System.DateTime>> ReadDateTimeAsync(string address)
    {
        return _plc?.ReadDateTimeAsync(address);
    }

    [DeviceMethod("ReadString", "")]
    public Task<OperResult<string>> ReadStringAsync(string address, Encoding encoding)
    {
        return _plc?.ReadStringAsync(address, encoding);
    }

    [DeviceMethod("WriteDate", "")]
    public Task<OperResult> WriteDateAsync(string address, System.DateTime dateTime)
    {
        return _plc?.WriteDateAsync(address, dateTime);
    }
    [DeviceMethod("WriteDateTime", "")]
    public Task<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime)
    {
        return _plc?.WriteDateTimeAsync(address, dateTime);
    }
    public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
    {
        return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value);
    }

    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length);
    }

}
