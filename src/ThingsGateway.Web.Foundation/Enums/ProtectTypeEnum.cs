namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 用户权限
/// </summary>
public enum ProtectTypeEnum
{
    /// <summary>
    /// 只读
    /// </summary>
    [Description("只读")]
    ReadOnly,
    /// <summary>
    /// 读写
    /// </summary>
    [Description("读写")]
    ReadWrite,
    /// <summary>
    /// 只写
    /// </summary>
    [Description("只写")]
    WriteOnly,
}
