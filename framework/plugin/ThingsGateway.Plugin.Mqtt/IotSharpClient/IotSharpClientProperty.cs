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

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class IotSharpClientProperty : UploadPropertyWithCacheT
{

    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "127.0.0.1";
    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 1883;
    /// <summary>
    /// Accesstoken
    /// </summary>
    [DeviceProperty("Accesstoken", "")]
    public string Accesstoken { get; set; } = "Accesstoken";
    /// <summary>
    /// 连接Id
    /// </summary>
    [DeviceProperty("连接Id", "")]
    public string ConnectId { get; set; } = "ThingsGatewayId";
    /// <summary>
    /// 连接超时时间
    /// </summary>
    [DeviceProperty("连接超时时间", "")]
    public int ConnectTimeOut { get; set; } = 3000;

    /// <summary>
    /// 允许Rpc写入
    /// </summary>
    [DeviceProperty("允许Rpc写入", "")]
    public bool DeviceRpcEnable { get; set; }




}
