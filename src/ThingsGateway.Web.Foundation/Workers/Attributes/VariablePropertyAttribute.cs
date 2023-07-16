#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
    public string Description { get; }
    /// <summary>
    /// 附加说明
    /// </summary>
    public string Remark { get; }
    /// <inheritdoc cref="VariablePropertyAttribute"/>>
    public VariablePropertyAttribute(string desc, string remark = "")
    {
        Description = desc;
        Remark = remark;
    }
}
