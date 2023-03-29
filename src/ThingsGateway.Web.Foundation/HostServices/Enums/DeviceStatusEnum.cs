namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 设备在线状态
/// </summary>
public enum DeviceStatusEnum
{
    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")]
    OnLine = 1,
    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    OffLine = 2,
    /// <summary>
    /// 暂停
    /// </summary>
    [Description("暂停")]
    Pause = 3,
    /// <summary>
    /// 部分失败
    /// </summary>
    [Description("部分失败")]
    OnLineButNoInitialValue = 4,
    /// <summary>
    /// 初始化
    /// </summary>
    [Description("初始化")]
    Default = 5,
}
