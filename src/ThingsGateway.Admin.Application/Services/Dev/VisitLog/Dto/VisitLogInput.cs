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
/// 访问日志分页输入
/// </summary>
public class VisitLogPageInput : BasePageInput
{
    /// <summary>
    /// 开始时间
    /// </summary>
    [Description("开始时间")]
    public DateTime? StartTime { get; set; } = DateTime.Now.AddDays(-1);

    /// <summary>
    /// 结束时间
    /// </summary>
    [Description("结束时间")]
    public DateTime? EndTime { get; set; } = DateTime.Now.AddDays(1);

    /// <summary>
    /// 分类
    /// </summary>
    [Description("分类")]
    public virtual string Category { get; set; } = CateGoryConst.Log_LOGIN;

    /// <summary>
    /// 账号
    /// </summary>
    [Description("账号")]
    public string Account { get; set; }
}

/// <summary>
/// 访问日志分页输入
/// </summary>
public class VisitLogInput
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; } = DateTime.Now.AddDays(-1);

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; } = DateTime.Now.AddDays(1);

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// 全部
    /// </summary>
    public bool All { get; set; }
}

/// <summary>
/// 访问日志删除输入
/// </summary>
public class VisitLogDeleteInput
{
    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }
}