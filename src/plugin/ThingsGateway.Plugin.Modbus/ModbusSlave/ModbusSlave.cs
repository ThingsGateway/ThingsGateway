//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;

using Newtonsoft.Json.Linq;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.X;
using ThingsGateway.NewLife.X.Threading;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Plugin.Modbus;

public class ModbusSlave : BusinessBase
{
    private readonly ModbusSlaveProperty _driverPropertys = new();
    private readonly ConcurrentQueue<(string, VariableRunTime)> _modbusVariableDict = new();
    private readonly ModbusSlaveVariableProperty _variablePropertys = new();
    private Dictionary<ModbusAddress, VariableRunTime> _modbusTags;
    private ThingsGateway.Foundation.Modbus.ModbusSlave _plc;

    private volatile bool success = true;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.ModbusSlave);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (Protocol.Channel.ChannelType == ChannelTypeEnum.TcpService)
                return typeof(ThingsGateway.Gateway.Razor.TcpServicePage);
            else
                return null;
        }
    }

    /// <inheritdoc/>
    public override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    /// <inheritdoc/>
    protected override BusinessPropertyBase _businessPropertyBase => _driverPropertys;

    protected IStringLocalizer Localizer { get; private set; }

    /// <inheritdoc/>
    protected override void Init(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            IsStringReverseByteWord = _driverPropertys.IsStringReverseByteWord,
            CacheTimeout = _driverPropertys.CacheTimeout,
            Station = _driverPropertys.Station,
            IsWriteMemory = _driverPropertys.IsWriteMemory,
            CheckClearTime = _driverPropertys.CheckClearTime,
            MulStation = _driverPropertys.MulStation,
            ModbusType = _driverPropertys.ModbusType,
            MaxClientCount = _driverPropertys.MaxClientCount,
            DtuId = _driverPropertys.DtuId,
            HeartbeatTime = _driverPropertys.HeartbeatTime,
            HeartbeatHexString = _driverPropertys.HeartbeatHexString,
        };

        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.VariableValueChangeEvent += VariableValueChange;
        CurrentDevice.VariableRunTimes.ForEach(a =>
        {
            VariableValueChange(a.Value, null);
        });

        _modbusTags = CurrentDevice.VariableRunTimes.ToDictionary(a =>
        {
            ModbusAddress address = ModbusAddress.ParseFrom(
                a.Value.GetPropertyValue(DeviceId,
                nameof(_variablePropertys.ServiceAddress)), _driverPropertys.Station, isCache: false);
            return address!;
        },
        a => a.Value
        );
        _plc.WriteData += OnWriteData;
        Localizer = NetCoreApp.CreateLocalizerByType(typeof(ModbusSlave))!;

        _plc.ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _modbusTags?.Clear();
        _modbusVariableDict?.Clear();
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        base.Dispose(disposing);
    }

    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
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
            try
            {
                await Protocol.Channel.CloseAsync().ConfigureAwait(false);
                await Protocol.Channel.ConnectAsync(3000, cancellationToken).ConfigureAwait(false);
                success = true;
            }
            catch (Exception ex)
            {
                if (success)
                    LogMessage.LogWarning(ex, Localizer["CanStartService"]);
                success = false;
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }
        }
        var list = _modbusVariableDict.ToListWithDequeue();
        foreach (var item in list)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var type = item.Item2.GetPropertyValue(CurrentDevice.Id, nameof(ModbusSlaveVariableProperty.DataType));
            if (Enum.TryParse(type, out DataTypeEnum result))
            {
                await _plc.WriteAsync(item.Item1, JToken.FromObject(item.Item2.Value), result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _plc.WriteAsync(item.Item1, JToken.FromObject(item.Item2.Value), item.Item2.DataType, cancellationToken).ConfigureAwait(false);
            }
        }

        await Delay(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// RPC写入
    /// </summary>
    private async ValueTask<OperResult> OnWriteData(ModbusRequest modbusRequest, IThingsGatewayBitConverter bitConverter, IClientChannel channel)
    {
        try
        {
            var tag = _modbusTags.FirstOrDefault(a => a.Key?.StartAddress == modbusRequest.StartAddress && a.Key?.Station == modbusRequest.Station && a.Key?.FunctionCode == modbusRequest.FunctionCode);
            if (tag.Value == null) return OperResult.Success;
            var enable = tag.Value.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable)).ToBoolean(false) && _driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult(Localizer["NotWriteEnable"]);
            var type = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.DataType));
            var addressStr = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.ServiceAddress));

            var thingsGatewayBitConverter = bitConverter.GetTransByAddress(ref addressStr);
            var writeData = modbusRequest.Data.ToArray();
            var data = thingsGatewayBitConverter.GetDataFormBytes(_plc, addressStr, writeData, 0, Enum.TryParse(type, out DataTypeEnum dataType) ? dataType : tag.Value.DataType);
            var result = await tag.Value.SetValueToDeviceAsync(data.ToSystemTextJsonString(),
                    $"{nameof(ModbusSlave)}-{CurrentDevice.Name}-{$"{channel}"}").ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    private void VariableValueChange(VariableRunTime variableRunTime, VariableData variableData)
    {
        if (!CurrentDevice.KeepRun)
            return;
        var address = variableRunTime.GetPropertyValue(DeviceId, nameof(_variablePropertys.ServiceAddress));
        if (address != null && variableRunTime.Value != null)
        {
            _modbusVariableDict?.Enqueue((address, variableRunTime));
        }
    }
}
