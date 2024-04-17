//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class BusinessPropertyWithCacheIntervalScript : BusinessPropertyWithCacheInterval
{
    /// <summary>
    /// 设备Topic
    /// </summary>
    [DynamicProperty]
    public bool IsDeviceList { get; set; } = true;

    /// <summary>
    /// 变量Topic
    /// </summary>
    [DynamicProperty]
    public bool IsVariableList { get; set; } = true;

    /// <summary>
    /// 报警Topic
    /// </summary>
    [DynamicProperty]
    public bool IsAlarmList { get; set; } = true;

    /// <summary>
    /// 设备Topic
    /// </summary>
    [DynamicProperty]
    public string DeviceTopic { get; set; } = "ThingsGateway/Device";

    /// <summary>
    /// 变量Topic
    /// </summary>
    [DynamicProperty]
    public string VariableTopic { get; set; } = "ThingsGateway/Variable";

    /// <summary>
    /// 报警Topic
    /// </summary>
    [DynamicProperty]
    public string AlarmTopic { get; set; } = "ThingsGateway/Alarm";

    /// <summary>
    /// 设备实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(ComponentType = typeof(Textarea), Rows = 1)]
    public string? BigTextScriptDeviceModel { get; set; }

    /// <summary>
    /// 变量实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(ComponentType = typeof(Textarea), Rows = 1)]
    public string? BigTextScriptVariableModel { get; set; }

    /// <summary>
    /// 报警实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(ComponentType = typeof(Textarea), Rows = 1)]
    public string? BigTextScriptAlarmModel { get; set; }
}