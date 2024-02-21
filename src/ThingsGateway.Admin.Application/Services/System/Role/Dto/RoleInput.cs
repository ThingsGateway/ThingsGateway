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
/// 角色查询参数
/// </summary>
public class RolePageInput : BasePageInput
{
    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }
}

/// <summary>
/// 角色添加参数
/// </summary>
public class RoleAddInput : SysRole
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "Name不能为空")]
    public override string Name { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    [Required(ErrorMessage = "Category不能为空")]
    public override string Category { get; set; } = CateGoryConst.Role_GLOBAL;
}

/// <summary>
/// 角色编辑参数
/// </summary>
public class RoleEditInput : RoleAddInput
{
    /// <summary>
    /// Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }
}

/// <summary>
/// 角色授权资源参数
/// </summary>
public class GrantResourceInput : RoleOwnResourceOutput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }

    /// <summary>
    /// 授权资源信息
    /// </summary>
    [Required(ErrorMessage = "GrantInfoList不能为空")]
    public override List<RelationRoleResource> GrantInfoList { get; set; }
}

/// <summary>
/// 角色授权权限参数
/// </summary>
public class GrantPermissionInput : RoleOwnPermissionOutput
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }

    /// <summary>
    /// 授权权限信息
    /// </summary>
    [Required(ErrorMessage = "GrantInfoList不能为空")]
    public override List<RelationRolePermission> GrantInfoList { get; set; }
}

/// <summary>
/// 角色授权用户参数
/// </summary>
public class GrantUserInput
{
    /// <summary>
    /// Id
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 授权权限信息
    /// </summary>
    [Required(ErrorMessage = "GrantInfoList不能为空")]
    public List<long> GrantInfoList { get; set; }
}

/// <summary>
/// 角色选择器参数
/// </summary>
public class RoleSelectorInput : BasePageInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public override string SearchKey { get; set; }

    /// <summary>
    /// 角色分类
    /// </summary>
    public string Category { get; set; }
}