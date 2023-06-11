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

public class MqttClientProperty : UpDriverPropertyBase
{


    [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";

    [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
    [DeviceProperty("账号", "")] public string UserName { get; set; } = "admin";
    [DeviceProperty("密码", "")] public string Password { get; set; } = "123456";
    [DeviceProperty("连接Id", "")] public string ConnectId { get; set; } = "ThingsGatewayId";

    [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;

    [DeviceProperty("线程循环间隔", "最小10ms")] public int CycleInterval { get; set; } = 1000;
    [DeviceProperty("缓存最大条数", "默认2千条")] public int CacheMaxCount { get; set; } = 2000;

    [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }

    [DeviceProperty("数据请求RpcTopic", "这个主题接收到任何数据都会把全部的信息发送到变量/设备主题中")] public string QuestRpcTopic { get; set; } = "ThingsGateway/Quest";


    [DeviceProperty("设备Topic", "")] public string DeviceTopic { get; set; } = "ThingsGateway/Device";
    [DeviceProperty("变量Topic", "")] public string VariableTopic { get; set; } = "ThingsGateway/Variable";
    [DeviceProperty("Rpc返回Topic", "")] public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";

    [DeviceProperty("Rpc写入Topic", "")] public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
    [DeviceProperty("设备实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptDeviceModel { get; set; }

    [DeviceProperty("变量实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptVariableModel { get; set; }


    [DeviceProperty("是否间隔上传", "False时为变化检测上传")] public bool IsInterval { get; set; } = false;
    [DeviceProperty("上传间隔时间", "最小1000ms")] public int UploadInterval { get; set; } = 1000;

}
