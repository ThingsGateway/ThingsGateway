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

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Demo;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSerialServer : UploadBase
{
    private readonly ModbusSerialServerProperty _driverPropertys = new();

    private readonly ModbusTcpServerVariableProperty _variablePropertys = new();

    private Dictionary<ModbusAddress, DeviceVariableRunTime> _modbusTags;

    private ConcurrentQueue<(string, DeviceVariableRunTime)> _modbusVariableDict = new();

    private ThingsGateway.Foundation.Adapter.Modbus.ModbusSerialServer _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ModbusSerialServerDebugPage);

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    /// <inheritdoc/>
    protected override IReadWrite _readWrite => _plc;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _modbusTags?.Values?.ToList()?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _modbusTags?.Clear();
        _modbusVariableDict?.Clear();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override void Init(ISenderClient client = null)
    {
        var service = new SerialPortClient();
        FoundataionConfig.SetSerialPortOption(new()
        {
            PortName = _driverPropertys.PortName,
            BaudRate = _driverPropertys.BaudRate,
            DataBits = _driverPropertys.DataBits,
            Parity = _driverPropertys.Parity,
            StopBits = _driverPropertys.StopBits,
        })
            ;
        service = new SerialPortClient();
        ((SerialPortClient)service).Setup(FoundataionConfig);
        //载入配置
        _plc = new(service)
        {
            DataFormat = _driverPropertys.DataFormat,
            Station = _driverPropertys.Station,
            CacheTimeout = _driverPropertys.CacheTimeout,
            WriteMemory = _driverPropertys.WriteMemory,
            MulStation = _driverPropertys.MulStation
        };

        var tags = CurrentDevice.DeviceVariableRunTimes
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
            ModbusAddress address = ModbusAddress.ParseFrom(
                a.GetPropertyValue(DeviceId,
                nameof(_variablePropertys.ServiceAddress)).Value, _driverPropertys.Station);
            return address;
        });
        _plc.OnWriteData += OnWriteData;

        _plc.Connect(CancellationToken.None);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        //获取设备连接状态
        if (IsConnected())
        {
            //更新设备活动时间
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
        else
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
        }
        var list = _modbusVariableDict.ToListWithDequeue();
        foreach (var item in list)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var type = item.Item2.GetPropertyValue(CurrentDevice.Id, nameof(ModbusTcpServerVariableProperty.ModbusType)).Value;
            if (Enum.TryParse(type, out DataTypeEnum result))
            {
                await _plc.WriteAsync(item.Item1, item.Item2.Value?.ToString(), 1, result, cancellationToken);
            }
            else
            {
                await _plc.WriteAsync(item.Item1, item.Item2.Value?.ToString(), 1, item.Item2.DataTypeEnum, cancellationToken);
            }
        }

        await Delay(_driverPropertys.CycleInterval, cancellationToken);
    }

    /// <summary>
    /// RPC写入
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <param name="thingsGatewayBitConverter"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    private async Task<OperResult> OnWriteData(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter thingsGatewayBitConverter, ISenderClient client)
    {
        try
        {
            var tag = _modbusTags.FirstOrDefault(a => a.Key?.AddressStart == address.AddressStart && a.Key?.Station == address.Station && a.Key?.ReadFunction == address.ReadFunction);
            if (tag.Value == null) return OperResult.CreateSuccessResult();
            var enable = tag.Value.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable)).Value.ToBool(false) && _driverPropertys.DeviceRpcEnable;
            if (!enable) return new OperResult("不允许写入");
            var type = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusTcpServerVariableProperty.ModbusType)).Value;
            var addressStr = tag.Value.GetPropertyValue(DeviceId, nameof(ModbusTcpServerVariableProperty.ServiceAddress)).Value;

            var bitConverter = ByteTransformUtil.GetTransByAddress(ref addressStr, thingsGatewayBitConverter);
            if (Enum.TryParse(type, out DataTypeEnum result))
            {
                var result1 = await RpcSingletonService.InvokeDeviceMethodAsync($"{nameof(ModbusTcpServer)}-{CurrentDevice.Name}-{$"{client}"}",
               new Dictionary<string, string>
    {
    {
                       tag.Value.Name,
                       bitConverter.GetDataFormBytes(   bytes,result).ToString()
                   },
    }

                );
                return result1.FirstOrDefault().Value;
            }
            else
            {
                var result1 = await RpcSingletonService.InvokeDeviceMethodAsync($"{nameof(ModbusTcpServer)}-{CurrentDevice.Name}-{$"{client}"}",
               new Dictionary<string, string>
    {
    {
                       tag.Value.Name,
                       bitConverter.GetDataFormBytes(   bytes,result).ToString()
                   },
    }
                );
                return result1.FirstOrDefault().Value;
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
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