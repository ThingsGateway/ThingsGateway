using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpcDaClient.Da;

using ThingsGateway.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA;


public class OPCDAClient : CollectBase
{
    public override void InitDataAdapter()
    {
    }
    internal CollectDeviceRunTime Device;
    internal ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient PLC = null;
    private List<CollectVariableRunTime> _deviceVariables = new();
    private OPCDAClientProperty driverPropertys = new();
    public OPCDAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    public override System.Type DriverImportUIType => typeof(ImportVariable);
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;
    public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);


    public override void AfterStop()
    {
        PLC?.Disconnect();
    }

    public override async Task BeforStartAsync()
    {
        PLC.Connect();
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (PLC != null)
            PLC.DataChangedHandler -= dataChangedHandler;
        PLC?.Disconnect();
        PLC?.Dispose();
        PLC = null;
    }


    public override OperResult IsConnected()
    {
        return PLC.IsConnected ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }

    public override bool IsSupportAddressRequest()
    {
        return !driverPropertys.ActiveSubscribe;
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
          return new DeviceVariableSourceRead(driverPropertys.UpdateRate)
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
        var result = PLC.ReadSub(deviceVariableSourceRead.Address);
        return result.Copy<byte[]>();
    }

    public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
    {
        await Task.CompletedTask;
        var result = PLC.Write(deviceVariable.VariableAddress, value);
        return result;
    }

    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        Device = device;
        OPCNode oPCNode = new();
        oPCNode.OPCIP = driverPropertys.OPCIP;
        oPCNode.OPCName = driverPropertys.OPCName;
        oPCNode.UpdateRate = driverPropertys.UpdateRate;
        oPCNode.DeadBand = driverPropertys.DeadBand;
        oPCNode.GroupSize = driverPropertys.GroupSize;
        oPCNode.ActiveSubscribe = driverPropertys.ActiveSubscribe;
        oPCNode.CheckRate = driverPropertys.CheckRate;
        if (PLC == null)
        {
            PLC = new(TouchSocketConfig.Container.Resolve<ILog>());
            PLC.DataChangedHandler += dataChangedHandler;
        }
        PLC.Init(oPCNode);
    }

    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        //不走ReadAsync
        throw new NotImplementedException();
    }
    private void dataChangedHandler(List<ItemReadResult> values)
    {
        try
        {
            if (!Device.Enable)
            {
                return;
            }
            Device.DeviceStatus = DeviceStatusEnum.OnLine;
            logMessage.Trace("报文-" + ToString() + "状态变化:" + Environment.NewLine + values.ToJson().FormatJson());

            foreach (var data in values)
            {
                if (!Device.Enable)
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
                        item.SetValue(value, time);

                    }
                    else
                    {
                        item.SetValue(null, time);
                        Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                        Device.DeviceOffMsg = $"{item.Name} 质量为Bad ";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
            Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
            Device.DeviceOffMsg = ex.Message;
        }
    }
}

public class OPCDAClientProperty : CollectDriverPropertyBase
{
    [DeviceProperty("IP", "")] public string OPCIP { get; set; } = "localhost";
    [DeviceProperty("OPC名称", "")] public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";

    [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
    [DeviceProperty("检测重连频率", "")] public int CheckRate { get; set; } = 60000;
    [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
    [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;
    [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;
    public override bool IsShareChannel { get; set; } = false;

    public override ShareChannelEnum ShareChannel => ShareChannelEnum.None;
}
