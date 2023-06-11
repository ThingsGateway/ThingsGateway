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

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 附加属性
/// </summary>
public class DependencyProperty
{
    /// <summary>
    /// 属性描述
    /// </summary>
    [Description("描述")]
    public string Description { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    [Description("名称")]
    public string PropertyName { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [Description("备注")]
    [MaxLength(50)]
    public string Remark { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    [Description("属性值")]
    public string Value { get; set; }
}

