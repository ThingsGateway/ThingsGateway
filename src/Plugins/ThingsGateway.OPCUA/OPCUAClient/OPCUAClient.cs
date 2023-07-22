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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Foundation.Extension.Enumerator;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA;

/// <summary>
/// OPCUA客户端
/// </summary>
public class OPCUAClient : CollectBase
{
    internal CollectDeviceRunTime Device;

    internal Foundation.Adapter.OPCUA.OPCUAClient PLC = null;

    private List<DeviceVariableRunTime> _deviceVariables = new();

    private OPCUAClientProperty driverPropertys = new();

    /// <inheritdoc cref="OPCUAClient"/>
    public OPCUAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(OPCUAClientDebugDriverPage);

    /// <inheritdoc/>
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    /// <inheritdoc/>
    public override bool IsSupportRequest => !driverPropertys.ActiveSubscribe;

    /// <inheritdoc/>
    public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        PLC?.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        await PLC?.ConnectAsync();
    }
    public override void InitDataAdapter()
    {
    }

    /// <inheritdoc/>
    public override OperResult IsConnected()
    {
        return PLC.Connected ? OperResult.CreateSuccessResult() : new OperResult();
    }
    /// <inheritdoc/>
    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = deviceVariables;
        PLC.Variables.AddRange(deviceVariables.Select(a => a.VariableAddress).ToList());
        var dataLists = deviceVariables.ChunkTrivialBetter(driverPropertys.GroupSize);
        var dataResult = new List<DeviceVariableSourceRead>();
        foreach (var variable in dataLists)
        {
            var sourVars = new DeviceVariableSourceRead(driverPropertys.UpdateRate)
            {
                Address = "",
                DeviceVariables = variable
            };
            dataResult.Add(sourVars);
        }

        return OperResult.CreateSuccessResult(dataResult);

    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var result = await PLC.ReadJTokenValueAsync(deviceVariableSourceRead.DeviceVariables.Select(a => a.VariableAddress).ToArray(), cancellationToken);
        foreach (var data in result)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var data1 = deviceVariableSourceRead.DeviceVariables.Where(a => a.VariableAddress == data.Item1);

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
                    var quality = StatusCode.IsGood(data.Item2.StatusCode);

                    var time = data.Item2.SourceTimestamp;
                    if (value != null && quality)
                    {
                        var operResult = item.SetValue(value, time);
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
                logMessage.Trace(LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + data.Item1 + ":" + data.Item3.ToString());

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
    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        var result = await PLC.WriteNodeAsync(deviceVariable.VariableAddress, JToken.Parse(value));
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (PLC != null)
        {
            PLC.DataChangedHandler -= dataChangedHandler;
            PLC.OpcStatusChange -= opcStatusChange;
            PLC.Disconnect();
            PLC.SafeDispose();
            PLC = null;
        }
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        Device = device;
        OPCNode oPCNode = new();
        oPCNode.OPCURL = driverPropertys.OPCURL;
        oPCNode.UpdateRate = driverPropertys.UpdateRate;
        oPCNode.DeadBand = driverPropertys.DeadBand;
        oPCNode.GroupSize = driverPropertys.GroupSize;
        oPCNode.KeepAliveInterval = driverPropertys.KeepAliveInterval;
        oPCNode.IsUseSecurity = driverPropertys.IsUseSecurity;
        oPCNode.ActiveSubscribe = driverPropertys.ActiveSubscribe;

        if (PLC == null)
        {
            PLC = new();
            PLC.DataChangedHandler += dataChangedHandler;
            PLC.OpcStatusChange += opcStatusChange; ;
        }
        if (!driverPropertys.UserName.IsNullOrEmpty())
        {
            PLC.UserIdentity = new UserIdentity(driverPropertys.UserName, driverPropertys.Password);
        }
        else
        {
            PLC.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
        }
        PLC.OPCNode = oPCNode;
    }

    /// <inheritdoc/>
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        //不走ReadAsync
        throw new NotImplementedException();
    }

    private void dataChangedHandler((NodeId id, DataValue dataValue, JToken jToken) data)
    {
        try
        {
            if (!Device.KeepRun)
            {
                return;
            }

            logMessage.Trace(LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + data.id + ":" + data.jToken.ToString());

            {
                if (!Device.KeepRun)
                {
                    return;
                }

                var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.id).ToList();
                foreach (var item in itemReads)
                {
                    object value;
                    if (data.jToken is JValue jValue)
                    {
                        value = jValue.Value;
                    }
                    else
                    {
                        value = data.jToken;
                    }
                    var quality = StatusCode.IsGood(data.dataValue.StatusCode);

                    var time = data.dataValue.SourceTimestamp;
                    if (value != null && quality)
                    {
                        var operResult = item.SetValue(value, time);
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
    private void opcStatusChange(object sender, OPCUAStatusEventArgs e)
    {
        if (e.Error)
        {
            _logger.LogWarning(e.Text);
            Device.ErrorCount = 999;
            Device.LastErrorMessage = $"{e.Text}";
        }
        else
        {
            Device.ErrorCount = 0;
            _logger.LogTrace(e.Text);
        }
    }
}
