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
using Riok.Mapperly.Abstractions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

[OrmTable("memory_variable", TableDescription = "内存变量表")]
[Tenant(SqlOrmConst.DB_Custom)]
[OrmIndex("unique_memory_variable_name", nameof(Variable.Name), OrderByType.Asc, true)]
public partial class MemoryVariable : Variable
{

    /// <summary>
    /// 设备
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override long DeviceId { get; set; } = MemoryConst.MemoryDeviceId;

    /// <summary>
    /// 写入后再次读取检查值是否一致
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override bool RpcWriteCheck { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时RegisterAddress为方法参数
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string OtherMethod { get; set; }

    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override DataTypeEnum DataType { get; set; } = DataTypeEnum.Object;

    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string RegisterAddress { get; set; } = MemoryConst.MemoryName;

    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override int? ArrayLength { get; set; }


    [OrmColumn(ColumnDescription = "触发方式", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual BusinessUpdateEnum BusinessUpdate { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public override ValidateForm? AlarmPropertysValidateForm { get; set; }

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public override NonBlockingDictionary<long, ModelValueValidateForm>? VariablePropertyModels { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
