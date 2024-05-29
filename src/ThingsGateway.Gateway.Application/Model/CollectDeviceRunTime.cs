//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集设备状态表示
/// </summary>
public class CollectDeviceRunTime : DeviceRunTime
{
    /// <summary>
    /// 特殊方法变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableMethod>? VariableMethods { get; set; }

    /// <summary>
    /// 特殊方法变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableMethod>? ReadVariableMethods { get; set; }

    /// <summary>
    /// 打包变量
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public List<VariableSourceRead>? VariableSourceReads { get; set; }

    /// <summary>
    /// 特殊方法数量
    /// </summary>
    public int MethodVariableCount => VariableMethods?.Count ?? 0;

    /// <summary>
    /// 设备读取打包数量
    /// </summary>
    public int SourceVariableCount => VariableSourceReads?.Count ?? 0;
}
