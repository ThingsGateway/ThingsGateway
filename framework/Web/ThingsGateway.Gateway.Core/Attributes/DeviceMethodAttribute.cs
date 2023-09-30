#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 设备方法的特性说明,注意方法返回值必须继承<see cref="OperResult"/>,并且注意方法内部trycatch
/// <br></br>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class DeviceMethodAttribute : Attribute
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Remark { get; }
    /// <inheritdoc cref="DeviceMethodAttribute"/>
    public DeviceMethodAttribute(string desc, string remark = "")
    {
        Description = desc;
        Remark = remark;
    }
}
