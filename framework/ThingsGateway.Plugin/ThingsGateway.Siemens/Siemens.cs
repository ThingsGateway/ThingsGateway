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

using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;

namespace ThingsGateway.Siemens;
/// <summary>
/// S7
/// </summary>
public abstract class Siemens : CollectBase
{
    /// <summary>
    /// PLC
    /// </summary>
    protected SiemensS7PLC _plc;
    /// <summary>
    /// SiemensProperty
    /// </summary>
    protected SiemensProperty driverPropertys = new();

    /// <inheritdoc/>
    public override bool IsSupportRequest => true;

    /// <inheritdoc/>
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override Task AfterStopAsync()
    {
        if (_plc != null)
            _plc?.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken token)
    {
        await _plc?.ConnectAsync(token);
    }

    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.TcpClientEx?.CanSend == true;

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _plc.Connect(CancellationToken.None);
        var data = deviceVariables.LoadSourceRead(_plc);
        _plc?.Disconnect();
        return data;
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateAsync(string,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DeviceMethod("ReadWriteDateAsync", "")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken token = default)
    {
        if (value == null)
            return await _plc?.ReadDateAsync(address, token);
        else
            return (await _plc?.WriteDateAsync(address, value.Value, token)).Copy<System.DateTime>();

    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateTimeAsync(string,CancellationToken)"/>
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [DeviceMethod("ReadWriteDateTimeAsync", "")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken token = default)
    {
        if (value == null)
            return await _plc?.ReadDateTimeAsync(address, token);
        else
            return (await _plc?.WriteDateTimeAsync(address, value.Value, token)).Copy<System.DateTime>();
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadStringAsync(string,Encoding,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DeviceMethod("ReadWriteStringAsync", "")]
    public async Task<OperResult<string>> ReadWriteStringAsync(string address, Encoding encoding, string value = null, CancellationToken token = default)
    {
        if (value == null)
            return await _plc?.ReadStringAsync(address, encoding, token);
        else
            return (await _plc?.WriteAsync(address, value, token)).Copy<string>();
    }





    /// <inheritdoc/>
    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, JToken value, CancellationToken token)
    {
        return await _plc.WriteAsync(deviceVariable.VariableAddress, deviceVariable.DataType, value.ToString(), token);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _plc?.Disconnect();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token)
    {
        return await _plc.ReadAsync(address, length, token);
    }

}
