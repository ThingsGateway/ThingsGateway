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

using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

namespace ThingsGateway.Plugin.OPCDA;
/// <summary>
/// OPCDAClient
/// </summary>
public class OPCDAClient : CollectBase
{
    internal ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient _plc = null;
    internal CollectDeviceRunTime Device;
    private readonly OPCDAClientProperty driverPropertys = new();
    private ConcurrentList<DeviceVariableRunTime> _deviceVariables = new();
    /// <inheritdoc/>
    public override System.Type DriverDebugUIType => typeof(OPCDAClientDebugPage);

    /// <inheritdoc/>
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    /// <inheritdoc/>
    public override bool IsSupportRequest => !driverPropertys.ActiveSubscribe;
    /// <inheritdoc/>
    public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);

    /// <inheritdoc/>
    protected override IReadWrite PLC => null;

    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        _plc?.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        _plc.Connect();
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
    }
    /// <inheritdoc/>
    public override bool IsConnected()
    {
        Device.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
        return _plc?.IsConnected == true;
    }

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = new(deviceVariables);
        if (deviceVariables.Count > 0)
        {
            var result = _plc.AddItemsWithSave(deviceVariables.Select(a => a.VariableAddress).ToList());
            var sourVars = result?.Select(
      it =>
      {
          return new DeviceVariableSourceRead()
          {
              TimerTick = new(driverPropertys.UpdateRate),
              VariableAddress = it.Key,
              DeviceVariableRunTimes = new(deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.VariableAddress)).ToList())
          };
      }).ToList();
            return sourVars;
        }
        else
        {
            return new();
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        try
        {
            _plc.ReadItemsWithGroup(deviceVariableSourceRead.VariableAddress);
            return OperResult.CreateSuccessResult(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>($"ReadSourceAsync Error：{Environment.NewLine}{ex}");
        }
    }


    /// <inheritdoc/>
    public override Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<DeviceVariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        var result = _plc.WriteItem(writeInfoLists.ToDictionary(a => a.Key.VariableAddress, a => a.Value.GetObjFromJToken()));
        return Task.FromResult(result.ToDictionary(a =>
        {
            return writeInfoLists.Keys.FirstOrDefault(b => b.VariableAddress == a.Key).Name;
        }, a =>
        {
            if (a.Value.Item1)
                return new OperResult(a.Value.Item2);
            else
                return OperResult.CreateSuccessResult();
        }
      ));
    }


    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
            _plc.DataChangedHandler -= DataChangedHandler;
        _plc?.Disconnect();
        _plc?.SafeDispose();
        _plc = null;
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        Device = device;
        OPCNode opcNode = new()
        {
            OPCIP = driverPropertys.OPCIP,
            OPCName = driverPropertys.OPCName,
            UpdateRate = driverPropertys.UpdateRate,
            DeadBand = driverPropertys.DeadBand,
            GroupSize = driverPropertys.GroupSize,
            ActiveSubscribe = driverPropertys.ActiveSubscribe,
            CheckRate = driverPropertys.CheckRate
        };
        if (_plc == null)
        {
            _plc = new((arg1, arg2, arg3, arg4) => Log_Out((LogLevel)arg1, arg2, arg3, arg4));
            _plc.DataChangedHandler += DataChangedHandler;
        }
        _plc.Init(opcNode);
    }

    /// <inheritdoc/>
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        //不走ReadAsync
        throw new NotImplementedException();
    }
    private void DataChangedHandler(List<ItemReadResult> values)
    {
        try
        {
            if (!Device.KeepRun)
            {
                return;
            }
            LogMessage.Trace($"{FoundationConst.LogMessageHeader}{ToString()}状态变化:{Environment.NewLine} {values?.ToJsonString()}");

            foreach (var data in values)
            {
                if (!Device.KeepRun)
                {
                    return;
                }
                var type = data.Value.GetType();
                if (data.Value is Array)
                {
                    type = type.GetElementType();
                }
                var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.Name).ToList();
                foreach (var item in itemReads)
                {
                    var value = data.Value;
                    var quality = data.Quality;
                    var time = data.TimeStamp.ToLocalTime();
                    if (value != null && quality == 192)
                    {
                        if (item.DataTypeEnum == DataTypeEnum.Object)
                            if (type.Namespace.StartsWith("System"))
                            {
                                var enumValues = Enum.GetValues<DataTypeEnum>();
                                var stringList = enumValues.Select(e => e.ToString()).ToList();
                                if (stringList.Contains(type.Name))
                                    try { item.DataTypeEnum = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                            }

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
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                    else
                    {
                        var operResult = item.SetValue(null, time, quality == 192);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                }
            }
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }
    }
}
