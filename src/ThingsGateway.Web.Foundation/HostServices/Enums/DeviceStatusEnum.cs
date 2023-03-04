namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 设备在线状态
/// </summary>
public enum DeviceStatusEnum
{
    [Description("在线")]
    OnLine = 1,
    [Description("离线")]
    OffLine = 2,
    [Description("暂停")]
    Pause = 3,
    [Description("部分失败")]
    OnLineButNoInitialValue = 4,
    [Description("初始化")]
    Default = 5,
}
