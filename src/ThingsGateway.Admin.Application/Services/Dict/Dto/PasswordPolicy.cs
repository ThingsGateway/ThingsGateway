
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Admin.Application;

public class PasswordPolicy
{
    /// <summary>
    /// 默认用户密码
    /// </summary>
    [Required]
    public string DefaultPassword { get; set; }

    /// <summary>
    /// 密码最小长度
    /// </summary>
    [MinValue(1)]
    public int PasswordMinLen { get; set; }

    /// <summary>
    /// 包含数字
    /// </summary>
    public bool PasswordContainNum { get; set; }

    /// <summary>
    /// 包含小写字母
    /// </summary>
    public bool PasswordContainLower { get; set; }

    /// <summary>
    /// 包含大写字母
    /// </summary>
    public bool PasswordContainUpper { get; set; }

    /// <summary>
    /// 包含特殊字符
    /// </summary>
    public bool PasswordContainChar { get; set; }
}