namespace ThingsGateway.Core
{
    /// <summary>
    /// 管理员才能访问特性
    /// </summary>
    public class SuperAdminAttribute : Attribute
    {
    }

    /// <summary>
    /// 忽略超级管理员才能访问特性
    /// </summary>
    public class IgnoreSuperAdminAttribute : Attribute
    {
    }
}