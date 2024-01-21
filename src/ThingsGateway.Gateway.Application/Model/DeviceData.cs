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

using Newtonsoft.Json;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备业务变化数据
/// </summary>
public class DeviceData : IPrimaryIdEntity
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long Id { get; set; }

    /// <inheritdoc cref="Device.Name"/>
    public string Name { get; set; }

    /// <inheritdoc cref="DeviceRunTime.ActiveTime"/>
    public DateTime ActiveTime { get; set; }

    /// <inheritdoc cref="DeviceRunTime.DeviceStatus"/>
    public DeviceStatusEnum DeviceStatus { get; set; }

    /// <inheritdoc cref="DeviceRunTime.LastErrorMessage"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string LastErrorMessage { get; set; }

    /// <inheritdoc cref="Device.Remark1"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark1 { get; set; }

    /// <inheritdoc cref="Device.Remark2"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark2 { get; set; }

    /// <inheritdoc cref="Device.Remark3"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark3 { get; set; }

    /// <inheritdoc cref="Device.Remark4"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark4 { get; set; }

    /// <inheritdoc cref="Device.Remark5"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark5 { get; set; }
}

/// <summary>
/// 设备业务基础数据
/// </summary>
public class DeviceBasicData : DeviceData
{
    /// <inheritdoc cref="Device.PluginName"/>
    public string PluginName { get; set; }

    /// <inheritdoc cref="Device.Description"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }
}