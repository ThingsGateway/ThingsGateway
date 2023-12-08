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
    private readonly OPCDAClientProperty _driverPropertys = new();

    /// <inheritdoc/>
    public override System.Type DriverDebugUIType => typeof(OPCDAClientDebugPage);

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => null;

    public override Type DriverUIType => null;
    private CancellationToken _token;

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _plc.Connect();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override string GetAddressDescription()
    {
        return "OPCDA ItemName";
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.IsConnected == true;

    protected override List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        if (deviceVariables.Count > 0)
        {
            var result = _plc.AddItemsWithSave(deviceVariables.Select(a => a.Address).ToList());
            var sourVars = result?.Select(
      it =>
      {
          return new DeviceVariableSourceRead()
          {
              IntervalTimeTick = new(_driverPropertys.UpdateRate),
              Address = it.Key,
              DeviceVariableRunTimes = new(deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.Address)).ToList())
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
            _plc.ReadItemsWithGroup(deviceVariableSourceRead.Address);
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
        var result = _plc.WriteItem(writeInfoLists.ToDictionary(a => a.Key.Address, a => a.Value.GetObjFromJToken()));
        return Task.FromResult(result.ToDictionary(a =>
        {
            return writeInfoLists.Keys.FirstOrDefault(b => b.Address == a.Key).Name;
        }, a =>
        {
            if (a.Value.Item1)
                return new OperResult(a.Value.Item2);
            else
                return OperResult.CreateSuccessResult();
        }
      ));
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverPropertys.ActiveSubscribe)
        {
            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
            }
            else
            {
                //if (!IsUploadBase)
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
            }
        }
        else
        {
            await base.ProtectedExecuteAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
            _plc.DataChangedHandler -= DataChangedHandler;
        _plc?.Disconnect();
        _plc?.SafeDispose();
        base.Dispose(disposing);
    }

    protected override void Init(ISenderClient client = null)
    {
        OPCDANode opcNode = new()
        {
            OPCIP = _driverPropertys.OPCIP,
            OPCName = _driverPropertys.OPCName,
            UpdateRate = _driverPropertys.UpdateRate,
            DeadBand = _driverPropertys.DeadBand,
            GroupSize = _driverPropertys.GroupSize,
            ActiveSubscribe = _driverPropertys.ActiveSubscribe,
            CheckRate = _driverPropertys.CheckRate
        };
        if (_plc == null)
        {
            _plc = new((arg1, arg2, arg3, arg4) => Log_Out((LogLevel)arg1, arg2, arg3, arg4));
            _plc.DataChangedHandler += DataChangedHandler;
        }
        _plc.Init(opcNode);

        base.Init();
    }

    private volatile bool success = true;

    private void DataChangedHandler(List<ItemReadResult> values)
    {
        try
        {
            if (!CurrentDevice.KeepRun)
                return;
            if (_token.IsCancellationRequested)
                return;
            LogMessage.Trace($"{FoundationConst.LogMessageHeader}{ToString()}状态变化:{Environment.NewLine} {values?.ToJsonString()}");

            foreach (var data in values)
            {
                if (!CurrentDevice.KeepRun)
                    return;
                if (_token.IsCancellationRequested)
                    return;
                var type = data.Value.GetType();
                if (data.Value is Array)
                {
                    type = type.GetElementType();
                }
                var itemReads = CurrentDevice.DeviceVariableRunTimes.Where(it => it.Address == data.Name).ToList();
                foreach (var item in itemReads)
                {
                    if (!CurrentDevice.KeepRun)
                        return;
                    if (_token.IsCancellationRequested)
                        return;
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
                        item.SetValue(newValue, time);
                    }
                    else
                    {
                        item.SetValue(null, time, $"错误质量戳：{quality}");
                    }
                }
            }

            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }
    }
}