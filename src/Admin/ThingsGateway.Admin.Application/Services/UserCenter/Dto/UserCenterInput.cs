//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 更新个人工作台
/// </summary>
public class WorkbenchInfo : PagePolicy
{
    public long Id { get; set; }
}

/// <summary>
/// 修改密码
/// </summary>
public class UpdatePasswordInput
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required]
    public string Password { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Required]
    public string NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [Required]
    public string ConfirmPassword { get; set; }

}
