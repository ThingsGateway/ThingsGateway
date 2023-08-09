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
public class ModbusRtuOverTcp : ReadWriteDevicesTcpClientBase
{


    /// <inheritdoc/>
    public ModbusRtuOverTcp(TcpClientEx tcpClient) : base(tcpClient)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <summary>
    /// 组包缓存时间/ms
    /// </summary>
    [Description("组包缓存时间ms")]
    public int CacheTimeout { get; set; } = 1000;
    /// <summary>
    /// Crc校验
    /// </summary>
    [Description("Crc校验")]
    public bool Crc16CheckEnable { get; set; } = true;
    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [Description("帧前时间ms")]
    public int FrameTime { get; set; }
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
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            if (commandResult.IsSuccess)
            {
                ResponsedData result = await SendThenReturnAsync(commandResult, token);
                return (MessageBase)result.RequestInfo;

            }
            else
            {
                return OperResult.CreateFailedResult<byte[]>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override void SetDataAdapter()
    {
        ModbusRtuDataHandleAdapter dataHandleAdapter = new()
        {
            Crc16CheckEnable = Crc16CheckEnable,
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
        TcpClientEx.SetDataHandlingAdapter(dataHandleAdapter);
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            if (commandResult.IsSuccess)
            {
                ResponsedData result = await SendThenReturnAsync(commandResult, token);
                return (MessageBase)result.RequestInfo;

            }
            else
            {
                return OperResult.CreateFailedResult<bool[]>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<bool[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            if (commandResult.IsSuccess)
            {
                ResponsedData result = await SendThenReturnAsync(commandResult, token);
                return (MessageBase)result.RequestInfo;

            }
            else
            {
                return OperResult.CreateFailedResult<bool[]>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<bool[]>(ex);
        }
    }

    /// <inheritdoc/>
    private async Task<ResponsedData> SendThenReturnAsync(OperResult<byte[]> commandResult, CancellationToken token)
    {

        var item = commandResult.Content;
        await Task.Delay(FrameTime, token);

        var result = await WaitingClientEx.SendThenResponseAsync(item, TimeOut, token);
        return result;


    }
}
