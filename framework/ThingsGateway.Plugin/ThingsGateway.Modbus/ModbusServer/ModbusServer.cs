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

using Furion;

using Microsoft.Extensions.Logging;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Admin.Core;
using ThingsGateway.Application;
using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;

namespace ThingsGateway.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusServer : UpLoadBase
{

    private readonly ModbusServerProperty driverPropertys = new();
    private readonly ModbusServerVariableProperty variablePropertys = new();
    private Dictionary<ModbusAddress, DeviceVariableRunTime> _ModbusTags;
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusServer _plc;
    private ConcurrentQueue<(string, DeviceVariableRunTime)> Values = new();
    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ModbusServerDebugDriverPage);
    /// <inheritdoc/>
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    /// <inheritdoc/>
    public override List<DeviceVariableRunTime> UploadVariables => _ModbusTags?.Values.ToList();
    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    RpcSingletonService RpcCore { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override Task AfterStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task BeforStartAsync(CancellationToken token)
    {
        return _plc?.ConnectAsync(token);
    }
    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken token)
    {
        var list = Values.ToListWithDequeue();
        foreach (var item in list)
        {
            var type = GetPropertyValue(item.Item2, nameof(ModbusServerVariableProperty.ModbusType));
            if (Enum.TryParse<DataTypeEnum>(type, out DataTypeEnum result))
            {
                await _plc.WriteAsync(item.Item1, result.GetSystemType(), item.Item2.Value?.ToString(), token);
            }
            else
            {
                await _plc.WriteAsync(item.Item1, item.Item2.DataType, item.Item2.Value?.ToString(), token);
            }
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, token);
            }
            catch
            {
            }
        }
        else
        {

        }
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return _plc?.TcpService?.ServerState == ServerState.Running;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _ModbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        if (_plc != null)
            _plc.WriteData -= WriteData;
        _plc?.Disconnect();
        _plc?.SafeDispose();
        _ModbusTags?.Clear();
        _ModbusTags = null;
        Values.Clear();
        Values = null;
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(UploadDeviceRunTime device)
    {
        IPHost iPHost = new(driverPropertys.Port);
        if (!string.IsNullOrEmpty(driverPropertys.IP))
        {
            iPHost = new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}");
        }
        FoundataionConfig.SetListenIPHosts(new IPHost[] { iPHost }).SetBufferLength(1024);
        var service = new TcpService();
        service.Setup(FoundataionConfig);
        //载入配置
        _plc = new(service)
        {
            DataFormat = driverPropertys.DataFormat,
            Station = driverPropertys.Station,
            MulStation = driverPropertys.MulStation
        };

        var _globalDeviceData = App.GetService<GlobalDeviceData>();
        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
            .Where(b => !string.IsNullOrEmpty(GetPropertyValue(b, nameof(variablePropertys.ServiceAddress))))
            .ToList();

        tags.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
            VariableValueChange(a);
        });
        _plc.WriteData += WriteData;
        try
        {
            _ModbusTags = tags.ToDictionary(a =>
            {
                ModbusAddress address = null;
                address = new ModbusAddress(
                    GetPropertyValue(a, nameof(variablePropertys.ServiceAddress))
                    , driverPropertys.Station);
                return address ?? new ModbusAddress() { AddressStart = -1, Station = 0, ReadFunction = 0 };
            });
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
            tags.ForEach(a =>
            {
                a.VariableValueChange -= VariableValueChange;
            });
            _plc.WriteData -= WriteData;
        }
        RpcCore = App.GetService<RpcSingletonService>();

    }
    /// <inheritdoc/>
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        var address = GetPropertyValue(collectVariableRunTime, nameof(variablePropertys.ServiceAddress));
        if (address != null && collectVariableRunTime.Value != null)
        {
            Values?.Enqueue((address, collectVariableRunTime));
        }
    }

    /// <summary>
    /// RPC写入
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <param name="thingsGatewayBitConverter"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    private OperResult WriteData(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter thingsGatewayBitConverter, SocketClient client)
    {
        try
        {
            var tag = _ModbusTags.FirstOrDefault(a => a.Key?.AddressStart == address.AddressStart && a.Key?.Station == address.Station && a.Key?.ReadFunction == address.ReadFunction);

            if (tag.Value == null) return OperResult.CreateSuccessResult();
            var enable =
                GetPropertyValue(tag.Value, nameof(variablePropertys.VariableRpcEnable)).ToBool()
                && driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult("不允许写入");
            var type = GetPropertyValue(tag.Value, nameof(ModbusServerVariableProperty.ModbusType));
            var addressStr = GetPropertyValue(tag.Value, nameof(ModbusServerVariableProperty.ServiceAddress));
            if (Enum.TryParse<DataTypeEnum>(type, out DataTypeEnum result))
            {
                var resultTask1 = RpcCore.InvokeDeviceMethodAsync($"{nameof(ModbusServer)}-{CurDevice.Name}-{client.IP + ":" + client.Port}",
                new(tag.Value.Name, thingsGatewayBitConverter.GetDynamicDataFormBytes(addressStr ?? string.Empty, result.GetSystemType(), bytes).ToString()));
                var result1 = resultTask1.ConfigureAwait(true).GetAwaiter().GetResult();
                return result1;
            }
            else
            {
                var resultTask1 = RpcCore.InvokeDeviceMethodAsync($"{nameof(ModbusServer)}-{CurDevice.Name}-{client.IP + ":" + client.Port}",
new(tag.Value.Name, thingsGatewayBitConverter.GetDynamicDataFormBytes(addressStr ?? string.Empty, tag.Value.DataType, bytes).ToString()));
                var result1 = resultTask1.ConfigureAwait(true).GetAwaiter().GetResult();
                return result1;
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex.Message);
        }

    }
}
