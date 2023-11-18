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

using Opc.Ua;

namespace ThingsGateway.Plugin.OPCUA;

/// <inheritdoc/>
public class OPCUAServerProperty : DriverPropertyBase
{
    [DeviceProperty("是否选择全部变量", "")] public bool IsAllVariable { get; set; } = false;
    /// <summary>
    /// 服务地址
    /// </summary>
    [DeviceProperty("服务地址", "分号分割数组，可设置多个url")]
    public string OpcUaStringUrl { get; set; } = "opc.tcp://127.0.0.1:49321";
    /// <summary>
    /// SubjectName
    /// </summary>
    [DeviceProperty("SubjectName", "")]
    public string BigTextSubjectName { get; set; } = "CN=ThingsGateway OPCUAServer, C=CN, S=GUANGZHOU, O=ThingsGateway, DC=" + System.Net.Dns.GetHostName();

    /// <summary>
    /// ApplicationUri
    /// </summary>
    [DeviceProperty("ApplicationUri", "")]
    public string BigTextApplicationUri { get; set; } = Utils.Format(@"urn:{0}:thingsgatewayopcuaserver", System.Net.Dns.GetHostName());


    /// <summary>
    /// 安全策略
    /// </summary>
    [DeviceProperty("安全策略", "")]
    public bool SecurityPolicy { get; set; }
    /// <summary>
    /// 接受不受信任的证书
    /// </summary>
    [DeviceProperty("自动接受不受信任的证书", "")]
    public bool AutoAcceptUntrustedCertificates { get; set; } = true;
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 1000;
}
