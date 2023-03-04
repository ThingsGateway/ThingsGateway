namespace ThingsGateway.Core;

/// <summary>
/// 排序
/// </summary>
public class OrderDataAttribute : Attribute
{
    public int Order { get; set; } = 999;
}
