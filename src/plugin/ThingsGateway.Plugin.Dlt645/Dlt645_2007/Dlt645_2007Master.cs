//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
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

    private ThingsGateway.Foundation.Dlt645.Dlt645_2007Master _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Demo.Dlt645_2007Master);

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
            SendDelayTime = _driverPropertys.SendDelayTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            ConnectTimeout = _driverPropertys.ConnectTimeout,
            Timeout = _driverPropertys.Timeout,
            EnableFEHead = _driverPropertys.EnableFEHead,
            OperCode = _driverPropertys.OperCode,
            Password = _driverPropertys.Password,
            CheckClear = _driverPropertys.CheckClear,
            Station = _driverPropertys.Station,
            HeartbeatHexString = _driverPropertys.HeartbeatHexString,
        };
        base.Init(channel);
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, 0, CurrentDevice.IntervalTime);
    }
}