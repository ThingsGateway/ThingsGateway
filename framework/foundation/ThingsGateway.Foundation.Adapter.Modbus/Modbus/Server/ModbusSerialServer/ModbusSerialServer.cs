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

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Adapter.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSerialServer : ReadWriteDevicesSerialSessionBase, IModbusServer
{
    /// <summary>
    /// 读写锁
    /// </summary>
    public EasyLock EasyLock { get; } = new();

    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public Func<ModbusAddress, byte[], IThingsGatewayBitConverter, ISenderClient, Task<OperResult>> OnWriteData { get; set; }

    /// <inheritdoc/>
    public ConcurrentDictionary<byte, ByteBlock> ModbusServer01ByteBlocks { get; set; } = new();

    /// <inheritdoc/>
    public ConcurrentDictionary<byte, ByteBlock> ModbusServer02ByteBlocks { get; set; } = new();

    /// <inheritdoc/>
    public ConcurrentDictionary<byte, ByteBlock> ModbusServer03ByteBlocks { get; set; } = new();

    /// <inheritdoc/>
    public ConcurrentDictionary<byte, ByteBlock> ModbusServer04ByteBlocks { get; set; } = new();
    /// <inheritdoc/>
    public ModbusSerialServer(SerialSession serialSession) : base(serialSession)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <inheritdoc/>
    public bool MulStation { get; set; }

    /// <inheritdoc/>
    public byte Station { get; set; } = 1;
    /// <inheritdoc/>
    public bool IsRtu => true;


    /// <inheritdoc/>
    public override void Dispose()
    {
        foreach (var item in ModbusServer01ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer02ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer03ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer04ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        ModbusServer01ByteBlocks.Clear();
        ModbusServer02ByteBlocks.Clear();
        ModbusServer03ByteBlocks.Clear();
        ModbusServer04ByteBlocks.Clear();
        base.Dispose();
    }

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack, defaultIntervalTime);
    }


    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {

        return ModbusServerHelpers.Read(this, address, length);

    }



    /// <inheritdoc/>
    public override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Read(address, length));
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(ISocketClient socketClient = default)
    {
        ModbusSerialServerDataHandleAdapter dataHandleAdapter = new();
        dataHandleAdapter.CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout);
        SerialSession.SetDataHandlingAdapter(dataHandleAdapter);
    }


    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {

        return ModbusServerHelpers.Write(this, address, value);

    }


    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
    {

        return ModbusServerHelpers.Write(this, address, value);

    }


    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Write(address, value));
    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Write(address, value));
    }

    /// <inheritdoc/>
    protected override async Task Received(SerialSession client, ReceivedDataEventArgs e)
    {
        try
        {
            await ModbusServerHelpers.Received(this, client, e);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ToString());
        }
    }



    /// <inheritdoc/>
    public void Init(ModbusAddress mAddress)
    {
        ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var data = new ByteBlock(ushort.MaxValue * 2);
            data.SetLength(ushort.MaxValue * 2);
            return data;
        });
        ModbusServer02ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var data = new ByteBlock(ushort.MaxValue * 2);
            data.SetLength(ushort.MaxValue * 2);
            return data;
        });
        ModbusServer03ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var data = new ByteBlock(ushort.MaxValue * 2);
            data.SetLength(ushort.MaxValue * 2);
            return data;
        });
        ModbusServer04ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var data = new ByteBlock(ushort.MaxValue * 2);
            data.SetLength(ushort.MaxValue * 2);
            return data;
        });
    }
}
