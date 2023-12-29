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
using ThingsGateway.Foundation.Demo;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Plugin.OPCUA;

/// <summary>
/// OPCUA客户端
/// </summary>
public class OPCUAClient : CollectBase
{
    internal ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient _plc = null;

    private readonly OPCUAClientProperty _driverPropertys = new();

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(OPCUAClientDebugPage);

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => null;

    public override Type DriverUIType => null;
    private CancellationToken _token;

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        await _plc?.ConnectAsync(cancellationToken);
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        _plc?.Disconnect();
        return base.AfterStopAsync();
    }

    protected override string GetAddressDescription()
    {
        return "OPCUA ItemName";
    }

    public override bool IsConnected() => _plc?.Connected == true;

    protected override List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        var dataLists = deviceVariables.ChunkBetter(_driverPropertys.GroupSize);
        _plc.Variables = new();
        _plc.Variables.AddRange(dataLists.Select(a => a.Select(a => a.Address).ToList()).ToList());
        var dataResult = new List<DeviceVariableSourceRead>();
        foreach (var variable in dataLists)
        {
            var sourVars = new DeviceVariableSourceRead()
            {
                IntervalTimeTick = new(_driverPropertys.UpdateRate),
                Address = "",
                DeviceVariableRunTimes = new(variable)
            };
            dataResult.Add(sourVars);
        }

        return dataResult;
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        var result = await _plc.ReadJTokenValueAsync(deviceVariableSourceRead.DeviceVariableRunTimes.Select(a => a.Address).ToArray(), cancellationToken);
        foreach (var data in result)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var data1 = deviceVariableSourceRead.DeviceVariableRunTimes.Where(a => a.Address == data.Item1);

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
                    if (isGood)
                    {
                        item.SetValue(value, time);
                    }
                    else
                    {
                        item.SetValue(null, time, data.Item2.StatusCode.ToString());
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
        var result = await _plc.WriteNodeAsync(writeInfoLists.ToDictionary(a => a.Key.Address, a => a.Value), cancellationToken);
        return result.ToDictionary(a =>
        {
            return writeInfoLists.Keys.FirstOrDefault(b => b.Address == a.Key)?.Name;
        }
        , a =>
        {
            if (a.Value.Item1)
                return new OperResult(a.Value.Item2);
            else
                return OperResult.CreateSuccessResult();
        });
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
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
        {
            _plc.DataChangedHandler -= DataChangedHandler;
            _plc.OpcStatusChange -= _plc_OpcStatusChange;

            _plc.Disconnect();
            _plc.SafeDispose();
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override void Init(ISenderClient client = null)
    {
        OPCUANode opcNode = new()
        {
            OPCUrl = _driverPropertys.OPCURL,
            UpdateRate = _driverPropertys.UpdateRate,
            DeadBand = _driverPropertys.DeadBand,
            GroupSize = _driverPropertys.GroupSize,
            KeepAliveInterval = _driverPropertys.KeepAliveInterval,
            IsUseSecurity = _driverPropertys.IsUseSecurity,
            ActiveSubscribe = _driverPropertys.ActiveSubscribe,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            CheckDomain = _driverPropertys.CheckDomain,
            LoadType = _driverPropertys.LoadType,
        };
        if (_plc == null)
        {
            _plc = new();
            _plc.OpcStatusChange += _plc_OpcStatusChange;
            _plc.DataChangedHandler += DataChangedHandler;
        }

        _plc.OPCNode = opcNode;
        base.Init();
    }

    private void _plc_OpcStatusChange(object sender, OpcUaStatusEventArgs e)
    {
        Log_Out((LogLevel)e.LogLevel, null, e.Text, null);
    }

    private volatile bool success = true;

    private void DataChangedHandler((VariableNode variableNode, DataValue dataValue, JToken jToken) data)
    {
        try
        {
            if (!CurrentDevice.KeepRun)
                return;
            if (_token.IsCancellationRequested)
                return;

            LogMessage.Trace($"{FoundationConst.LogMessageHeader}{ToString()} 状态变化: {Environment.NewLine} {data.variableNode.NodeId} : {data.jToken?.ToString()}");

            if (!CurrentDevice.KeepRun)
            {
                return;
            }
            //尝试固定点位的数据类型
            var type = TypeInfo.GetSystemType(TypeInfo.GetBuiltInType(data.variableNode.DataType, _plc.Session.SystemContext.TypeTable), data.variableNode.ValueRank);

            var itemReads = CurrentDevice.DeviceVariableRunTimes.Where(it => it.Address == data.variableNode.NodeId).ToList();

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
            if (_driverPropertys.SourceTimestampEnable)
            {
                time = data.dataValue.SourceTimestamp.ToLocalTime();
            }
            foreach (var item in itemReads)
            {
                if (!CurrentDevice.KeepRun)
                    return;
                if (_token.IsCancellationRequested)
                    return;
                if (item.DataTypeEnum == DataTypeEnum.Object)
                    if (type.Namespace.StartsWith("System"))
                    {
                        var enumValues = Enum.GetValues<DataTypeEnum>();
                        var stringList = enumValues.Select(e => e.ToString()).ToList();
                        if (stringList.Contains(type.Name))
                            try { item.DataTypeEnum = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                    }
                if (isGood)
                {
                    item.SetValue(value, time);
                }
                else
                {
                    item.SetValue(null, time, data.Item2.StatusCode.ToString());
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