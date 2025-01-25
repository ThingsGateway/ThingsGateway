//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Debug;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusMaster : CollectBase
{
    private readonly ModbusMasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.Modbus.ModbusMaster _plc = new();

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.ModbusMaster);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (FoundationDevice.Channel?.ChannelType == ChannelTypeEnum.TcpService)
                return typeof(ThingsGateway.Gateway.Razor.TcpServicePage);
            else
                return null;
        }
    }

    /// <inheritdoc/>
    public override IDevice FoundationDevice => _plc;

    public override Type DriverVariableAddressUIType => typeof(ModbusAddressComponent);

    /// <inheritdoc/>
    protected override void InitChannel(IChannel? channel = null)
    {

        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc.DataFormat = _driverPropertys.DataFormat;
        _plc.DtuId = _driverPropertys.DtuId;
        _plc.IsStringReverseByteWord = _driverPropertys.IsStringReverseByteWord;
        _plc.SendDelayTime = _driverPropertys.SendDelayTime;
        _plc.Station = _driverPropertys.Station;
        _plc.Timeout = _driverPropertys.Timeout;
        _plc.CheckClearTime = _driverPropertys.CheckClearTime;
        _plc.ModbusType = _driverPropertys.ModbusType;
        _plc.Heartbeat = _driverPropertys.Heartbeat;
        _plc.InitChannel(channel, LogMessage);
        base.InitChannel(channel);
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRuntime> deviceVariables)
    {
        return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, _driverPropertys.MaxPack, CurrentDevice.IntervalTime);
    }
}
