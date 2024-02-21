//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 用户选择器参数
/// </summary>
public class UserSelectorInput : BasePageInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public override string SearchKey { get; set; }
}

/// <summary>
/// 用户分页查询参数
/// </summary>
public class UserPageInput : BasePageInput
{
    /// <summary>
    /// 动态查询条件
    /// </summary>
    public Expressionable<SysUser> Expression { get; set; }

    /// <summary>
    /// 用户状态
    /// </summary>

    public bool? UserStatus { get; set; }
}

/// <summary>
/// 添加用户参数
/// </summary>
public class UserAddInput : SysUser
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "Account不能为空")]
    public override string Account { get; set; }
}

/// <summary>
/// 编辑用户参数
/// </summary>
public class UserEditInput : UserAddInput
{
    /// <summary>
    /// Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }
}

/// <summary>
/// 授权用户角色参数
/// </summary>
public class UserGrantRoleInput
{
    /// <summary>
    /// Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 授权权限信息
    /// </summary>
    [Required(ErrorMessage = "RoleIdList不能为空")]
    public List<long> RoleIdList { get; set; }
}

public class UserGrantResourceInput : GrantResourceInput
{
}