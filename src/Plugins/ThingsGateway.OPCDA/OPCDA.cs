using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpcDaClient.Da;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA
{
    public class OPCDAClient : DriverBase
    {
        private Foundation.Adapter.OPCDA.OPCDAClient _plc = null;

        public OPCDAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        [DeviceProperty("IP", "")] public string OPCIP { get; set; } = "localhost";
        [DeviceProperty("OPC名称", "")] public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";
        [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
        [DeviceProperty("检测重连频率", "")] public int CheckRate { get; set; } = 60000;
        [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
        [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;
        [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;

        public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
        public override void AfterStop()
        {
            _plc?.Disconnect();
        }

        public override async Task BeforStart()
        {
            _plc.Connect();
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _plc.DataChangedHandler -= dataChangedHandler;
            _plc.Disconnect();
            _plc.Dispose();
            _plc = null;
        }


        public override bool IsSupportAddressRequest()
        {
            return !ActiveSubscribe;
        }
        private List<CollectVariableRunTime> _deviceVariables = new();
        public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            _deviceVariables = deviceVariables;
            if (deviceVariables.Count > 0)
            {
                var result = _plc.SetTags(deviceVariables.Select(a => a.VariableAddress).ToList());
                var sourVars = result?.Select(
          it =>
          {
              return new DeviceVariableSourceRead(UpdateRate)
              {
                  Address = it.Key,
                  DeviceVariables = deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.VariableAddress)).ToList()
              };
          }).ToList();
                return OperResult.CreateSuccessResult(sourVars);
            }
            else
            {
                return OperResult.CreateSuccessResult(new List<DeviceVariableSourceRead>());
            }
        }

        public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var result = _plc.ReadSub(deviceVariableSourceRead.Address);
            return result.Copy<byte[]>();
        }

        public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
        {
            await Task.CompletedTask;
            var result = _plc.Write(deviceVariable.VariableAddress, value);
            return result;
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            _device = device;
            OPCNode oPCNode = new();
            oPCNode.OPCIP = OPCIP;
            oPCNode.OPCName = OPCName;
            oPCNode.UpdateRate = UpdateRate;
            oPCNode.DeadBand = DeadBand;
            oPCNode.GroupSize = GroupSize;
            oPCNode.ActiveSubscribe = ActiveSubscribe;
            oPCNode.CheckRate = CheckRate;
            if (_plc == null)
            {
                _plc = new(TouchSocketConfig.Container.Resolve<ILog>());
                _plc.DataChangedHandler += dataChangedHandler;
            }
            _plc.Init(oPCNode);
        }

        protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            //不走ReadAsync
            throw new NotImplementedException();
        }
        private CollectDeviceRunTime _device;

        private void dataChangedHandler(List<ItemReadResult> values)
        {
            try
            {
                if (!_device.Enable)
                {
                    return;
                }
                _device.DeviceStatus = DeviceStatusEnum.OnLine;

                if (IsLogOut)
                    _logger?.LogTrace(ToString() + " OPC值变化" + values.ToJson());

                foreach (var data in values)
                {
                    if (!_device.Enable)
                    {
                        return;
                    }

                    var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.Name).ToList();
                    foreach (var item in itemReads)
                    {
                        var value = data.Value;
                        var quality = data.Quality;
                        var time = data.TimeStamp;
                        if (value != null && quality == 192)
                        {
                            item.SetValue(value);
                        }
                        else
                        {
                            item.SetValue(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ToString());
                _device.DeviceOffMsg = ex.Message;
            }
        }
    }
}
