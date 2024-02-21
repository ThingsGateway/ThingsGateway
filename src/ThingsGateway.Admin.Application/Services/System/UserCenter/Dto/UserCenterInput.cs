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
/// 编辑个人信息参数
/// </summary>
public class UpdateInfoInput : SysUser
{
}

public class UpdateSignatureInput
{
}

/// <summary>
/// 更新个人工作台
/// </summary>
public class UpdateWorkbenchInput
{    /// <summary>
     /// 工作台数据
     /// </summary>
    [Required(ErrorMessage = "WorkbenchData不能为空")]
    public List<long> WorkbenchData { get; set; }
}

/// <summary>
/// 更新个人主页
/// </summary>
public class UpdateDefaultRazorInput
{
    /// <summary>
    /// 个人主页数据
    /// </summary>
    [MinValue(1, ErrorMessage = "DefaultRazorData不能为空")]
    public long DefaultRazorData { get; set; }
}

/// <summary>
/// 修改密码
/// </summary>
public class UpdatePasswordInput : BaseIdInput, IValidatableObject
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Description("旧密码")]
    [Required(ErrorMessage = "不能为空")]
    public string Password { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Description("新密码")]
    [Required(ErrorMessage = "不能为空")]
    public string NewPassword { get; set; }

    /// <summary>
    /// 确认密码
    /// </summary>
    [Description("确认密码")]
    [Required(ErrorMessage = "不能为空")]
    public string ConfirmPassword { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPassword != ConfirmPassword)
            yield return new ValidationResult("两次密码不一致", new[] { nameof(ConfirmPassword) });
    }
}