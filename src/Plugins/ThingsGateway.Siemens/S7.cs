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

using System.Text;

using ThingsGateway.Foundation;

namespace ThingsGateway.Siemens;


public class SiemensProperty : CollectDriverPropertyBase
{
    [DeviceProperty("IP", "")] public override string IP { get; set; } = "127.0.0.1";
    [DeviceProperty("端口", "")] public override int Port { get; set; } = 102;
    [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
    [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
    [DeviceProperty("DestTSAP", "为0时不写入，通常默认0即可")] public int DestTSAP { get; set; } = 0;
    [DeviceProperty("LocalTSAP", "为0时不写入，通常默认0即可")] public int LocalTSAP { get; set; } = 0;
    public override bool IsShareChannel { get; set; } = false;
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.None;
}

public abstract class S7 : CollectBase
{
    protected SiemensS7PLC _plc;

    protected SiemensProperty driverPropertys = new SiemensProperty();

    protected S7(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    public override Task AfterStopAsync()
    {
        _plc?.Disconnect();
        return Task.CompletedTask;
    }

    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await _plc.ConnectAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        base.Dispose(disposing);
    }

    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }
    public override OperResult IsConnected()
    {
        return _plc?.TGTcpClient?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override bool IsSupportRequest => true;

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _plc.Connect(CancellationToken.None);
        var data = deviceVariables.LoadSourceRead(_logger, _plc);
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
    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        return await _plc.WriteAsync(deviceVariable.VariableAddress, deviceVariable.DataType, value, cancellationToken);
    }

    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length, cancellationToken);
    }

}
