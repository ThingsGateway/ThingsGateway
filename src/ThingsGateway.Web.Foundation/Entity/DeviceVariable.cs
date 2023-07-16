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
using ThingsGateway.Foundation;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 设备变量表
/// </summary>
[SugarTable("deviceVariable", TableDescription = "设备变量表")]
[Tenant(SqlsugarConst.DB_CustomId)]
[SugarIndex("index_device", nameof(DeviceVariable.DeviceId), OrderByType.Asc)]
[SugarIndex("unique_deviceVariable_name", nameof(DeviceVariable.Name), OrderByType.Asc, true)]
public class DeviceVariable : MemoryVariable
{
    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnName = "DeviceId", ColumnDescription = "设备")]
    [OrderTable(Order = 3)]
    public virtual long DeviceId { get; set; }


    /// <summary>
    /// 写入表达式
    /// </summary>
    [SugarColumn(ColumnName = "WriteExpressions", ColumnDescription = "写入表达式", Length = 200, IsNullable = true)]
    [OrderTable(Order = 7)]
    [Excel]
    public string WriteExpressions { get; set; }
    /// <summary>
    /// 执行间隔
    /// </summary>
    [SugarColumn(ColumnName = "InvokeInterval", ColumnDescription = "执行间隔")]
    [OrderTable(Order = 3)]
    [Excel]
    public virtual int IntervalTime { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时Address为方法参数
    /// </summary>
    [SugarColumn(ColumnName = "OtherMethod", ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [OrderTable(Order = 7)]
    [Excel]
    public string OtherMethod { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [OrderTable(Order = 3)]
    [Excel]
    public string VariableAddress { get; set; }


    /// <summary>
    /// 是否中间变量
    /// </summary>
    [SugarColumn(ColumnName = "IsMemoryVariable", ColumnDescription = "是否中间变量", IsNullable = false)]
    public override bool? IsMemoryVariable { get; set; } = false;
}

/// <summary>
/// 用户权限
/// </summary>
public enum ProtectTypeEnum
{
    /// <summary>
    /// 只读
    /// </summary>
    [Description("只读")]
    ReadOnly,
    /// <summary>
    /// 读写
    /// </summary>
    [Description("读写")]
    ReadWrite,
    /// <summary>
    /// 只写
    /// </summary>
    [Description("只写")]
    WriteOnly,
}

/// <summary>
/// 数据类型
/// </summary>
public enum DataTypeEnum
{
    /// <inheritdoc/>
    Object,

    /// <inheritdoc/>
    String,
    /// <inheritdoc/>
    Boolean,
    /// <inheritdoc/>
    Byte,
    /// <inheritdoc/>
    Int16,
    /// <inheritdoc/>
    UInt16,
    /// <inheritdoc/>
    Int32,
    /// <inheritdoc/>
    UInt32,
    /// <inheritdoc/>
    Int64,
    /// <inheritdoc/>
    UInt64,
    /// <inheritdoc/>
    Single,
    /// <inheritdoc/>
    Double,
}