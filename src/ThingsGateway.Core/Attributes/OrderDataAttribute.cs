namespace ThingsGateway.Core;

/// <summary>
/// 排序
/// </summary>
public class OrderTableAttribute : Attribute
{
    public int Order { get; set; } = 999;
}
