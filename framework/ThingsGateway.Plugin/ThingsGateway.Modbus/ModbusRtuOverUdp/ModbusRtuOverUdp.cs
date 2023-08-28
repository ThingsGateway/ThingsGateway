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

using ThingsGateway.Application;
using ThingsGateway.Foundation;

namespace ThingsGateway.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRtuOverUdp : CollectBase
{
    private readonly ModbusRtuOverUdpProperty driverPropertys = new();
    /// <inheritdoc/>
    protected override IReadWriteDevice PLC => _plc;
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtuOverUdp _plc;
    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ModbusRtuOverUdpDebugDriverPage);

    /// <inheritdoc/>
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    /// <inheritdoc/>
    public override bool IsSupportRequest => true;

    /// <inheritdoc/>
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        _plc.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task BeforStartAsync(CancellationToken token)
    {
        return _plc.ConnectAsync(token);
    }
    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return _plc?.UdpSession?.CanSend == true;
    }

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        return deviceVariables.LoadSourceRead(_plc, driverPropertys.MaxPack);
    }



    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            FoundataionConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                .SetBindIPHost(new IPHost(0))
                ;

            client = new UdpSession();
            ((UdpSession)client).Setup(FoundataionConfig);
        }
        //载入配置
        _plc = new((UdpSession)client)
        {
            Crc16CheckEnable = driverPropertys.Crc16CheckEnable,
            FrameTime = driverPropertys.FrameTime,
            DataFormat = driverPropertys.DataFormat,
            ConnectTimeOut = driverPropertys.ConnectTimeOut,
            Station = driverPropertys.Station,
            TimeOut = driverPropertys.TimeOut
        };
    }
    /// <inheritdoc/>
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token)
    {
        return await _plc.ReadAsync(address, length, token);
    }

}
