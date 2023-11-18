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

namespace ThingsGateway.Foundation.Adapter.DLT645;
/// <summary>
/// DLT645_2007
/// </summary>
public class DLT645_2007OverTcp : ReadWriteDevicesTcpClientBase, IDLT645_2007
{
    /// <summary>
    /// DLT645_2007
    /// </summary>
    /// <param name="tcpClient"></param>
    public DLT645_2007OverTcp(TcpClient tcpClient) : base(tcpClient)
    {
        ThingsGatewayBitConverter = new DLT645_2007BitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <inheritdoc/>
    [Description("前导符报文头")]
    public bool EnableFEHead { get; set; }

    /// <inheritdoc/>
    [Description("操作员代码")]
    public string OperCode { get; set; }

    /// <inheritdoc/>
    [Description("写入密码")]
    public string Password { get; set; }

    /// <inheritdoc/>
    [Description("通讯地址")]
    public string Station { get; set; }
    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return base.GetAddressDescription() + Environment.NewLine + DLT645_2007Util.GetAddressDescription();
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(ISocketClient socketClient = default)
    {
        var dataHandleAdapter = new DLT645_2007DataHandleAdapter
        {
            EnableFEHead = EnableFEHead
        };
        TcpClient.SetDataHandlingAdapter(dataHandleAdapter);
    }



    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        return DLT645_2007Util.Read(this, address, length, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        return DLT645_2007Util.ReadAsync(this, address, length, cancellationToken);
    }


    /// <inheritdoc/>
    public override OperResult Write(string address, string value, CancellationToken cancellationToken = default)
    {

        return DLT645_2007Util.Write(this, address, value, cancellationToken);


    }


    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default)
    {

        return DLT645_2007Util.WriteAsync(this, address, value, cancellationToken);

    }


    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default) => Write(address, value.ToString(), cancellationToken);

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default) => Write(address, value.ToString(), cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, uint value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, double value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, float value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, long value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ulong value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ushort value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, short value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, int value, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default) => throw new NotImplementedException();


    #region 其他方法

    /// <inheritdoc cref="DLT645_2007Util.BroadcastTime(IDLT645_2007,DateTime,  CancellationToken)"/>
    public OperResult BroadcastTime(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        return DLT645_2007Util.BroadcastTime(this, dateTime, cancellationToken);

    }

    /// <inheritdoc cref="DLT645_2007Util.FreezeAsync(IDLT645_2007,DateTime,  CancellationToken)"/>
    public async Task<OperResult> FreezeAsync(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        return await DLT645_2007Util.FreezeAsync(this, dateTime, cancellationToken);

    }



    /// <inheritdoc cref="DLT645_2007Util.ReadDeviceStationAsync(IDLT645_2007,  CancellationToken)"/>
    public async Task<OperResult<string>> ReadDeviceStationAsync(CancellationToken cancellationToken = default)
    {
        return await DLT645_2007Util.ReadDeviceStationAsync(this, cancellationToken);


    }



    /// <inheritdoc cref="DLT645_2007Util.WriteBaudRateAsync(IDLT645_2007, int, CancellationToken)"/>
    public async Task<OperResult> WriteBaudRateAsync(int baudRate, CancellationToken cancellationToken = default)
    {
        return await DLT645_2007Util.WriteBaudRateAsync(this, baudRate, cancellationToken);

    }

    /// <inheritdoc cref="DLT645_2007Util.WriteDeviceStationAsync(IDLT645_2007, string, CancellationToken)"/>
    public async Task<OperResult> WriteDeviceStationAsync(string station, CancellationToken cancellationToken = default)
    {
        return await DLT645_2007Util.WriteDeviceStationAsync(this, station, cancellationToken);
    }

    /// <inheritdoc cref="DLT645_2007Util.WritePasswordAsync(IDLT645_2007, byte,string,string, CancellationToken)"/>
    public async Task<OperResult> WritePasswordAsync(byte level, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        return await DLT645_2007Util.WritePasswordAsync(this, level, oldPassword, newPassword, cancellationToken);
    }
    #endregion


}
