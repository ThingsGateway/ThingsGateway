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

using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension.Bool;
using ThingsGateway.Foundation.Extension.Generic;

internal static class ModbusServerHelpers
{
    internal static OperResult<byte[]> Read(IModbusServer modbusServer, string address, int length)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, modbusServer.Station);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
        if (modbusServer.MulStation)
        {
            modbusServer.Init(mAddress);
        }
        else
        {
            if (modbusServer.Station != mAddress.Station)
            {
                return new OperResult<byte[]>("地址错误");
            }
            modbusServer.Init(mAddress);
        }
        try
        {
            modbusServer.EasyLock.Wait();

            var ModbusServer01ByteBlock = modbusServer.ModbusServer01ByteBlocks[mAddress.Station];
            var ModbusServer02ByteBlock = modbusServer.ModbusServer02ByteBlocks[mAddress.Station];
            var ModbusServer03ByteBlock = modbusServer.ModbusServer03ByteBlocks[mAddress.Station];
            var ModbusServer04ByteBlock = modbusServer.ModbusServer04ByteBlocks[mAddress.Station];
            int len = mAddress.ReadFunction == 2 || mAddress.ReadFunction == 1 ? length : length * modbusServer.RegisterByteLength;

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
                    ModbusServer03ByteBlock.Pos = mAddress.AddressStart * modbusServer.RegisterByteLength;
                    ModbusServer03ByteBlock.Read(bytes3);
                    return OperResult.CreateSuccessResult(bytes3);

                case 4:
                    byte[] bytes4 = new byte[len];
                    ModbusServer04ByteBlock.Pos = mAddress.AddressStart * modbusServer.RegisterByteLength;
                    ModbusServer04ByteBlock.Read(bytes4);
                    return OperResult.CreateSuccessResult(bytes4);
            }
        }
        finally
        {
            modbusServer.EasyLock.Release();
        }
        return new OperResult<byte[]>("功能码错误");
    }

    internal static OperResult Write(IModbusServer modbusServer, string address, byte[] value)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, modbusServer.Station);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
        if (modbusServer.MulStation)
        {
            modbusServer.Init(mAddress);
        }
        else
        {
            if (modbusServer.Station != mAddress.Station)
            {
                return new OperResult("地址错误");
            }
            modbusServer.Init(mAddress);
        }
        try
        {
            modbusServer.EasyLock.Wait();
            var ModbusServer03ByteBlock = modbusServer.ModbusServer03ByteBlocks[mAddress.Station];
            var ModbusServer04ByteBlock = modbusServer.ModbusServer04ByteBlocks[mAddress.Station];
            switch (mAddress.ReadFunction)
            {
                case 3:
                    ModbusServer03ByteBlock.Pos = mAddress.AddressStart * modbusServer.RegisterByteLength;
                    ModbusServer03ByteBlock.Write(value);
                    return OperResult.CreateSuccessResult();

                case 4:
                    ModbusServer04ByteBlock.Pos = mAddress.AddressStart * modbusServer.RegisterByteLength;
                    ModbusServer04ByteBlock.Write(value);
                    return OperResult.CreateSuccessResult();
            }
        }
        finally { modbusServer.EasyLock.Release(); }
        return new OperResult("功能码错误");
    }

    internal static OperResult Write(IModbusServer modbusServer, string address, bool[] value)
    {
        ModbusAddress mAddress;
        try
        {
            mAddress = ModbusAddress.ParseFrom(address, modbusServer.Station);
        }
        catch (Exception ex)
        {
            return (new OperResult(ex));
        }
        if (modbusServer.MulStation)
        {
            modbusServer.Init(mAddress);
        }
        else
        {
            if (modbusServer.Station != mAddress.Station)
            {
                return (new OperResult("地址错误"));
            }
            modbusServer.Init(mAddress);
        }
        try
        {
            modbusServer.EasyLock.Wait();
            var ModbusServer01ByteBlock = modbusServer.ModbusServer01ByteBlocks[mAddress.Station];
            var ModbusServer02ByteBlock = modbusServer.ModbusServer02ByteBlocks[mAddress.Station];
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
        }
        finally
        {
            modbusServer.EasyLock.Release();
        }

        return new OperResult("功能码错误");
    }

    internal static async Task Received(IModbusServer modbusServer, ISenderClient client, ReceivedDataEventArgs e)
    {
        var requestInfo = e.RequestInfo;
        //接收外部报文
        if (requestInfo is IModbusServerMessage modbusServerMessage)
        {
            if (modbusServerMessage.ModbusAddress == null)
            {
                return;//无法解析直接返回
            }
            if (!modbusServerMessage.IsSuccess)
            {
                return;//无法解析直接返回
            }

            if (modbusServerMessage.ModbusAddress.WriteFunction == 0)//读取
            {
                var data = modbusServer.Read(modbusServerMessage.ModbusAddress.ToString(), modbusServerMessage.Length);
                if (data.IsSuccess)
                {
                    var coreData = data.Content;
                    if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                    {
                        coreData = data.Content.Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling(modbusServerMessage.Length / 8.0));
                    }
                    //rtu返回头
                    if (modbusServer.IsRtu)
                    {
                        var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2).SpliceArray(new byte[] { (byte)coreData.Length }, coreData);
                        client.Send(sendData);
                    }
                    else
                    {
                        var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8).SpliceArray(new byte[] { (byte)coreData.Length }, coreData);
                        sendData[5] = (byte)(sendData.Length - 6);
                        client.Send(sendData);
                    }
                }
                else
                {
                    WriteError(modbusServer.IsRtu, client, modbusServerMessage);//返回错误码
                }
            }
            else//写入
            {
                var coreData = modbusServerMessage.Content;
                if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                {
                    //写入继电器
                    if (modbusServer.OnWriteData != null)
                    {
                        // 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
                        if ((await modbusServer.OnWriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, modbusServer.ThingsGatewayBitConverter, client)).IsSuccess)
                        {
                            WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                            if (modbusServer.WriteMemory)
                            {
                                var result = modbusServer.Write(modbusServerMessage.ModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                                if (result.IsSuccess)
                                    WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                                else
                                    WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                            }
                            else
                                WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                        else
                        {
                            WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                    }
                    else
                    {
                        //写入内存区
                        var result = modbusServer.Write(modbusServerMessage.ModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                        if (result.IsSuccess)
                        {
                            WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                        else
                        {
                            WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                    }
                }
                else
                {
                    //写入寄存器
                    if (modbusServer.OnWriteData != null)
                    {
                        if ((await modbusServer.OnWriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, modbusServer.ThingsGatewayBitConverter, client)).IsSuccess)
                        {
                            if (modbusServer.WriteMemory)
                            {
                                var result = modbusServer.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                                if (result.IsSuccess)
                                    WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                                else
                                    WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                            }
                            else
                                WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                        else
                        {
                            WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                    }
                    else
                    {
                        var result = modbusServer.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                        if (result.IsSuccess)
                        {
                            WriteSuccess(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                        else
                        {
                            WriteError(modbusServer.IsRtu, client, modbusServerMessage);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 返回错误码
    /// </summary>
    private static void WriteError(bool isRtu, ISenderClient client, IModbusServerMessage modbusServerMessage)
    {
        if (isRtu)
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2)
.SpliceArray(new byte[] { (byte)1 });//01 lllegal function
            sendData[1] = (byte)(sendData[1] + 128);
            client.Send(sendData);
        }
        else
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8)
.SpliceArray(new byte[] { (byte)1 });//01 lllegal function
            sendData[5] = (byte)(sendData.Length - 6);
            sendData[7] = (byte)(sendData[7] + 128);
            client.Send(sendData);
        }
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    internal static void WriteSuccess(bool isRtu, ISenderClient client, IModbusServerMessage modbusServerMessage)
    {
        if (isRtu)
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 6);
            client.Send(sendData);
        }
        else
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 12);
            sendData[5] = (byte)(sendData.Length - 6);
            client.Send(sendData);
        }
    }
}