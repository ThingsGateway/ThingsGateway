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

using ThingsGateway.Foundation;

namespace ThingsGateway.Application;
/// <summary>
/// 设备变量表
/// </summary>
[SugarTable("deviceVariable", TableDescription = "设备变量表")]
[Tenant(ThingsGatewayConst.DB_ThingsGateway)]
[SugarIndex("index_device", nameof(DeviceVariable.DeviceId), OrderByType.Asc)]
[SugarIndex("unique_deviceVariable_name", nameof(DeviceVariable.Name), OrderByType.Asc, true)]
public class DeviceVariable : MemoryVariable
{
    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnName = "DeviceId", ColumnDescription = "设备")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    [IgnoreExcel]
    public virtual long DeviceId { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [SugarColumn(ColumnName = "Unit", ColumnDescription = "单位", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string Unit { get; set; }

    /// <summary>
    /// 执行间隔
    /// </summary>
    [SugarColumn(ColumnName = "InvokeInterval", ColumnDescription = "执行间隔")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public virtual int IntervalTime { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时Address为方法参数
    /// </summary>
    [SugarColumn(ColumnName = "OtherMethod", ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, Sortable = true)]

    public string OtherMethod { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string VariableAddress { get; set; }



    /// <summary>
    /// 是否中间变量
    /// </summary>
    [SugarColumn(ColumnName = "IsMemoryVariable", ColumnDescription = "是否中间变量", IsNullable = false)]
    [IgnoreExcel]
    public override bool IsMemoryVariable { get; set; } = false;
}

