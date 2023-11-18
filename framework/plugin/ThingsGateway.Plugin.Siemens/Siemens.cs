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

using System.Text;

namespace ThingsGateway.Plugin.Siemens;
/// <summary>
/// S7
/// </summary>
public class Siemens : CollectBase
{
    /// <summary>
    /// PLC
    /// </summary>
    protected SiemensS7PLC _plc;
    /// <summary>
    /// SiemensProperty
    /// </summary>
    protected SiemensProperty _driverPropertys = new();

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => _plc;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override Type DriverDebugUIType => typeof(SiemensDebugPage);
    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;
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
            DataFormat = _driverPropertys.DataFormat,
            ConnectTimeOut = _driverPropertys.ConnectTimeOut,
            TimeOut = _driverPropertys.TimeOut,
            SiemensEnum = _driverPropertys.SiemensEnum
        };
        if (_driverPropertys.LocalTSAP != 0)
        {
            _plc.LocalTSAP = _driverPropertys.LocalTSAP;
        }
        if (_driverPropertys.Rack != 0)
        {
            _plc.Rack = _driverPropertys.Rack;
        }
        if (_driverPropertys.Slot != 0)
        {
            _plc.Slot = _driverPropertys.Slot;
        }
        base.Init(client);
    }

    protected override List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        try { _plc.Connect(CancellationToken.None); } catch { }
        try
        {
            var data = _plc.LoadSourceRead<DeviceVariableSourceRead, DeviceVariableRunTime>(deviceVariables, 0, CurrentDevice.IntervalTime);
            return data;
        }
        finally { _plc.Disconnect(); }
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateAsync(string,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DeviceMethod("ReadWriteDateAsync", "")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc?.ReadDateAsync(address, cancellationToken);
        else
            return new(await _plc?.WriteDateAsync(address, value.Value, cancellationToken));
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateTimeAsync(string,CancellationToken)"/>
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DeviceMethod("ReadWriteDateTimeAsync", "")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc?.ReadDateTimeAsync(address, cancellationToken);
        else
            return new(await _plc?.WriteDateTimeAsync(address, value.Value, cancellationToken));
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadStringAsync(string,Encoding,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DeviceMethod("ReadWriteStringAsync", "")]
    public async Task<OperResult<string>> ReadWriteStringAsync(string address, Encoding encoding, string value = null, CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        if (value == null)
            return await _plc?.ReadStringAsync(address, encoding, cancellationToken);
        else
            return new(await _plc?.WriteAsync(address, value, cancellationToken));
    }

}
