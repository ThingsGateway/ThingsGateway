namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备方法的特性说明
/// <br></br>
/// 继承<see cref="DriverBase"/>的驱动插件，在需主动暴露的方法中加上这个特性<see cref="DeviceMethodAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class DeviceMethodAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public DeviceMethodAttribute(string name, string description = "")
    {
        Name = name;
        Description = description;
    }
}
