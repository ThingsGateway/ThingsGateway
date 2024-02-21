//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 菜单树查询参数
/// </summary>

public class MenuPageInput : BasePageInput
{
    /// <summary>
    /// 父ID
    /// </summary>
    [Required(ErrorMessage = "ParentId不能为空")]
    public long ParentId { get; set; }
}

/// <summary>
/// 添加菜单参数
/// </summary>

public class MenuAddInput : SysResource, IValidatableObject
{
    /// <summary>
    /// 父ID
    /// </summary>
    [Required(ErrorMessage = "ParentId不能为空")]
    [Description("父级")]
    public override long? ParentId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "Title不能为空")]
    public override string Title { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public override MenuTypeEnum MenuType { get; set; } = MenuTypeEnum.MENU;

    /// <summary>
    /// 路径
    /// </summary>
    [Required(ErrorMessage = "Href不能为空")]
    public override string Href { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [Required(ErrorMessage = "Icon不能为空")]
    public override string Icon { get; set; }

    /// <summary>
    /// 特殊验证
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        //如果菜单类型是菜单
        if (MenuType == MenuTypeEnum.MENU)
        {
            if (string.IsNullOrEmpty(Href))
                yield return new ValidationResult("Href不能为空", new[] { nameof(Href) });
        }
        //如果是内链或者外链
        else if (MenuType == MenuTypeEnum.IFRAME || MenuType == MenuTypeEnum.LINK)
        {
            if (string.IsNullOrEmpty(Href))
                yield return new ValidationResult("Href不能为空", new[] { nameof(Href) });
        }
        else
        {
            if (string.IsNullOrEmpty(Href))
                yield return new ValidationResult("Href不能为空", new[] { nameof(Href) });
        }
        //设置分类为菜单
        Category = CateGoryConst.Resource_MENU;
    }
}

/// <summary>
/// 编辑菜单输入参数
/// </summary>
public class MenuEditInput : MenuAddInput
{
    /// <summary>
    /// ID
    /// </summary>
    [MinValue(1, ErrorMessage = "Id不能为空")]
    public override long Id { get; set; }
}