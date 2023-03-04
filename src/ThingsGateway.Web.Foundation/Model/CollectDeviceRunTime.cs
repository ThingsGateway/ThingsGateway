
namespace ThingsGateway.Web.Foundation;

public class CollectDeviceRunTime : CollectDevice
{
    /// <summary>
    /// 设备驱动名称
    /// </summary>
    [Description("设备驱动名称")]
    public string PluginName { get; set; }
    /// <summary>
    /// 设备属性数量
    /// </summary>
    [Description("属性数量")]
    public int PropertysNum { get => DevicePropertys == null ? 0 : DevicePropertys.Count; }
    /// <summary>
    /// 设备变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<CollectVariableRunTime> DeviceVariableRunTimes { get; set; }
    /// <summary>
    /// 设备变量数量
    /// </summary>
    [Description("变量数量")]
    public int DeviceVariablesNum { get => DeviceVariableRunTimes == null ? 0 : DeviceVariableRunTimes.Count; }

    /// <summary>
    /// 设备读取分包数量
    /// </summary>
    [Description("分包数量")]
    public int SourceVariableNum { get; set; }
    /// <summary>
    /// 设备特殊方法数量
    /// </summary>
    [Description("特殊方法数量")]
    public int MethodVariableNum { get; set; }
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
                DeviceStatusCahnge?.Invoke(this);
            }
        }
    }
    private DeviceStatusEnum deviceStatus = DeviceStatusEnum.Default;
    public event DelegateOnDeviceChanged DeviceStatusCahnge;

    private string deviceOffMsg;
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
public delegate void DelegateOnDeviceChanged(CollectDeviceRunTime collectDeviceRunTime);

