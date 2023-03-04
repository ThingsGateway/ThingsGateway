namespace ThingsGateway.Core
{
    /// <summary>
    /// 需要角色授权权限
    /// </summary>
    public class RolePermissionAttribute : Attribute
    {
    }

    /// <summary>
    /// 忽略角色授权权限
    /// </summary>
    public class IgnoreRolePermissionAttribute : Attribute
    {
    }
}