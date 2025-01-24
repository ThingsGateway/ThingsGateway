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
/// 登录输入参数
/// </summary>
public class OpenApiLoginInput
{
    /// <summary>
    /// 账号
    ///</summary>
    /// <example>SuperAdmin</example>
    [Required]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    ///</summary>
    ///<example>111111</example>
    [Required]
    public string Password { get; set; }


    /// <summary>
    /// 租户Id
    ///</summary>
    ///<example>252885263003720</example>
    [Required]
    public long TenantId { get; set; }
}

/// <summary>
/// 登录输入参数
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 账号
    ///</summary>
    /// <example>SuperAdmin</example>
    [Required]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    ///</summary>
    ///<example>111111</example>
    [Required]
    public string Password { get; set; }
    /// <summary>
    /// 租户ID
    /// </summary>
    ///<example>252885263003720</example>
    public long? TenantId { get; set; } = RoleConst.DefaultTenantId;

    /// <summary>
    /// 设备类型，默认PC
    /// </summary>
    /// <example>0</example>
    public string Device { get; set; } = "PC";
}
