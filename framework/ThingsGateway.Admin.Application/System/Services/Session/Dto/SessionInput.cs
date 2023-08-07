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
using System.ComponentModel.DataAnnotations;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话分页查询
/// </summary>
public class SessionPageInput : BasePageInput
{
    /// <summary>
    /// 账号
    /// </summary>
    [Description("账号")]
    public string Account { get; set; }
    /// <summary>
    /// 最新登录IP
    /// </summary>
    [Description("最新登录IP")]
    public string LatestLoginIp { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Description("姓名")]
    public string Name { get; set; }
}

/// <summary>
/// 退出参数
/// </summary>
public class ExitVerificatInput : BaseIdInput
{
    /// <summary>
    /// 验证ID列表
    /// </summary>
    [Required(ErrorMessage = "VerificatIds不能为空")]
    public List<long> VerificatIds { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }
}