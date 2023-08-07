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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Admin.Core;
using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Extension;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA;
/// <summary>
/// OPCDAClient
/// </summary>
public class OPCDAClient : CollectBase
{
    internal CollectDeviceRunTime Device;

    internal ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient PLC = null;

    private readonly OPCDAClientProperty driverPropertys = new();
    private ConcurrentList<DeviceVariableRunTime> _deviceVariables = new();
    /// <inheritdoc/>
    public override System.Type DriverDebugUIType => typeof(OPCDAClientDebugDriverPage);

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
    public override async Task BeforStartAsync(CancellationToken token)
    {
        PLC.Connect();
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
    }
    /// <inheritdoc/>
    public override bool IsConnected() => PLC?.IsConnected == true;
    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = new(deviceVariables);
        if (deviceVariables.Count > 0)
        {
            var result = PLC.AddItemsWithSave(deviceVariables.Select(a => a.VariableAddress).ToList());
            var sourVars = result?.Select(
      it =>
      {
          return new DeviceVariableSourceRead(driverPropertys.UpdateRate)
          {
              Address = it.Key,
              DeviceVariables = deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.VariableAddress)).ToList()
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
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken token)
    {
        await Task.CompletedTask;
        var result = PLC.ReadItemsWithGroup(deviceVariableSourceRead.Address);
        return result.Copy<byte[]>();
    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, JToken value, CancellationToken token)
    {
        var result = PLC.WriteItem(deviceVariable.VariableAddress, value);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (PLC != null)
            PLC.DataChangedHandler -= DataChangedHandler;
        PLC?.Disconnect();
        PLC?.SafeDispose();
        PLC = null;
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
        if (PLC == null)
        {
            PLC = new(LogMessage);
            PLC.DataChangedHandler += DataChangedHandler;
        }
        PLC.Init(opcNode);
    }

    /// <inheritdoc/>
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token)
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
            LogMessage.Trace(FoundationConst.LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + values?.ToJson());

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
                    var time = new DateTimeOffset(data.TimeStamp);
                    if (value != null && quality == 192)
                    {
                        if (item.DataTypeEnum == DataTypeEnum.Object)
                            if (type.Namespace.StartsWith("System"))
                                try { item.DataTypeEnum = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
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
                        var operResult = item.SetValue(null, time);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                }
            }
            CurDevice.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime, 0);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }
    }
}
