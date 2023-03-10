using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Opc.Ua;
using Opc.Ua.Client;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA
{
    public class OPCUAClient : DriverBase
    {
        private Foundation.Adapter.OPCUA.OPCUAClient _plc = null;

        public OPCUAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        [DeviceProperty("连接Url", "")] public string OPCURL { get; set; } = "opc.tcp://127.0.0.1:49320";
        [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
        [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
        [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;
        [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;
        [DeviceProperty("心跳频率", "")] public int ReconnectPeriod { get; set; } = 5000;

        [DeviceProperty("登录账号", "为空时将采用匿名方式登录")] public string UserName { get; set; }
        [DeviceProperty("登录密码", "")] public string Password { get; set; }

        public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
        public override void AfterStop()
        {
            _plc?.Disconnect();
        }

        public override async Task BeforStart()
        {
            await _plc.ConnectServer();
        }

        public override void Dispose()
        {
            _plc.DataChangedHandler -= dataChangedHandler;
            _plc.OpcStatusChange -= opcStatusChange;
            _plc.Disconnect();
            _plc.Dispose();
            _plc = null;
        }

        private void dataChangedHandler(List<(MonitoredItem monitoredItem, MonitoredItemNotification monitoredItemNotification)> values)
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

                    var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.monitoredItem.StartNodeId).ToList();
                    foreach (var item in itemReads)
                    {
                        var value = data.monitoredItemNotification.Value.Value;
                        var quality = StatusCode.IsBad(data.monitoredItemNotification.Value.StatusCode) ? 0 : 192;

                        var time = data.monitoredItemNotification.Value.SourceTimestamp;
                        if (value != null && quality == 192)
                        {
                            item.SetValue(value);
                        }
                        else
                        {
                            item.SetValue(null);
                            _device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                            _device.DeviceOffMsg = $"{item.Name} 质量为Bad ";
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
                  DeviceVariables = deviceVariables.Where(a => it.Value.Contains(a.VariableAddress)).ToList()
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
            var result = await _plc.ReadNodeAsync(deviceVariableSourceRead.DeviceVariables.Select(a => a.VariableAddress).ToArray());

            if (result.Any(a => StatusCode.IsBad(a.StatusCode)))
            {
                return new OperResult<byte[]>($"读取失败");
            }
            else
            {
                return OperResult.CreateSuccessResult<byte[]>(null);
            }
        }

        public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
        {
            await Task.CompletedTask;
            var result = _plc.WriteNode(deviceVariable.VariableAddress, value);
            return result ? OperResult.CreateSuccessResult() : new OperResult();
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            _device = device;
            OPCNode oPCNode = new();
            oPCNode.OPCUrl = OPCURL;
            oPCNode.UpdateRate = UpdateRate;
            oPCNode.DeadBand = DeadBand;
            oPCNode.GroupSize = GroupSize;
            oPCNode.ReconnectPeriod = ReconnectPeriod;
            if (_plc == null)
            {
                _plc = new();
                _plc.DataChangedHandler += dataChangedHandler;
                _plc.OpcStatusChange += opcStatusChange; ;
            }
            if (!UserName.IsNullOrEmpty())
            {
                _plc.UserIdentity = new UserIdentity("Administrator", "111111");

            }
            else
            {
                _plc.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
            }
            _plc.OPCNode = oPCNode;
        }

        private void opcStatusChange(object sender, OPCUAStatusEventArgs e)
        {
            if (e.Error)
            {
                _logger.LogError(e.Text);
                _device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                _device.DeviceOffMsg = $"{e.Text}";
            }
            else
            {
                _logger.LogTrace(e.Text);
            }
        }

        protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            //不走ReadAsync
            throw new NotImplementedException();
        }
        private CollectDeviceRunTime _device;


    }
}
