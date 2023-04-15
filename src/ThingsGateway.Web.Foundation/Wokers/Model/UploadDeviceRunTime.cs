
namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 上传设备运行状态
/// </summary>
public class UploadDeviceRunTime : UploadDevice
{
    /// <summary>
    /// 设备驱动名称
    /// </summary>
    [Description("设备驱动名称")]
    public string PluginName { get; set; }
    /// <summary>
    /// 关联变量数量
    /// </summary>
    [Description("关联变量数量")]
    public int UploadVariableNum { get; set; }

    /// <summary>
    /// 设备活跃时间
    /// </summary>
    [Description("活跃时间")]
    public DateTime ActiveTime { get; set; } = DateTime.MinValue;
    /// <summary>
    /// 设备状态
    /// </summary>
    [Description("设备状态")]
    public DeviceStatusEnum DeviceStatus
    {
        get
        {
            return deviceStatus;
        }
        set
        {
            if (deviceStatus != value)
            {
                deviceStatus = value;
            }
        }
    }
    private DeviceStatusEnum deviceStatus = DeviceStatusEnum.Default;

    private string deviceOffMsg;

    /// <summary>
    /// 失败原因
    /// </summary>
    [Description("失败原因")]
    public string DeviceOffMsg
    {
        get
        {
            if (deviceStatus == DeviceStatusEnum.OnLine)
            {
                return "";
            }
            else
                return deviceOffMsg;
        }

        set => deviceOffMsg = value;
    }
}

