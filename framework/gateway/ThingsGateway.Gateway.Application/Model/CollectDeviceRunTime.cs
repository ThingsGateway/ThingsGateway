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

using System.ComponentModel;

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 采集设备状态表示
/// </summary>
public class CollectDeviceRunTime : DeviceRunTime
{
    /// <summary>
    /// 特殊方法轮询变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<DeviceVariableMethodSource> DeviceVariableMethodReads { get; set; } = new();

    /// <summary>
    /// 特殊方法只写变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<DeviceVariableMethodSource> DeviceVariableMethodSources { get; set; } = new();

    /// <summary>
    /// 打包变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<DeviceVariableSourceRead> DeviceVariableSourceReads { get; set; } = new();

    /// <summary>
    /// 特殊方法数量
    /// </summary>
    [Description("特殊方法数量")]
    public int MethodVariableCount => DeviceVariableMethodReads.Count + DeviceVariableMethodSources.Count;

    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    [Description("打包数量")]
    public int SourceVariableCount => DeviceVariableSourceReads.Count;
}