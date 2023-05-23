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

