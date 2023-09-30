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

namespace ThingsGateway.Gateway.Core;
/// <summary>
/// 设备上传DTO
/// </summary>
public class DeviceData
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long Id { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.PluginName"/>
    public string PluginName { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.DeviceVariableCount"/>
    public int DeviceVariablesCount { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.ActiveTime"/>
    public DateTime ActiveTime { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.DeviceStatus"/>
    public int DeviceStatus { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.LastErrorMessage"/>
    public string LastErrorMessage { get; set; }
    /// <inheritdoc cref="UploadDevice.Name"/>
    public string Name { get; set; }
    /// <inheritdoc cref="UploadDevice.Description"/>
    public string Description { get; set; }
    /// <inheritdoc cref="CollectDeviceRunTime.KeepRun"/>
    public bool KeepRun { get; set; }

}

