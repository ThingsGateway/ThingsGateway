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

using System.ComponentModel;

namespace ThingsGateway.Foundation.Adapter.Modbus;
/// <inheritdoc/>
public class ModbusTcpDtu : ReadWriteDevicesTcpServerBase
{
    /// <inheritdoc/>
    public ModbusTcpDtu(TcpService tcpService) : base(tcpService)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
        ModbusTcpDtuPlugin modbusTcpSalvePlugin = new ModbusTcpDtuPlugin();
        tcpService.Config.ConfigurePlugins(a =>
         {
             a.Add(modbusTcpSalvePlugin);
         });
        tcpService.Setup(tcpService.Config);
    }

    /// <summary>
    /// 检测事务标识符
    /// </summary>
    [Description("检测事务标识符")]
    public bool IsCheckMessageId { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    [Description("站号")]
    public byte Station { get; set; } = 1;

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
        try
        {
            Connect(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            return SendThenReturn(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(object socketClient = null)
    {
        if (socketClient is SocketClient client)
        {
            ModbusTcpDataHandleAdapter dataHandleAdapter = new()
            {
                IsCheckMessageId = IsCheckMessageId,
                CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
            };
            client.SetDataHandlingAdapter(dataHandleAdapter);
        }
        else
        {
            foreach (var item in TcpService.GetClients())
            {
                ModbusTcpDataHandleAdapter dataHandleAdapter = new()
                {
                    IsCheckMessageId = IsCheckMessageId,
                    CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                };
                item.SetDataHandlingAdapter(dataHandleAdapter);
            }
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return SendThenReturn(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            return SendThenReturn(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(string id, OperResult<byte[]> commandResult, CancellationToken cancellationToken)
    {
        if (commandResult.IsSuccess)
        {
            if (TcpService.TryGetSocketClient($"ID={id}", out var client))
            {
                SetDataAdapter(client);

                var item = commandResult.Content;
                if (FrameTime != 0)
                    Thread.Sleep(FrameTime);
                var WaitingClientEx = client.CreateWaitingClient(new() { ThrowBreakException = true });
                var result = WaitingClientEx.SendThenResponse(item, TimeOut, cancellationToken);
                return (MessageBase)result.RequestInfo;
            }
            else
            {
                return new OperResult<byte[]>("客户端未连接");
            }
        }
        else
        {
            return commandResult;
        }


    }

    private async Task<OperResult<byte[]>> SendThenReturnAsync(string id, OperResult<byte[]> commandResult, CancellationToken cancellationToken)
    {
        if (commandResult.IsSuccess)
        {
            if (TcpService.TryGetSocketClient($"ID={id}", out var client))
            {
                SetDataAdapter(client);

                var item = commandResult.Content;
                await Task.Delay(FrameTime, cancellationToken);
                var WaitingClientEx = client.CreateWaitingClient(new() { ThrowBreakException = true });
                var result = await WaitingClientEx.SendThenResponseAsync(item, TimeOut, cancellationToken);
                return (MessageBase)result.RequestInfo;
            }
            else
            {
                return new OperResult<byte[]>("客户端未连接");
            }
        }
        else
        {
            return commandResult;
        }


    }

    internal class ModbusTcpDtuPlugin : PluginBase, ITcpReceivingPlugin
    {
        public Task OnTcpReceiving(ITcpClientBase client, ByteBlockEventArgs e)
        {
            if (client is ISocketClient socket)
            {
                if (!socket.Id.StartsWith("ID="))
                {
                    ByteBlock byteBlock = e.ByteBlock;
                    var id = $"ID={byteBlock.ToArray().ToHexString()}";
                    socket.ResetId(id);
                }
            }
            return e.InvokeNext();//如果本插件无法处理当前数据，请将数据转至下一个插件。
        }
    }
}
