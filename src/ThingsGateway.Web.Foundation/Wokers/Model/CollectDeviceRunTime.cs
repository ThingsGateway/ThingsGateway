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

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集设备状态表示
/// </summary>
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
            if (Name == "m1")
            {

            }
            if (deviceStatus != value)
            {
                deviceStatus = value;
                DeviceStatusCahnge?.Invoke(this);
            }
        }
    }
    private DeviceStatusEnum deviceStatus = DeviceStatusEnum.Default;
    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event DelegateOnDeviceChanged DeviceStatusCahnge;

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
/// <summary>
/// 设备变化委托
/// </summary>
/// <param name="collectDeviceRunTime"></param>
public delegate void DelegateOnDeviceChanged(CollectDeviceRunTime collectDeviceRunTime);

