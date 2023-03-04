namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备属性的特性说明
/// <br></br>
/// 继承<see cref="DriverBase"/>的驱动插件，在需主动暴露的配置属性中加上这个特性<see cref="DevicePropertyAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class DevicePropertyAttribute : Attribute
{

    public string Name { get; }
    public string Remark { get; }

    public DevicePropertyAttribute(string name, string remark = null)
    {
        Name = name;
        Remark = remark;
    }
}
