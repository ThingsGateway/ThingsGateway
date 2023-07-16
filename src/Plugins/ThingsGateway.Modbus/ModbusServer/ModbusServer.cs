#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusServer : UpLoadBase
{

    private Dictionary<ModbusAddress, DeviceVariableRunTime> _ModbusTags;
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusServer _plc;

    private ModbusServerProperty driverPropertys = new();
    private ConcurrentQueue<(string, DeviceVariableRunTime)> Values = new();
    private ModbusServerVariableProperty variablePropertys = new();
    public ModbusServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override Type DriverDebugUIType => typeof(ModbusServerDebugDriverPage);
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    public override List<DeviceVariableRunTime> UploadVariables => _ModbusTags?.Values.ToList();
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override Task BeforStartAsync(CancellationToken cancellationToken)
    {
        return _plc?.ConnectAsync(cancellationToken);
    }
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var list = Values.ToListWithDequeue();
        foreach (var item in list)
        {
            var type = GetPropertyValue(item.Item2, nameof(ModbusServerVariableProperty.ModbusType));
            if (Enum.TryParse<DataTypeEnum>(type, out DataTypeEnum result))
            {
                await _plc.WriteAsync(item.Item1, result.GetSystemType(), item.Item2.Value?.ToString(), cancellationToken);
            }
            else
            {
                await _plc.WriteAsync(item.Item1, item.Item2.DataType, item.Item2.Value?.ToString(), cancellationToken);
            }
        }
        await Task.Delay(100, cancellationToken);
    }

    public override OperResult IsConnected()
    {
        if (_plc?.TcpService?.ServerState == ServerState.Running)
        {
            return OperResult.CreateSuccessResult();
        }
        else
        {
            return new OperResult();
        }
    }

    protected override void Dispose(bool disposing)
    {
        _ModbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        if (_plc != null)
            _plc.Write -= WriteAsync;
        _plc?.Disconnect();
        _plc?.SafeDispose();
        _ModbusTags?.Clear();
        _ModbusTags = null;
        Values.Clear();
        Values = null;
        base.Dispose(disposing);
    }
    protected override void Init(UploadDeviceRunTime device)
    {
        IPHost iPHost = new IPHost(driverPropertys.Port);
        if (!driverPropertys.IP.IsNullOrEmpty())
        {
            iPHost = new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}");
        }
        TouchSocketConfig.SetListenIPHosts(new IPHost[] { iPHost }).SetBufferLength(1024);
        var service = TouchSocketConfig.Container.Resolve<TcpService>();
        service.Setup(TouchSocketConfig);
        //载入配置
        _plc = new(service);
        _plc.DataFormat = driverPropertys.DataFormat;
        _plc.Station = driverPropertys.Station;
        _plc.MulStation = driverPropertys.MulStation;

        var serviceScope = _scopeFactory.CreateScope();
        var _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
            .Where(b => !GetPropertyValue(b, nameof(variablePropertys.ServiceAddress)).IsNullOrEmpty())
            .ToList();

        tags.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
            VariableValueChange(a);
        });
        _plc.Write += WriteAsync;
        try
        {
            _ModbusTags = tags.ToDictionary(a =>
            {
                ModbusAddress address = null;
                address = new ModbusAddress(
                    GetPropertyValue(a, nameof(variablePropertys.ServiceAddress))
                    , driverPropertys.Station);
                return address ?? new ModbusAddress() { AddressStart = -1, Station = -1, ReadFunction = -1 };
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ToString());
            tags.ForEach(a =>
            {
                a.VariableValueChange -= VariableValueChange;
            });
            _plc.Write -= WriteAsync;
        }
        RpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();

    }
    RpcSingletonService RpcCore { get; set; }
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        var address = GetPropertyValue(collectVariableRunTime, nameof(variablePropertys.ServiceAddress));
        if (address != null && collectVariableRunTime.Value != null)
        {
            Values.Enqueue((address, collectVariableRunTime));
        }
    }

    private async Task<OperResult> WriteAsync(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter thingsGatewayBitConverter, SocketClient client)
    {
        try
        {

            var tag = _ModbusTags.FirstOrDefault(a => a.Key?.AddressStart == address.AddressStart && a.Key?.Station == address.Station && a.Key?.ReadFunction == address.ReadFunction);

            if (tag.Value == null) return OperResult.CreateSuccessResult();
            var enable =
                GetPropertyValue(tag.Value, nameof(variablePropertys.VariableRpcEnable)).ToBoolean()
                && driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult("不允许写入");
            var result = await RpcCore.InvokeDeviceMethodAsync($"{nameof(ModbusServer)}-{CurDevice.Name}-{client.IP + ":" + client.Port}",
            new(tag.Value.Name, thingsGatewayBitConverter.GetDynamicDataFormBytes(tag.Value.VariableAddress, tag.Value.DataType, bytes).ToString()), CancellationToken.None);
            return result;
        }
        catch (Exception ex)
        {
            return new OperResult(ex.Message);
        }

    }
}
