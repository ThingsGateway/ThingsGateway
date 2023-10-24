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

namespace ThingsGateway.Plugin.Siemens;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensProperty : CollectDriverPropertyBase
{
    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")] public override string IP { get; set; } = "127.0.0.1";
    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")] public override int Port { get; set; } = 102;
    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
    /// <summary>
    /// 读写超时时间
    /// </summary>
    [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
    /// <summary>
    /// Rack
    /// </summary>
    [DeviceProperty("机架号", "为0时不写入")] public byte Rack { get; set; } = 0;
    /// <summary>
    /// Slot
    /// </summary>
    [DeviceProperty("槽位号", "为0时不写入")] public byte Slot { get; set; } = 0;

    /// <summary>
    /// LocalTSAP
    /// </summary>
    [DeviceProperty("LocalTSAP", "为0时不写入，通常默认0即可")] public int LocalTSAP { get; set; } = 0;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsShareChannel { get; set; } = false;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ChannelEnum ShareChannel => ChannelEnum.None;
}
