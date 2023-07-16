#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 上传设备表
/// </summary>
[SugarTable("uploadDevice", TableDescription = "上传设备表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class UploadDevice : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "名称", Length = 200)]
    [OrderTable(Order = 1)]
    [Excel]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [OrderTable(Order = 2)]
    [Excel]
    public string Description { get; set; }

    /// <summary>
    /// 插件Id
    /// </summary>
    [SugarColumn(ColumnName = "PluginId", ColumnDescription = "插件")]
    public virtual long PluginId { get; set; }

    /// <summary>
    /// 设备使能
    /// </summary>
    [SugarColumn(ColumnName = "Enable", ColumnDescription = "设备使能")]
    [OrderTable(Order = 3)]
    [Excel]
    public virtual bool Enable { get; set; }

    /// <summary>
    /// 设备组
    /// </summary>
    [SugarColumn(ColumnName = "DeviceGroup", ColumnDescription = "设备组", IsNullable = true)]
    [OrderTable(Order = 3)]
    [Excel]
    public virtual string DeviceGroup { get; set; }

    /// <summary>
    /// 输出日志
    /// </summary>
    [SugarColumn(ColumnName = "IsLogOut", ColumnDescription = "输出日志")]
    [OrderTable(Order = 3)]
    [Excel]
    public virtual bool IsLogOut { get; set; }

    /// <summary>
    /// 设备属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnName = "DevicePropertys", ColumnDescription = "设备属性Json", IsNullable = true)]
    public List<DependencyProperty> DevicePropertys { get; set; }

}

