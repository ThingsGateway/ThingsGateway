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

using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道表
/// </summary>
[OrmTable("channel", TableDescription = "通道表")]
[Tenant(SqlOrmConst.DB_Custom)]
[OrmIndex("unique_channel_name", nameof(Channel.Name), OrderByType.Asc, true)]
public class Channel : ChannelOptionsBase, IPrimaryIdEntity, IBaseDataEntity, IBaseEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [OrmColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false, Sortable = true, DefaultSort = true, DefaultSortOrder = SortOrder.Asc)]
    [System.ComponentModel.DataAnnotations.Key]
    public virtual long Id { get; set; }

    /// <summary>
    /// 通道名称
    /// </summary>
    [OrmColumn(ColumnDescription = "名称", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    [StringNotMemory]
    public virtual string Name { get; set; }

    /// <inheritdoc/>
    [OrmColumn(ColumnDescription = "通道类型", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override ChannelTypeEnum ChannelType { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    [OrmColumn(ColumnDescription = "插件名称")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    public virtual string PluginName { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [OrmColumn(ColumnDescription = "使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// LogLevel
    /// </summary>
    [OrmColumn(ColumnDescription = "日志等级")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// 远程地址，可由<see cref="IPHost"/> 与 <see cref="string"/> 相互转化
    /// </summary>
    [OrmColumn(ColumnDescription = "远程地址", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [UriValidation]
    public override string RemoteUrl { get; set; } = "127.0.0.1:502";

    /// <summary>
    /// 本地地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    [OrmColumn(ColumnDescription = "本地地址", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [UriValidation]
    public override string BindUrl { get; set; }

    /// <summary>
    /// COM
    /// </summary>
    [OrmColumn(ColumnDescription = "COM", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override string PortName { get; set; } = "COM1";

    /// <summary>
    /// 波特率
    /// </summary>
    [OrmColumn(ColumnDescription = "波特率", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override int BaudRate { get; set; } = 9600;

    /// <summary>
    /// 数据位
    /// </summary>
    [OrmColumn(ColumnDescription = "数据位", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override int DataBits { get; set; } = 8;

    /// <summary>
    /// 校验位
    /// </summary>
    [OrmColumn(ColumnDescription = "校验位", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override Parity Parity { get; set; }

    /// <summary>
    /// 停止位
    /// </summary>
    [OrmColumn(ColumnDescription = "停止位", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// DtrEnable
    /// </summary>
    [OrmColumn(ColumnDescription = "DtrEnable", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override bool DtrEnable { get; set; }

    /// <summary>
    /// RtsEnable
    /// </summary>
    [OrmColumn(ColumnDescription = "RtsEnable", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override bool RtsEnable { get; set; }

    /// <summary>
    /// StreamAsync
    /// </summary>
    [OrmColumn(ColumnDescription = "StreamAsync", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override bool StreamAsync { get; set; }

    /// <summary>
    /// Handshake
    /// </summary>
    [OrmColumn(ColumnDescription = "Handshake", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override Handshake Handshake { get; set; }

    /// <summary>
    /// 缓存超时
    /// </summary>
    [OrmColumn(ColumnDescription = "缓存超时", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [MinValue(100)]
    public override int CacheTimeout { get; set; } = 500;

    /// <summary>
    /// 连接超时
    /// </summary>
    [OrmColumn(ColumnDescription = "连接超时", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [MinValue(100)]
    public override int ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// 最大并发数
    /// </summary>
    [OrmColumn(ColumnDescription = "最大并发数", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [MinValue(1)]
    public override int MaxConcurrentCount { get; set; } = 1;

    [OrmColumn(ColumnDescription = "最大连接数", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override int MaxClientCount { get; set; } = 10000;

    [OrmColumn(ColumnDescription = "客户端滑动过期时间", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override int CheckClearTime { get; set; } = 120000;

    [OrmColumn(ColumnDescription = "心跳内容", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override string Heartbeat { get; set; }

    [OrmColumn(ColumnDescription = "心跳内容是否Hex", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override bool HeartbeatHex { get; set; }

    #region dtu终端

    [OrmColumn(ColumnDescription = "心跳间隔", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override int HeartbeatTime { get; set; } = 60000;

    [OrmColumn(ColumnDescription = "DtuId", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override string DtuId { get; set; }

    [OrmColumn(ColumnDescription = "DtuId是否Hex", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override bool DtuIdHex { get; set; }
    #endregion

    [OrmColumn(ColumnDescription = "Dtu类型", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public override DtuSeviceType DtuSeviceType { get; set; }

    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [OrmColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual long CreateOrgId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [OrmColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [OrmColumn(ColumnDescription = "创建人", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [NotNull]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string? CreateUser { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [OrmColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long CreateUserId { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [OrmColumn(ColumnDescription = "软删除", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual bool IsDelete { get; set; } = false;

    /// <summary>
    /// 更新时间
    /// </summary>
    [OrmColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [OrmColumn(ColumnDescription = "更新人", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string UpdateUser { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [OrmColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long UpdateUserId { get; set; }

    /// <summary>
    /// 排序码
    ///</summary>
    [OrmColumn(ColumnDescription = "排序码", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, DefaultSort = true, Sortable = true, DefaultSortOrder = SortOrder.Asc)]
    [IgnoreExcel]
    public int SortCode { get; set; }

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal bool IsUp;
}
