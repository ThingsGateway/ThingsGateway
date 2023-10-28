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

using Opc.Ua;

using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Plugin.OPCUA;

/// <summary>
/// OPCUA客户端
/// </summary>
public class OPCUAClient : CollectBase
{
    internal Foundation.Adapter.OPCUA.OPCUAClient _plc = null;

    private readonly OPCUAClientProperty driverPropertys = new();

    private List<DeviceVariableRunTime> _deviceVariables = new();


    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(OPCUAClientDebugPage);

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
        await _plc?.ConnectAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        if (_plc.Session != null)
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
        }
        return _plc?.Connected == true;
    }

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = deviceVariables;
        _plc.Variables.AddRange(deviceVariables.Select(a => a.VariableAddress).ToList());
        var dataLists = deviceVariables.ChunkTrivialBetter(driverPropertys.GroupSize);
        var dataResult = new List<DeviceVariableSourceRead>();
        foreach (var variable in dataLists)
        {
            var sourVars = new DeviceVariableSourceRead()
            {
                TimerTick = new(driverPropertys.UpdateRate),
                VariableAddress = "",
                DeviceVariableRunTimes = new(variable)
            };
            dataResult.Add(sourVars);
        }

        return dataResult;

    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        var result = await _plc.ReadJTokenValueAsync(deviceVariableSourceRead.DeviceVariableRunTimes.Select(a => a.VariableAddress).ToArray(), cancellationToken);
        foreach (var data in result)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var data1 = deviceVariableSourceRead.DeviceVariableRunTimes.Where(a => a.VariableAddress == data.Item1);

                foreach (var item in data1)
                {

                    object value;
                    if (data.Item3 is JValue jValue)
                    {
                        value = jValue.Value;
                    }
                    else
                    {
                        value = data.Item3;
                    }
                    var isGood = StatusCode.IsGood(data.Item2.StatusCode);

                    var time = data.Item2.SourceTimestamp;
                    if (value != null && isGood)
                    {
                        var operResult = item.SetValue(value, time);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                    else
                    {
                        var operResult = item.SetValue(null, time, isGood);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                }
                LogMessage.Trace($"{FoundationConst.LogMessageHeader} {ToString()} 状态变化:{Environment.NewLine}{data.Item1} : {data.Item3}");

            }
        }
        if (result.Any(a => StatusCode.IsBad(a.Item2.StatusCode)))
        {
            return new OperResult<byte[]>($"读取失败");
        }
        else
        {
            return OperResult.CreateSuccessResult<byte[]>(null);
        }
    }


    /// <inheritdoc/>
    public override async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<DeviceVariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        var result = await _plc.WriteNodeAsync(writeInfoLists.ToDictionary(a => a.Key.VariableAddress, a => a.Value), cancellationToken);
        return result.ToDictionary(a =>
        {
            return writeInfoLists.Keys.FirstOrDefault(b => b.VariableAddress == a.Key)?.Name;
        }
        , a =>
        {
            if (a.Value.Item1)
                return new OperResult(a.Value.Item2);
            else
                return OperResult.CreateSuccessResult();
        });

    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
        {
            _plc.DataChangedHandler -= DataChangedHandler;
            _plc.OpcStatusChange -= _plc_OpcStatusChange;

            _plc.Disconnect();
            _plc.SafeDispose();
            _plc = null;
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        OPCNode opcNode = new()
        {
            OPCUrl = driverPropertys.OPCURL,
            UpdateRate = driverPropertys.UpdateRate,
            DeadBand = driverPropertys.DeadBand,
            GroupSize = driverPropertys.GroupSize,
            KeepAliveInterval = driverPropertys.KeepAliveInterval,
            IsUseSecurity = driverPropertys.IsUseSecurity,
            ActiveSubscribe = driverPropertys.ActiveSubscribe,
            UserName = driverPropertys.UserName,
            Password = driverPropertys.Password,
            CheckDomain = driverPropertys.CheckDomain,
            LoadType = driverPropertys.LoadType,
        };
        if (_plc == null)
        {
            _plc = new();
            _plc.OpcStatusChange += _plc_OpcStatusChange;
            _plc.DataChangedHandler += DataChangedHandler;
        }

        _plc.OPCNode = opcNode;
    }

    private void _plc_OpcStatusChange(object sender, OpcUaStatusEventArgs e)
    {
        Log_Out((LogLevel)e.LogLevel, null, e.Text, null);
    }

    /// <inheritdoc/>
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        //不走ReadAsync
        throw new NotImplementedException();
    }

    private void DataChangedHandler((VariableNode variableNode, DataValue dataValue, JToken jToken) data)
    {
        try
        {
            if (!CurrentDevice.KeepRun)
            {
                return;
            }

            LogMessage.Trace($"{FoundationConst.LogMessageHeader}{ToString()} 状态变化: {Environment.NewLine} {data.variableNode.NodeId} : {data.jToken?.ToString()}");

            if (!CurrentDevice.KeepRun)
            {
                return;
            }
            //尝试固定点位的数据类型
            var type = TypeInfo.GetSystemType(TypeInfo.GetBuiltInType(data.variableNode.DataType, _plc.Session.SystemContext.TypeTable), data.variableNode.ValueRank);

            var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.variableNode.NodeId).ToList();

            object value;
            if (data.jToken is JValue jValue)
            {
                value = jValue.Value;
            }
            else
            {
                value = data.jToken;
            }
            var isGood = StatusCode.IsGood(data.dataValue.StatusCode);
            DateTime time = default;
            if (driverPropertys.SourceTimestampEnable)
            {
                time = data.dataValue.SourceTimestamp.ToLocalTime();
            }
            foreach (var item in itemReads)
            {
                if (item.DataTypeEnum == DataTypeEnum.Object)
                    if (type.Namespace.StartsWith("System"))
                    {
                        var enumValues = Enum.GetValues<DataTypeEnum>();
                        var stringList = enumValues.Select(e => e.ToString()).ToList();
                        if (stringList.Contains(type.Name))
                            try { item.DataTypeEnum = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                    }
                if (value != null && isGood)
                {
                    var operResult = item.SetValue(value, time);
                    if (!operResult.IsSuccess)
                    {
                        LogMessage?.LogWarning(operResult.Message);
                    }
                }
                else
                {
                    var operResult = item.SetValue(null, time, isGood);
                    if (!operResult.IsSuccess)
                    {
                        LogMessage?.LogWarning(operResult.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

}
