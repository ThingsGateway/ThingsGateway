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

using Mapster;

using System.ComponentModel;
using System.Reflection;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 附加属性
/// </summary>
public class DependencyPropertyWithInfo : DependencyProperty
{
    /// <summary>
    /// 备注
    /// </summary>
    [Description("备注")]
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Remark { get; set; }

    /// <summary>
    /// 属性类型
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public PropertyInfo? PropertyType { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public MethodInfo? MethodInfo { get; set; }
}

public class DependencyPropertyWithInfoMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<DependencyPropertyWithInfo, DependencyPropertyWithInfo>()
            .Map(dest => dest.Remark, src => src.Remark)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PropertyType, src => src.PropertyType)
            .Map(dest => dest.MethodInfo, src => src.MethodInfo);
        config.ForType<DependencyPropertyWithInfo, DependencyProperty>()
    .Map(dest => dest.Name, src => src.Name)
    .Map(dest => dest.Description, src => src.Description);
    }
}