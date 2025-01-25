//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Debug;
using ThingsGateway.Foundation.SiemensS7;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7Master : CollectBase
{
    private readonly SiemensS7MasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.SiemensS7.SiemensS7Master _plc = new();

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.SiemensS7Master);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (FoundationDevice.Channel?.ChannelType == ChannelTypeEnum.TcpService)
                return typeof(ThingsGateway.Gateway.Razor.TcpServicePage);
            else
                return null;
        }
    }

    /// <inheritdoc/>
    public override IDevice FoundationDevice => _plc;

    public override Type DriverVariableAddressUIType => typeof(SiemensS7AddressComponent);


    protected override void InitChannel(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel);
        //载入配置
        _plc.DataFormat = _driverPropertys.DataFormat;
        _plc.SendDelayTime = _driverPropertys.SendDelayTime;
        _plc.SiemensS7Type = _driverPropertys.SiemensS7Type;
        _plc.Timeout = _driverPropertys.Timeout;
        _plc.LocalTSAP = _driverPropertys.LocalTSAP;
        _plc.Rack = _driverPropertys.Rack;
        _plc.Slot = _driverPropertys.Slot;
        _plc.InitChannel(channel, LogMessage);
        base.InitChannel(channel);
    }

    /// <summary>
    /// 获取jtoken值代表的字节数组，不包含字符串
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public byte[] GetBytes(DataTypeEnum dataType, JToken value)
    {
        //排除字符串
        if (value is JArray jArray)
        {
            return dataType switch
            {
                DataTypeEnum.Boolean => jArray.ToObject<Boolean[]>().BoolArrayToByte(),
                DataTypeEnum.Byte => jArray.ToObject<Byte[]>(),
                DataTypeEnum.Int16 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<Int16[]>()),
                DataTypeEnum.UInt16 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<UInt16[]>()),
                DataTypeEnum.Int32 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<Int32[]>()),
                DataTypeEnum.UInt32 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<UInt32[]>()),
                DataTypeEnum.Int64 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<Int64[]>()),
                DataTypeEnum.UInt64 => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<UInt64[]>()),
                DataTypeEnum.Single => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<Single[]>()),
                DataTypeEnum.Double => _plc.ThingsGatewayBitConverter.GetBytes(jArray.ToObject<Double[]>()),
                _ => throw new(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
            };
        }
        else
        {
            return dataType switch
            {
                DataTypeEnum.Boolean => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Boolean>()),
                DataTypeEnum.Byte => [value.ToObject<Byte>()],
                DataTypeEnum.Int16 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Int16>()),
                DataTypeEnum.UInt16 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<UInt16>()),
                DataTypeEnum.Int32 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Int32>()),
                DataTypeEnum.UInt32 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<UInt32>()),
                DataTypeEnum.Int64 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Int64>()),
                DataTypeEnum.UInt64 => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<UInt64>()),
                DataTypeEnum.Single => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Single>()),
                DataTypeEnum.Double => _plc.ThingsGatewayBitConverter.GetBytes(value.ToObject<Double>()),
                _ => throw new(DefaultResource.Localizer["DataTypeNotSupported", dataType]),
            };
        }
    }


    protected override async ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {

        using var writeLock = ReadWriteLock.WriterLock();
        try
        {

            // 检查协议是否为空，如果为空则抛出异常
            if (FoundationDevice == null)
                throw new NotSupportedException();

            // 创建用于存储操作结果的并发字典
            ConcurrentDictionary<string, OperResult> operResults = new();

            //转换
            Dictionary<VariableRuntime, SiemensS7Address> addresses = new();
            var w1 = writeInfoLists.Where(a => a.Key.DataType != DataTypeEnum.String);
            var w2 = writeInfoLists.Where(a => a.Key.DataType == DataTypeEnum.String);
            foreach (var item in w1)
            {
                SiemensS7Address siemensS7Address = SiemensS7Address.ParseFrom(item.Key.RegisterAddress);
                siemensS7Address.Data = GetBytes(item.Key.DataType, item.Value);
                siemensS7Address.Length = siemensS7Address.Data.Length;
                siemensS7Address.BitLength = 1;
                siemensS7Address.IsBit = item.Key.DataType == DataTypeEnum.Boolean;
                if (item.Key.DataType == DataTypeEnum.Boolean)
                {
                    if (item.Value is JArray jArray)
                    {
                        siemensS7Address.BitLength = jArray.ToObject<Boolean[]>().Length;
                    }
                }
                addresses.Add(item.Key, siemensS7Address);
            }
            if (addresses.Count > 0)
            {

                var result = await _plc.S7WriteAsync(addresses.Select(a => a.Value).ToArray(), cancellationToken).ConfigureAwait(false);
                foreach (var writeInfo in addresses)
                {
                    if (result.TryGetValue(writeInfo.Value, out var r1))
                    {
                        operResults.TryAdd(writeInfo.Key.Name, r1);
                    }
                }
            }

            // 使用并发方式遍历写入信息列表，并进行异步写入操作
            await w2.ParallelForEachAsync(async (writeInfo, cancellationToken) =>
            {
                try
                {
                    // 调用协议的写入方法，将写入信息中的数据写入到对应的寄存器地址，并获取操作结果
                    var result = await FoundationDevice.WriteAsync(writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, cancellationToken).ConfigureAwait(false);

                    // 将操作结果添加到结果字典中，使用变量名称作为键
                    operResults.TryAdd(writeInfo.Key.Name, result);
                }
                catch (Exception ex)
                {
                    operResults.TryAdd(writeInfo.Key.Name, new(ex));
                }
            }, Environment.ProcessorCount, cancellationToken).ConfigureAwait(false);


            // 返回包含操作结果的字典
            return new Dictionary<string, OperResult>(operResults);
        }
        finally
        {

        }

    }

    [ThingsGateway.Gateway.Application.DynamicMethod("ReadWriteDateAsync", "读写日期格式")]
    public async Task<IOperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateAsync(address, cancellationToken).ConfigureAwait(false);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateAsync(address, value.Value, cancellationToken).ConfigureAwait(false));
    }

    [ThingsGateway.Gateway.Application.DynamicMethod("ReadWriteDateTimeAsync", "读写日期时间格式")]
    public async Task<IOperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateTimeAsync(address, cancellationToken).ConfigureAwait(false);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateTimeAsync(address, value.Value, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRuntime> deviceVariables)
    {
        try
        {
            return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, _plc.OnLine ? _plc.PduLength : _driverPropertys.MaxPack, CurrentDevice.IntervalTime);
        }
        finally { }
    }
}
