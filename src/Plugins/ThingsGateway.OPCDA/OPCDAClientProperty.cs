#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Web.Foundation;

namespace ThingsGateway.OPCDA;

public class OPCDAClientProperty : CollectDriverPropertyBase
{
    [DeviceProperty("IP", "")] public string OPCIP { get; set; } = "localhost";
    [DeviceProperty("OPC名称", "")] public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";
    [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
    [DeviceProperty("检测重连频率", "")] public int CheckRate { get; set; } = 60000;
    [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
    [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;
    public override bool IsShareChannel { get; set; } = false;
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.None;
    [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;
}
