
using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;
/// <summary>
/// 设备上传DTO
/// </summary>
public class DeviceData
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long id { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.PluginName"/>
    public string pluginName { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.DeviceVariablesNum"/>
    public int deviceVariablesNum { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.ActiveTime"/>
    public DateTime activeTime { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.DeviceStatus"/>
    public int deviceStatus { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.DeviceOffMsg"/>
    public string deviceOffMsg { get; set; }
    /// <inheritdoc cref="UploadDevice.Name"/>
    public string name { get; set; }
    /// <inheritdoc cref="UploadDevice.Description"/>
    public string description { get; set; }
    /// <inheritdoc cref="UploadDevice.Enable"/>
    public bool enable { get; set; }

}

