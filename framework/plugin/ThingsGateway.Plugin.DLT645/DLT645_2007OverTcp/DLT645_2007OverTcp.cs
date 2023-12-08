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

namespace ThingsGateway.Plugin.DLT645;

/// <inheritdoc/>
public class DLT645_2007OverTcp : CollectBase
{
    private readonly DLT645_2007OverTcpProperty _driverPropertys = new();
    private ThingsGateway.Foundation.Adapter.DLT645.DLT645_2007OverTcp _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(DLT645_2007OverTcpDebugPage);

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => _plc;

    public override Type DriverUIType => null;

    protected override List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        return _plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(deviceVariables, 0, CurrentDevice.IntervalTime);
    }

    /// <inheritdoc/>
    protected override void Init(ISenderClient client = null)
    {
        if (client == null)
        {
            FoundataionConfig.SetRemoteIPHost(new IPHost($"{_driverPropertys.IP}:{_driverPropertys.Port}"))
                ;
            client = new TcpClient();
            ((TcpClient)client).Setup(FoundataionConfig);
        }
        //载入配置
        _plc = new((TcpClient)client)
        {
            FrameTime = _driverPropertys.FrameTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            DataFormat = _driverPropertys.DataFormat,
            EnableFEHead = _driverPropertys.EnableFEHead,
            OperCode = _driverPropertys.OperCode,
            Password = _driverPropertys.Password,
            Station = _driverPropertys.Station,
            TimeOut = _driverPropertys.TimeOut
        };
        base.Init(client);
    }
}