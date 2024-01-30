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

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSlave : BusinessBase
{
    private readonly ModbusSlaveProperty _driverPropertys = new();
    private readonly ModbusSlaveVariableProperty _variablePropertys = new();
    private Dictionary<ModbusAddress, VariableRunTime> _modbusTags;
    private readonly ConcurrentQueue<(string, VariableRunTime)> _modbusVariableDict = new();

    private ThingsGateway.Foundation.Modbus.ModbusSlave _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Demo.ModbusSlave);

    /// <inheritdoc/>
    protected override BusinessPropertyBase _businessPropertyBase => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    /// <inheritdoc/>
    protected override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _modbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _modbusTags?.Clear();
        _modbusVariableDict?.Clear();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);
        ArgumentNullException.ThrowIfNull(channel, "通道配置不能为null");
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            IsStringReverseByteWord = _driverPropertys.IsStringReverseByteWord,
            CacheTimeout = _driverPropertys.CacheTimeout,
            Station = _driverPropertys.Station,
            IsWriteMemory = _driverPropertys.IsWriteMemory,
            CheckClear = _driverPropertys.CheckClear,
            MulStation = _driverPropertys.MulStation,
            ModbusType = _driverPropertys.ModbusType,
            MaxClientCount = _driverPropertys.MaxClientCount,
        };

        var tags = CurrentDevice.VariableRunTimes
            .Where(b => !string.IsNullOrEmpty(
                b.GetPropertyValue(DeviceId, nameof(_variablePropertys.ServiceAddress))?.Value))
            .ToList();

        tags.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
            VariableValueChange(a);
        });

        _modbusTags = tags.ToDictionary(a =>
        {
            ModbusAddress address = ModbusAddressHelper.ParseFrom(
                a.GetPropertyValue(DeviceId,
                nameof(_variablePropertys.ServiceAddress)).Value, _driverPropertys.Station);
            return address;
        });
        _plc.WriteData += OnWriteData;
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        //获取设备连接状态
        if (IsConnected())
        {
            //更新设备活动时间
            CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, 0);
        }
        else
        {
            CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, 999);
        }
        var list = _modbusVariableDict.ToListWithDequeue();
        foreach (var item in list)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var type = item.Item2.GetPropertyValue(CurrentDevice.Id, nameof(ModbusSlaveVariableProperty.ModbusType)).Value;
            if (Enum.TryParse(type, out DataTypeEnum result))
            {
                await _plc.WriteAsync(item.Item1, JToken.FromObject(item.Item2.Value), result, cancellationToken);
            }
            else
            {
                await _plc.WriteAsync(item.Item1, JToken.FromObject(item.Item2.Value), item.Item2.DataType, cancellationToken);
            }
        }

        await Delay(cancellationToken);
    }

    /// <summary>
    /// RPC写入
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <param name="thingsGatewayBitConverter"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    private async Task<OperResult> OnWriteData(ModbusAddress modbusAddress, byte[] writeValue, IThingsGatewayBitConverter bitConverter, IClientChannel channel)
    {
        try
        {
            var tag = _modbusTags.FirstOrDefault(a => a.Key?.AddressStart == modbusAddress.AddressStart && a.Key?.Station == modbusAddress.Station && a.Key?.ReadFunction == modbusAddress.ReadFunction);
            if (tag.Value == null) return new();
            var enable = tag.Value.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable)).Value.ToBool(false) && _driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult("不允许写入");
            var type = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.ModbusType)).Value;
            var addressStr = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.ServiceAddress)).Value;

            var thingsGatewayBitConverter = ByteTransUtil.GetTransByAddress(ref addressStr, bitConverter);

            var result = await tag.Value.SetValueToDeviceAsync(thingsGatewayBitConverter.GetDataFormBytes(writeValue, Enum.TryParse(type, out DataTypeEnum dataType) ? dataType : tag.Value.DataType).ToString(),
                    $"{nameof(ModbusSlave)}-{CurrentDevice.Name}-{$"{channel}"}");
            return result;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    private void VariableValueChange(VariableRunTime collectVariableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        var address = collectVariableRunTime.GetPropertyValue(DeviceId, nameof(_variablePropertys.ServiceAddress)).Value;
        if (address != null && collectVariableRunTime.Value != null)
        {
            _modbusVariableDict?.Enqueue((address, collectVariableRunTime));
        }
    }
}