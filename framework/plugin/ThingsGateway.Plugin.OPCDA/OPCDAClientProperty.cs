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

namespace ThingsGateway.Plugin.OPCDA;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class OPCDAClientProperty : DriverPropertyBase
{
    /// <summary>
    /// OPCIP
    /// </summary>
    [DeviceProperty("IP", "")] public string OPCIP { get; set; } = "localhost";

    /// <summary>
    /// OPC名称
    /// </summary>
    [DeviceProperty("OPC名称", "")] public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";

    /// <summary>
    /// 激活订阅
    /// </summary>
    [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;

    /// <summary>
    /// 检测重连频率/min
    /// </summary>
    [DeviceProperty("检测重连频率/min", "")] public int CheckRate { get; set; } = 10;

    /// <summary>
    /// 死区
    /// </summary>
    [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;

    /// <summary>
    /// 自动分组大小
    /// </summary>
    [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;

    /// <summary>
    /// 更新频率
    /// </summary>
    [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsShareChannel { get; set; } = false;

    /// <inheritdoc/>
    public override ChannelEnum ShareChannel => ChannelEnum.None;
}