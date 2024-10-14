//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
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
    [DynamicProperty(Remark = "可使用${key}作为匹配项，key必须是上传实体中的属性，比如ThingsGateway/Device/${Name}")]
    public string DeviceTopic { get; set; }

    /// <summary>
    /// 变量Topic
    /// </summary>
    [DynamicProperty(Remark = "可使用${key}作为匹配项，key必须是上传实体中的属性，比如ThingsGateway/Variable/${DeviceName}")]
    public string VariableTopic { get; set; } = "ThingsGateway/Variable";

    /// <summary>
    /// 报警Topic
    /// </summary>
    [DynamicProperty(Remark = "可使用${key}作为匹配项，key必须是上传实体中的属性，比如ThingsGateway/Alarm/${DeviceName}")]
    public string AlarmTopic { get; set; }

    /// <summary>
    /// 设备实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(Visible = true, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false)]
    public string? BigTextScriptDeviceModel { get; set; }

    /// <summary>
    /// 变量实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(Visible = true, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false)]
    public string? BigTextScriptVariableModel { get; set; }

    /// <summary>
    /// 报警实体脚本
    /// </summary>
    [DynamicProperty]
    [AutoGenerateColumn(Visible = true, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false)]
    public string? BigTextScriptAlarmModel { get; set; }
}
