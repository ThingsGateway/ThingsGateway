//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Master : CollectBase
{
    private readonly Dlt645_2007MasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.Dlt645.Dlt645_2007Master _plc = new();

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.Dlt645_2007Master);

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

    public override Type DriverVariableAddressUIType => typeof(ThingsGateway.Debug.Dlt645_2007AddressComponent);

    /// <inheritdoc/>
    public override IDevice? FoundationDevice => _plc;


    protected override void InitChannel(IChannel? channel = null)
    {

        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc.DataFormat = _driverPropertys.DataFormat;
        _plc.DtuId = _driverPropertys.DtuId;
        _plc.SendDelayTime = _driverPropertys.SendDelayTime;
        _plc.Timeout = _driverPropertys.Timeout;
        _plc.FEHead = _driverPropertys.FEHead;
        _plc.OperCode = _driverPropertys.OperCode;
        _plc.Password = _driverPropertys.Password;
        _plc.CheckClearTime = _driverPropertys.CheckClearTime;
        _plc.Station = _driverPropertys.Station;
        _plc.Heartbeat = _driverPropertys.Heartbeat;
        _plc.InitChannel(channel,LogMessage);

        base.InitChannel(channel);
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRuntime> deviceVariables)
    {
        return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, 0, CurrentDevice.IntervalTime);
    }
}
