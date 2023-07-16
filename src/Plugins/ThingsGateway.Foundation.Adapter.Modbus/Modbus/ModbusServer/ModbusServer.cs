#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusServer : ReadWriteDevicesTcpServerBase
    {
        public ModbusServerDataHandleAdapter DataHandleAdapter = new();
        /// <summary>
        /// 继电器
        /// </summary>
        public Dictionary<int, ByteBlock> ModbusServer01ByteBlocks = new();

        /// <summary>
        /// 开关输入
        /// </summary>
        public Dictionary<int, ByteBlock> ModbusServer02ByteBlocks = new();

        /// <summary>
        /// 输入寄存器
        /// </summary>
        public Dictionary<int, ByteBlock> ModbusServer03ByteBlocks = new();

        /// <summary>
        /// 保持寄存器
        /// </summary>
        public Dictionary<int, ByteBlock> ModbusServer04ByteBlocks = new();

        public Func<ModbusAddress, byte[], IThingsGatewayBitConverter, SocketClient, Task<OperResult>> Write;

        public ModbusServer(TcpService tcpService) : base(tcpService)
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

        public override string GetAddressDescription()
        {
            return base.GetAddressDescription() + Environment.NewLine + ModbusHelper.GetAddressDescription();
        }
        public override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default)
        {
            ModbusAddress mAddress = null;
            try
            {
                mAddress = new ModbusAddress(address, Station);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperResult<byte[]>(ex));
            }
            if (MulStation)
            {
                ModbusServer01ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer02ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer03ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer04ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer01ByteBlocks[mAddress.Station].SetLength(1024 * 128);
                ModbusServer02ByteBlocks[mAddress.Station].SetLength(1024 * 128);
                ModbusServer03ByteBlocks[mAddress.Station].SetLength(1024 * 128);
                ModbusServer04ByteBlocks[mAddress.Station].SetLength(1024 * 128);
            }
            else
            {
                if (Station != mAddress.Station)
                {
                    return Task.FromResult(new OperResult<byte[]>("地址错误"));
                }
                ModbusServer01ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer02ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer03ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer04ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer01ByteBlocks[Station].SetLength(1024 * 128);
                ModbusServer02ByteBlocks[Station].SetLength(1024 * 128);
                ModbusServer03ByteBlocks[Station].SetLength(1024 * 128);
                ModbusServer04ByteBlocks[Station].SetLength(1024 * 128);
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
                    return Task.FromResult(OperResult.CreateSuccessResult(bytes0));
                case 2:
                    byte[] bytes1 = new byte[len];
                    ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                    ModbusServer02ByteBlock.Read(bytes1);
                    return Task.FromResult(OperResult.CreateSuccessResult(bytes1));
                case 3:

                    byte[] bytes3 = new byte[len];
                    ModbusServer03ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                    ModbusServer03ByteBlock.Read(bytes3);
                    return Task.FromResult(OperResult.CreateSuccessResult(bytes3));
                case 4:
                    byte[] bytes4 = new byte[len];
                    ModbusServer04ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                    ModbusServer04ByteBlock.Read(bytes4);
                    return Task.FromResult(OperResult.CreateSuccessResult(bytes4));
            }
            return Task.FromResult(new OperResult<byte[]>("功能码错误"));
        }

        public override void SetDataAdapter(SocketClient client)
        {
            DataHandleAdapter = new();
            client.SetDataHandlingAdapter(DataHandleAdapter);
        }
        public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default)
        {
            ModbusAddress mAddress = null;
            try
            {
                mAddress = new ModbusAddress(address, Station);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperResult(ex));
            }
            if (MulStation)
            {
                ModbusServer03ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer04ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer03ByteBlocks[mAddress.Station].SetLength(1024 * 128);
                ModbusServer04ByteBlocks[mAddress.Station].SetLength(1024 * 128);
            }
            else
            {
                if (Station != mAddress.Station)
                {
                    return Task.FromResult(new OperResult("地址错误"));
                }
                ModbusServer03ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer04ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer03ByteBlocks[Station].SetLength(1024 * 128);
                ModbusServer04ByteBlocks[Station].SetLength(1024 * 128);
            }


            var ModbusServer03ByteBlock = ModbusServer03ByteBlocks[mAddress.Station];
            var ModbusServer04ByteBlock = ModbusServer04ByteBlocks[mAddress.Station];
            switch (mAddress.ReadFunction)
            {
                case 3:
                    ModbusServer03ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                    ModbusServer03ByteBlock.Write(value);
                    return Task.FromResult(OperResult.CreateSuccessResult());
                case 4:
                    ModbusServer04ByteBlock.Pos = mAddress.AddressStart * RegisterByteLength;
                    ModbusServer04ByteBlock.Write(value);
                    return Task.FromResult(OperResult.CreateSuccessResult());
            }
            return Task.FromResult(new OperResult("功能码错误"));
        }

        public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default)
        {
            ModbusAddress mAddress = null;
            try
            {
                mAddress = new ModbusAddress(address, Station);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperResult(ex));
            }
            if (MulStation)
            {
                ModbusServer01ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer02ByteBlocks.TryAdd(mAddress.Station, new(1024 * 128));
                ModbusServer01ByteBlocks[mAddress.Station].SetLength(1024 * 128);
                ModbusServer02ByteBlocks[mAddress.Station].SetLength(1024 * 128);
            }
            else
            {
                if (Station != mAddress.Station)
                {
                    return Task.FromResult(new OperResult("地址错误"));
                }
                ModbusServer01ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer02ByteBlocks.TryAdd(Station, new(1024 * 128));
                ModbusServer01ByteBlocks[Station].SetLength(1024 * 128);
                ModbusServer02ByteBlocks[Station].SetLength(1024 * 128);
            }

            var ModbusServer01ByteBlock = ModbusServer01ByteBlocks[mAddress.Station];
            var ModbusServer02ByteBlock = ModbusServer02ByteBlocks[mAddress.Station];
            switch (mAddress.ReadFunction)
            {
                case 1:
                    ModbusServer01ByteBlock.Pos = mAddress.AddressStart;
                    ModbusServer01ByteBlock.Write(value.BoolArrayToByte());
                    return Task.FromResult(OperResult.CreateSuccessResult());
                case 2:
                    ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                    ModbusServer02ByteBlock.Write(value.BoolArrayToByte());
                    return Task.FromResult(OperResult.CreateSuccessResult());
            }
            return Task.FromResult(new OperResult("功能码错误"));
        }

        protected override void Dispose(bool disposing)
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
            Disconnect();
            base.Dispose(disposing);
        }

        protected override async Task ReceivedAsync(SocketClient client, IRequestInfo requestInfo)
        {

            if (requestInfo is ModbusServerMessage modbusServerMessage)
            {
                if (!modbusServerMessage.IsSuccess)
                {
                    return;
                }
                if (modbusServerMessage.CurModbusAddress == null)
                {
                    WriteError(client, modbusServerMessage);
                }
                if (modbusServerMessage.CurModbusAddress.WriteFunction == 0)
                {
                    var data = await ReadAsync(modbusServerMessage.CurModbusAddress.ToString(), modbusServerMessage.CurModbusAddress.Length);
                    if (data.IsSuccess)
                    {
                        var coreData = data.Content;
                        if (modbusServerMessage.CurModbusAddress.ReadFunction == 1 || modbusServerMessage.CurModbusAddress.ReadFunction == 2)
                        {
                            coreData = data.Content.Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling((double)modbusServerMessage.CurModbusAddress.Length / 8.0));
                        }
                        var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8)
                            .SpliceArray(new byte[] { (byte)coreData.Length }, coreData);
                        sendData[5] = (byte)(sendData.Length - 6);
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
                else
                {
                    var coreData = modbusServerMessage.Content;
                    if (modbusServerMessage.CurModbusAddress.ReadFunction == 1 || modbusServerMessage.CurModbusAddress.ReadFunction == 2)
                    {
                        //写入继电器
                        if (Write != null)
                        {

                            if ((await Write(modbusServerMessage.CurModbusAddress, modbusServerMessage.Content, ThingsGatewayBitConverter, client)).IsSuccess)
                            {
                                var result = await WriteAsync(modbusServerMessage.CurModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.CurModbusAddress.Length));
                                if (result.IsSuccess)
                                {
                                    WriteSuccess03(client, modbusServerMessage);

                                }
                                else
                                {
                                    WriteError(client, modbusServerMessage);

                                }

                            }
                        }
                        else
                        {
                            var result = await WriteAsync(modbusServerMessage.CurModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.CurModbusAddress.Length));
                            if (result.IsSuccess)
                            {
                                WriteSuccess03(client, modbusServerMessage);

                            }
                            else
                            {
                                WriteError(client, modbusServerMessage);

                            }
                        }
                    }
                    else
                    {
                        //写入寄存器
                        if (Write != null)
                        {

                            if ((await Write(modbusServerMessage.CurModbusAddress, modbusServerMessage.Content, ThingsGatewayBitConverter, client)).IsSuccess)
                            {
                                var result = await WriteAsync(modbusServerMessage.CurModbusAddress.ToString(), coreData);
                                if (result.IsSuccess)
                                {
                                    WriteSuccess03(client, modbusServerMessage);
                                }
                                else
                                {
                                    WriteError(client, modbusServerMessage);
                                }
                            }
                            else
                            {
                                WriteError(client, modbusServerMessage);
                            }
                        }
                        else
                        {
                            var result = await WriteAsync(modbusServerMessage.CurModbusAddress.ToString(), coreData);
                            if (result.IsSuccess)
                            {
                                WriteSuccess03(client, modbusServerMessage);

                            }
                            else
                            {
                                WriteError(client, modbusServerMessage);

                            }
                        }
                    }
                }
            }


            static void WriteError(SocketClient client, ModbusServerMessage modbusServerMessage)
            {
                var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8)
.SpliceArray(new byte[] { (byte)1 });//01 lllegal function
                sendData[5] = (byte)(sendData.Length - 6);
                sendData[7] = (byte)(sendData[7] + 128);
                client.Send(sendData);


            }
        }

        private static void WriteSuccess03(SocketClient client, ModbusServerMessage modbusServerMessage)
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 12);
            sendData[5] = (byte)(sendData.Length - 6);
            client.Send(sendData);
        }
    }
}
