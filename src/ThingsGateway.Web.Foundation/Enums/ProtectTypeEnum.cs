namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 用户权限
/// </summary>
public enum ProtectTypeEnum
{
    [Description("只读")]
    ReadOnly,

    [Description("读写")]
    ReadWrite,

    [Description("只写")]
    WriteOnly,
}
