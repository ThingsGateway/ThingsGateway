#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Mqtt;

public class IotSharpClientProperty : UpDriverPropertyBase
{
    [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
    [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
    [DeviceProperty("Accesstoken", "")] public string Accesstoken { get; set; } = "Accesstoken";
    [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }
    [DeviceProperty("缓存最大条数", "默认2千条")] public int CacheMaxCount { get; set; } = 2000;
    [DeviceProperty("列表分割大小", "默认1千条")] public int SplitSize { get; set; } = 1000;
    [DeviceProperty("线程循环间隔", "最小10ms")] public int CycleInterval { get; set; } = 1000;
}
