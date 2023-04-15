namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 变量上传属性的特性说明
/// <br></br>
/// 继承<see cref="VariablePropertyBase"/>，在需主动暴露的变量配置属性中加上这个特性<see cref="VariablePropertyAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class VariablePropertyAttribute : Attribute
{
    /// <summary>
    /// 变量属性名称
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 附加说明
    /// </summary>
    public string Description { get; }
    /// <inheritdoc cref="VariablePropertyAttribute"/>>
    public VariablePropertyAttribute(string name, string desc = "")
    {
        Name = name;
        Description = desc;
    }
}
