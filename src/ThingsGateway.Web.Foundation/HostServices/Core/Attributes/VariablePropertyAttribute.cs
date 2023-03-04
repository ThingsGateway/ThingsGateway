namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 变量属性的特性说明
/// <br></br>
/// 继承<see cref="DriverBase"/>的上传插件，在需主动暴露的变量配置属性中加上这个特性<see cref="VariablePropertyAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class VariablePropertyAttribute : Attribute
{

    public string Name { get; }
    public string Remark { get; }

    public VariablePropertyAttribute(string name, string remark = null)
    {
        Name = name;
        Remark = remark;
    }
}
