//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

using ThingsGateway.Core.List;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话信息
/// </summary>

[SugarTable("verificatinfo", TableDescription = "验证缓存表")]
[Tenant(SqlSugarConst.DB_TokenCache)]
public class VerificatInfo : PrimaryIdEntity
{
    /// <summary>
    /// 客户端ID列表
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    [SugarColumn(ColumnDescription = "客户端ID列表", IsNullable = true, IsJson = true)]
    public ConcurrentList<long> ClientIds { get; set; } = new();

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public long UserId { get; set; }

    /// <summary>
    /// 验证Id
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [IgnoreExcel]
    public override long Id { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SugarColumn(IsIgnore = true)]
    public bool Online => ClientIds.Any();

    /// <summary>
    /// 过期时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public int Expire { get; set; }

    /// <summary>
    /// verificat剩余有效期
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    [SugarColumn(IsIgnore = true)]
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string VerificatRemain { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public DateTime VerificatTimeout { get; set; }

    /// <summary>
    /// 登录设备
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true, Width = 100)]
    public AuthDeviceTypeEnum Device { get; set; }
}
