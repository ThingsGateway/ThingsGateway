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

using System.ComponentModel;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话输出
/// </summary>
public class OpenApiSessionOutput : PrimaryKeyEntity
{
    /// <summary>
    /// 账号
    ///</summary>
    [Description("账号")]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Account { get; set; }

    /// <summary>
    /// 最新登录ip
    ///</summary>
    [Description("最新登录ip")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string LatestLoginIp { get; set; }

    /// <summary>
    /// 最新登录时间
    ///</summary>
    [Description("最新登录时间")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public DateTime? LatestLoginTime { get; set; }

    /// <summary>
    /// 令牌数量
    /// </summary>
    [Description("令牌数量")]
    [DataTable(Order = 4, IsShow = true, Sortable = false)]
    public int VerificatCount { get; set; }

    /// <summary>
    /// 令牌信息集合
    /// </summary>
    [Description("令牌列表")]
    public List<VerificatInfo> VerificatSignList { get; set; }
}