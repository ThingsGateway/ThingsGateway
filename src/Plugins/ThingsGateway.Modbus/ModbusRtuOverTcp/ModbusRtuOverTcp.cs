#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusRtuOverTcp : CollectBase, IDisposable
{

    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtuOverTcp _plc;
    private ModbusRtuOverTcpProperty driverPropertys = new ModbusRtuOverTcpProperty();

    public ModbusRtuOverTcp(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override Type DriverDebugUIType => typeof(ModbusRtuOverTcpDebugDriverPage);
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;
    public override bool IsSupportRequest => true;
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }
    public override Task AfterStopAsync()
    {
        _plc?.Disconnect();
        return Task.CompletedTask;
    }

    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await _plc?.ConnectAsync(cancellationToken);
    }
    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }

    public override OperResult IsConnected()
    {
        return _plc?.TGTcpClient?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        return deviceVariables.LoadSourceRead(_logger, _plc, driverPropertys.MaxPack);
    }

    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        return await _plc.WriteAsync(deviceVariable.VariableAddress, deviceVariable.DataType, value, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        base.Dispose(disposing);
    }
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            TouchSocketConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                .SetBufferLength(1024);
            client = TouchSocketConfig.Container.Resolve<TGTcpClient>();
            ((TGTcpClient)client).Setup(TouchSocketConfig);
        }
        //载入配置
        _plc = new((TGTcpClient)client);
        _plc.Crc16CheckEnable = driverPropertys.Crc16CheckEnable;
        _plc.FrameTime = driverPropertys.FrameTime;
        _plc.CacheTimeout = driverPropertys.CacheTimeout;
        _plc.DataFormat = driverPropertys.DataFormat;
        _plc.ConnectTimeOut = driverPropertys.ConnectTimeOut;
        _plc.Station = driverPropertys.Station;
        _plc.TimeOut = driverPropertys.TimeOut;
    }
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length, cancellationToken);
    }

}
