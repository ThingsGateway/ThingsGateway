//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Management;

public class VariableDataWithValue
{
    /// <inheritdoc cref="Variable.Name"/>
    public string Name { get; set; }

    /// <inheritdoc cref="VariableRuntime.Value"/>
    public object RawValue { get; set; }

    /// <inheritdoc cref="VariableRuntime.CollectTime"/>
    public DateTime CollectTime { get; set; }

    /// <inheritdoc cref="VariableRuntime.IsOnline"/>
    public bool IsOnline { get; set; }

    /// <inheritdoc cref="VariableRuntime.LastErrorMessage"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? LastErrorMessage { get; set; }
}
