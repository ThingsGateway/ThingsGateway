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

using ThingsGateway.Foundation.Extension.Bool;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusSerialServer : ReadWriteDevicesSerialSessionBase
{
    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public Func<ModbusAddress, byte[], IThingsGatewayBitConverter, SerialSession, OperResult> WriteData;

    /// <summary>
    /// 继电器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer01ByteBlocks = new();

    /// <summary>
    /// 开关输入
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer02ByteBlocks = new();

    /// <summary>
    /// 输入寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer03ByteBlocks = new();

    /// <summary>
    /// 保持寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer04ByteBlocks = new();
    /// <inheritdoc/>
    public ModbusSerialServer(SerialSession serialSession) : base(serialSession)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <summary>
    /// 多站点
    /// </summary>
    public bool MulStation { get; set; }

    /// <summary>
    /// 默认站点
    /// </summary>
    public byte Station { get; set; } = 1;


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
        return base.GetAddressDescription() + Environment.NewLine + ModbusHelper.GetAddressDescription();
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack)
    {
        return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack);

    }
    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, Station);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
        if (MulStation)
        {
            Init(mAddress);
        }
        else
        {
            if (Station != mAddress.Station)
            {
                return new OperResult<byte[]>("地址错误");
            }
            Init(mAddress);

        }

        var ModbusServer01ByteBlock = ModbusServer01ByteBlocks[mAddress.Station];
        var ModbusServer02ByteBlock = ModbusServer02ByteBlocks[mAddress.Station];
        var ModbusServer03ByteBlock = ModbusServer03ByteBlocks[mAddress.Station];
        var ModbusServer04ByteBlock = ModbusServer04ByteBlocks[mAddress.Station];
        int len = mAddress.ReadFunction == 2 || mAddress.ReadFunction == 1 ? length : length * RegisterByteLength;
        switch (mAddress.ReadFunction)
        {
            case 1:
                byte[] bytes0 = new byte[len];
                ModbusServer01ByteBlock.Pos = mAddress.AddressStart;
                ModbusServer01ByteBlock.Read(bytes0);
                return OperResult.CreateSuccessResult(bytes0);
            case 2:
                byte[] bytes1 = new byte[len];
                ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                ModbusServer02ByteBlock.Read(bytes1);
                return OperResult.CreateSuccessResult(bytes1);
            case 3:

                byte[] bytes3 = new byte[len];
                ModbusServer03ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                ModbusServer03ByteBlock.Read(bytes3);
                return OperResult.CreateSuccessResult(bytes3);
            case 4:
                byte[] bytes4 = new byte[len];
                ModbusServer04ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                ModbusServer04ByteBlock.Read(bytes4);
                return OperResult.CreateSuccessResult(bytes4);
        }
        return new OperResult<byte[]>("功能码错误");
    }

    /// <inheritdoc/>
    public override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Read(address, length));
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(object socketClient)
    {
        ModbusSerialServerDataHandleAdapter dataHandleAdapter = new();
        dataHandleAdapter.CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout);
        SerialSession.SetDataHandlingAdapter(dataHandleAdapter);
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, Station);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
        if (MulStation)
        {
            Init(mAddress);
        }
        else
        {
            if (Station != mAddress.Station)
            {
                return new OperResult("地址错误");
            }
            Init(mAddress);
        }
        var ModbusServer03ByteBlock = ModbusServer03ByteBlocks[mAddress.Station];
        var ModbusServer04ByteBlock = ModbusServer04ByteBlocks[mAddress.Station];
        switch (mAddress.ReadFunction)
        {
            case 3:
                ModbusServer03ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                ModbusServer03ByteBlock.Write(value);
                return OperResult.CreateSuccessResult();
            case 4:
                ModbusServer04ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                ModbusServer04ByteBlock.Write(value);
                return OperResult.CreateSuccessResult();
        }
        return new OperResult("功能码错误");
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, Station);
        }
        catch (Exception ex)
        {
            return (new OperResult(ex));
        }
        if (MulStation)
        {
            Init(mAddress);

        }
        else
        {
            if (Station != mAddress.Station)
            {
                return (new OperResult("地址错误"));
            }
            Init(mAddress);

        }

        var ModbusServer01ByteBlock = ModbusServer01ByteBlocks[mAddress.Station];
        var ModbusServer02ByteBlock = ModbusServer02ByteBlocks[mAddress.Station];
        switch (mAddress.ReadFunction)
        {
            case 1:
                ModbusServer01ByteBlock.Pos = mAddress.AddressStart;
                ModbusServer01ByteBlock.Write(value.BoolArrayToByte());
                return (OperResult.CreateSuccessResult());
            case 2:
                ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                ModbusServer02ByteBlock.Write(value.BoolArrayToByte());
                return (OperResult.CreateSuccessResult());
        }
        return new OperResult("功能码错误");
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
    protected override void Received(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        try
        {

            //接收外部报文
            if (requestInfo is ModbusSerialServerMessage modbusServerMessage)
            {
                if (modbusServerMessage.CurModbusAddress == null)
                {
                    return;//无法解析直接返回
                }
                if (!modbusServerMessage.IsSuccess)
                {
                    return;//无法解析直接返回
                }

                if (modbusServerMessage.CurModbusAddress.WriteFunction == 0)//读取
                {
                    var data = Read(modbusServerMessage.CurModbusAddress.ToString(), modbusServerMessage.Length);
                    if (data.IsSuccess)
                    {
                        var coreData = data.Content;
                        if (modbusServerMessage.CurModbusAddress.ReadFunction == 1 || modbusServerMessage.CurModbusAddress.ReadFunction == 2)
                        {
                            coreData = data.Content.Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling(modbusServerMessage.Length / 8.0));
                        }
                        var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2).SpliceArray(new byte[] { (byte)coreData.Length }, coreData);
                        SerialSession.Send(sendData);
                    }
                    else
                    {
                        WriteError(SerialSession, modbusServerMessage);//返回错误码
                    }
                }
                else//写入
                {
                    var coreData = modbusServerMessage.Content;
                    if (modbusServerMessage.CurModbusAddress.ReadFunction == 1 || modbusServerMessage.CurModbusAddress.ReadFunction == 2)
                    {
                        //写入继电器
                        if (WriteData != null)
                        {
                            // 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
                            if ((WriteData(modbusServerMessage.CurModbusAddress, modbusServerMessage.Content, ThingsGatewayBitConverter, SerialSession)).IsSuccess)
                            {
                                var result = Write(modbusServerMessage.CurModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                                if (result.IsSuccess)
                                {
                                    WriteSuccess03(SerialSession, modbusServerMessage);
                                }
                                else
                                {
                                    WriteError(SerialSession, modbusServerMessage);
                                }
                            }
                            else
                            {
                                WriteError(SerialSession, modbusServerMessage);
                            }
                        }
                        else
                        {
                            //写入内存区
                            var result = Write(modbusServerMessage.CurModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                            if (result.IsSuccess)
                            {
                                WriteSuccess03(SerialSession, modbusServerMessage);
                            }
                            else
                            {
                                WriteError(SerialSession, modbusServerMessage);
                            }
                        }
                    }
                    else
                    {
                        //写入寄存器
                        if (WriteData != null)
                        {

                            if ((WriteData(modbusServerMessage.CurModbusAddress, modbusServerMessage.Content, ThingsGatewayBitConverter, SerialSession)).IsSuccess)
                            {
                                var result = Write(modbusServerMessage.CurModbusAddress.ToString(), coreData);
                                if (result.IsSuccess)
                                {
                                    WriteSuccess03(SerialSession, modbusServerMessage);
                                }
                                else
                                {
                                    WriteError(SerialSession, modbusServerMessage);
                                }
                            }
                            else
                            {
                                WriteError(SerialSession, modbusServerMessage);
                            }
                        }
                        else
                        {
                            var result = Write(modbusServerMessage.CurModbusAddress.ToString(), coreData);
                            if (result.IsSuccess)
                            {
                                WriteSuccess03(SerialSession, modbusServerMessage);

                            }
                            else
                            {
                                WriteError(SerialSession, modbusServerMessage);

                            }
                        }
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ToString());
        }
        //返回错误码
        static void WriteError(SerialSession SerialSession, ModbusSerialServerMessage modbusServerMessage)
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2)
.SpliceArray(new byte[] { (byte)1 });//01 lllegal function
            sendData[1] = (byte)(sendData[1] + 128);
            SerialSession.Send(sendData);

        }
    }

    private static void WriteSuccess03(SerialSession SerialSession, ModbusSerialServerMessage modbusServerMessage)
    {
        var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 6);
        SerialSession.Send(sendData);
    }

    private void Init(ModbusAddress mAddress)
    {
        if (ModbusServer01ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128)))
            ModbusServer01ByteBlocks[mAddress.Station].SetLength(1024 * 128);
        if (ModbusServer02ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128)))
            ModbusServer02ByteBlocks[mAddress.Station].SetLength(1024 * 128);
        if (ModbusServer03ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128)))
            ModbusServer03ByteBlocks[mAddress.Station].SetLength(1024 * 128);
        if (ModbusServer04ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128)))
            ModbusServer04ByteBlocks[mAddress.Station].SetLength(1024 * 128);
    }
}
