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

using Newtonsoft.Json.Linq;

using ThingsGateway.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA;

public class OPCDAClient : CollectBase
{
    internal CollectDeviceRunTime Device;

    internal ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient PLC = null;

    private ConcurrentList<DeviceVariableRunTime> _deviceVariables = new();

    private OPCDAClientProperty driverPropertys = new();

    public OPCDAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    public override System.Type DriverDebugUIType => typeof(OPCDAClientDebugDriverPage);

    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    public override bool IsSupportRequest => !driverPropertys.ActiveSubscribe;
    public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);

    public override Task AfterStopAsync()
    {
        PLC?.Disconnect();
        return Task.CompletedTask;
    }

    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        PLC.Connect();
        await Task.CompletedTask;
    }

    public override void InitDataAdapter()
    {
    }
    public override OperResult IsConnected()
    {
        return PLC.IsConnected ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = new(deviceVariables);
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
        var result = PLC.ReadGroup(deviceVariableSourceRead.Address);
        return result.Copy<byte[]>();
    }

    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var result = PLC.Write(deviceVariable.VariableAddress, value);
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (PLC != null)
            PLC.DataChangedHandler -= dataChangedHandler;
        PLC?.Disconnect();
        PLC?.SafeDispose();
        PLC = null;
        base.Dispose(disposing);
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
            if (!Device.KeepRun)
            {
                return;
            }
            logMessage.Trace(LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + values.ToJson().FormatJson());

            foreach (var data in values)
            {
                if (!Device.KeepRun)
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
                        var jToken = JToken.FromObject(value);
                        object newValue;
                        if (jToken is JValue jValue)
                        {
                            newValue = jValue.Value;
                        }
                        else
                        {
                            newValue = jToken;
                        }
                        var operResult = item.SetValue(newValue, time);
                        if (!operResult.IsSuccess)
                        {
                            _logger?.LogWarning(operResult.Message, ToString());
                        }
                    }
                    else
                    {
                        var operResult = item.SetValue(null, time);
                        if (!operResult.IsSuccess)
                        {
                            _logger?.LogWarning(operResult.Message, ToString());
                        }
                        Device.LastErrorMessage = $"{item.Name} 质量为Bad ";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
            Device.LastErrorMessage = ex.Message;
        }
    }
}
