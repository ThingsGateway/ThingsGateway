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

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusMaster : CollectBase
{
    private readonly ModbusMasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.Modbus.ModbusMaster _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Demo.ModbusMaster);

    /// <inheritdoc/>
    public override CollectPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel, "通道配置不能为null");
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            IsStringReverseByteWord = _driverPropertys.IsStringReverseByteWord,
            SendDelayTime = _driverPropertys.SendDelayTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            ConnectTimeout = _driverPropertys.ConnectTimeout,
            Station = _driverPropertys.Station,
            Timeout = _driverPropertys.Timeout,
            CheckClear = _driverPropertys.CheckClear,
            ModbusType = _driverPropertys.ModbusType,
            HeartbeatHexString = _driverPropertys.HeartbeatHexString,
        };
        base.Init(channel);
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, _driverPropertys.MaxPack, CurrentDevice.IntervalTime);
    }
}