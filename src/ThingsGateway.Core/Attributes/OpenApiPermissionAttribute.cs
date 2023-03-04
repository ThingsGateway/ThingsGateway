namespace ThingsGateway.Core
{
    /// <summary>
    /// 需要授权权限
    /// </summary>
    public class OpenApiPermissionAttribute : Attribute
    {
    }

    /// <summary>
    /// 忽略授权权限
    /// </summary>
    public class IgnoreOpenApiPermissionAttribute : Attribute
    {
    }
}