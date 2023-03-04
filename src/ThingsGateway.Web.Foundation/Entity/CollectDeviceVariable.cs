using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 设备变量表
/// </summary>
[SugarTable("collectdevice_variable", TableDescription = "设备变量表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class CollectDeviceVariable : MemoryVariable
{
    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnName = "DeviceId", ColumnDescription = "设备")]
    [OrderData(Order = 3)]
    public virtual long DeviceId { get; set; }

    /// <summary>
    /// 读取表达式
    /// </summary>
    [SugarColumn(ColumnName = "ReadExpressions", ColumnDescription = "读取表达式", Length = 200, IsNullable = true)]
    [OrderData(Order = 7)]
    public string ReadExpressions { get; set; }
    /// <summary>
    /// 写入表达式
    /// </summary>
    [SugarColumn(ColumnName = "WriteExpressions", ColumnDescription = "写入表达式", Length = 200, IsNullable = true)]
    [OrderData(Order = 7)]
    public string WriteExpressions { get; set; }
    /// <summary>
    /// 执行间隔
    /// </summary>
    [SugarColumn(ColumnName = "InvokeInterval", ColumnDescription = "执行间隔")]
    [OrderData(Order = 3)]
    public virtual int IntervalTime { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时Address为方法参数
    /// </summary>
    [SugarColumn(ColumnName = "OtherMethod", ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [OrderData(Order = 7)]
    public string OtherMethod { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [OrderData(Order = 3)]
    public string VariableAddress { get; set; }

    /// <summary>
    /// 是否允许远程Rpc写入，不包含Blazor Web页
    /// </summary>
    [SugarColumn(ColumnName = "RpcWriteEnable", ColumnDescription = "允许远程写入", Length = 200, IsNullable = true)]
    [OrderData(Order = 4)]
    public bool RpcWriteEnable { get; set; }

}
