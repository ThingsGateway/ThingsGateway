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

using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Kafka;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class KafkaProducerProperty : DriverPropertyBase
{
    [DeviceProperty("服务地址", "")] public string BootStrapServers { get; set; } = "127.0.0.1:9092";
    [DeviceProperty("设备主题", "")] public string DeviceTopic { get; set; } = "test1";
    [DeviceProperty("变量主题", "")] public string VariableTopic { get; set; } = "test2";
    [DeviceProperty("客户端ID", "")] public string ClientId { get; set; } = "test-consumer";
    [DeviceProperty("线程循环间隔", "最小10ms")] public int CycleInterval { get; set; } = 1000;
    [DeviceProperty("发布超时时间", "ms")] public int TimeOut { get; set; } = 5000;

    [DeviceProperty("列表分割大小", "默认1千条")] public int SplitSize { get; set; } = 1000;
    [DeviceProperty("缓存最大条数", "默认2千条")] public int CacheMaxCount { get; set; } = 2000;
}
