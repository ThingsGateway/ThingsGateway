
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Gateway.Application;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Plugin.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7Master : CollectBase
{
    private readonly SiemensS7MasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.SiemensS7.SiemensS7Master _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.SiemensS7Master);

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            SendDelayTime = _driverPropertys.SendDelayTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            ConnectTimeout = _driverPropertys.ConnectTimeout,
            SiemensS7Type = _driverPropertys.SiemensS7Type,
            Timeout = _driverPropertys.Timeout,
            LocalTSAP = _driverPropertys.LocalTSAP,
            Rack = _driverPropertys.Rack,
            Slot = _driverPropertys.Slot,
        };
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        try { _plc.Channel.ConnectAsync(_driverPropertys.ConnectTimeout).GetFalseAwaitResult(); } catch { }
        try
        {
            return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, _plc.OnLine? _plc.PduLength:_driverPropertys.MaxPack, CurrentDevice.IntervalTime);
        }
        finally { _plc.Channel.Close(); }
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateAsync(string,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DynamicMethod("ReadWriteDateAsync", "读写日期格式")]
    public async ValueTask<IOperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateAsync(address, cancellationToken).ConfigureAwait(false);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateAsync(address, value.Value, cancellationToken));
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateTimeAsync(string,CancellationToken)"/>
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DynamicMethod("ReadWriteDateTimeAsync", "读写日期时间格式")]
    public async ValueTask<IOperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateTimeAsync(address, cancellationToken);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateTimeAsync(address, value.Value, cancellationToken).ConfigureAwait(false));
    }
}
