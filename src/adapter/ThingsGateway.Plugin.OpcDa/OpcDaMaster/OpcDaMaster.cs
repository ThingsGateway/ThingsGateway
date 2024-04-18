//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Threading;

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.OpcDa;
using ThingsGateway.Foundation.OpcDa.Da;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcDa;

/// <summary>
/// <inheritdoc/>
/// </summary>
[OnlyWindowsSupport]
public class OpcDaMaster : CollectBase
{
    private readonly OpcDaMasterProperty _driverProperties = new();

    private ThingsGateway.Foundation.OpcDa.OpcDaMaster _plc;

    private CancellationToken _token;

    private volatile bool success = true;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.OpcDaMaster);

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverProperties;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => null;

    public override string ToString()
    {
        return $"{_driverProperties.OpcIP}-{_driverProperties.OpcName}";
    }

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        //载入配置
        OpcDaProperty opcNode = new()
        {
            OpcIP = _driverProperties.OpcIP,
            OpcName = _driverProperties.OpcName,
            UpdateRate = _driverProperties.UpdateRate,
            DeadBand = _driverProperties.DeadBand,
            GroupSize = _driverProperties.GroupSize,
            ActiveSubscribe = _driverProperties.ActiveSubscribe,
            CheckRate = _driverProperties.CheckRate
        };
        if (_plc == null)
        {
            _plc = new();
            _plc.DataChangedHandler += DataChangedHandler;
            _plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        }
        _plc.Init(opcNode);

    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.IsConnected == true;

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _plc.Connect();
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override string GetAddressDescription()
    {
        return _plc?.GetAddressDescription();
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        if (deviceVariables.Count > 0)
        {
            var result = _plc.AddItemsWithSave(deviceVariables.Where(a => !a.RegisterAddress.IsNullOrEmpty()).Select(a => a.RegisterAddress!).ToList());
            var sourVars = result?.Select(
      it =>
      {
          var read = new VariableSourceRead()
          {
              TimeTick = new(_driverProperties.UpdateRate),
              RegisterAddress = it.Key,
          };
          var variables = deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.RegisterAddress));
          foreach (var v in variables)
          {
              read.AddVariable(v);
          }
          return read;
      }).ToList();
            return sourVars;
        }
        else
        {
            return new();
        }
    }

    /// <inheritdoc/>
    protected override async Task<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        // 如果是单线程模式，并且有其他线程正在等待写入锁
        if (IsSingleThread && WriteLock.IsWaitting)
        {
            // 等待写入锁释放
            await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            // 立即释放写入锁，允许其他线程继续执行写入操作
            WriteLock.Release();
        }
        try
        {
            _plc.ReadItemsWithGroup(deviceVariableSourceRead.RegisterAddress);
            return OperResult.CreateSuccessResult(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>($"ReadSourceAsync Error：{Environment.NewLine}{ex}");
        }
    }

    /// <inheritdoc/>
    protected override async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);
            var result = _plc.WriteItem(writeInfoLists.ToDictionary(a => a.Key.RegisterAddress!, a => a.Value.GetObjectFromJToken()!));
            return result.ToDictionary(a =>
            {
                return writeInfoLists.Keys.FirstOrDefault(b => b.RegisterAddress == a.Key).Name;
            }, a =>
            {
                if (!a.Value.Item1)
                    return new OperResult(a.Value.Item2);
                else
                    return new();
            }
                 );
        }
        finally
        {
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverProperties.ActiveSubscribe)
        {
            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间
                CurrentDevice.SetDeviceStatus(TimerX.Now, 0);
            }
            else
            {
                CurrentDevice.SetDeviceStatus(TimerX.Now, 999);
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
        _plc?.SafeDispose();
        base.Dispose(disposing);
    }

    private void DataChangedHandler(List<ItemReadResult> values)
    {
        try
        {
            if (!CurrentDevice.KeepRun)
                return;
            if (_token.IsCancellationRequested)
                return;
            LogMessage.Trace($"{ToString()} Change:{Environment.NewLine} {values?.ToJsonString()}");

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
                var itemReads = CurrentDevice.VariableRunTimes.Values.Where(it => it.RegisterAddress == data.Name);
                foreach (var item in itemReads)
                {
                    if (!CurrentDevice.KeepRun)
                        return;
                    if (_token.IsCancellationRequested)
                        return;
                    var value = data.Value;
                    var quality = data.Quality;
                    DateTime time = default;
                    if (_driverProperties.SourceTimestampEnable)
                    {
                        time = data.TimeStamp.ToLocalTime();
                    }
                    if (quality == 192)
                    {
                        if (item.DataType == DataTypeEnum.Object)
                            if (type.Namespace.StartsWith("System"))
                            {
                                var enumValues = Enum.GetValues<DataTypeEnum>();
                                var stringList = enumValues.Select(e => e.ToString());
                                if (stringList.Contains(type.Name))
                                    try { item.DataType = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
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
                        item.SetValue(null, time, false);
                        item.VariableSource.LastErrorMessage = $"Bad quality：{quality}";
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
