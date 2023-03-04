using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Web.Foundation;

public class DependencyProperty
{
    /// <summary>
    /// 属性名称
    /// </summary>
    [Description("名称")]
    public string PropertyName { get; set; }
    /// <summary>
    /// 属性描述
    /// </summary>
    [Description("描述")]
    public string Description { get; set; }
    /// <summary>
    /// 属性值
    /// </summary>
    [Description("属性值")]
    public string Value { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [Description("备注")]
    [MaxLength(50)]
    public string Remark { get; set; }
}

