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

    private ThingsGateway.Foundation.Dlt645.Dlt645_2007Master _plc;

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.Dlt645_2007Master);

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    protected override void Init(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            DtuId = _driverPropertys.DtuId,
            SendDelayTime = _driverPropertys.SendDelayTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            ConnectTimeout = _driverPropertys.ConnectTimeout,
            Timeout = _driverPropertys.Timeout,
            FEHead = _driverPropertys.FEHead,
            OperCode = _driverPropertys.OperCode,
            Password = _driverPropertys.Password,
            CheckClearTime = _driverPropertys.CheckClearTime,
            Station = _driverPropertys.Station,
            HeartbeatHexString = _driverPropertys.HeartbeatHexString,
        };
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        await base.ProtectedBeforStartAsync(cancellationToken).ConfigureAwait(false);
        if (CurrentDevice.Channel.ChannelType == ChannelTypeEnum.TcpService)
            await Task.Delay(6000).ConfigureAwait(false);//等待6秒连接
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, 0, CurrentDevice.IntervalTime);
    }
}
