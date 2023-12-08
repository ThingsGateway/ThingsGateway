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

using ThingsGateway.Foundation.Demo;

namespace ThingsGateway.Plugin.Modbus;

/// <inheritdoc/>
public class ModbusRtu : CollectBase
{
    private readonly ModbusRtuProperty _driverPropertys = new();

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => _plc;

    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtu _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ModbusRtuDebugPage);

    /// <inheritdoc/>
    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    /// <inheritdoc/>
    protected override List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(deviceVariables, _driverPropertys.MaxPack, CurrentDevice.IntervalTime);
    }

    /// <inheritdoc/>
    protected override void Init(ISenderClient client = null)
    {
        if (client == null)
        {
            FoundataionConfig.SetSerialPortOption(new()
            {
                PortName = _driverPropertys.PortName,
                BaudRate = _driverPropertys.BaudRate,
                DataBits = _driverPropertys.DataBits,
                Parity = _driverPropertys.Parity,
                StopBits = _driverPropertys.StopBits,
            });
            client = new SerialPortClient();
            ((SerialPortClient)client).Setup(FoundataionConfig);
        }
        //载入配置
        _plc = new((SerialPortClient)client)
        {
            Crc16CheckEnable = _driverPropertys.Crc16CheckEnable,
            FrameTime = _driverPropertys.FrameTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            DataFormat = _driverPropertys.DataFormat,
            Station = _driverPropertys.Station,
            TimeOut = _driverPropertys.TimeOut
        };
        base.Init(client);
    }
}