using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusServer : UpLoadBase
{

    private Dictionary<ModbusAddress, CollectVariableRunTime> _ModbusTags;
    private ThingsGateway.Foundation.Adapter.Modbus.ModbusServer _plc;

    private UploadDevice curDevice;

    private ModbusServerProperty driverPropertys = new();
    private bool IsFirst = true;
    private ConcurrentQueue<(string, CollectVariableRunTime)> Values = new();
    private ModbusServerVariableProperty variablePropertys = new();
    public ModbusServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    public override List<CollectVariableRunTime> UploadVariables => _ModbusTags?.Values.ToList();
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override Type DriverDebugUIType => typeof(ModbusServerDebugDriverPage);

    public override async Task BeforStartAsync()
    {
        _plc?.Start();
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _ModbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        if (_plc != null)
            _plc.Write -= WriteAsync;
        _plc?.Stop();
        _plc?.Dispose();
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (IsFirst)
            _ModbusTags.Values.ToList().ForEach(a => VariableValueChange(a));
        IsFirst = false;

        var list = Values.ToListWithDequeue();
        foreach (var item in list)
        {
            await _plc.WriteAsync(item.Item2.DataType, item.Item1, item.Item2.Value?.ToString());
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
    protected override void Init(UploadDevice device)
    {
        curDevice = device;
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
        var _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
            .Where(b => b.VariablePropertys[device.Id].Any(c =>
            {
                if (c.PropertyName == nameof(variablePropertys.ServiceAddress))
                {
                    if (c.Value != null)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }))
            .ToList();

        tags.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });
        _plc.Write += WriteAsync;
        try
        {
            _ModbusTags = tags.ToDictionary(a =>
            {
                ModbusAddress address = null;
                address = new ModbusAddress(a.VariablePropertys[device.Id].FirstOrDefault(a => a.PropertyName == nameof(variablePropertys.ServiceAddress)).Value, driverPropertys.Station);
                return address ?? new ModbusAddress() { AddressStart = -1, Station = -1, ReadFunction = -1 };
            });
        }
        catch
        {
            tags.ForEach(a =>
            {
                a.VariableValueChange -= VariableValueChange;
            });
            _plc.Write -= WriteAsync;
            throw;
        }
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        var property = collectVariableRunTime.VariablePropertys[curDevice.Id].FirstOrDefault(a => a.PropertyName == nameof(variablePropertys.ServiceAddress));
        if (property != null && collectVariableRunTime.Value != null)
        {
            Values.Enqueue((property.Value, collectVariableRunTime));
        }
    }

    private async Task<OperResult> WriteAsync(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter thingsGatewayBitConverter, SocketClient client)
    {
        try
        {
            var serviceScope = _scopeFactory.CreateScope();
            var rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
            var tag = _ModbusTags.FirstOrDefault(a => a.Key?.AddressStart == address.AddressStart && a.Key?.Station == address.Station && a.Key?.ReadFunction == address.ReadFunction);

            if (tag.Value == null) return OperResult.CreateSuccessResult();
            var enable = tag.Value.VariablePropertys[curDevice.Id].FirstOrDefault(a => a.PropertyName == nameof(variablePropertys.VariableRpcEnable))?.Value.ToBoolean() == true && driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult("不允许写入");
            var result = await rpcCore.InvokeDeviceMethodAsync($"{nameof(ModbusServer)}-{curDevice.Name}-{client.IP + ":" + client.Port}",
            new()
            {
                Name = tag.Value.Name,
                Value = thingsGatewayBitConverter.GetDynamicData(tag.Value.DataType, bytes).ToString()
            });
            return result;
        }
        catch (Exception ex)
        {
            return new OperResult(ex.Message);
        }

    }
}
public class ModbusServerProperty : UpDriverPropertyBase
{


    [DeviceProperty("IP", "")]
    public string IP { get; set; } = "";


    [DeviceProperty("端口", "")]
    public int Port { get; set; } = 502;

    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    [DeviceProperty("多站点", "")]
    public bool MulStation { get; set; } = true;
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    [DeviceProperty("允许写入", "")]
    public bool DeviceRpcEnable { get; set; }
}

public class ModbusServerVariableProperty : VariablePropertyBase
{
    [VariableProperty("从站变量地址", "")]
    public string ServiceAddress { get; set; }
    [VariableProperty("允许写入", "")]
    public bool VariableRpcEnable { get; set; }
}
