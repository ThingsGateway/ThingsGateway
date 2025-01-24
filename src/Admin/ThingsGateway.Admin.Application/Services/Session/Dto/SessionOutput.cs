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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话输出
/// </summary>
public class SessionOutput : PrimaryIdEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public override long Id { get; set; }

    /// <summary>
    /// 账号
    ///</summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public virtual string Account { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    public bool Online { get; set; }

    /// <summary>
    /// 最新登录ip
    ///</summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string LatestLoginIp { get; set; }

    /// <summary>
    /// 最新登录时间
    ///</summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public DateTime? LatestLoginTime { get; set; }

    /// <summary>
    /// 令牌数量
    /// </summary>
    public int VerificatCount { get; set; }

    /// <summary>
    /// 令牌信息集合
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public List<VerificatInfo> VerificatSignList { get; set; }
}
