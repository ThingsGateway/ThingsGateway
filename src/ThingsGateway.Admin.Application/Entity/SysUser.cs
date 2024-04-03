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

using Mapster;

using SqlSugar;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 系统用户表
///</summary>
[SugarTable("sys_user", TableDescription = "系统用户表")]
[Tenant(SqlSugarConst.DB_Admin)]
public class SysUser : BaseEntity
{
    /// <summary>
    /// 头像
    ///</summary>
    [SugarColumn(ColumnDescription = "头像", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = false, Filterable = false)]
    [AdaptIgnore]
    public virtual string? Avatar { get; set; }

    /// <summary>
    /// 账号
    ///</summary>
    [SugarColumn(ColumnDescription = "账号", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual string Account { get; set; }

    /// <summary>
    /// 密码
    ///</summary>
    [SugarColumn(ColumnDescription = "密码", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [AutoGenerateColumn(Ignore = true)]
    public string Password { get; set; }

    /// <summary>
    /// 是否启用
    ///</summary>
    [SugarColumn(ColumnDescription = "是否启用")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public bool Status { get; set; }

    /// <summary>
    /// 手机
    /// 这里使用了SM4自动加密解密
    ///</summary>
    [SugarColumn(ColumnDescription = "手机", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱
    ///</summary>
    [SugarColumn(ColumnDescription = "邮箱", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string? Email { get; set; }

    /// <summary>
    /// 上次登录ip
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录ip", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// 上次登录设备
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录设备", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LastLoginDevice { get; set; }

    /// <summary>
    /// 上次登录时间
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录时间", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 上次登录地点
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录地点", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string LastLoginAddress { get; set; }

    /// <summary>
    /// 最新登录ip
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录ip", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LatestLoginIp { get; set; }

    /// <summary>
    /// 最新登录时间
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录时间", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public DateTime? LatestLoginTime { get; set; }

    /// <summary>
    /// 最新登录设备
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录设备", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LatestLoginDevice { get; set; }

    /// <summary>
    /// 最新登录地点
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录地点", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string LatestLoginAddress { get; set; }

    /// <summary>
    /// 默认模块
    ///</summary>
    [SugarColumn(ColumnDescription = "默认模块")]
    [AutoGenerateColumn(Ignore = true)]
    public long DefaultModule { get; set; }

    #region other

    /// <summary>
    /// 按钮码集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<string, List<string>> ButtonCodeList { get; set; } = new();

    /// <summary>
    /// 权限码集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public IEnumerable<string> PermissionCodeList { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// 角色码集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public IEnumerable<string> RoleCodeList { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// 角色ID集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public IEnumerable<long> RoleIdList { get; set; } = Enumerable.Empty<long>();

    /// <summary>
    /// 数据范围集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public IEnumerable<DataScope> DataScopeList { get; set; } = Enumerable.Empty<DataScope>();

    /// <summary>
    /// 模块集合
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public IEnumerable<SysResource> ModuleList { get; set; } = Enumerable.Empty<SysResource>();

    #endregion other
}

/// <summary>
/// 数据范围类
/// </summary>
public class DataScope
{
    /// <summary>
    /// API接口
    /// </summary>
    public string ApiUrl { get; set; }
}