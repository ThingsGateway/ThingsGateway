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

using Microsoft.Extensions.DependencyInjection;

using System.IO.Ports;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusRtu : CollectBase, IDisposable
{
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtu _plc;

    private ModbusRtuProperty driverPropertys = new ModbusRtuProperty();
    public override Type DriverDebugUIType => typeof(ModbusRtuDebugDriverPage);


    public ModbusRtu(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

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
    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        _plc?.SafeDispose();
        base.Dispose(disposing);
    }

    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }
    public override OperResult IsConnected()
    {
        return _plc?.SerialClient?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override bool IsSupportRequest()
    {
        return true;
    }

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
    {
        return deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, driverPropertys.MaxPack);
    }

    public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value, deviceVariable.DataTypeEnum == DataTypeEnum.Bcd, cancellationToken);
    }

    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            TouchSocketConfig.SetSerialProperty(new()
            {
                PortName = driverPropertys.PortName,
                BaudRate = driverPropertys.BaudRate,
                DataBits = driverPropertys.DataBits,
                Parity = driverPropertys.Parity,
                StopBits = driverPropertys.StopBits,
            })
                .SetBufferLength(1024);
            client = TouchSocketConfig.Container.Resolve<SerialClient>();
            ((SerialClient)client).Setup(TouchSocketConfig);
        }
        //载入配置
        _plc = new((SerialClient)client);
        _plc.Crc16CheckEnable = driverPropertys.Crc16CheckEnable;
        _plc.FrameTime = driverPropertys.FrameTime;
        _plc.CacheTimeout = driverPropertys.CacheTimeout;
        _plc.DataFormat = driverPropertys.DataFormat;
        _plc.Station = driverPropertys.Station;
        _plc.TimeOut = driverPropertys.TimeOut;
    }
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return await _plc.ReadAsync(address, length, cancellationToken);
    }

}
public class ModbusRtuProperty : CollectDriverPropertyBase
{
    [DeviceProperty("波特率", "通常为：38400/19200/9600/4800")]
    public override int BaudRate { get; set; } = 9600;

    [DeviceProperty("CRC检测", "")]
    public bool Crc16CheckEnable { get; set; } = true;

    [DeviceProperty("数据位", "通常为：8/7/6")]
    public override byte DataBits { get; set; } = 8;

    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }

    [DeviceProperty("帧前时间", "某些设备性能较弱，报文间需要间隔较长时间")]
    public int FrameTime { get; set; } = 0;
    [DeviceProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1s")]
    public double CacheTimeout { get; set; } = 1;
    [DeviceProperty("共享链路", "")]
    public override bool IsShareChannel { get; set; } = false;

    [DeviceProperty("最大打包长度", "")]
    public ushort MaxPack { get; set; } = 100;

    [DeviceProperty("校验位", "示例：None/Odd/Even/Mark/Space")]
    public override Parity Parity { get; set; } = Parity.None;

    [DeviceProperty("COM口", "示例：COM1")]
    public override string PortName { get; set; } = "COM1";
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.SerialClient;

    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;

    [DeviceProperty("停止位", "示例：None/One/Two/OnePointFive")]
    public override StopBits StopBits { get; set; } = StopBits.One;
    [DeviceProperty("读写超时时间", "")]
    public ushort TimeOut { get; set; } = 3000;
}
