namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备属性的特性说明
/// <br></br>
/// 继承<see cref="DriverPropertyBase"/>，在需主动暴露的配置属性中加上这个特性<see cref="DevicePropertyAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class DevicePropertyAttribute : Attribute
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; }
    /// <inheritdoc cref="DevicePropertyAttribute"/>
    public DevicePropertyAttribute(string name, string desc = "")
    {
        Name = name;
        Description = desc;
    }
}
