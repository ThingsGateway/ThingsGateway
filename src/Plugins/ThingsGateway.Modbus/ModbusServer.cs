using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using System.Net;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Modbus
{
    public class ModbusServer : UpLoadBase
    {

        private ThingsGateway.Foundation.Adapter.Modbus.ModbusServer _plc;

        public ModbusServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        public override OperResult Success()
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
        [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
        [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 502;
        [DeviceProperty("默认站号", "")] public byte Station { get; set; } = 1;
        [DeviceProperty("多站点", "")] public bool MulStation { get; set; } = true;
        [DeviceProperty("允许写入", "")] public bool DeviceRpcEnable { get; set; }

        [VariableProperty("从站变量地址", "")] public string ServiceAddress { get; set; }
        [VariableProperty("允许写入", "")] public bool VariableRpcEnable { get; set; }



        public override async Task BeforStart()
        {
            _plc?.Start();
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _ModbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            _plc.Write -= Write;
            _plc.Stop();
            _plc.Dispose();
        }
        private UploadDevice curDevice;
        private TouchSocketConfig TouchSocketConfig = new();
        protected override void Init(UploadDevice device)
        {
            curDevice = device;
            TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(new EasyLogger(Log_Out)));

            TouchSocketConfig.SetListenIPHosts(new IPHost[] { new IPHost(IPAddress.Parse(IP), Port) })
                    .SetBufferLength(1024);
            var service = TouchSocketConfig.Container.Resolve<TcpService>();
            service.Setup(TouchSocketConfig);
            //载入配置
            _plc = new(service);
            _plc.DataFormat = DataFormat;
            _plc.Station = Station;
            _plc.MulStation = MulStation;

            using var serviceScope = _scopeFactory.CreateScope();
            var _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
            var modbusTag = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
                .Where(b => b.VariablePropertys[device.Id].Any(c =>
                {
                    if (c.PropertyName == nameof(ServiceAddress))
                    {
                        if (c.Value != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }))
                .ToList();

            modbusTag.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });
            _plc.Write += Write;
            try
            {
                _ModbusTags = modbusTag.ToDictionary(a =>
                {
                    ModbusAddress address = null;
                    address = new ModbusAddress(a.VariablePropertys[device.Id].FirstOrDefault(a => a.PropertyName == nameof(ServiceAddress)).Value, Station);
                    return address ?? new ModbusAddress() { AddressStart = -1, Station = -1, ReadFunction = -1 };
                });
            }
            catch
            {
                modbusTag.ForEach(a =>
                {
                    a.VariableValueChange -= VariableValueChange;
                });
                _plc.Write -= Write;
                throw;
            }
        }

        private async Task<OperResult> Write(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter thingsGatewayBitConverter, SocketClient client)
        {
            try
            {
                using var serviceScope = _scopeFactory.CreateScope();
                var rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();
                var tag = _ModbusTags.FirstOrDefault(a => a.Key?.AddressStart == address.AddressStart && a.Key?.Station == address.Station && a.Key?.ReadFunction == address.ReadFunction);

                if (tag.Value == null) return OperResult.CreateSuccessResult();
                var enable = tag.Value.VariablePropertys[curDevice.Id].FirstOrDefault(a => a.PropertyName == nameof(VariableRpcEnable))?.Value.ToBoolean() == true && DeviceRpcEnable;
                if (!enable) return new OperResult("不允许写入");
                var result = await rpcCore.InvokeDeviceMethod($"{nameof(ModbusServer)}-{curDevice.Name}-{client.GetIPPort()}",
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

        private Dictionary<ModbusAddress, CollectVariableRunTime> _ModbusTags;

        private IntelligentConcurrentQueue<(string, CollectVariableRunTime)> Values = new(100000);
        private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
        {
            var property = collectVariableRunTime.VariablePropertys[curDevice.Id].FirstOrDefault(a => a.PropertyName == nameof(ServiceAddress));
            if (property != null && collectVariableRunTime.Value != null)
            {
                Values.Enqueue((property.Value, collectVariableRunTime));
            }
        }
        private bool IsFirst = true;
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (IsFirst)
                _ModbusTags.Values.ToList().ForEach(a => VariableValueChange(a));
            IsFirst = false;

            var list = Values.ToListWithDequeue(100000);
            await Task.Yield();
            foreach (var item in list)
            {
                await _plc.WriteAsync(item.Item2.DataType, item.Item1, item.Item2.Value?.ToString());
                //直接异步等待1s
            }
            await Task.Delay(100, cancellationToken);
        }
    }
}
