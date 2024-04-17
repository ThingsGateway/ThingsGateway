
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 动态属性的特性说明
/// <br></br>
/// 在需主动暴露的配置属性中加上这个特性<see cref="DynamicPropertyAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class DynamicPropertyAttribute : Attribute
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Remark { get; set; }
}