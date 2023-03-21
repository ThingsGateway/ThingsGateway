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
        internal Foundation.Adapter.OPCUA.OPCUAClient PLC = null;

        internal CollectDeviceRunTime Device;

        private List<CollectVariableRunTime> _deviceVariables = new();

        public OPCUAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        [DeviceProperty("连接Url", "")] public string OPCURL { get; set; } = "opc.tcp://127.0.0.1:49320";
        [DeviceProperty("登录账号", "为空时将采用匿名方式登录")] public string UserName { get; set; }
        [DeviceProperty("登录密码", "")] public string Password { get; set; }
        [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
        [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
        public override Type DriverUI => typeof(ImportVariable);
        [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;
        public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
        [DeviceProperty("重连频率", "")] public int ReconnectPeriod { get; set; } = 5000;
        [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;
        public override void AfterStop()
        {
            PLC?.Disconnect();
        }

        public override async Task BeforStart()
        {
            await PLC.ConnectServer();
        }

        public override void Dispose()
        {
            PLC.DataChangedHandler -= dataChangedHandler;
            PLC.OpcStatusChange -= opcStatusChange;
            PLC.Disconnect();
            PLC.Dispose();
            PLC = null;
        }

        public override bool IsSupportAddressRequest()
        {
            return !ActiveSubscribe;
        }

        public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            _deviceVariables = deviceVariables;
            if (deviceVariables.Count > 0)
            {
                var result = PLC.SetTags(deviceVariables.Select(a => a.VariableAddress).ToList());
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
            var result = await PLC.ReadNodeAsync(deviceVariableSourceRead.DeviceVariables.Select(a => a.VariableAddress).ToArray());

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
            var result = PLC.WriteNode(deviceVariable.VariableAddress,Convert.ChangeType( value,deviceVariable.DataType));
            return result ? OperResult.CreateSuccessResult() : new OperResult();
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            Device = device;
            OPCNode oPCNode = new();
            oPCNode.OPCUrl = OPCURL;
            oPCNode.UpdateRate = UpdateRate;
            oPCNode.DeadBand = DeadBand;
            oPCNode.GroupSize = GroupSize;
            oPCNode.ReconnectPeriod = ReconnectPeriod;
            if (PLC == null)
            {
                PLC = new();
                PLC.DataChangedHandler += dataChangedHandler;
                PLC.OpcStatusChange += opcStatusChange; ;
            }
            if (!UserName.IsNullOrEmpty())
            {
                PLC.UserIdentity = new UserIdentity("Administrator", "111111");

            }
            else
            {
                PLC.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
            }
            PLC.OPCNode = oPCNode;
        }

        protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            //不走ReadAsync
            throw new NotImplementedException();
        }

        private void dataChangedHandler(List<(MonitoredItem monitoredItem, MonitoredItemNotification monitoredItemNotification)> values)
        {
            try
            {
                if (!Device.Enable)
                {
                    return;
                }
                Device.DeviceStatus = DeviceStatusEnum.OnLine;

                if (IsLogOut)
                    _logger?.LogTrace(ToString() + " OPC值变化" + values.ToJson());

                foreach (var data in values)
                {
                    if (!Device.Enable)
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
                            Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                            Device.DeviceOffMsg = $"{item.Name} 质量为Bad ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ToString());
                Device.DeviceOffMsg = ex.Message;
            }
        }
        private void opcStatusChange(object sender, OPCUAStatusEventArgs e)
        {
            if (e.Error)
            {
                _logger.LogError(e.Text);
                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                Device.DeviceOffMsg = $"{e.Text}";
            }
            else
            {
                _logger.LogTrace(e.Text);
            }
        }
    }
}
